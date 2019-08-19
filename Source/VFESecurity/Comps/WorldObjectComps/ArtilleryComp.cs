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

        private IEnumerable<CompLongRangeArtillery> ArtilleryComps => artillery.Select(a => a.TryGetComp<CompLongRangeArtillery>());

        private ThingDef ArtilleryGunDef => ArtilleryDef.building.turretGunDef;

        public bool HasArtillery => ArtilleryDef != null && artilleryCount > 0;
        public bool Attacking
        {
            get
            {
                if (parent.Faction == Faction.OfPlayer)
                    return ArtilleryComps.Any(a => a.targetedTile != GlobalTargetInfo.Invalid);
                return bombardmentDurationTicks > 0;
            }
        }
        private bool CanAttack => HasArtillery && !Attacking && bombardmentCooldownTicks <= 0;

        private CompProperties_LongRangeArtillery ArtilleryProps => cachedArtilleryDef.GetCompProperties<CompProperties_LongRangeArtillery>();

        public IEnumerable<GlobalTargetInfo> Targets
        {
            get
            {
                if (parent.Faction == Faction.OfPlayer)
                {
                    foreach (var tile in ArtilleryComps.Where(a => a.targetedTile != GlobalTargetInfo.Invalid).Select(a => a.targetedTile).Distinct())
                        yield return tile;
                    yield break;
                }

                var settlements = Find.WorldObjects.Settlements.Where(s => s.Faction == Faction.OfPlayer && Find.WorldGrid.TraversalDistanceBetween(parent.Tile, s.Tile) <= ArtilleryProps.worldTileRange);
                foreach (var settlement in settlements)
                    yield return new GlobalTargetInfo(settlement);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (parent.Faction == Faction.OfPlayer)
            {
                // Target other map tiles
                var target = new Command_ArtilleryStrike()
                {
                    defaultLabel = "VFESecurity.TargetWorldTile".Translate(),
                    defaultDesc = "VFESecurity.TargetWorldTile_Description".Translate(parent.def.label),
                    icon = CompLongRangeArtillery.TargetWorldTileIcon,
                    artilleryComps = ArtilleryComps.ToList()
                };

                if (!ArtilleryComps.Any())
                    target.Disable("VFESecurity.CommandTargetTileFailNoArtillery".Translate(parent.def.label));

                yield return target;

                // Cancel targeting
                if (Targets.Any())
                {
                    yield return new Command_Action()
                    {
                        defaultLabel = "CommandStopForceAttack".Translate(),
                        defaultDesc = "CommandStopForceAttackDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true),
                        action = () =>
                        {
                            foreach (var artilleryComp in ArtilleryComps)
                                artilleryComp.ResetForcedTarget();
                        }
                    };
                }
            }
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            Find.World.GetComponent<ArtilleryLineRenderer>().TryAdd(parent);
            base.Initialize(props);
        }

        private void TryResolveArtilleryCount()
        {
            if (artilleryCount == -1)
            {
                artilleryCount = Rand.Bool ? Props.ArtilleryCountFor(parent.Faction.def) : 0;
            }
        }

        public override void CompTick()
        {
            if (parent.Faction != Faction.OfPlayer)
            {
                TryResolveArtilleryCount();

                if (Attacking)
                {
                    // Try and bombard player settlements
                    if (artilleryCooldownTicks > 0)
                        artilleryCooldownTicks--;
                    else if (Targets.Select(t => t.WorldObject).Cast<Settlement>().TryRandomElementByWeight(s => StorytellerUtility.DefaultThreatPointsNow(s.Map), out Settlement targetSettlement))
                    {
                        if (artilleryWarmupTicks < 0)
                            artilleryWarmupTicks = ArtilleryDef.building.turretBurstWarmupTime.SecondsToTicks();
                        else
                        {
                            artilleryWarmupTicks--;
                            if (artilleryWarmupTicks == -1)
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
                        }
                    }
                    else
                        artilleryWarmupTicks = -1;

                    // Reduce the duration of the bombardment
                    bombardmentDurationTicks--;
                    if (bombardmentDurationTicks == 0)
                        EndBombardment();
                }

                if (bombardmentCooldownTicks > 0)
                    bombardmentCooldownTicks--;
            }
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
            Find.World.GetComponent<ArtilleryLineRenderer>().TryRemove(parent);
            base.PostPostRemove();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref artilleryCount, "artilleryCount", -1);
            Scribe_Values.Look(ref artilleryWarmupTicks, "artilleryWarmupTicks", -1);
            Scribe_Values.Look(ref artilleryCooldownTicks, "artilleryCooldownTicks");
            Scribe_Values.Look(ref bombardmentDurationTicks, "bombardmentDurationTicks");
            Scribe_Values.Look(ref bombardmentCooldownTicks, "bombardmentCooldownTicks");
            Scribe_Collections.Look(ref artillery, "artillery", LookMode.Reference);
            base.PostExposeData();
        }

        private int artilleryCount = -1;
        private int artilleryWarmupTicks = -1;
        private int artilleryCooldownTicks;
        private int bombardmentDurationTicks;
        private int bombardmentCooldownTicks;
        public HashSet<Thing> artillery = new HashSet<Thing>();

        private ThingDef cachedArtilleryDef;

    }

}
