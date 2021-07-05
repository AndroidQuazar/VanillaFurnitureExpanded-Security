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
using HarmonyLib;

namespace VFESecurity
{

    public class CompTerrainSetter : ThingComp
    {

        private CompProperties_TerrainSetter Props => (CompProperties_TerrainSetter)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            var terrainGrid = parent.Map.terrainGrid;
            var occupiedCells = parent.OccupiedRect().ToList();
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                var cell = occupiedCells[i];
                terrainGrid.SetTerrain(cell, Props.terrainDef);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            var terrainGrid = map.terrainGrid;
            var occupiedCells = parent.OccupiedRect().ToList();
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                var cell = occupiedCells[i];
                terrainGrid.RemoveTopLayer(cell, false);
            }
        }

    }

}
