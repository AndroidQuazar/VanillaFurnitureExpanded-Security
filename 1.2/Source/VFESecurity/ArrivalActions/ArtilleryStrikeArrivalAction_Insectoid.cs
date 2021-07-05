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

    public class ArtilleryStrikeArrivalAction_Insectoid : ArtilleryStrikeArrivalAction_Settlement
    {

        private const int RetaliationTicksPerRetaliation = GenDate.TicksPerDay * 8;
        private const int RetaliationTicksPerExtraPointsMultiplier = GenDate.TicksPerDay * 15;
        private static readonly IntRange RaidIntervalRange = new IntRange(GenDate.TicksPerDay / 2, GenDate.TicksPerDay);

        public ArtilleryStrikeArrivalAction_Insectoid()
        {
        }

        public ArtilleryStrikeArrivalAction_Insectoid(WorldObject worldObject, Map sourceMap)
        {
            this.worldObject = worldObject;
            this.sourceMap = sourceMap;
        }

        protected override int BaseSize => MapSize;

        // Not really destroy in this context
        protected override float DestroyChancePerCellInRect => 0.008f;

        protected override void StrikeAction(ActiveArtilleryStrike strike, CellRect mapRect, CellRect baseRect, ref bool destroyed)
        {
            var radialCells = GenRadial.RadialCellsAround(mapRect.RandomCell, strike.shellDef.projectile.explosionRadius, true);
            int cellsInRect = radialCells.Count(c => baseRect.Contains(c));

            // Aggro the insects
            if (cellsInRect > 0 && Rand.Chance(cellsInRect * DestroyChancePerCellInRect))
            {
                var artilleryComp = Settlement.GetComponent<ArtilleryComp>();
                var parms = new IncidentParms();
                parms.target = sourceMap;
                parms.points = StorytellerUtility.DefaultThreatPointsNow(sourceMap) * (1 + ((float)artilleryComp.recentRetaliationTicks / RetaliationTicksPerExtraPointsMultiplier));
                parms.faction = Settlement.Faction;
                parms.generateFightersOnly = true;
                parms.forced = true;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + RaidIntervalRange.RandomInRange, parms);
                artilleryComp.recentRetaliationTicks += RetaliationTicksPerRetaliation;
            }
        }

        protected override void PostStrikeAction(bool destroyed)
        {
            // No PostStrikeAction to see here
        }

    }

}
