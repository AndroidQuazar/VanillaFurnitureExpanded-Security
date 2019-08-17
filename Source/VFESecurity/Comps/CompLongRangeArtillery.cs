using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace VFESecurity
{

    public class CompLongRangeArtillery : ThingComp
    {

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

        private bool UnderRoof => parent.OccupiedRect().Cells.Any(c => c.Roofed(parent.Map));

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
                    icon = LongRangeArtilleryUtility.launchGizmoIconCache[parent.def],
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
                if (!TurretManned)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailUnmanned".Translate(parent.def.LabelCap));

                // Cooldown
                if ((int)NonPublicFields.Building_TurretGun_burstCooldownTicksLeft.GetValue(Turret) > 0)
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

                var mapParent = t.WorldObject as MapParent;
                if (mapParent != null)
                    return "VFESecurity.TargetWorldTileSite".Translate();
                return "VFESecurity.TargetWorldTileEmpty".Translate();
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

            var mapParent = target.WorldObject as MapParent;
            if (mapParent != null)
                TryLaunch(target.Tile, new ArtilleryStrikeArrivalAction_Map(mapParent, Turret.CurrentEffectiveVerb.verbProps.forcedMissRadius));
            else
                TryLaunch(target.Tile, null);
            return true;
        }

        public void TryLaunch(int destinationTile, ArtilleryStrikeArrivalAction arrivalAction)
        {
            // Make active artillery strike thing
            var activeArtilleryStrike = (ActiveArtilleryStrike)ThingMaker.MakeThing(ThingDefOf.VFE_ActiveArtilleryStrike);
            activeArtilleryStrike.artilleryShellDefs = new List<ThingDef>();

            // Simulate an attack
            if (ChangeableProjectile != null)
            {
                activeArtilleryStrike.artilleryShellDefs.Add(ChangeableProjectile.Projectile);
                ChangeableProjectile.Notify_ProjectileLaunched();
            }
            else
            {
                var verb = Turret.CurrentEffectiveVerb;
                for (int i = 0; i < verb.verbProps.burstShotCount; i++)
                {
                    activeArtilleryStrike.artilleryShellDefs.Add(verb.GetProjectile());
                    if (verb.verbProps.consumeFuelPerShot > 0 && RefuelableComp != null)
                        RefuelableComp.ConsumeFuel(verb.verbProps.consumeFuelPerShot);
                }
            }
            NonPublicMethods.Building_TurretGun_BurstComplete(Turret);

            var artilleryStrikeLeaving = (ArtilleryStrikeLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.VFE_ArtilleryStrikeLeaving, activeArtilleryStrike);
            artilleryStrikeLeaving.destinationTile = destinationTile;
            artilleryStrikeLeaving.arrivalAction = arrivalAction;

            int angle = (int)Find.WorldGrid.GetDirection8WayFromTo(parent.Map.Tile, destinationTile) * 45;
            var skyfallerPos = GenAdj.CellsAdjacent8Way(parent).MinBy(c => Mathf.Abs(angle - (c - parent.Position).AngleFlat));

            var turretTop = (TurretTop)NonPublicFields.Building_TurretGun_top.GetValue(Turret);
            int cooldownTicks = (int)NonPublicFields.Building_TurretGun_burstCooldownTicksLeft.GetValue(Turret);
            NonPublicProperties.TurretTop_set_CurRotation(turretTop, angle);
            NonPublicFields.TurretTop_ticksUntilIdleTurn.SetValue(turretTop, (int)NonPublicFields.TurretTop_ticksUntilIdleTurn.GetValue(turretTop) + cooldownTicks);

            GenSpawn.Spawn(artilleryStrikeLeaving, skyfallerPos, parent.Map);
            CameraJumper.TryHideWorld();
        }

        //private IEnumerable<FloatMenuOption> GetArtilleryFloatMenuOptionsAt(int tile)
        //{
        //    bool anything = false;

        //}

    }

}
