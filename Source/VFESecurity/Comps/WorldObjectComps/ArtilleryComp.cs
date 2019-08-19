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

    public class ArtilleryComp : WorldObjectComp
    {

        private const int BombardmentStartDelay = GenTicks.TicksPerRealSecond * 5;

        private WorldObjectCompProperties_Artillery Props => (WorldObjectCompProperties_Artillery)props;

        private ThingDef ArtilleryDef
        {
            get
            {
                if (cachedArtilleryDef == null)
                    cachedArtilleryDef = Props.ArtilleryDefFor(parent.Faction.def);
                return cachedArtilleryDef;
            }
        }

        private ThingDef ArtilleryGunDef => ArtilleryDef.building.turretGunDef;

        private bool HasArtillery => ArtilleryDef != null && artilleryCount > 0;
        private bool Attacking => bombardmentDurationTicks > 0;
        private bool CanAttack => HasArtillery && !Attacking && bombardmentCooldownTicks <= 0;

        private CompProperties_LongRangeArtillery ArtilleryProps => cachedArtilleryDef.GetCompProperties<CompProperties_LongRangeArtillery>();

        private IEnumerable<Settlement> PlayerSettlementsWithinRange => Find.WorldObjects.Settlements.Where(s => s.Faction == Faction.OfPlayer && Find.WorldGrid.TraversalDistanceBetween(parent.Tile, s.Tile) <= ArtilleryProps.worldTileRange);

        private void TryPostInitialise()
        {
            if (!postInitialised)
            {
                artilleryCount = Rand.Bool ? Props.ArtilleryCountFor(parent.Faction.def) : 0;
                postInitialised = true;
            }
        }

        public override void CompTick()
        {
            TryPostInitialise();

            if (Attacking)
            {
                // Try and bombard player settlements
                if (artilleryCooldownTicks > 0)
                    artilleryCooldownTicks--;
                else if (PlayerSettlementsWithinRange.TryRandomElementByWeight(s => StorytellerUtility.DefaultThreatPointsNow(s.Map), out Settlement targetSettlement))
                {
                    var shell = ArtilleryStrikeUtility.GetRandomShellFor(ArtilleryGunDef, parent.Faction.def);
                    if (shell != null)
                    {
                        float missRadius = ArtilleryStrikeUtility.FinalisedMissRadius(ArtilleryGunDef.Verbs[0].forcedMissRadius, ArtilleryProps.maxForcedMissRadiusFactor, parent.Tile, targetSettlement.Tile, ArtilleryProps.worldTileRange);
                        var map = targetSettlement.Map;
                        var strikeCells = ArtilleryStrikeUtility.PotentialStrikeCells(map, missRadius);
                        for (int i = 0; i < artilleryCount; i++)
                            ArtilleryStrikeUtility.SpawnArtilleryStrikeSkyfaller(shell, map, strikeCells.RandomElement());
                        artilleryCooldownTicks = ArtilleryDef.building.turretBurstCooldownTime.SecondsToTicks();
                    }
                    else
                        Log.ErrorOnce($"Tried to get shell for bombardment but got null instead (artilleryGunDef={ArtilleryGunDef}, factionDef={parent.Faction.def})", 8173352);
                }

                // Reduce the duration of the bombardment
                bombardmentDurationTicks--;
                if (bombardmentDurationTicks == 0)
                    EndBombardment();
            }

            if (bombardmentCooldownTicks > 0)
                bombardmentCooldownTicks--;
        }

        public void TryStartBombardment()
        {
            if (CanAttack)
            {
                artilleryCooldownTicks = BombardmentStartDelay;
                bombardmentDurationTicks = Props.bombardmentDurationRange.RandomInRange;
                bombardmentCooldownTicks = Props.bombardmentCooldownRange.RandomInRange;
                Find.LetterStack.ReceiveLetter("VFESecurity.ArtilleryStrikeSettlement_Letter".Translate(), "VFESecurity.ArtilleryStrikeSettlement_LetterText".Translate(parent.Faction.def.pawnsPlural, parent.Label), LetterDefOf.ThreatBig, parent);
            }
        }

        private void EndBombardment()
        {
            Messages.Message("VFESecurity.Message_ArtilleryBombardmentEnded".Translate(parent.Faction.def.pawnsPlural.CapitalizeFirst(), parent.Label), MessageTypeDefOf.PositiveEvent);
        }

        public override void PostPostRemove()
        {
            if (Attacking)
                EndBombardment();
            base.PostPostRemove();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref postInitialised, "postInitialised");
            Scribe_Values.Look(ref artilleryCount, "artilleryCount");
            Scribe_Values.Look(ref artilleryCooldownTicks, "artilleryCooldownTicks");
            Scribe_Values.Look(ref bombardmentDurationTicks, "bombardmentDurationTicks");
            Scribe_Values.Look(ref bombardmentCooldownTicks, "bombardmentCooldownTicks");
            base.PostExposeData();
        }

        private bool postInitialised;
        private int artilleryCount;
        private int artilleryCooldownTicks;
        private int bombardmentDurationTicks;
        private int bombardmentCooldownTicks;

        private ThingDef cachedArtilleryDef;

    }

}
