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
    public static class NonPublicMethods
    {

        public static Action<Building_TurretGun> Building_TurretGun_BurstComplete = (Action<Building_TurretGun>)
            Delegate.CreateDelegate(typeof(Action<Building_TurretGun>), null, AccessTools.Method(typeof(Building_TurretGun), "BurstComplete"));
        public static Action<Building_TurretGun> Building_TurretGun_ResetCurrentTarget = (Action<Building_TurretGun>)
            Delegate.CreateDelegate(typeof(Action<Building_TurretGun>), null, AccessTools.Method(typeof(Building_TurretGun), "ResetCurrentTarget"));
        public static Action<Building_TurretGun> Building_TurretGun_ResetForcedTarget = (Action<Building_TurretGun>)
            Delegate.CreateDelegate(typeof(Action<Building_TurretGun>), null, AccessTools.Method(typeof(Building_TurretGun), "ResetForcedTarget"));

        public static Action<DefeatAllEnemiesQuestComp> DefeatAllEnemiesQuestComp_GiveRewardsAndSendLetter = (Action<DefeatAllEnemiesQuestComp>)
            Delegate.CreateDelegate(typeof(Action<DefeatAllEnemiesQuestComp>), null, AccessTools.Method(typeof(DefeatAllEnemiesQuestComp), "GiveRewardsAndSendLetter"));

        public static Func<Explosion, IntVec3, int> Explosion_GetCellAffectTick = (Func<Explosion, IntVec3, int>)
            Delegate.CreateDelegate(typeof(Func<Explosion, IntVec3, int>), null, AccessTools.Method(typeof(Explosion), "GetCellAffectTick"));

        public static Action<Projectile> Projectile_ImpactSomething = (Action<Projectile>)
            Delegate.CreateDelegate(typeof(Action<Projectile>), null, AccessTools.Method(typeof(Projectile), "ImpactSomething"));

    }

}
