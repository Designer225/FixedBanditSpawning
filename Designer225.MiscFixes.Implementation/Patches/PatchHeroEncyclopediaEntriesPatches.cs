using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Designer225.MiscFixes.Implementation.Util;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.GauntletUI.TextureProviders.ImageIdentifiers;

namespace Designer225.MiscFixes.Implementation.Patches
{
    public static class PatchHeroEncyclopediaEntriesPatches
    {
        [HarmonyPatch]
        public static class FaceGenGetMaturityTypeWithAgePatches
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Prepare()
            {
                return Settings.Instance!.PatchHeroEncyclopediaEntries;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                // TaleWorlds.Core.ViewModelCollection
                yield return AccessTools.Method(typeof(CharacterViewModel), nameof(CharacterViewModel.FillFrom),
                    new[] {typeof(BasicCharacterObject), typeof(int), typeof(string)});
                // TaleWorlds.CampaignSystem
                //yield return AccessTools.Method(typeof(PartyScreenLogic), nameof(PartyScreenLogic.IsExecutable));
                // TaleWorlds.CampaignSystem.ViewModelCollection
                // yield return AccessTools.Method(typeof(ClanLordItemVM), nameof(ClanLordItemVM.UpdateProperties)); // crashes game for some reason
                yield return AccessTools.Constructor(typeof(HeroVM), new [] { typeof(Hero), typeof(bool) });
                yield return AccessTools.Method(typeof(HeroViewModel), nameof(HeroViewModel.FillFrom),
                    new[] { typeof(Hero), typeof(int), typeof(bool), typeof(bool) });
                //yield return AccessTools.Method(typeof(PartyCharacterVM), nameof(PartyCharacterVM.ExecuteExecuteTroop));
                // TaleWorlds.MountAndBlade.GauntletUI
                yield return AccessTools.Method(typeof(CharacterImageTextureProvider), "OnCreateImageWithId");
                // yield return AccessTools.Method(typeof(ImageIdentifierTextureProvider), nameof(ImageIdentifierTextureProvider.ReleaseCache));
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions);
                codeMatcher.MatchStartForward(
                    CodeMatch.Calls(AccessTools.Method(typeof(FaceGen), nameof(FaceGen.GetMaturityTypeWithAge))),
                    CodeMatch.LoadsConstant(1));
                if (codeMatcher.IsValid)
                {
                    codeMatcher.Advance();
                    var labels = codeMatcher.Labels;
                    // var blocks = codeMatcher.Blocks;
                    codeMatcher.RemoveInstruction()
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(labels));
                }

                return codeMatcher.InstructionEnumeration();
            }
        }
        
        [HarmonyPatch(typeof(ClanLordItemVM), nameof(ClanLordItemVM.RefreshValues))]
        public static class ClanLordItemVmUpdatePropertiesPatch
        {
            public static bool Prepare()
            {
                return Settings.Instance!.PatchHeroEncyclopediaEntries;
            }
        
            // ReSharper disable once InconsistentNaming
            public static void Postfix(ClanLordItemVM __instance)
            {
            
                __instance.IsChild =
                    FaceGen.GetMaturityTypeWithAge(__instance.GetHero().Age) <= BodyMeshMaturityType.Toddler;
            }
        }
    }
}