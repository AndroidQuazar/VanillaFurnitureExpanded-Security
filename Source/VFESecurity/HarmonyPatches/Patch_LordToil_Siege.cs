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

    public static class Patch_LordToil_Siege
    {

        [HarmonyPatch(typeof(LordToil_Siege), nameof(LordToil_Siege.LordToilTick))]
        public static class LordToilTick
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var getIsShellInfo = AccessTools.Property(typeof(ThingDef), nameof(ThingDef.IsShell)).GetGetMethod();
                var getItemInfo = AccessTools.Method(typeof(List<Thing>), "get_Item");

                var isActuallyValidShellInfo = AccessTools.Method(typeof(LordToilTick), nameof(IsActuallyValidShell));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Look for any calls to IsShell (should only be one); we need to make this check SMARTER!
                    if (instruction.opcode == OpCodes.Callvirt && instruction.operand == getIsShellInfo)
                    {
                        yield return instruction; // thingList[j].def.IsShell
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6); // thingList
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 7); // j
                        yield return new CodeInstruction(OpCodes.Callvirt, getItemInfo); // thingList[j]
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // data
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        instruction = new CodeInstruction(OpCodes.Call, isActuallyValidShellInfo); // IsActuallyValidShell(thingList[j].def.IsShell, thingList[j], data, this)
                    }

                    yield return instruction;
                }
            }

            private static bool IsActuallyValidShell(bool isShell, Thing thing, LordToilData_Siege data, LordToil_Siege instance)
            {
                var raiderFaction = instance.lord.faction;
                var listerThings = instance.Map.listerThings;
                var validArtillery = listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(b => b.Faction == raiderFaction && TurretGunUtility.NeedsShells(b.def));
                var validFrameAndBlueprintEntities = ((IEnumerable<Thing>)listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)).Concat(listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)).
                    Where(f => f.Faction == raiderFaction).Select(f => f.def.entityDefToBuild).Where(e => e is ThingDef t && TurretGunUtility.NeedsShells(t)).Cast<ThingDef>();

                return isShell &&
                    (validArtillery.Any(a => ((Building_TurretGun)a).gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings.AllowedToAccept(thing)) ||
                    validFrameAndBlueprintEntities.Any(t => t.building.turretGunDef.building.defaultStorageSettings.AllowedToAccept(thing)));
            }

        }

        [HarmonyPatch(typeof(LordToil_Siege), "SetAsBuilder")]
        public static class SetAsBuilder
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var actualMinimumConstructionSkillInfo = AccessTools.Method(typeof(SetAsBuilder), nameof(ActualMinimumConstructionSkill));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Replace minLevel with the actual minimum level for blueprinted artillery if appropriate
                    if (instruction.opcode == OpCodes.Ldloc_1)
                    {
                        yield return instruction; // minLevel
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // data
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        instruction = new CodeInstruction(OpCodes.Call, actualMinimumConstructionSkillInfo); // ActualMinimumConstructionSkill(minLevel, data, this)
                    }

                    yield return instruction;
                }
            }

            private static int ActualMinimumConstructionSkill(int minLevel, LordToilData_Siege data, LordToil_Siege instance)
            {
                var blueprintedBuildings = instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).
                    Where(p => p.Faction == instance.lord.faction && p.Position.InHorDistOf(data.siegeCenter, data.baseRadius)).Select(p => p.def.entityDefToBuild).Where(e => e is ThingDef).Cast<ThingDef>();

                if (blueprintedBuildings.Any())
                    return Mathf.Max(minLevel, blueprintedBuildings.Select(b => b.constructionSkillPrerequisite).Max());
                return minLevel;
            }

        }

    }

}
