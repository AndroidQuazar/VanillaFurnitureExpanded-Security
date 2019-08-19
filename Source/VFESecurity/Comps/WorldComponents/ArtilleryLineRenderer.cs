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
    public class ArtilleryLineRenderer : WorldComponent
    {

        private static readonly Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, new Color(1, 0.5f, 0.5f), WorldMaterials.WorldLineRenderQueue);
        private static readonly Material NonPlayerTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayTransparent, new Color(0.5f, 0.5f, 1), WorldMaterials.WorldLineRenderQueue);

        private List<WorldObject> cachedWorldObjects;

        public ArtilleryLineRenderer(World world) : base(world)
        {
        }

        private void TryPostInit()
        {
            if (cachedWorldObjects == null)
            {
                cachedWorldObjects = new List<WorldObject>();
                foreach (var worldObject in Find.WorldObjects.AllWorldObjects)
                {
                    var artilleryComp = worldObject.GetComponent<ArtilleryComp>();
                    if (artilleryComp != null && artilleryComp.HasArtillery)
                        TryAdd(worldObject);
                }
            }
        }

        public override void WorldComponentUpdate()
        {
            TryPostInit();

            // Render shoot lines
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

        public void TryAdd(WorldObject o)
        {
            if (cachedWorldObjects == null)
                cachedWorldObjects = new List<WorldObject>();
            if (!cachedWorldObjects.Contains(o))
                cachedWorldObjects.Add(o);
        }

        public void TryRemove(WorldObject o)
        {
            if (cachedWorldObjects.Contains(o))
                cachedWorldObjects.Remove(o);
        }

    }

}
