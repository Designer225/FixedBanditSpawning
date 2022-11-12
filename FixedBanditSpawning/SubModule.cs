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
            Debug.Print("[FixedBanditSpawning] Attempting to bypass age checker in Mission.SpawnAgent()");
            var codes = instructions.ToList();

            int stage = 0;
            int replaceIndex = -1;
            Label jumpLabel = default;
            for (int i = 0; i < codes.Count; i++)
            {
                if (stage == 0 && codes[i].opcode == OpCodes.Ldloc_3)
                {
                    stage = 1;
                }
                else if (stage == 1)
                {
                    if (codes[i].opcode != OpCodes.Ldc_R4)
                    {
                        stage = 2;
                    }
                    else
                    {
                        stage = 0;
                    }
                }
                else if (stage == 2)
                {
                    if (codes[i].opcode == OpCodes.Bne_Un_S && codes[i].operand is Label)
                    {
                        stage = 3;
                        replaceIndex = i;
                    }
                    else
                    {
                        stage = 0;
                    }
                }
                else if (stage == 3)
                {
                    if (codes[i].opcode == OpCodes.Ldarg_1)
                    {
                        stage = 4;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
                else if (stage == 4)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S)
                    {
                        stage = 5;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
                else if (stage == 5)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo method
                        && method == AccessTools.Method(typeof(AgentBuildData), nameof(AgentBuildData.Age)))
                    {
                        stage = 6;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
                else if (stage == 6)
                {
                    if (codes[i].opcode == OpCodes.Pop)
                    {
                        stage = 7;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
                else if (stage == 7)
                {
                    if (codes[i].opcode == OpCodes.Br_S && codes[i].operand is Label label)
                    {
                        jumpLabel = label;
                        break;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
            }

            if (replaceIndex != -1 && jumpLabel != default)
            {
                codes[replaceIndex].operand = jumpLabel;
                Debug.Print("[FixedBanditSpawning] Age checker in Mission.SpawnAgent() bypassed :)");
            }

            return codes.AsEnumerable();
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

    //[HarmonyPatch(typeof(Mission), "BuildAgent")]
    //public static class Mission_BuildAgent_Patch
    //{
    //    public static bool Prepare()
    //    {
    //        if (D225MiscFixesSettingsUtil.Instance.PatchInvincibleChildren)
    //        {
    //            Debug.Print("[FixedBanditSpawning] Will patch invincible children");
    //            return true;
    //        }
    //        Debug.Print("[FixedBanditSpawning] Will NOT patch invincible children");
    //        return false;
    //    }

    //    public static void Postfix(Agent agent)
    //    {   
    //        if (agent.IsHuman && agent.Age < 18f)
    //        {
    //            SubModule.DisableInvulnerability(agent);
    //            agent.AgentVisuals.BatchLastLodMeshes();
    //            agent.PreloadForRendering();
    //            //agent.UpdateSpawnEquipmentAndRefreshVisuals(agent.SpawnEquipment);
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(Agent), nameof(Agent.UpdateSpawnEquipmentAndRefreshVisuals))]
    //public static class UpdateSpawnEquipmentAndRefreshVisualsPatch
    //{
    //    public static bool Prepare()
    //    {
    //        if (D225MiscFixesSettingsUtil.Instance.PatchInvincibleChildren)
    //            return true;
    //        return false;
    //    }

    //    public static void Postfix(Agent __instance)
    //    {
    //        if (__instance.IsHuman && __instance.Age < 18f)
    //            SubModule.DisableInvulnerability(__instance);
    //    }
    //}

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
            Debug.Print("[FixedBanditSpawning] Attempting to bypass 2 age checkers in HeroCreator.CreateNewHero()");
            var codes = instructions.ToList();

            for (int j = 0; j < codes.Count - 3; j++)
            {
                if (codes[j].opcode == OpCodes.Ldarg_0 && codes[j+1].opcode == OpCodes.Callvirt
                    && codes[j+2].opcode == OpCodes.Conv_I4 && codes[j+3].opcode == OpCodes.Starg_S)
                {
                    codes[j] = new CodeInstruction(OpCodes.Nop);
                    codes[j + 1] = new CodeInstruction(OpCodes.Nop);
                    codes[j + 2] = new CodeInstruction(OpCodes.Nop);
                    codes[j + 3] = new CodeInstruction(OpCodes.Nop);
                    Debug.Print("[FixedBanditSpawning] Age checker 1 in HeroCreator.CreateNewHero() bypassed :)");
                    break;
                }
            }

            int stage = 0;
            int replaceIndex = -1;
            Label jumpLabel = default;
            for (int i = 0; i < codes.Count; i++)
            {
                if (stage == 0 && codes[i].opcode == OpCodes.Ldarg_1)
                {
                    stage = 1;
                    replaceIndex = i;
                }
                else if (stage == 1)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S)
                    {
                        stage = 2;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
                else if (stage == 2)
                {
                    if (codes[i].opcode == OpCodes.Bge_S && codes[i].operand is Label label)
                    {
                        jumpLabel = label;
                        break;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
            }

            if (replaceIndex != -1 && jumpLabel != default)
            {
                codes[replaceIndex] = new CodeInstruction(OpCodes.Br, jumpLabel);
                Debug.Print("[FixedBanditSpawning] Age checker 2 in HeroCreator.CreateNewHero() bypassed :)");
            }

            return codes.AsEnumerable();
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
            Debug.Print("[FixedBanditSpawning] Attempting to bypass artificial age adder in UrbanCharactersCampaignBehavior.CreateCompanion()");
            var codes = instructions.ToList();

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt
                    && codes[i].operand is MethodInfo method && method == AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge)))
                {
                    codes[i + 1] = new CodeInstruction(OpCodes.Nop);
                    codes[i + 2] = new CodeInstruction(OpCodes.Nop);
                    codes[i + 3] = new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(D225MiscFixesSettingsUtil), nameof(D225MiscFixesSettingsUtil.WandererRngMaxAge)));
                    Debug.Print("[FixedBanditSpawning] Artificial age adder in UrbanCharactersCampaignBehavior.CreateCompanion() bypassed :)");
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(BasicCharacterTableau), "RefreshCharacterTableau")]
    static class InitializeAgentVisualsTranspiler
    {
        private static readonly List<string> incompatibleInstances = new List<string>
        {
            "mod.bannerlord.popowanobi.dcc"
        };

        public static bool Prepare(MethodBase original)
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchSavePreviewGenderBug)
            {
                if (original == null) return true;
                var info = Harmony.GetPatchInfo(original);
                if (info == default || info.Transpilers.Select(x => x.owner).Intersect(incompatibleInstances).IsEmpty())
                {
                    Debug.Print("[FixedBanditSpawning] Preparing to patch incorrect save preview stuff");
                    return true;
                }
                Debug.Print("[FixedBanditSpawning] Patch to fix misgendered character rendering already exists, skipping");
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Print("[FixedBanditSpawning] Attempting to fix save preview misgendering");
            var code = instructions.ToList();

            // locate call to MBAgentVisuals function and work backwards
            int i;
            for (i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Call && code[i].operand is MethodInfo method
                    && method == AccessTools.Method(typeof(MBAgentVisuals), nameof(MBAgentVisuals.FillEntityWithBodyMeshesWithoutAgentVisuals)))
                    break;
            }

            if (i < code.Count)
            {
                for (; i >= 0; i--)
                {
                    if (code[i].opcode == OpCodes.Ldloc_S && code[i].operand is LocalBuilder lb && lb.LocalIndex == 4)
                    {
                        // replace current instruction
                        code[i] = new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BasicCharacterTableau), "_isFemale"));
                        // insert ahead this
                        code.Insert(i, new CodeInstruction(OpCodes.Ldarg_0)); // instance methods use ldarg_0 as this
                        Debug.Print("[FixedBanditSpawning] Save preview misgendering fixed");
                        break;
                    }
                }
            }

            return code.AsEnumerable();
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
