using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFESecurity
{
    public class ArtilleryStrikeLeaving : ArtilleryStrikeSkyfaller
    {
        private static List<ArtilleryStrikeLeaving> tmpActiveArtilleryStrikes = new List<ArtilleryStrikeLeaving>();

        public LocalTargetInfo startCell;
        public LocalTargetInfo edgeCell;
        public float rotation;
        public int groupID;
        private bool alreadyLeft;
        public int destinationTile;
        public ArtilleryStrikeArrivalAction arrivalAction;

        private Graphic cachedShellGraphic;

        protected override ThingDef ShellDef => ((ActiveArtilleryStrike)innerContainer[0]).shellDef;

        public override Vector3 DrawPos => Vector3.Lerp(startCell.CenterVector3, edgeCell.CenterVector3, (float)ticksToImpact / 220);

        public override Graphic Graphic
        {
            get
            {
                if (cachedShellGraphic == null)
                {
                    var impliedGraphicData = new GraphicData();
                    impliedGraphicData.CopyFrom(ShellDef.graphicData);
                    impliedGraphicData.shaderType = ShaderTypeDefOf.CutoutFlying;
                    cachedShellGraphic = impliedGraphicData.Graphic;
                }
                return cachedShellGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                angle = rotation;
            }
        }

        public override void Tick()
        {
            Position = DrawPos.ToIntVec3();
            base.Tick();
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
        }

        protected override void LeaveMap()
        {
            if (alreadyLeft)
            {
                base.LeaveMap();
                Destroy();
            }

            if (destinationTile < 0)
            {
                Log.Error("Artillery strike left the map, but its destination tile is " + destinationTile);
                Destroy(DestroyMode.Vanish);
                return;
            }

            // Make world object
            var travellingArtilleryStrike = (TravellingArtilleryStrike)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.VFES_TravellingArtilleryStrike);
            travellingArtilleryStrike.Tile = Map.Tile;
            travellingArtilleryStrike.SetFaction(Faction.OfPlayer);
            travellingArtilleryStrike.destinationTile = destinationTile;
            travellingArtilleryStrike.arrivalAction = arrivalAction;
            Find.WorldObjects.Add(travellingArtilleryStrike);

            // Transfer artillery strikes to world object
            tmpActiveArtilleryStrikes.Clear();
            tmpActiveArtilleryStrikes.AddRange(Map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder).Where(t => t is ArtilleryStrikeLeaving).Cast<ArtilleryStrikeLeaving>());
            for (int i = 0; i < tmpActiveArtilleryStrikes.Count; i++)
            {
                var strike = tmpActiveArtilleryStrikes[i];
                if (strike != null && strike.groupID == groupID)
                {
                    strike.alreadyLeft = true;
                    strike.innerContainer.TryTransferAllToContainer(travellingArtilleryStrike.innerContainer);
                    strike.Destroy();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_TargetInfo.Look(ref startCell, "startCell");
            Scribe_TargetInfo.Look(ref edgeCell, "edgeCell");
            Scribe_Values.Look(ref rotation, "rotation");
            Scribe_Values.Look(ref groupID, "groupID");
            Scribe_Values.Look(ref destinationTile, "destinationTile");
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
        }
    }
}