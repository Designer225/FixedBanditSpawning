using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Helpers;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace FixedBanditSpawning
{
    public class SubModule : MBSubModuleBase
    {
        //internal static Dictionary<Agent, float> AgentAgeDict { get; set; } = new Dictionary<Agent, float>();
        private bool _isLoaded = false;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (_isLoaded) return;
            base.OnBeforeInitialModuleScreenSetAsRoot();

            Harmony harmony = new Harmony("d225.fixedbanditspawning");
            harmony.PatchAll();
            _isLoaded = true;
        }

        internal static void DisableInvulnerability(Agent agent)
        {
            if (agent.IsHuman && agent.Age < 18f)
            {
                float age = agent.Age;
                float scale = agent.AgentScale;
                agent.Age = 18f;
                //AccessTools.PropertySetter(typeof(Agent), nameof(Agent.Age)).Invoke(agent, new object[] { 18f });

                SkinGenerationParams skinParams = GenerateSkinGenParams(agent);
                agent.AgentVisuals.AddSkinMeshes(skinParams, agent.BodyPropertiesValue, true, agent.Character != null && agent.Character.FaceMeshCache);
                AccessTools.Method(typeof(Agent), "SetInitialAgentScale").Invoke(agent, new object[] { scale });
                //AccessTools.PropertySetter(typeof(Agent), nameof(Agent.Age)).Invoke(agent, new object[] { age });
                agent.Age = age;
            }
        }

        internal static SkinGenerationParams GenerateSkinGenParams(Agent agent)
        {
            return new SkinGenerationParams((int)SkinMask.NoneVisible, agent.SpawnEquipment.GetUnderwearType(agent.IsFemale && agent.Age >= 14),
                    (int)agent.SpawnEquipment.BodyMeshType, (int)agent.SpawnEquipment.HairCoverType, (int)agent.SpawnEquipment.BeardCoverType,
                    (int)agent.SpawnEquipment.BodyDeformType, agent == Agent.Main, agent.Character.FaceDirtAmount, agent.IsFemale ? 1 : 0,
                    agent.Character.Race, false, false);
        }
    }

    public static class PatchUtility
    {
        internal static bool Matches(this CodeInstruction instruction, OpCode opcode) => instruction.opcode == opcode;

        internal static bool Matches<T>(this CodeInstruction instruction, OpCode opcode, T operand)
            => instruction.Matches(opcode) && instruction.operand is T t && t.Equals(operand);
    }

    [HarmonyPatch(typeof(MobileParty), "FillPartyStacks")]
    public static class MobileParty_FillPartyStacks_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchBanditSpawning)
            {
                Debug.Print("[FixedBanditSpawning] Will patch bandit spawning in MobileParty.FillPartyStack()");
                return true;
            }
            Debug.Print("[FixedBanditSpawning] Will NOT patch bandit spawning in MobileParty.FillPartyStack()");
            return false;
        }
        
        public static bool Prefix(MobileParty __instance, PartyTemplateObject pt, int troopNumberLimit)
        {
            if (__instance.IsBandit) // TaleWorlds hardcoding strikes again
            {
                double num1 = 0.4 + 0.8 * Campaign.Current.PlayerProgress;
                int num2 = MBRandom.RandomInt(2);
                double num3 = num2 == 0 ? MBRandom.RandomFloat : (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * 4.0);
                double num4 = num2 == 0 ? (num3 * 0.8 + 0.2) : 1 + num3;

                foreach (PartyTemplateStack stack in pt.Stacks)
                {
                    int numTroopsToAdd = MBRandom.RoundRandomized((float)(stack.MinValue + num1 * num4 * MBRandom.RandomFloat * (stack.MaxValue - stack.MinValue)));
                    __instance.AddElementToMemberRoster(stack.Character, numTroopsToAdd);
                }
            }
            else if (__instance.IsVillager)
            {
                int index = MBRandom.RandomInt(pt.Stacks.Count);
                for (int troopCount = 0; troopCount < troopNumberLimit; troopCount++)
                {
                    __instance.AddElementToMemberRoster(pt.Stacks[index].Character, 1);
                    index = MBRandom.RandomInt(pt.Stacks.Count);
                }
            }
            else // everything else looks fine; hand stack filling to original method
                return true;

            return false;
        }
    }

    [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
    public static class Mission_SpawnAgent_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchAgentSpawning)
                return true;
            Debug.Print("[FixedBanditSpawning] Will NOT attempt tp bypass age checker in Mission.SpawnAgent()");
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Matches(OpCodes.Call, AccessTools.Method(typeof(MBBodyProperties), nameof(MBBodyProperties.GetMaturityType))))
                    list[i + 1] = new CodeInstruction(OpCodes.Ldc_I4_0);
            }
        }
    }

    [HarmonyPatch(typeof(Agent), nameof(Agent.EquipItemsFromSpawnEquipment))]
    public static class Agent_EquipItemFromSpawnEquipmentPatch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchInvincibleChildren)
            {
                Debug.Print("[FixedBanditSpawning] Will patch invincible children");
                return true;
            }
            return false;
        }

        public static void Postfix(Agent __instance)
        {
            if (__instance.IsHuman && __instance.Age < 18f)
                SubModule.DisableInvulnerability(__instance);
        }
    }

    [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
    public static class HeroCreator_CreateNewHero_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchWandererSpawning)
                return true;
            Debug.Print("[FixedBanditSpawning] Will NOT attempt to patch age checkers in HeroCreator.CreateNewHero()");
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Matches(OpCodes.Ldarg_0) && i + 3 < list.Count && list[i + 3].Matches(OpCodes.Starg_S))
                {
                    list.RemoveRange(i, 3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return list[i];
                }
                else if (list[i].Matches(OpCodes.Stloc_S) && list[i].operand is LocalBuilder lb && lb.LocalIndex == 5)
                {
                    yield return list[i];
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(list[i]);
                }
                else
                    yield return list[i];
            }
        }
    }

    [HarmonyPatch(typeof(CompanionsCampaignBehavior), "CreateCompanionAndAddToSettlement")]
    public static class UrbanCharactersCampaignBehavior_CreateCompanion_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchWandererSpawning)
                return true;
            Debug.Print("[FixedBanditSpawning] Will NOT attempt to bypass artificial age adder in UrbanCharactersCampaignBehavior.CreateCompanion()");
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.Matches(OpCodes.Ldc_I4_5))
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                else if (instruction.Matches(OpCodes.Ldc_I4_S, 27))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(D225MiscFixesSettingsUtil), nameof(D225MiscFixesSettingsUtil.Instance)));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ID225MiscFixesSettings), nameof(ID225MiscFixesSettings.WanderSpawningRngMax)));
                }
                else
                    yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(BasicCharacterTableau), "RefreshCharacterTableau")]
    static class InitializeAgentVisualsTranspiler
    {
        public static bool Prepare(MethodBase original)
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchSavePreviewGenderBug)
            {
                Debug.Print("[FixedBanditSpawning] Preparing to patch incorrect save preview stuff");
                return true;
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Matches(OpCodes.Ldfld, AccessTools.Field(typeof(BasicCharacterTableau), "_faceDirtAmount"))
                    && list[i + 1].Matches(OpCodes.Ldloc_S) && list[i + 1].operand is LocalBuilder lb && lb.LocalIndex == 4)
                {
                    list.RemoveAt(i + 1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BasicCharacterTableau), "_isFemale"));
                }
            }
        }
    }

    [HarmonyPatch(typeof(Agent), "OnWeaponReloadPhaseChange")]
    static class OnWeaponReloadPhaseChangePatch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.FixMachineGunCrosshair)
            {
                Debug.Print("[FixedBanditSpawning] Fixing crossbow crosshairs");
                return true;
            }
            return false;
        }

        public static void Prefix(Agent __instance, EquipmentIndex slotIndex, ref short reloadPhase)
        {
            if (__instance.Equipment[slotIndex].Ammo > 0) reloadPhase = 2;
        }
    }
}
