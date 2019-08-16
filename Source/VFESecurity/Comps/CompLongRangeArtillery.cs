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

        private CompRefuelable RefuelableComp => parent.TryGetComp<CompRefuelable>();

        private bool TurretHasFuel => (RefuelableComp == null || RefuelableComp.HasFuel);

        private bool TurretLoaded => (ChangeableProjectile == null || ChangeableProjectile.Loaded);

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
                    defaultDesc = "VFESecurity.TargetWorldTile_Description".Translate(),
                    icon = LongRangeArtilleryUtility.launchGizmoIconCache[parent.def],
                    action = StartChoosingTarget
                };

                // No fuel
                if (!TurretHasFuel)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailNoFuel".Translate(parent.def.LabelCap, RefuelableComp.Props.FuelLabel));

                // No projectile loaded
                if (!TurretLoaded)
                    targetTile.Disable("VFESecurity.CommandTargetTileFailNotLoaded".Translate(parent.def.LabelCap));

                // Under a roof
                if (!UnderRoof)
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
            Find.WorldTargeter.BeginTargeting(ChooseWorldTarget, true, CompLaunchable.TargeterMouseAttachment, true, () => GenDraw.DrawWorldRadiusRing(tile, Props.worldTileRange), (GlobalTargetInfo t) =>
            {
                // Invalid location
                if (!t.IsValid)
                    return null;

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

            // Out of range
            if (Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile) > Props.worldTileRange)
            {
                Messages.Message("VFESecurity.MessageTargetWorldTileOutOfRange".Translate(parent.def.label), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            TryLaunch(target.Tile);
            return true;
        }

        public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
        {
        }

        //private IEnumerable<FloatMenuOption> GetArtilleryFloatMenuOptionsAt(int tile)
        //{
        //    bool anything = false;

        //}

    }

}
