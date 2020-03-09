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
using HarmonyLib;

namespace VFESecurity
{

    public class ArtilleryStrikeArrivalAction_Map : ArtilleryStrikeArrivalAction
    {

        public ArtilleryStrikeArrivalAction_Map()
        {
        }

        public ArtilleryStrikeArrivalAction_Map(MapParent mapParent)
        {
            this.mapParent = mapParent;
        }

        public override void Arrived(List<ActiveArtilleryStrike> artilleryStrikes, int tile)
        {
            // Boom
            var map = mapParent.Map;
            if (map != null)
            {
                for (int i = 0; i < artilleryStrikes.Count; i++)
                {
                    var strike = artilleryStrikes[i];
                    var potentialCells = ArtilleryStrikeUtility.PotentialStrikeCells(map, strike.missRadius);
                    for (int j = 0; j < strike.shellCount; j++)
                        ArtilleryStrikeUtility.SpawnArtilleryStrikeSkyfaller(strike.shellDef, map, potentialCells.RandomElement());
                }
            }
            else
                ArtilleryComp.ResetForcedTarget();
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref mapParent, "mapParent");
            base.ExposeData();
        }

        public MapParent mapParent;

    }

}
