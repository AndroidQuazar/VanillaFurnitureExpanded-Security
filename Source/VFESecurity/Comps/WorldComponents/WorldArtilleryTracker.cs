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
                var worldObjects = Find.WorldObjects.AllWorldObjects;
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    var worldObject = worldObjects[i];
                    var artilleryComp = worldObject.GetComponent<ArtilleryComp>();
                    if (artilleryComp != null)
                    {
                        RegisterWorldObject(worldObject);
                        var artilleryComps = artilleryComp.ArtilleryComps.ToList();
                        for (int j = 0; j < artilleryComps.Count; j++)
                        {
                            var artillery = artilleryComps[j];
                            RegisterArtilleryComp(artillery);
                        }
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
                var worldObjectList = cachedWorldObjects.ToList();
                for (int i = 0; i < worldObjectList.Count; i++)
                {
                    var worldObject = worldObjectList[i];
                    var artilleryComp = worldObject.GetComponent<ArtilleryComp>();
                    if (artilleryComp != null && artilleryComp.Attacking)
                    {
                        var material = worldObject.Faction == Faction.OfPlayer ? ForcedTargetLineMat : NonPlayerTargetLineMat;
                        var targetList = artilleryComp.Targets.ToList();
                        for (int j = 0; j < targetList.Count; j++)
                        {
                            var target = targetList[j];
                            var start = worldGrid.GetTileCenter(worldObject.Tile);
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

            if (cachedWorldObjects == null)
                cachedWorldObjects = new List<WorldObject>();
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

            if (listerArtilleryComps == null)
                listerArtilleryComps = new List<CompLongRangeArtillery>();
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
