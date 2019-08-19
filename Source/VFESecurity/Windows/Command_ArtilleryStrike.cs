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

    [StaticConstructorOnStartup]
    public class Command_ArtilleryStrike : Command
    {

        private static readonly Texture2D TargetIcon = ContentFinder<Texture2D>.Get("Things/Mote/FeedbackShoot");

        public List<CompLongRangeArtillery> artilleryComps;
        private List<CompLongRangeArtillery> allArtilleryComps;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (allArtilleryComps == null)
                allArtilleryComps = new List<CompLongRangeArtillery>();
            allArtilleryComps.AddRange(artilleryComps);

            var floatMenuOptions = FloatMenuOptions().ToList();
            if (floatMenuOptions.Count == 1)
                floatMenuOptions[0].action();
            else
                Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (allArtilleryComps == null)
                allArtilleryComps = new List<CompLongRangeArtillery>();
            var otherStrike = (Command_ArtilleryStrike)other;
            allArtilleryComps.AddRange(otherStrike.artilleryComps);
            return false;
        }

        private IEnumerable<FloatMenuOption> FloatMenuOptions()
        {
            var groupedComps = allArtilleryComps.GroupBy(a => a.parent.def);

            if (groupedComps.Count() > 1)
                yield return new FloatMenuOption("AllDays".Translate(), () => OrderGroupedArtilleryStrike(allArtilleryComps));
            foreach (var group in groupedComps)
            {
                yield return new FloatMenuOption($"{group.Key.LabelCap} ({group.Count()})", () =>
                {
                    OrderGroupedArtilleryStrike(group.ToList());
                });
            }
        }

        private void OrderGroupedArtilleryStrike(List<CompLongRangeArtillery> artillery)
        {
            Find.WorldTargeter.BeginTargeting(t =>
            {
                artillery.ForEach(a => a.SetTargetedTile(t, a, false));
                return true;
            },
            true, TargetIcon,
            onUpdate: () =>
            {
                var distinctComps = artillery.Distinct();
                foreach (var comp in distinctComps)
                    GenDraw.DrawWorldRadiusRing(comp.parent.Map.Tile, comp.Props.worldTileRange);
            });
        }

    }

}
