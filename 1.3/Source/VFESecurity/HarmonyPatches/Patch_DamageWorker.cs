using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFESecurity
{
    public static class Patch_DamageWorker
    {
        /*[HarmonyPatch(typeof(DamageWorker), nameof(DamageWorker.ExplosionAffectCell))]
        public static class ExplosionAffectCell
        {
            public static bool Prefix(DamageWorker __instance, Explosion explosion, IntVec3 c)
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
                        List<IntVec3> cellsToAffect = (List<IntVec3>)NonPublicFields.Explosion_cellsToAffect.GetValue(explosion);
                        int cacheDuration = cellsToAffect.Select(eC => NonPublicMethods.Explosion_GetCellAffectTick(explosion, eC)).Max();
                        Log.Message($"preIntercept explosion: {explosion}");
                        s.affectedThings.Add(explosion, cacheDuration);
                        s.affectedThings.Keys.ToList().ForEach(t => Log.Message($"key: {t}"));
                    });
                return executeOriginal;
            }
        }*/
    }
}