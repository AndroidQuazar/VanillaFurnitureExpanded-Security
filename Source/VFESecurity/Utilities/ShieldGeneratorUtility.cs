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

    public static class ShieldGeneratorUtility
    {

        public static bool AffectsShields(this DamageDef damageDef)
        {
            return damageDef.isExplosive || damageDef == DamageDefOf.EMP;
        }

        public static void CheckIntercept(Thing thing, Map map, int damageAmount, DamageDef damageDef, Func<IEnumerable<IntVec3>> cellGetter, Func<bool> canIntercept = null, Func<Building_Shield, bool> preIntercept = null, Action<Building_Shield> postIntercept = null)
        {
            if (canIntercept == null || canIntercept())
            {
                var occupiedCells = new HashSet<IntVec3>(cellGetter());
                var listerShields = map.GetComponent<ListerThingsExtended>().ListerShieldGensActive.ToList();
                for (int i = 0; i < listerShields.Count; i++)
                {
                    var shield = listerShields[i];
                    var coveredCells = new HashSet<IntVec3>(shield.coveredCells);
                    if ((preIntercept == null || preIntercept.Invoke(shield)) && occupiedCells.Any(c => coveredCells.Contains(c)))
                    {
                        shield.AbsorbDamage(damageAmount, damageDef, thing);
                        postIntercept?.Invoke(shield);
                        return;
                    }
                }
            }
        }

        public static bool BlockableByShield(this Projectile proj, Building_Shield shieldGen)
        {
            if (!proj.def.projectile.flyOverhead)
                return true;
            return !shieldGen.coveredCells.Contains(((Vector3)NonPublicFields.Projectile_origin.GetValue(proj)).ToIntVec3()) && 
                (int)NonPublicFields.Projectile_ticksToImpact.GetValue(proj) / (float)NonPublicProperties.Projectile_get_StartingTicksToImpact(proj) <= 0.5f;
        }

        //---added--- inspired by Frontier Security's method (distributed under an open-source non-profit license)
        public static bool CheckPodHostility(DropPodIncoming dropPod)  //this is me. Didn't want to zap trader deliveries or allies
        {
            var innerContainer = dropPod.Contents.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (innerContainer[i] is Pawn pawn)
                {
                    if (GenHostility.IsActiveThreatToPlayer(pawn) || pawn.RaceProps.IsMechanoid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void KillPawn(Pawn pawn, IntVec3 position, Map map) //FD inspired - means zapped pawns actually count as dead. FD has a tale recorder in here so cause of death is right, but should work ok.
        {
            // spawn on map for just an instant
            GenPlace.TryPlaceThing(pawn, position, map, ThingPlaceMode.Near);
            pawn.inventory.DestroyAll();
            pawn.Kill(new DamageInfo(DamageDefOf.Crush, 100));
            pawn.Corpse.Destroy();
        }

    }

}
