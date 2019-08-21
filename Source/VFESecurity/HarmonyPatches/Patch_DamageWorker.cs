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

    public static class Patch_DamageWorker
    {

        [HarmonyPatch(typeof(DamageWorker), nameof(DamageWorker.ExplosionAffectCell))]
        public static class ExplosionAffectCell
        {

            public static bool Prefix(Explosion explosion, IntVec3 c)
            {
                // Don't want firefoam bursting our bubbles
                if (explosion.damType.isExplosive)
                {
                    var shieldGens = explosion.Map.ListerShieldGenerators(g => g.Active && g.Energy > 0);
                    foreach (var shieldGen in shieldGens)
                    {
                        var coveredCells = new HashSet<IntVec3>(shieldGen.CoveredCells);
                        if (explosion.instigator != null && !coveredCells.Contains(explosion.instigator.Position) && coveredCells.Contains(c))
                        {
                            // Damage absorption
                            if (!shieldGen.affectedExplosionCache.ContainsKey(explosion))
                            {
                                shieldGen.AbsorbDamage(explosion.GetDamageAmountAt(c), explosion.damType, (shieldGen.TrueCenter() - explosion.TrueCenter()).AngleFlat());
                                var cellsToAffect = (List<IntVec3>)NonPublicFields.Explosion_cellsToAffect.GetValue(explosion);
                                int cacheDuration = cellsToAffect.Select(eC => NonPublicMethods.Explosion_GetCellAffectTick(explosion, eC)).Max();
                                shieldGen.affectedExplosionCache.Add(explosion, cacheDuration);
                            }
                            return false;
                        }
                    }
                }
                return true;
            }

        }

    }

}
