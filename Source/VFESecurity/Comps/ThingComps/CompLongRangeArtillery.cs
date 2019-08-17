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

        private static readonly Texture2D TargetWorldTileIcon = ContentFinder<Texture2D>.Get("UI/Commands/ArtilleryTargetTile");

        private CompProperties_LongRangeArtillery Props => (CompProperties_LongRangeArtillery)props;

        private Building_TurretGun Turret => (Building_TurretGun)parent;
        private CompChangeableProjectile ChangeableProjectile => Turret.gun.TryGetComp<CompChangeableProjectile>();
        private CompRefuelable RefuelableComp => parent.GetComp<CompRefuelable>();
        private CompPowerTrader PowerComp => parent.GetComp<CompPowerTrader>();
        private CompMannable MannableComp => parent.GetComp<CompMannable>();

        private bool TurretPowered => PowerComp == null || PowerComp.PowerOn;
        private bool TurretHasFuel => RefuelableComp == null || RefuelableComp.HasFuel;
        private bool TurretLoaded => ChangeableProjectile == null || ChangeableProjectile.Loaded;
        private bool TurretManned => MannableComp == null || MannableComp.MannedNow;
        private bool OnCooldownPeriod => (int)NonPublicFields.Building_TurretGun_burstCooldownTicksLeft.GetValue(Turret) > 0;
        private bool UnderRoof => parent.OccupiedRect().Cells.Any(c => c.Roofed(parent.Map));

        private bool CanLaunch => TurretPowered && TurretHasFuel && TurretLoaded && TurretManned && !OnCooldownPeriod && !UnderRoof;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Don't want to do this to enemy artillery :P
            if (Turret.Faction == Faction.OfPlayer)
            {
                // Target other map tiles
                var targetTile = new Command_Action()
                {
                    defaultLabel = "VFESecurity.TargetWorldTile".Translate(),
                    defaultDesc = "VFESecurity.TargetWorldTile_Description".Translate(parent.def.label),
                    icon = TargetWorldTileIcon,
                    action = StartChoosingTarget
                };

                // No power
                if (!TurretPowered)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailNoPower".Translate(parent.def.LabelCap));

                // No fuel
                else if (!TurretHasFuel)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailNoFuel".Translate(parent.def.LabelCap, RefuelableComp.Props.FuelLabel));

                // No projectile loaded
                else if (!TurretLoaded)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailNotLoaded".Translate(parent.def.LabelCap));

                // Not manned
                else if (!TurretManned)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailUnmanned".Translate(parent.def.LabelCap));

                // Cooldown
                else if (OnCooldownPeriod)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailCooldown".Translate(parent.def.LabelCap));

                // Under a roof
                else if (UnderRoof)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailUnderRoof".Translate(parent.def.LabelCap));

                yield return targetTile;
            }
        }

        private void StartChoosingTarget()
        {
            // Adapted from transport pod code
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            int tile = parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(ChooseWorldTarget, true, (Texture2D)Turret.def.building.turretTopMat.mainTexture, true, () => GenDraw.DrawWorldRadiusRing(tile, Props.worldTileRange), (GlobalTargetInfo t) =>
            {
                // Invalid location
                if (!t.IsValid)
                    return null;

                // Same as map tile
                if (t.Tile == tile)
                {
                    GUI.color = Color.red;
                    return "VFESecurity.TargetWorldTileSameTile".Translate();
                }
                
                // Out of range
                if (Find.WorldGrid.TraversalDistanceBetween(tile, t.Tile) > Props.worldTileRange)
                {
                    GUI.color = Color.red;
                    return "VFESecurity.TargetWorldTileOutOfRange".Translate();
                }

                else
                {
                    var floatMenuOptions = GetArtilleryFloatMenuOptionsAt(t.Tile);

                    // No float menu options (this should never be the case)
                    if (!floatMenuOptions.Any())
                        return string.Empty;

                    // Only one option
                    if (floatMenuOptions.Count() == 1)
                    {
                        var option = floatMenuOptions.First();
                        if (option.Disabled)
                            GUI.color = Color.red;
                        return option.Label;
                    }

                    // Multiple options
                    var mapParent = t.WorldObject as MapParent;
                    if (mapParent != null)
                        return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
                    return "ClickToSeeAvailableOrders_Empty".Translate();
                }
            }
            );
        }

        private bool ChooseWorldTarget(GlobalTargetInfo target)
        {
            // Invalid tile
            if (!target.IsValid)
            {
                Messages.Message("VFESecurity.MessageTargetWorldTileInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Same as map tile
            if (target.Tile == parent.Map.Tile)
            {
                Messages.Message("VFESecurity.TargetWorldTileSameTile".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Out of range
            if (Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile) > Props.worldTileRange)
            {
                Messages.Message("VFESecurity.MessageTargetWorldTileOutOfRange".Translate(parent.def.label), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            var options = GetArtilleryFloatMenuOptionsAt(target.Tile);

            // No options
            if (!options.Any())
            {
                TryLaunch(target.Tile, null);
                return true;
            }

            else
            {
                // One option
                if (options.Count() == 1)
                {
                    var option = options.First();
                    if (!option.Disabled)
                        option.action();
                    return false;
                }

                // Multiple options
                Find.WindowStack.Add(new FloatMenu(options.ToList()));
                return false;
            }
        }

        private IEnumerable<FloatMenuOption> GetArtilleryFloatMenuOptionsAt(int tile)
        {
            bool anything = false;

            // World objects
            foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
            {
                if (worldObject.Tile == tile)
                {
                    // Map - target around the centre
                    if (worldObject is MapParent mapParent && mapParent.HasMap)
                    {
                        yield return new FloatMenuOption("VFESecurity.TargetMap".Translate(), () => TryLaunch(tile, new ArtilleryStrikeArrivalAction_Map(mapParent)));
                        anything = true;
                    }

                    // Peace talks - cause badwill and potentially cause a raid
                    if (worldObject is PeaceTalks talks)
                    {
                        yield return new FloatMenuOption("VFESecurity.TargetPeaceTalks".Translate(), () => TryLaunch(tile, new ArtilleryStrikeArrivalAction_PeaceTalks(parent.Map)));
                        anything = true;
                    }

                    // Settlement - cause badwill, potentially cause an artillery retaliation and potentially destroy
                    if (worldObject is Settlement settlement && settlement.Faction != Faction.OfPlayer)
                    {
                        yield return new FloatMenuOption("VFESecurity.TargetSettlement".Translate(), () => TryLaunch(tile, new ArtilleryStrikeArrivalAction_Settlement(settlement)));
                        anything = true;
                    }

                    if (worldObject is Site site)
                    {
                        // Bandit camp - potentially destroy
                        if (site.core.def == SiteCoreDefOf.Nothing && site.parts.Any(p => p.def == SitePartDefOf.Outpost))
                        {
                            yield return new FloatMenuOption("VFESecurity.TargetOutpost".Translate(), () => TryLaunch(tile, new ArtilleryStrikeArrivalAction_Outpost(site)));
                            anything = true;
                        }
                    }
                }
            }
            if (!anything)
            {
                yield return new FloatMenuOption("VFESecurity.TargetWorldTileEmpty".Translate(), () => TryLaunch(tile, null));
            }
        }

        public void TryLaunch(int destinationTile, ArtilleryStrikeArrivalAction arrivalAction)
        {
            CameraJumper.TryHideWorld();
            var artilleryComps = Find.Selector.SelectedObjectsListForReading.Where(o => o is Thing t && t.TryGetComp<CompLongRangeArtillery>() != null).Select(t => ((Thing)t).TryGetComp<CompLongRangeArtillery>()).Where(c => c.CanLaunch);

            foreach (var comp in artilleryComps)
            {
                // Play sounds
                var verb = comp.Turret.CurrentEffectiveVerb;
                if (verb.verbProps.soundCast != null)
                    verb.verbProps.soundCast.PlayOneShot(new TargetInfo(comp.parent.Position, comp.parent.Map));
                if (verb.verbProps.soundCastTail != null)
                    verb.verbProps.soundCastTail.PlayOneShotOnCamera(comp.parent.Map);

                // Make active artillery strike thing
                var activeArtilleryStrike = (ActiveArtilleryStrike)ThingMaker.MakeThing(ThingDefOf.VFE_ActiveArtilleryStrike);
                activeArtilleryStrike.missRadius = ArtilleryStrikeUtility.FinalisedMissRadius(comp.Turret.CurrentEffectiveVerb.verbProps.forcedMissRadius, comp.Props.maxForcedMissRadiusFactor, comp.parent.Tile, destinationTile, comp.Props.worldTileRange);

                // Simulate an attack
                if (comp.ChangeableProjectile != null)
                {
                    activeArtilleryStrike.shellDef = comp.ChangeableProjectile.Projectile;
                    activeArtilleryStrike.shellCount = 1;
                    comp.ChangeableProjectile.Notify_ProjectileLaunched();
                }
                else
                {
                    activeArtilleryStrike.shellDef = verb.GetProjectile();
                    for (int j = 0; j < verb.verbProps.burstShotCount; j++)
                    {
                        activeArtilleryStrike.shellCount++;
                        if (verb.verbProps.consumeFuelPerShot > 0 && comp.RefuelableComp != null)
                            comp.RefuelableComp.ConsumeFuel(verb.verbProps.consumeFuelPerShot);
                    }
                }
                NonPublicMethods.Building_TurretGun_BurstComplete(comp.Turret);

                var artilleryStrikeLeaving = (ArtilleryStrikeLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.VFE_ArtilleryStrikeLeaving, activeArtilleryStrike);
                artilleryStrikeLeaving.destinationTile = destinationTile;
                artilleryStrikeLeaving.arrivalAction = arrivalAction;
                artilleryStrikeLeaving.groupID = Find.TickManager.TicksGame;

                int angle = (int)Find.WorldGrid.GetDirection8WayFromTo(comp.parent.Map.Tile, destinationTile) * 45;
                var skyfallerPos = GenAdj.CellsAdjacent8Way(comp.parent).MinBy(c => Mathf.Abs(angle - (c - comp.parent.Position).AngleFlat));

                var turretTop = (TurretTop)NonPublicFields.Building_TurretGun_top.GetValue(comp.Turret);
                int cooldownTicks = (int)NonPublicFields.Building_TurretGun_burstCooldownTicksLeft.GetValue(comp.Turret);
                NonPublicProperties.TurretTop_set_CurRotation(turretTop, angle);
                NonPublicFields.TurretTop_ticksUntilIdleTurn.SetValue(turretTop, (int)NonPublicFields.TurretTop_ticksUntilIdleTurn.GetValue(turretTop) + cooldownTicks);

                GenSpawn.Spawn(artilleryStrikeLeaving, skyfallerPos, comp.parent.Map);
            }
        }

        public override void PostExposeData()
        {
            Scribe_TargetInfo.Look(ref targetedTile, "targetedTile");
            base.PostExposeData();
        }

        public GlobalTargetInfo targetedTile;

    }

}
