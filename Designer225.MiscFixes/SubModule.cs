using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;

namespace Designer225.MiscFixes
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

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (!(game.GameType is Campaign) || !(gameStarterObject is CampaignGameStarter gameStarter))
                return;
            
            // add game models
            if (D225MiscFixesSettingsUtil.Instance.PatchWandererSpawning)
                gameStarter.AddModel(new D225MiscFixesHeroCreationModel(gameStarter.Models
                    .WhereQ(x => x is HeroCreationModel).Cast<HeroCreationModel>().Last()));
        }

        internal static void DisableInvulnerability(Agent agent, bool prepareImmediately)
        {
            if (agent.IsHuman && agent.Age < 18f)
            {
                var age = agent.Age;
                var scale = agent.AgentScale;
                agent.Age = 18f;
                //AccessTools.PropertySetter(typeof(Agent), nameof(Agent.Age)).Invoke(agent, new object[] { 18f });

                SkinGenerationParams skinParams = GenerateSkinGenParams(agent);
                agent.AgentVisuals.AddSkinMeshes(skinParams, agent.BodyPropertiesValue, prepareImmediately,
                    prepareImmediately);
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

    // TaleWorlds appears to have finally fixed this, need verification
    // [HarmonyPatch(typeof(MobileParty), "FillPartyStacks")]
    // public static class MobileParty_FillPartyStacks_Patch
    // {
    //     public static bool Prepare()
    //     {
    //         if (D225MiscFixesSettingsUtil.Instance.PatchBanditSpawning)
    //         {
    //             Debug.Print("[Designer225.MiscFixes] Will patch bandit spawning in MobileParty.FillPartyStack()");
    //             return true;
    //         }
    //         Debug.Print("[Designer225.MiscFixes] Will NOT patch bandit spawning in MobileParty.FillPartyStack()");
    //         return false;
    //     }
    //     
    //     public static bool Prefix(MobileParty __instance, PartyTemplateObject pt, int troopNumberLimit)
    //     {
    //         if (__instance.IsBandit) // TaleWorlds hardcoding strikes again
    //         {
    //             double num1 = 0.4 + 0.8 * Campaign.Current.PlayerProgress;
    //             int num2 = MBRandom.RandomInt(2);
    //             double num3 = num2 == 0 ? MBRandom.RandomFloat : (MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * 4.0);
    //             double num4 = num2 == 0 ? (num3 * 0.8 + 0.2) : 1 + num3;
    //
    //             foreach (PartyTemplateStack stack in pt.Stacks)
    //             {
    //                 int numTroopsToAdd = MBRandom.RoundRandomized((float)(stack.MinValue + num1 * num4 * MBRandom.RandomFloat * (stack.MaxValue - stack.MinValue)));
    //                 __instance.AddElementToMemberRoster(stack.Character, numTroopsToAdd);
    //             }
    //         }
    //         else if (__instance.IsVillager)
    //         {
    //             int index = MBRandom.RandomInt(pt.Stacks.Count);
    //             for (int troopCount = 0; troopCount < troopNumberLimit; troopCount++)
    //             {
    //                 __instance.AddElementToMemberRoster(pt.Stacks[index].Character, 1);
    //                 index = MBRandom.RandomInt(pt.Stacks.Count);
    //             }
    //         }
    //         else // everything else looks fine; hand stack filling to original method
    //             return true;
    //
    //         return false;
    //     }
    // }

    [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
    public static class MissionSpawnAgentPatch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchAgentSpawning)
                return true;
            Debug.Print("[Designer225.MiscFixes] Will NOT attempt tp bypass age checker in Mission.SpawnAgent()");
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(
                CodeMatch.Calls(AccessTools.Method(typeof(MBBodyProperties), nameof(MBBodyProperties.GetMaturityType))),
                CodeMatch.LoadsConstant(3));
            if (codeMatcher.IsValid)
                codeMatcher.Advance().RemoveInstruction().InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_M1));
            return codeMatcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(Agent), nameof(Agent.EquipItemsFromSpawnEquipment))]
    public static class AgentEquipItemFromSpawnEquipmentPatch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchInvincibleChildren)
            {
                Debug.Print("[Designer225.MiscFixes] Will patch invincible children");
                return true;
            }
            return false;
        }

        public static void Postfix(Agent __instance, bool neededBatchedItems, bool prepareImmediately)
        {
            if (__instance.IsHuman && __instance.Age < 18f)
                SubModule.DisableInvulnerability(__instance, !neededBatchedItems | prepareImmediately);
        }
    }

    [HarmonyPatch(typeof(BasicCharacterTableau), "RefreshCharacterTableau")]
    static class InitializeAgentVisualsTranspiler
    {
        public static bool Prepare(MethodBase original)
        {
            if (D225MiscFixesSettingsUtil.Instance.PatchSavePreviewGenderBug)
            {
                Debug.Print("[Designer225.MiscFixes] Preparing to patch incorrect save preview stuff");
                return true;
            }
            return false;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(
                CodeMatch.LoadsField(AccessTools.Field(typeof(BasicCharacterTableau), "_faceDirtAmount")),
                CodeMatch.LoadsLocal());
            if (codeMatcher.IsValid)
                codeMatcher.Advance().RemoveInstruction().InsertAndAdvance(CodeInstruction.LoadArgument(0),
                    CodeInstruction.LoadField(typeof(BasicCharacterTableau), "_isFemale"));
            return codeMatcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(MissionGauntletCrosshair))]
    static class MissionGauntletCrosshairPatches
    {
        private static MethodInfo? _getShouldArrowsBeVisibleMethod;
        
        private static Func<bool>? _getShouldArrowsBeVisibleDelegate;

        public static bool Prepare()
        {
            if (D225MiscFixesSettingsUtil.Instance.FixMachineGunCrosshair)
            {
                _getShouldArrowsBeVisibleMethod = AccessTools.Method(typeof(MissionGauntletCrosshair), "GetShouldArrowsBeVisible");
                if (_getShouldArrowsBeVisibleMethod is null) return false;
                Debug.Print("[Designer225.MiscFixes] Fixing crossbow crosshairs");
                return true;
            }
            return false;
        }

        [HarmonyPatch("OnCreateView")]
        [HarmonyPostfix]
        public static void OnCreateViewPostfix(MissionGauntletCrosshair __instance)
        {
            if (_getShouldArrowsBeVisibleMethod is null) return;
            _getShouldArrowsBeVisibleDelegate =
                AccessTools.MethodDelegate<Func<bool>>(_getShouldArrowsBeVisibleMethod, __instance);
        }

        [HarmonyPatch("OnDestroyView")]
        [HarmonyPrefix]
        public static void OnCreateViewPrefix() => _getShouldArrowsBeVisibleDelegate = null;

        [HarmonyPatch("GetShouldCrosshairBeVisible")]
        [HarmonyPostfix]
        public static void GetShouldCrosshairBeVisiblePostfix(MissionGauntletCrosshair __instance, ref bool __result)
        {
            if (_getShouldArrowsBeVisibleDelegate is null) return;
            if (__instance.Mission.MainAgent == null) return;

            if (!_getShouldArrowsBeVisibleDelegate() || !BannerlordConfig.DisplayTargetingReticule) return;
            
            var wieldedWeapon = __instance.Mission.MainAgent.WieldedWeapon;
            if (wieldedWeapon.IsEmpty) return;
            if (!wieldedWeapon.CurrentUsageItem.IsRangedWeapon) return;
            if (wieldedWeapon.CurrentUsageItem.WeaponClass != WeaponClass.Crossbow) return;
            __result = __result || wieldedWeapon.Ammo > 0;
        }
    }
}
