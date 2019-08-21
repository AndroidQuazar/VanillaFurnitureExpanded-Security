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
using Harmony;

namespace VFESecurity
{

    public class CompTerrainSetter : ThingComp
    {

        private CompProperties_TerrainSetter Props => (CompProperties_TerrainSetter)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            var terrainGrid = parent.Map.terrainGrid;
            foreach (var cell in parent.OccupiedRect())
                terrainGrid.SetTerrain(cell, Props.terrainDef);
        }

        public override void PostDeSpawn(Map map)
        {
            var terrainGrid = map.terrainGrid;
            foreach (var cell in parent.OccupiedRect())
                terrainGrid.RemoveTopLayer(cell, false);
        }

    }

}
