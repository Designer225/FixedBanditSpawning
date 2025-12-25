using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.GauntletUI.TextureProviders.ImageIdentifiers;

namespace Designer225.MiscFixes
{
    [HarmonyPatch]
    public static class FaceGenGetMaturityTypeWithAgePatches
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool Prepare()
        {
            return D225MiscFixesSettingsUtil.Instance.PatchHeroEncyclopediaEntries;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static IEnumerable<MethodBase> TargetMethods()
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
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(
                CodeMatch.Calls(AccessTools.Method(typeof(FaceGen), nameof(FaceGen.GetMaturityTypeWithAge))),
                CodeMatch.LoadsConstant(1));
            if (codeMatcher.IsValid)
                codeMatcher.Advance().RemoveInstruction().InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0));
            return codeMatcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(ClanLordItemVM), nameof(ClanLordItemVM.RefreshValues))]
    public static class ClanLordItemVMUpdatePropertiesPatch
    {
        static bool Prepare()
        {
            return D225MiscFixesSettingsUtil.Instance.PatchHeroEncyclopediaEntries;
        }
    
        static void Postfix(ClanLordItemVM __instance)
        {
            
            __instance.IsChild =
                FaceGen.GetMaturityTypeWithAge(__instance.GetHero().Age) <= BodyMeshMaturityType.Toddler;
        }
    }
}
