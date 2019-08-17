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

    public class ArtilleryStrikeArrivalAction_Map : ArtilleryStrikeArrivalAction
    {

        public ArtilleryStrikeArrivalAction_Map(MapParent mapParent, float missRadius)
        {
            this.mapParent = mapParent;
            this.missRadius = missRadius;
        }

        public override void Arrived(ActiveArtilleryStrike artilleryStrike, int tile)
        {
            var map = mapParent.Map;
            if (map != null)
            {
                var potentialCells = GenRadial.RadialCellsAround(map.Center, missRadius, true);
                foreach (var def in artilleryStrike.artilleryShellDefs)
                {
                    var artilleryStrikeIncoming = (ArtilleryStrikeIncoming)SkyfallerMaker.MakeSkyfaller(ThingDefOf.VFE_ArtilleryStrikeIncoming);
                    artilleryStrikeIncoming.artilleryShellDef = def;
                    GenSpawn.Spawn(artilleryStrikeIncoming, potentialCells.RandomElement(), map);
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Values.Look(ref missRadius, "missRadius");
            base.ExposeData();
        }

        public MapParent mapParent;
        public float missRadius;

    }

}
