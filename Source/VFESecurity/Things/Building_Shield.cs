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
using Verse.Sound;
using RimWorld;
using Harmony;

namespace VFESecurity
{

    [StaticConstructorOnStartup]
    public class Building_Shield : Building, IAttackTarget
    {

        private const float MaxEnergyFactorInactive = 0.2f;
        private const int ContinueDrawingForTicks = 1000;
        private const int RechargeTicksWhenDepleted = 3200;
        private const float EnergyLossPerDamage = 0.033f;
        private static readonly Material BaseBubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        public ExtendedBuildingProperties ExtendedBuildingProps => def.GetModExtension<ExtendedBuildingProperties>() ?? ExtendedBuildingProperties.defaultValues;
        public float MaxEnergy => this.GetStatValue(StatDefOf.VFES_EnergyShieldEnergyMax);
        private float CurMaxEnergy => MaxEnergy * (Active ? 1 : MaxEnergyFactorInactive);
        public float EnergyGainPerTick => this.GetStatValue(StatDefOf.VFES_EnergyShieldRechargeRate) / 60;
        public float ShieldRadius => this.GetStatValue(StatDefOf.VFES_EnergyShieldRadius);

        private CompPowerTrader PowerTraderComp
        {
            get
            {
                if (!checkedPowerComp)
                {
                    cachedPowerComp = GetComp<CompPowerTrader>();
                    checkedPowerComp = true;
                }
                return cachedPowerComp;
            }
        }

        private bool CanFunction => PowerTraderComp == null || PowerTraderComp.PowerOn;
        public bool Active => ParentHolder is Map && CanFunction && GenHostility.AnyHostileActiveThreatTo(MapHeld, Faction);
        public float Energy
        {
            get => energy;
            set
            {
                energy = Mathf.Clamp(value, 0, CurMaxEnergy);
                if (energy == 0)
                    Notify_EnergyDepleted();
            }
        }
        public IEnumerable<IntVec3> CoveredCells => Active ? GenRadial.RadialCellsAround(PositionHeld, ShieldRadius, true) : null;
        public IEnumerable<Thing> ThingsWithinRadius
        {
            get
            {
                foreach (var cell in CoveredCells)
                    foreach (var thing in cell.GetThingList(MapHeld))
                        yield return thing;
            }
        }

        Thing IAttackTarget.Thing => this;
        LocalTargetInfo IAttackTarget.TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

        private void Notify_EnergyDepleted()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Position, Map));
            MoteMaker.MakeStaticMote(this.TrueCenter(), Map, RimWorld.ThingDefOf.Mote_ExplosionFlash, 12);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = this.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteMaker.ThrowDustPuff(loc, Map, Rand.Range(0.8f, 1.2f));
            }
            ticksToRecharge = RechargeTicksWhenDepleted;
        }

        public override void Tick()
        {
            UpdateExplosionCache();

            if (CanFunction)
            {
                // Recharge shield
                if (ticksToRecharge > 0)
                {
                    ticksToRecharge--;
                    if (ticksToRecharge == 0)
                        Energy = MaxEnergy * MaxEnergyFactorInactive;
                }
                else
                    Energy += EnergyGainPerTick;

                // If shield is active
                if (Active)
                {
                    // Power consumption
                    if (PowerTraderComp != null)
                        PowerTraderComp.PowerOutput = -PowerTraderComp.Props.basePowerConsumption;

                    if (Energy > 0)
                        EnergyShieldTick();
                }
                else if (PowerTraderComp != null)
                    PowerTraderComp.PowerOutput = -ExtendedBuildingProps.inactiveShieldGenPowerConsumption;
            }

            base.Tick();
        }

        private void UpdateExplosionCache()
        {
            for (int i = 0; i < affectedExplosionCache.Count; i++)
            {
                var curKey = affectedExplosionCache.Keys.ToList()[i];
                if (affectedExplosionCache[curKey] <= 0)
                    affectedExplosionCache.Remove(curKey);
                else
                    affectedExplosionCache[curKey]--;
            }
        }

        private void EnergyShieldTick()
        {
            var thingsWithinRadius = new HashSet<Thing>(ThingsWithinRadius);
            foreach (var thing in thingsWithinRadius)
            {
                // Try and block projectiles from outside
                if (thing is Projectile proj)
                {
                    var launcher = NonPublicFields.Projectile_launcher.GetValue(proj) as Thing;
                    if (launcher != null && !thingsWithinRadius.Contains(launcher))
                    {
                        // Explosives are handled separately
                        if (!(proj is Projectile_Explosive))
                            AbsorbDamage(proj.DamageAmount, proj.def.projectile.damageDef, proj.ExactRotation.eulerAngles.y);
                        NonPublicFields.Projectile_usedTarget.SetValue(proj, new LocalTargetInfo(proj.Position));
                        NonPublicMethods.Projectile_ImpactSomething(proj);
                        if (proj.Spawned && proj is Projectile_Explosive)
                            NonPublicFields.Projectile_Explosive_ticksToDetonation.SetValue(proj, 1);
                    }
                }
            }
        }

        private float EnergyLossMultiplier(DamageDef damageDef)
        {
            // EMP - on shield
            if (damageDef == DamageDefOf.EMP)
                return 4;

            return 1;
        }

        public void AbsorbDamage(float amount, DamageDef def, float angle)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Position, Map, false));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(angle);
            Vector3 loc = this.TrueCenter() + impactAngleVect.RotatedBy(180f) * (ShieldRadius / 2);
            float flashSize = Mathf.Min(10f, 2f + amount / 10f);
            MoteMaker.MakeStaticMote(loc, Map, RimWorld.ThingDefOf.Mote_ExplosionFlash, flashSize);
            int dustCount = (int)flashSize;
            for (int i = 0; i < dustCount; i++)
            {
                MoteMaker.ThrowDustPuff(loc, Map, Rand.Range(0.8f, 1.2f));
            }
            Energy -= amount * EnergyLossMultiplier(def) * EnergyLossPerDamage;
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
        }

        public override void Draw()
        {
            base.Draw();

            // Draw shield bubble
            if (Active && Energy > 0)
            {
                float size = ShieldRadius * 2 * Mathf.Lerp(0.9f, 1.1f, Energy / MaxEnergy);
                Vector3 pos = DrawPos;
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int ticksSinceAbsorbDamage = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (ticksSinceAbsorbDamage < 8)
                {
                    float sizeMod = (8 - ticksSinceAbsorbDamage) / 8f * 0.05f;
                    pos += impactAngleVect * sizeMod;
                    size -= sizeMod;
                }
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(size, 1f, size);
                Matrix4x4 matrix = default;
                matrix.SetTRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
                var mat = new Material(BaseBubbleMat);
                mat.color = ExtendedBuildingProps.shieldColour;
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            // EMP - direct
            if (dinfo.Def == DamageDefOf.EMP)
                Energy = 0;

            base.PreApplyDamage(ref dinfo, out absorbed);
        }

        public override string GetInspectString()
        {
            var inspectBuilder = new StringBuilder();

            // Inactive
            if (!Active)
                inspectBuilder.AppendLine("InactiveFacility".Translate().CapitalizeFirst());

            inspectBuilder.AppendLine(base.GetInspectString());

            return inspectBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Shield health
            if (Find.Selector.SingleSelectedThing == this)
            {
                yield return new Gizmo_EnergyShieldGeneratorStatus()
                {
                    shieldGenerator = this
                };
            }

            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticksToRecharge, "ticksToRecharge");
            Scribe_Values.Look(ref energy, "energy");
            base.ExposeData();
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            if (Energy == 0)
                return true;
            if (!disabledFor.CurrentEffectiveVerb.IsEMP())
                return true;
            return !CanFunction ;
        }

        private int ticksToRecharge;
        private float energy;

        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick;
        private bool checkedPowerComp;
        private CompPowerTrader cachedPowerComp;
        public Dictionary<Explosion, int> affectedExplosionCache = new Dictionary<Explosion, int>();

    }

}
