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
    public class WorldArtilleryTracker : WorldComponent
    {

        private static readonly Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, new Color(1, 0.5f, 0.5f), WorldMaterials.WorldLineRenderQueue);
        private static readonly Material NonPlayerTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, new Color(0.5f, 0.5f, 1), WorldMaterials.WorldLineRenderQueue);

        private List<WorldObject> cachedWorldObjects;
        private List<CompLongRangeArtillery> listerArtilleryComps;
        public List<WorldObject> bombardingWorldObjects = new List<WorldObject>();

        public WorldArtilleryTracker(World world) : base(world)
        {
        }

        private void TryPostInit()
        {
            bool init = false;
            if (cachedWorldObjects == null)
            {
                cachedWorldObjects = new List<WorldObject>();
                init = true;
            }
            if (listerArtilleryComps == null)
            {
                listerArtilleryComps = new List<CompLongRangeArtillery>();
                init = true;
            }

            if (init)
            {
                foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
                {
                    var artilleryComp = worldObject.GetComponent<ArtilleryComp>();
                    if (artilleryComp != null)
                    {
                        RegisterWorldObject(worldObject);
                        foreach (var artillery in artilleryComp.ArtilleryComps)
                            RegisterArtilleryComp(artillery);
                    }
                }
            }
        }

        public override void WorldComponentUpdate()
        {
            TryPostInit();

            // Draw shoot lines
            if (WorldRendererUtility.WorldRenderedNow)
            {
                var worldGrid = Find.WorldGrid;
                foreach (var worldObject in cachedWorldObjects)
                {
                    var artilleryComp = worldObject.GetComponent<ArtilleryComp>();
                    if (artilleryComp != null && artilleryComp.Attacking)
                    {
                        var material = worldObject.Faction == Faction.OfPlayer ? ForcedTargetLineMat : NonPlayerTargetLineMat;
                        foreach (var target in artilleryComp.Targets)
                        {
                            var start = worldGrid.GetTileCenter(worldObject.Tile);
                            var end = worldGrid.GetTileCenter(target.Tile);

                            var drawPoints = ArtilleryStrikeUtility.WorldLineDrawPoints(start, end).ToList();
                            for (int i = 1; i < drawPoints.Count; i++)
                            {
                                var a = drawPoints[i - 1];
                                var b = drawPoints[i];
                                GenDraw.DrawWorldLineBetween(a, b, material);
                            }
                        }
                    }
                }
            }
        }

        public void Notify_WorldObjectRemoved(WorldObject o)
        {
            foreach (var artillery in listerArtilleryComps)
                if (artillery.targetedTile == o)
                    artillery.ResetForcedTarget();
        }

        public void RegisterWorldObject(WorldObject o)
        {
            if (o == null)
                return;

            if (cachedWorldObjects == null)
                cachedWorldObjects = new List<WorldObject>();
            if (!cachedWorldObjects.Contains(o))
                cachedWorldObjects.Add(o);
        }

        public void DeregisterWorldObject(WorldObject o)
        {
            if (o == null)
                return;

            if (cachedWorldObjects.Contains(o))
                cachedWorldObjects.Remove(o);
        }

        public void RegisterArtilleryComp(CompLongRangeArtillery a)
        {
            if (a == null)
                return;

            if (listerArtilleryComps == null)
                listerArtilleryComps = new List<CompLongRangeArtillery>();
            if (!listerArtilleryComps.Contains(a))
                listerArtilleryComps.Add(a);
        }

        public void DeregisterArtilleryComp(CompLongRangeArtillery a)
        {
            if (a == null)
                return;

            if (listerArtilleryComps.Contains(a))
                listerArtilleryComps.Remove(a);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref bombardingWorldObjects, "bombardingWorldObjects", LookMode.Reference);
            base.ExposeData();
        }

    }

}
