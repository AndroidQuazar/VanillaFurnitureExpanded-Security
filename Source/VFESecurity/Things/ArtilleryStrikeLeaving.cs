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

    public class ArtilleryStrikeLeaving : Skyfaller
    {

        public int destinationTile;
        public ArtilleryStrikeArrivalAction arrivalAction;
        public float shellMissRadius;

        protected override void LeaveMap()
        {
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
            innerContainer.TryTransferAllToContainer(travellingArtilleryStrike.innerContainer);
            Destroy();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref destinationTile, "destinationTile");
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
            base.ExposeData();
        }

    }

}
