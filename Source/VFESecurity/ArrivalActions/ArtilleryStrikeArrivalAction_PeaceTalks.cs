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

    public class ArtilleryStrikeArrivalAction_PeaceTalks : ArtilleryStrikeArrivalAction
    {

        public ArtilleryStrikeArrivalAction_PeaceTalks(Map source)
        {
            this.source = source;
        }

        private static readonly IntRange RaidIntervalRange = new IntRange(GenDate.TicksPerDay, GenDate.TicksPerDay * 2);
        private Map source;

        public override void Arrived(List<ActiveArtilleryStrike> artilleryStrikes, int tile)
        {
            if (artilleryStrikes.Any(s => s.shellDef.projectile.damageDef.harmsHealth) && Find.WorldObjects.WorldObjectAt(tile, RimWorld.WorldObjectDefOf.PeaceTalks) is PeaceTalks peaceTalks)
            {
                var faction = peaceTalks.Faction;
                faction.TryAffectGoodwillWith(Faction.OfPlayer, -99999, reason: "VFESecurity.GoodwillChangedReason_ArtilleryStrike".Translate());

                // 50% chance of causing a raid
                if (Rand.Bool)
                {
                    var parms = new IncidentParms();
                    parms.target = source;
                    parms.points = StorytellerUtility.DefaultThreatPointsNow(source);
                    parms.faction = faction;
                    parms.generateFightersOnly = true;
                    parms.forced = true;
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + RaidIntervalRange.RandomInRange, parms);
                }

                Find.LetterStack.ReceiveLetter("VFESecurity.ArtilleryStrikePeaceTalks_Letter".Translate(), "VFESecurity.ArtilleryStrikePeaceTalks_LetterText".Translate(faction.Name), LetterDefOf.NegativeEvent);
                Find.WorldObjects.Remove(peaceTalks);
            }
            
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref source, "source");
            base.ExposeData();
        }

    }

}
