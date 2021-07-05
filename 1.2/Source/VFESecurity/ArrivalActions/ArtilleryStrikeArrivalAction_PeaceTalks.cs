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

    public class ArtilleryStrikeArrivalAction_PeaceTalks : ArtilleryStrikeArrivalAction
    {

        public ArtilleryStrikeArrivalAction_PeaceTalks()
        {
        }

        public ArtilleryStrikeArrivalAction_PeaceTalks(Map source)
        {
            sourceMap = source;
        }

        private static readonly IntRange RaidIntervalRange = new IntRange(GenDate.TicksPerDay, GenDate.TicksPerDay * 2);
        private Map sourceMap;

        public override void Arrived(List<ActiveArtilleryStrike> artilleryStrikes, int tile)
        {
            if (Find.WorldObjects.WorldObjectAt(tile, RimWorld.WorldObjectDefOf.PeaceTalks) is PeaceTalks peaceTalks)
            {
                if (artilleryStrikes.Any(s => s.shellDef.projectile.damageDef.harmsHealth))
                {
                    var faction = peaceTalks.Faction;
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -99999, reason: "VFESecurity.GoodwillChangedReason_ArtilleryStrike".Translate());

                    // 50% chance of causing a raid
                    if (Rand.Bool)
                    {
                        var parms = new IncidentParms();
                        parms.target = sourceMap;
                        parms.points = StorytellerUtility.DefaultThreatPointsNow(sourceMap);
                        parms.faction = faction;
                        parms.generateFightersOnly = true;
                        parms.forced = true;
                        Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + RaidIntervalRange.RandomInRange, parms);
                    }

                    Find.QuestManager.QuestsListForReading.Where(q => q.QuestLookTargets.Contains(peaceTalks)).ToList().ForEach(q => q.End(QuestEndOutcome.Fail));
                    Find.LetterStack.ReceiveLetter("VFESecurity.ArtilleryStrikeProvokedGeneric_Letter".Translate(peaceTalks.def.label), "VFESecurity.ArtilleryStrikePeaceTalks_LetterText".Translate(faction.Name), LetterDefOf.NegativeEvent);
                    Find.WorldObjects.Remove(peaceTalks);

                    if (ArtilleryComp != null)
                        ArtilleryComp.ResetForcedTarget();
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref sourceMap, "sourceMap");
            base.ExposeData();
        }

    }

}
