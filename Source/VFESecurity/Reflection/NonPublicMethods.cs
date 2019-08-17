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
    public static class NonPublicMethods
    {

        public static Action<Building_TurretGun> Building_TurretGun_BurstComplete = (Action<Building_TurretGun>)
            Delegate.CreateDelegate(typeof(Action<Building_TurretGun>), null, AccessTools.Method(typeof(Building_TurretGun), "BurstComplete"));

        public static Action<DefeatAllEnemiesQuestComp> DefeatAllEnemiesQuestComp_GiveRewardsAndSendLetter = (Action<DefeatAllEnemiesQuestComp>)
            Delegate.CreateDelegate(typeof(Action<DefeatAllEnemiesQuestComp>), null, AccessTools.Method(typeof(DefeatAllEnemiesQuestComp), "GiveRewardsAndSendLetter"));

        public static Action<Projectile> Projectile_ImpactSomething = (Action<Projectile>)
            Delegate.CreateDelegate(typeof(Action<Projectile>), null, AccessTools.Method(typeof(Projectile), "ImpactSomething"));

    }

}
