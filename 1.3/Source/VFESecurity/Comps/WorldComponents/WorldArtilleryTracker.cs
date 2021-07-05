using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFESecurity
{
    [StaticConstructorOnStartup]
    public class WorldArtilleryTracker : WorldComponent
    {
        private static readonly Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, new Color(1, 0.5f, 0.5f), WorldMaterials.WorldLineRenderQueue);
        private static readonly Material NonPlayerTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, new Color(0.5f, 0.5f, 1), WorldMaterials.WorldLineRenderQueue);

        private readonly List<CompLongRangeArtillery> listerArtilleryComps = new List<CompLongRangeArtillery>();
        public List<WorldObject> bombardingWorldObjects = new List<WorldObject>();

        private HashSet<ArtilleryComp> cachedArtilleryCompsBombarding = new HashSet<ArtilleryComp>();

        public WorldArtilleryTracker(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            foreach (ArtilleryComp comp in cachedArtilleryCompsBombarding)
            {
                comp.BombardmentTick();
            }
        }

        public override void WorldComponentUpdate()
        {
            // Draw shoot lines
            if (WorldRendererUtility.WorldRenderedNow)
            {
                var worldGrid = Find.WorldGrid;
                foreach (var artilleryComp in cachedArtilleryCompsBombarding)
                {
                    var material = artilleryComp.parent.Faction == Faction.OfPlayer ? ForcedTargetLineMat : NonPlayerTargetLineMat;
                    foreach (var target in artilleryComp.Targets)
                    {
                        var start = worldGrid.GetTileCenter(artilleryComp.parent.Tile);
                        var end = worldGrid.GetTileCenter(target.Tile);

                        var drawPoints = ArtilleryStrikeUtility.WorldLineDrawPoints(start, end).ToList();
                        for (int k = 1; k < drawPoints.Count; k++)
                        {
                            var a = drawPoints[k - 1];
                            var b = drawPoints[k];
                            GenDraw.DrawWorldLineBetween(a, b, material);
                        }
                    }
                }
            }
        }

        public void Notify_WorldObjectRemoved(WorldObject o)
        {
            for (int i = 0; i < listerArtilleryComps.Count; i++)
            {
                var artillery = listerArtilleryComps[i];
                if (artillery.targetedTile == o)
                    artillery.ResetForcedTarget();
            }
        }

        public bool RegisterBombardment(WorldObject o)
        {
            if (o is null)
            {
                return false;
            }
            return o.GetComponent<ArtilleryComp>() is ArtilleryComp comp && cachedArtilleryCompsBombarding.Add(comp);
        }

        public bool DeregisterBombardment(WorldObject o)
        {
            if (o is null)
            {
                return false;
            }
            return o.GetComponent<ArtilleryComp>() is ArtilleryComp comp && cachedArtilleryCompsBombarding.Remove(comp);
        }

        public void RegisterArtilleryComp(CompLongRangeArtillery a)
        {
            if (a == null)
                return;

            if (!listerArtilleryComps.Contains(a))
            {
                listerArtilleryComps.Add(a);
            }
        }

        public void DeregisterArtilleryComp(CompLongRangeArtillery a)
        {
            if (a == null)
                return;

            if (listerArtilleryComps.Contains(a))
            {
                listerArtilleryComps.Remove(a);
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look<WorldObject>(ref bombardingWorldObjects, "bombardingWorldObjects", LookMode.Reference);
            Scribe_Collections.Look<ArtilleryComp>(ref cachedArtilleryCompsBombarding, "cachedArtilleryCompsBombarding", LookMode.Reference);
            base.ExposeData();
        }
    }
}