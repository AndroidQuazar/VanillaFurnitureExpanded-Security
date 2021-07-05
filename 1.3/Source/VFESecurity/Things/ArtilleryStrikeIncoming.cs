using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VFESecurity
{

    public class ArtilleryStrikeIncoming : ArtilleryStrikeSkyfaller
    {

        public ThingDef artilleryShellDef;

        protected override ThingDef ShellDef => artilleryShellDef;

        public override Graphic Graphic
        {
            get
            {
                if (artilleryShellDef.GetModExtension<ThingDefExtension>() is ThingDefExtension thingDefExtension && thingDefExtension.incomingSkyfallerGraphicData != null)
                    return thingDefExtension.incomingSkyfallerGraphicData.Graphic;
                return base.Graphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            var projectileProps = ShellDef.projectile;
            ShieldGeneratorUtility.CheckIntercept(this, map, projectileProps.GetDamageAmount(1), projectileProps.damageDef, () => this.OccupiedRect().Cells,
            postIntercept: s =>
            {
                if (s.Energy > 0)
                    Destroy();
            });
        }

        public override void Tick()
        {
            // Sounds
            if (ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && !artilleryShellDef.projectile.soundImpactAnticipate.NullOrUndefined())
                artilleryShellDef.projectile.soundImpactAnticipate.PlayOneShot(this);

            base.Tick();
        }

        protected override void HitRoof()
        {
            if (Map.roofGrid.RoofAt(Position) is RoofDef roof && roof.isThickRoof)
            {
                Impact();
                return;
            }

            base.HitRoof();
        }

        protected override void Impact()
        {
            var projectile = (Projectile)ThingMaker.MakeThing(artilleryShellDef);
            GenSpawn.Spawn(projectile, Position, Map);
            NonPublicMethods.Projectile_ImpactSomething(projectile);
            base.Impact();
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref artilleryShellDef, "artilleryShellDef");
            base.ExposeData();
        }

    }

}
