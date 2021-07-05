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
using HarmonyLib;

namespace VFESecurity
{

    [StaticConstructorOnStartup]
    public class Building_Shield : Building, IAttackTarget
    {

        private const float EdgeCellRadius = 5;
        private const int CacheUpdateInterval = 15;
        private const float EnergyLossPerDamage = 0.033f;
        private static readonly Material BaseBubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        public ExtendedBuildingProperties ExtendedBuildingProps => def.GetModExtension<ExtendedBuildingProperties>() ?? ExtendedBuildingProperties.defaultValues;
        public float MaxEnergy => this.GetStatValue(StatDefOf.VFES_EnergyShieldEnergyMax);
        private float CurMaxEnergy => MaxEnergy * (active ? 1 : ExtendedBuildingProps.initialEnergyPercentage);
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

        private bool CanFunction => (PowerTraderComp == null || PowerTraderComp.PowerOn) && !this.IsBrokenDown();

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
        public IEnumerable<Thing> ThingsWithinRadius
        {
            get
            {
                foreach (var cell in coveredCells)
                {
                    var thingList = cell.GetThingList(MapHeld);
                    for (int i = 0; i < thingList.Count; i++)
                        yield return thingList[i];
                }
            }
        }
        public IEnumerable<Thing> ThingsWithinScanArea
        {
            get
            {
                foreach (var cell in scanCells)
                {
                    var thingList = cell.GetThingList(MapHeld);
                    for (int i = 0; i < thingList.Count; i++)
                        yield return thingList[i];
                }
                    
            }
        }

        public Thing Thing => this;

        public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

        public float TargetPriorityFactor => 1;

        private void Notify_EnergyDepleted()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Position, Map));
            MoteMaker.MakeStaticMote(this.TrueCenter(), Map, RimWorld.ThingDefOf.Mote_ExplosionFlash, 12);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = this.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteMaker.ThrowDustPuff(loc, Map, Rand.Range(0.8f, 1.2f));
            }
            ticksToRecharge = ExtendedBuildingProps.rechargeTicksWhenDepleted;
        }

        public bool WithinBoundary(IntVec3 sourcePos, IntVec3 checkedPos)
        {
            return (coveredCells.Contains(sourcePos) && coveredCells.Contains(checkedPos)) || (!coveredCells.Contains(sourcePos) && !coveredCells.Contains(checkedPos));
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            map.GetComponent<ListerThingsExtended>().listerShieldGens.Add(this);

            // Set up shield coverage
            coveredCells = new HashSet<IntVec3>(GenRadial.RadialCellsAround(PositionHeld, ShieldRadius, true));
            if (ShieldRadius < EdgeCellRadius + 1)
                scanCells = coveredCells;
            else
            {
                var interiorCells = GenRadial.RadialCellsAround(PositionHeld, ShieldRadius - EdgeCellRadius, true);
                scanCells = new HashSet<IntVec3>(coveredCells.Where(c => !interiorCells.Contains(c)));
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Map.GetComponent<ListerThingsExtended>().listerShieldGens.Remove(this);
            coveredCells = null;
            scanCells = null;
            base.DeSpawn(mode);
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(CacheUpdateInterval))
                UpdateCache();

            if (CanFunction)
            {
                // Recharge shield
                if (ticksToRecharge > 0)
                {
                    ticksToRecharge--;
                    if (ticksToRecharge == 0)
                        Energy = MaxEnergy * ExtendedBuildingProps.initialEnergyPercentage;
                }
                else
                    Energy += EnergyGainPerTick;

                // If shield is active
                if (active)
                {
                    // Power consumption
                    if (PowerTraderComp != null)
                        PowerTraderComp.PowerOutput = -PowerTraderComp.Props.basePowerConsumption;

                    if ((ShieldRadius < EdgeCellRadius + 1 || Find.TickManager.TicksGame % 2 == 0) && Energy > 0)
                        EnergyShieldTick();
                }
                else if (PowerTraderComp != null)
                    PowerTraderComp.PowerOutput = -ExtendedBuildingProps.inactivePowerConsumption;
            }
            else if (PowerTraderComp != null)
                PowerTraderComp.PowerOutput = -ExtendedBuildingProps.inactivePowerConsumption;

            base.Tick();
        }

        private void UpdateCache()
        {
            for (int i = 0; i < affectedThings.Count; i++)
            {
                var curKey = affectedThings.Keys.ToList()[i];
                if (affectedThings[curKey] <= 0)
                    affectedThings.Remove(curKey);
                else
                    affectedThings[curKey] -= CacheUpdateInterval;
            }

            active = ParentHolder is Map && CanFunction &&
                (Find.World.GetComponent<WorldArtilleryTracker>().bombardingWorldObjects.Any() || 
                GenHostility.AnyHostileActiveThreatTo(MapHeld, Faction) || 
                Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Tornado).Any() ||
                Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.DropPodIncoming).Any() || shieldBuffer > 0);

            if((Find.World.GetComponent<WorldArtilleryTracker>().bombardingWorldObjects.Any() || GenHostility.AnyHostileActiveThreatTo(MapHeld, Faction) || Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Tornado).Any() || Map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.DropPodIncoming).Any()) && shieldBuffer < 15)
                shieldBuffer = 15;
            else 
                shieldBuffer-=1;


        }

        private void EnergyShieldTick()
        {
            var thingsWithinRadius = new HashSet<Thing>(ThingsWithinRadius);
            var thingsWithinScanArea = new HashSet<Thing>(ThingsWithinScanArea);
            foreach (var thing in thingsWithinScanArea)
            {
                // Try and block projectiles from outside
                if (thing is Projectile proj && proj.BlockableByShield(this))
                {
                    var launcher = NonPublicFields.Projectile_launcher.GetValue(proj) as Thing;
                    if (launcher != null && !thingsWithinRadius.Contains(launcher))
                    {
                        // Explosives are handled separately
                        if (!(proj is Projectile_Explosive))
                            AbsorbDamage(proj.DamageAmount, proj.def.projectile.damageDef, proj.ExactRotation.eulerAngles.y);
                        proj.Position += Rot4.FromAngleFlat((Position - proj.Position).AngleFlat).Opposite.FacingCell;
                        NonPublicFields.Projectile_usedTarget.SetValue(proj, new LocalTargetInfo(proj.Position));
                        NonPublicMethods.Projectile_ImpactSomething(proj);
                    }
                }
                if (thing is Skyfaller skyfaller)
                {

                
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

        public void AbsorbDamage(float amount, DamageDef def, Thing source)
        {
            AbsorbDamage(amount, def, (this.TrueCenter() - source.TrueCenter()).AngleFlat());
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
            float energyLoss = amount * EnergyLossMultiplier(def) * EnergyLossPerDamage;
            Energy -= energyLoss;
            //Log.Message("lost Energy: " + energyLoss + " remaining: " + Energy);

            // try to do short circuit
            if (Rand.Chance(energyLoss * ExtendedBuildingProps.shortCircuitChancePerEnergyLost))
                GenExplosion.DoExplosion(this.OccupiedRect().RandomCell, Map, 1.9f, DamageDefOf.Flame, null);

            lastAbsorbDamageTick = Find.TickManager.TicksGame;
        }

        public override void Draw()
        {
            base.Draw();

            // Draw shield bubble
            if (active && Energy > 0)
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
                float angle = Rand.Range(0, 45);
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
            if (!active)
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
            Scribe_Values.Look(ref shieldBuffer, "shieldBuffer");
            Scribe_Values.Look(ref active, "active");
            Scribe_Collections.Look(ref affectedThings, "affectedThings", LookMode.Reference, LookMode.Value, ref affectedThingsKeysWorkingList, ref affectedThingsValuesWorkingList);
            base.ExposeData();
        }

        public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
        {
            // No energy
            if (Energy == 0)
                return true;

            // Attacker isn't using EMPs
            if (!disabledFor.CurrentEffectiveVerb.IsEMP())
                return true;

            // Return whether or not the shield can function
            return !CanFunction;
        }

        private int ticksToRecharge;
        private float energy;
        private int shieldBuffer = 0;
        public bool active;

        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick;
        private bool checkedPowerComp;
        private CompPowerTrader cachedPowerComp;
        public HashSet<IntVec3> coveredCells;
        public HashSet<IntVec3> scanCells;

        private List<Thing> affectedThingsKeysWorkingList;
        private List<int> affectedThingsValuesWorkingList;
        public Dictionary<Thing, int> affectedThings = new Dictionary<Thing, int>();

    }

}
