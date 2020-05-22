using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Helpers;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace FixedBanditSpawning
{
    public class SubModule : MBSubModuleBase
    {
        //internal static Dictionary<Agent, float> AgentAgeDict { get; set; } = new Dictionary<Agent, float>();

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            Harmony harmony = new Harmony("d225.fixedbanditspawning");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(MobileParty), "FillPartyStacks")]
    public static class MobileParty_FillPartyStacks_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance != null && D225MiscFixesSettings.Instance.PatchBanditSpawning)
            {
                Debug.Print("[FixedBanditSpawning] Will patch bandit spawning in MobileParty.FillPartyStack()");
                return true;
            }
            Debug.Print("[FixedBanditSpawning] Will NOT patch bandit spawning in MobileParty.FillPartyStack()");
            return false;
        }
        
        public static bool Prefix(MobileParty __instance, PartyTemplateObject pt, MobileParty.PartyTypeEnum partyType, int troopNumberLimit)
        {
            switch (partyType)
            {
                case MobileParty.PartyTypeEnum.Bandit: // TaleWorlds hardcoding strikes again
                    double num1 = 0.4 + 0.6 * MiscHelper.GetGameProcess();
                    int num2 = MBRandom.RandomInt(2);
                    double num3 = num2 == 0 ? MBRandom.RandomFloat : (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * 4.0);
                    double num4 = num2 == 0 ? (num3 * 0.8 + 0.2) : 1 + num3;

                    foreach (PartyTemplateStack stack in pt.Stacks)
                    {
                        int numTroopsToAdd = MBRandom.RoundRandomized((float) (stack.MinValue + num1 * num4 * MBRandom.RandomFloat * (stack.MaxValue - stack.MinValue)));
                        __instance.AddElementToMemberRoster(stack.Character, numTroopsToAdd);
                    }
                    return false;
                case MobileParty.PartyTypeEnum.Villager: // ... and again
                    int index = MBRandom.RandomInt(pt.Stacks.Count);
                    for (int troopCount = 0; troopCount < troopNumberLimit; troopCount++)
                    {
                        __instance.AddElementToMemberRoster(pt.Stacks[index].Character, 1);
                        index = MBRandom.RandomInt(pt.Stacks.Count);
                    }
                    return false;
                default: // everything else looks fine; hand stack filling to original method
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
    public static class Mission_SpawnAgent_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance != null && D225MiscFixesSettings.Instance.PatchAgentSpawning)
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
                    if (codes[i].opcode == OpCodes.Brtrue_S && codes[i].operand is Label)
                    {
                        stage = 2;
                        replaceIndex = i;
                    }
                    else
                    {
                        stage = 0;
                    }
                }
                else if (stage == 2)
                {
                    if (codes[i].opcode == OpCodes.Ldarg_1)
                    {
                        stage = 3;
                    }
                    else
                    {
                        stage = 0;
                        replaceIndex = -1;
                    }
                }
                else if (stage == 3)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S)
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
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo
                        && codes[i].operand as MethodInfo == AccessTools.Method(typeof(AgentBuildData), nameof(AgentBuildData.Age)))
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
                    if (codes[i].opcode == OpCodes.Pop)
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
                    if (codes[i].opcode == OpCodes.Br_S && codes[i].operand is Label)
                    {
                        jumpLabel = (Label)(codes[i].operand);
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

    [HarmonyPatch(typeof(Mission), "BuildAgent")]
    public static class Mission_BuildAgent_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance != null && D225MiscFixesSettings.Instance.PatchInvincibleChildren)
            {
                Debug.Print("[FixedBanditSpawning] Will patch invincible children");
                return true;
            }
            Debug.Print("[FixedBanditSpawning] Will NOT patch invincible children");
            return false;
        }

        public static void Postfix(ref Agent agent)
        {   
            if (agent.IsHuman && agent.Age < 18f)
            {
                float age = agent.Age;
                float scale = agent.AgentScale;
                //Debug.Print(string.Format("[FixedBanditSpawning] original agent {0} scale: {1}", agent.Name, agent.AgentScale));
                AccessTools.PropertySetter(typeof(Agent), nameof(Agent.Age)).Invoke(agent, new object[] { 18f });
                SkinGenerationParams skinParams = new SkinGenerationParams((int)(SkinMask.NoneVisible), agent.SpawnEquipment.GetUnderwearType(agent.IsFemale),
                    (int)agent.SpawnEquipment.BodyMeshType, (int)agent.SpawnEquipment.HairCoverType, (int)agent.SpawnEquipment.BeardCoverType,
                    (int)agent.SpawnEquipment.BodyDeformType, agent == Agent.Main, agent.Character.FaceDirtAmount, agent.IsFemale ? 1 : 0, false, false);
                agent.AgentVisuals.AddSkinMeshes(skinParams, agent.BodyPropertiesValue);
                //Debug.Print(string.Format("[FixedBanditSpawning] original agent {0} scale: {1}", agent.Name, agent.AgentScale));
                AccessTools.Method(typeof(Agent), "SetInitialAgentScale").Invoke(agent, new object[] { scale });
                //Debug.Print(string.Format("[FixedBanditSpawning] original agent {0} scale: {1}", agent.Name, agent.AgentScale));
                agent.AgentVisuals.BatchLastLodMeshes();
                agent.PreloadForRendering();
                AccessTools.PropertySetter(typeof(Agent), nameof(Agent.Age)).Invoke(agent, new object[] { age });
            }
        }
    }

    [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
    public static class HeroCreator_CreateNewHero_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance != null && D225MiscFixesSettings.Instance.PatchWandererSpawning)
                return true;
            Debug.Print("[FixedBanditSpawning] Will NOT attempt to patch age checkers in HeroCreator.CreateNewHero()");
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Print("[FixedBanditSpawning] Attempting to bypass 2 age checkers in HeroCreator.CreateNewHero()");
            var codes = instructions.ToList();

            int stage0 = 0;
            for (int j = 0; j < codes.Count; j++)
            {
                if (codes[j].opcode == OpCodes.Ldarg_0)
                {
                    if (stage0 == 0)
                    {
                        stage0 = 1;
                    }
                    else if (stage0 == 1)
                    {
                        codes[j] = new CodeInstruction(OpCodes.Nop);
                        codes[j + 1] = new CodeInstruction(OpCodes.Nop);
                        codes[j + 2] = new CodeInstruction(OpCodes.Nop);
                        codes[j + 3] = new CodeInstruction(OpCodes.Nop);
                        Debug.Print("[FixedBanditSpawning] Age checker 1 in HeroCreator.CreateNewHero() bypassed :)");
                        break;
                    }
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
                    if (codes[i].opcode == OpCodes.Bge_S && codes[i].operand is Label)
                    {
                        jumpLabel = (Label)(codes[i].operand);
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

    [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior), "CreateCompanion")]
    public static class UrbanCharactersCampaignBehavior_CreateCompanion_Patch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance != null && D225MiscFixesSettings.Instance.PatchWandererSpawning)
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
                    && codes[i].operand is MethodInfo && codes[i].operand as MethodInfo == AccessTools.PropertyGetter(typeof(AgeModel), nameof(AgeModel.HeroComesOfAge)))
                {
                    codes[i + 1] = new CodeInstruction(OpCodes.Nop);
                    codes[i + 2] = new CodeInstruction(OpCodes.Nop);
                    codes[i + 3] = new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(D225MiscFixesSettings), nameof(D225MiscFixesSettings.WandererRngMaxAge)));
                    Debug.Print("[FixedBanditSpawning] Artificial age adder in UrbanCharactersCampaignBehavior.CreateCompanion() bypassed :)");
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
}
