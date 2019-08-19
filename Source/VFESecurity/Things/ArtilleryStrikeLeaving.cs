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

    public class ArtilleryStrikeLeaving : ArtilleryStrikeSkyfaller
    {

        private static List<ArtilleryStrikeLeaving> tmpActiveArtilleryStrikes = new List<ArtilleryStrikeLeaving>();

        public int groupID;
        private bool alreadyLeft;
        public int destinationTile;
        public ArtilleryStrikeArrivalAction arrivalAction;

        protected override ThingDef ShellDef => ((ActiveArtilleryStrike)innerContainer[0]).shellDef;

        public override Graphic Graphic
        {
            get
            {
                if (ShellDef.GetModExtension<ThingDefExtension>() is ThingDefExtension thingDefExtension && thingDefExtension.leavingSkyfallerGraphicData != null)
                    return thingDefExtension.leavingSkyfallerGraphicData.Graphic;
                return base.Graphic;
            }
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
            var travellingArtilleryStrike = (TravellingArtilleryStrike)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravellingArtilleryStrike);
            travellingArtilleryStrike.Tile = Map.Tile;
            travellingArtilleryStrike.SetFaction(Faction.OfPlayer);
            travellingArtilleryStrike.destinationTile = destinationTile;
            travellingArtilleryStrike.arrivalAction = arrivalAction;
            Find.WorldObjects.Add(travellingArtilleryStrike);

            // Transfer artillery strikes to world object
            tmpActiveArtilleryStrikes.Clear();
            tmpActiveArtilleryStrikes.AddRange(Map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder).Where(t => t is ArtilleryStrikeLeaving).Cast<ArtilleryStrikeLeaving>());
            foreach (var strike in tmpActiveArtilleryStrikes)
            {
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
            Scribe_Values.Look(ref groupID, "groupID");
            Scribe_Values.Look(ref destinationTile, "destinationTile");
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
            base.ExposeData();
        }

    }

}
