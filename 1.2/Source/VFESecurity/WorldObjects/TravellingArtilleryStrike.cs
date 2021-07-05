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

    public class TravellingArtilleryStrike : WorldObject, IThingHolder
    {

        private const float TravelSpeedPerShellSpeed = 0.00025f / 300;

        private int initialTile;
        public int destinationTile;
        private float travelledPct;
        float cachedDistance = -1;

        public ArtilleryStrikeArrivalAction arrivalAction;
        public ThingOwner innerContainer = new ThingOwner<Thing>();

        public override void PostAdd()
        {
            base.PostAdd();
            initialTile = Tile;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);
        private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

        public override Vector3 DrawPos => Vector3.Slerp(Start, End, travelledPct);

        private IEnumerable<ActiveArtilleryStrike> ArtilleryStrikes => innerContainer.Cast<ActiveArtilleryStrike>();

        public override string Label
        {
            get
            {
                int count = ArtilleryStrikes.Count();
                if (count > 1)
                    return base.Label + $" x{count}";
                return base.Label;
            }
        }

        public override string GetInspectString()
        {
            var inspectBuilder = new StringBuilder();
            inspectBuilder.AppendLine(base.GetInspectString());

            // Display unique shells
            inspectBuilder.AppendLine($"{"TabShells".Translate()}:");
            var shellCounts = ArtilleryStrikes.GroupBy(s => s.shellDef).ToList();
            for (int i = 0; i < shellCounts.Count; i++)
            {
                var shellCount = shellCounts[i];
                inspectBuilder.AppendLine($" - {shellCount.Key.LabelCap} x{shellCount.Select(s => s.shellCount).Aggregate((s1, s2) => s1 + s2)}");
            }

            return inspectBuilder.ToString().TrimEndNewlines();
        }

        private float TravelledPctStepPerTick
        {
            get
            {
                if (Start == End)
                    return 1;

                float sphericalDist = GenMath.SphericalDistance(Start.normalized, End.normalized);
                if (sphericalDist == 0)
                    return 1;

                return ArtilleryStrikes.Select(s => s.Speed).Average() * TravelSpeedPerShellSpeed / sphericalDist;
            }
        }

        public override void Tick()
        {
            base.Tick();
            travelledPct += TravelledPctStepPerTick;
            if (travelledPct >= 1)
            {
                travelledPct = 1;
                Arrived();
            }
        }

        private void Arrived()
        {
            if (arrivalAction != null)
            {
                try
                {
                    arrivalAction.Arrived(ArtilleryStrikes.ToList(), destinationTile);
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception in artillery strike arrival action: {ex}");
                }
            }
            else
            {
                innerContainer.ClearAndDestroyContents();
            }
            Find.WorldObjects.Remove(this);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref initialTile, "initialTile");
            Scribe_Values.Look(ref destinationTile, "destinationTile");
            Scribe_Values.Look(ref travelledPct, "travelledPct");
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction", new object[0]);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", new object[] { this });
            base.ExposeData();
        }

    }

}
