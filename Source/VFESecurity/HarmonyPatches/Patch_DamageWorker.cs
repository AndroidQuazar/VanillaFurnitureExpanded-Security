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

    public static class Patch_DamageWorker
    {

        [HarmonyPatch(typeof(DamageWorker), nameof(DamageWorker.ExplosionAffectCell))]
        public static class ExplosionAffectCell
        {

            public static bool Prefix(Explosion explosion, IntVec3 c)
            {
                bool executeOriginal = true;
                ShieldGeneratorUtility.CheckIntercept(explosion, explosion.Map, explosion.damAmount, explosion.damType, () => new List<IntVec3>() { c }, () => explosion.damType.AffectsShields(),
                    s =>
                    {
                        executeOriginal = s.WithinBoundary(explosion.Position, c);
                        return !s.affectedThings.ContainsKey(explosion);
                    },
                    s =>
                    {
                        var cellsToAffect = (List<IntVec3>)NonPublicFields.Explosion_cellsToAffect.GetValue(explosion);
                        int cacheDuration = cellsToAffect.Select(eC => NonPublicMethods.Explosion_GetCellAffectTick(explosion, eC)).Max();
                        s.affectedThings.Add(explosion, cacheDuration);
                    });
                return executeOriginal;
            }

        }

    }

}
