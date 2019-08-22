using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace VFESecurity
{

    [StaticConstructorOnStartup]
    public class CompLongRangeArtillery : ThingComp
    {

        public static readonly Texture2D TargetWorldTileIcon = ContentFinder<Texture2D>.Get("UI/Commands/ArtilleryTargetTile");

        public CompProperties_LongRangeArtillery Props => (CompProperties_LongRangeArtillery)props;

        private Building_TurretGun Turret => (Building_TurretGun)parent;
        private CompChangeableProjectile ChangeableProjectile => Turret.gun.TryGetComp<CompChangeableProjectile>();
        private CompRefuelable RefuelableComp => parent.GetComp<CompRefuelable>();
        private CompPowerTrader PowerComp => parent.GetComp<CompPowerTrader>();
        private CompMannable MannableComp => parent.GetComp<CompMannable>();

        public bool CanLaunch => (PowerComp == null || PowerComp.PowerOn) && (RefuelableComp == null || RefuelableComp.HasFuel) && (ChangeableProjectile == null || ChangeableProjectile.Loaded)
            && (MannableComp == null || MannableComp.MannedNow) && (int)NonPublicFields.Building_TurretGun_burstCooldownTicksLeft.GetValue(Turret) <= 0 && !parent.OccupiedRect().Cells.Any(c => c.Roofed(parent.Map));

        private int CurAngle => targetedTile != GlobalTargetInfo.Invalid ? (int)Find.WorldGrid.GetDirection8WayFromTo(parent.Map.Tile, targetedTile.Tile) * 45 : -1;

        public LocalTargetInfo FacingEdgeCell
        {
            get
            {
                if (targetedTile != GlobalTargetInfo.Invalid)
                {
                    var edgeCells = new CellRect(0, 0, parent.Map.Size.x, parent.Map.Size.z).EdgeCells;
                    return edgeCells.MinBy(c => Mathf.Abs(CurAngle - (c.ToVector3() - parent.TrueCenter()).AngleFlat()));
                }
                return LocalTargetInfo.Invalid;
            }
        }

        private ArtilleryComp ArtilleryMapComp(Map map)
        {
            return Find.WorldObjects.AllWorldObjects.FirstOrDefault(o => o.Tile == map.Tile && o.GetComponent(typeof(ArtilleryComp)) != null).GetComponent<ArtilleryComp>();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            // Register to map component
            var mapComp = ArtilleryMapComp(parent.Map);
            if (mapComp != null)
                mapComp.artillery.Add(parent);

            // Register to world component
            Find.World.GetComponent<WorldArtilleryTracker>().RegisterArtilleryComp(this);

            ResetWarmupTicks();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            // Deregister from map component
            var mapComp = ArtilleryMapComp(map);
            if (mapComp != null)
                mapComp.artillery.Remove(parent);

            // Deregister from world component
            Find.World.GetComponent<WorldArtilleryTracker>().DeregisterArtilleryComp(this);

            base.PostDeSpawn(map);
        }

        public override void CompTick()
        {
            // Automatically attack if there is a forced target
            if (targetedTile != GlobalTargetInfo.Invalid)
            {
                var turretTop = (TurretTop)NonPublicFields.Building_TurretGun_top.GetValue(Turret);
                NonPublicProperties.TurretTop_set_CurRotation(turretTop, CurAngle);
                NonPublicFields.TurretTop_ticksUntilIdleTurn.SetValue(turretTop, Rand.RangeInclusive(150, 350));
                if (CanLaunch)
                { 
                    if (warmupTicksLeft == 0)
                    {
                        TryLaunch(targetedTile.Tile);
                        ResetWarmupTicks();
                    }
                    else
                        warmupTicksLeft--;
                }
            }

            // Set warmup ticks if the turret is unmanned
            if (MannableComp != null && !MannableComp.MannedNow)
                ResetWarmupTicks();
        }

        private void ResetWarmupTicks()
        {
            warmupTicksLeft = Mathf.Max(1, Turret.def.building.turretBurstWarmupTime.SecondsToTicks());
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Don't want to do this to enemy artillery :P
            if (Turret.Faction == Faction.OfPlayer)
            {
                // Target other map tiles
                yield return new Command_Action()
                {
                    defaultLabel = "VFESecurity.TargetWorldTile".Translate(),
                    defaultDesc = "VFESecurity.TargetWorldTile_Description".Translate(parent.def.label),
                    icon = TargetWorldTileIcon,
                    action = () => StartChoosingTarget()
                };

                // Cancel targeting
                if (targetedTile != GlobalTargetInfo.Invalid)
                {
                    yield return new Command_Action()
                    {
                        defaultLabel = "CommandStopForceAttack".Translate(),
                        defaultDesc = "CommandStopForceAttackDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true),
                        action = () => ResetForcedTarget()
                    };
                }
            }
        }

        public void StartChoosingTarget()
        {
            // Adapted from transport pod code
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            int tile = parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(ChooseWorldTarget, true, (Texture2D)Turret.def.building.turretTopMat.mainTexture, true, () => GenDraw.DrawWorldRadiusRing(tile, Props.worldTileRange), TargetChooserLabel);
        }

        public bool ChooseWorldTarget(GlobalTargetInfo t)
        {
            // Invalid tile
            if (!t.IsValid)
            {
                Messages.Message("VFESecurity.MessageTargetWorldTileInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Out of range
            int parentMap = parent.Map.Tile;
            if (Find.WorldGrid.TraversalDistanceBetween(parentMap, t.Tile) > Props.worldTileRange)
            {
                Messages.Message("VFESecurity.MessageTargetWorldTileOutOfRange".Translate(parent.def.label), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Transport pods or artillery strike
            if (t.WorldObject is TravellingArtilleryStrike || t.WorldObject is TravellingArtilleryStrike)
            {
                Messages.Message("VFESecurity.TargetWorldFlyingObject".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Same as map tile
            if (parentMap == t.Tile)
            {
                Messages.Message("VFESecurity.TargetWorldTileSameTile".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            var floatMenuOptions = FloatMenuOptionsFor(t.Tile);

            // No options
            if (!floatMenuOptions.Any())
            {
                SetTargetedTile(t);
                return true;
            }
            else
            {
                // One option
                if (floatMenuOptions.Count() == 1)
                {
                    if (!floatMenuOptions.First().Disabled)
                        floatMenuOptions.First().action();
                    return false;
                }

                // Multiple options
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions.ToList()));
                return false;
            }
        }

        public string TargetChooserLabel(GlobalTargetInfo t)
        {
            // Invalid location
            if (!t.IsValid)
                return null;

            // Out of range
            int parentMap = parent.Map.Tile;
            if (Find.WorldGrid.TraversalDistanceBetween(parentMap, t.Tile) > Props.worldTileRange)
            {
                GUI.color = Color.red;
                return "VFESecurity.TargetWorldTileOutOfRange".Translate();
            }

            // Transport pods or artillery strike
            if (t.WorldObject is TravellingArtilleryStrike || t.WorldObject is TravellingArtilleryStrike)
            {
                GUI.color = Color.red;
                return "VFESecurity.TargetWorldFlyingObject".Translate();
            }

            // Same as map tile
            if (parentMap == t.Tile)
            {
                GUI.color = Color.red;
                return "VFESecurity.TargetWorldTileSameTile".Translate();
            }

            var floatMenuOptions = FloatMenuOptionsFor(t.Tile);

            // No options
            if (!floatMenuOptions.Any())
                return string.Empty;

            // Return first option label
            if (floatMenuOptions.Count() == 1)
            {
                if (floatMenuOptions.First().Disabled)
                    GUI.color = Color.red;
                return floatMenuOptions.First().Label;
            }

            // Multiple options
            MapParent mapParent = t.WorldObject as MapParent;
            if (mapParent != null)
                return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);

            // No orders
            return "ClickToSeeAvailableOrders_Empty".Translate();
        }

        public IEnumerable<FloatMenuOption> FloatMenuOptionsFor(int tile)
        {
            bool anything = false;
            foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
            {
                if (worldObject.Tile == tile)
                {
                    if (worldObject != null)
                    {
                        if (worldObject is MapParent mapParent && mapParent.HasMap)
                        {
                            yield return new FloatMenuOption("VFESecurity.TargetMap".Translate(), () => SetTargetedTile(worldObject));
                            anything = true;
                        }

                        // Peace talks - cause badwill and potentially cause a raid
                        if (worldObject is PeaceTalks talks)
                        {
                            yield return new FloatMenuOption("VFESecurity.TargetPeaceTalks".Translate(), () => SetTargetedTile(worldObject));
                            anything = true;
                        }

                        // Settlement - cause badwill, potentially cause an artillery retaliation and potentially destroy
                        if (worldObject is Settlement settlement && settlement.Faction != Faction.OfPlayer)
                        {
                            yield return new FloatMenuOption("VFESecurity.TargetSettlement".Translate(), () => SetTargetedTile(worldObject));
                            anything = true;
                        }

                        if (worldObject is Site site)
                        {
                            // Bandit camp - potentially destroy
                            if (site.core.def == SiteCoreDefOf.Nothing && site.parts.Any(p => p.def == SitePartDefOf.Outpost))
                            {
                                yield return new FloatMenuOption("VFESecurity.TargetOutpost".Translate(), () => SetTargetedTile(worldObject));
                                anything = true;
                            }
                        }
                    }
                }
            }
            
            if (!anything)
            {
                yield return new FloatMenuOption("VFESecurity.TargetWorldTileEmpty".Translate(), () => SetTargetedTile(new GlobalTargetInfo(tile)));
            }
        }

        public void SetTargetedTile(GlobalTargetInfo t)
        {
            CameraJumper.TryHideWorld();

            var allSelectedComps = Find.Selector.SelectedObjectsListForReading.Where(o => o is Thing th && th.TryGetComp<CompLongRangeArtillery>() != null).Select(o => ((Thing)o).TryGetComp<CompLongRangeArtillery>());
            foreach (var comp in allSelectedComps)
            {
                NonPublicMethods.Building_TurretGun_ResetForcedTarget(comp.Turret);
                NonPublicMethods.Building_TurretGun_ResetCurrentTarget(comp.Turret);
                comp.targetedTile = t;
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(comp.parent.Position, comp.parent.Map, false));
                comp.ResetWarmupTicks();
            }
        }

        public void ResetForcedTarget()
        {
            targetedTile = GlobalTargetInfo.Invalid;
            ResetWarmupTicks();
        }

        private ArtilleryStrikeArrivalAction CurrentArrivalAction
        {
            get
            {
                var worldObject = targetedTile.WorldObject;
                if (worldObject != null)
                {
                    if (worldObject is MapParent mapParent && mapParent.HasMap)
                        return new ArtilleryStrikeArrivalAction_Map(mapParent);

                    // Peace talks - cause badwill and potentially cause a raid
                    if (worldObject is PeaceTalks talks)
                        return new ArtilleryStrikeArrivalAction_PeaceTalks(parent.Map);

                    // Settlement - cause badwill, potentially cause an artillery retaliation and potentially destroy
                    if (worldObject is Settlement settlement && settlement.Faction != Faction.OfPlayer)
                        return new ArtilleryStrikeArrivalAction_Settlement(settlement);

                    if (worldObject is Site site)
                    {
                        // Bandit camp - potentially destroy
                        if (site.core.def == SiteCoreDefOf.Nothing && site.parts.Any(p => p.def == SitePartDefOf.Outpost))
                            return new ArtilleryStrikeArrivalAction_Outpost(site);
                    }
                }

                return null;
            }
        }

        public void TryLaunch(int destinationTile)
        {
            var arrivalAction = CurrentArrivalAction;

            if (arrivalAction != null)
                arrivalAction.source = parent;

            // Play sounds
            var verb = Turret.CurrentEffectiveVerb;
            if (verb.verbProps.soundCast != null)
                verb.verbProps.soundCast.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
            if (verb.verbProps.soundCastTail != null)
                verb.verbProps.soundCastTail.PlayOneShotOnCamera(parent.Map);

            // Make active artillery strike thing
            var activeArtilleryStrike = (ActiveArtilleryStrike)ThingMaker.MakeThing(ThingDefOf.VFES_ActiveArtilleryStrike);
            activeArtilleryStrike.missRadius = ArtilleryStrikeUtility.FinalisedMissRadius(Turret.CurrentEffectiveVerb.verbProps.forcedMissRadius, Props.maxForcedMissRadiusFactor, parent.Tile, destinationTile, Props.worldTileRange);

            // Simulate an attack
            if (ChangeableProjectile != null)
            {
                activeArtilleryStrike.shellDef = ChangeableProjectile.Projectile;
                activeArtilleryStrike.shellCount = 1;
                ChangeableProjectile.Notify_ProjectileLaunched();
            }
            else
            {
                activeArtilleryStrike.shellDef = verb.GetProjectile();
                for (int j = 0; j < verb.verbProps.burstShotCount; j++)
                {
                    activeArtilleryStrike.shellCount++;
                    if (verb.verbProps.consumeFuelPerShot > 0 && RefuelableComp != null)
                        RefuelableComp.ConsumeFuel(verb.verbProps.consumeFuelPerShot);
                }
            }
            NonPublicMethods.Building_TurretGun_BurstComplete(Turret);

            var artilleryStrikeLeaving = (ArtilleryStrikeLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.VFES_ArtilleryStrikeLeaving, activeArtilleryStrike);
            artilleryStrikeLeaving.destinationTile = destinationTile;
            artilleryStrikeLeaving.arrivalAction = arrivalAction;
            artilleryStrikeLeaving.groupID = Find.TickManager.TicksGame;

            var skyfallerPos = GenAdj.CellsAdjacent8Way(parent).MinBy(c => Mathf.Abs(CurAngle - (c - parent.Position).AngleFlat));
            GenSpawn.Spawn(artilleryStrikeLeaving, skyfallerPos, parent.Map);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref warmupTicksLeft, "warmupTicksLeft");
            Scribe_TargetInfo.Look(ref targetedTile, "targetedTile", GlobalTargetInfo.Invalid);
            base.PostExposeData();
        }

        public int warmupTicksLeft;
        public GlobalTargetInfo targetedTile = GlobalTargetInfo.Invalid;

    }

}
