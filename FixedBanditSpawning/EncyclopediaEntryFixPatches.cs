using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace FixedBanditSpawning
{
    [HarmonyPatch]
    public static class FaceGen_GetMaturityTypeWithAgePatches
    {
        static bool Prepare()
        {
            return D225MiscFixesSettingsUtil.Instance.PatchHeroEncyclopediaEntries;
        }

        static IEnumerable<MethodBase> TargetMethods()
        {
            // TaleWorlds.Core.ViewModelCollection
            yield return AccessTools.Method(typeof(CharacterViewModel), nameof(CharacterViewModel.FillFrom),
                new Type[] {typeof(BasicCharacterObject), typeof(int)});
            // TaleWorlds.CampaignSystem
            //yield return AccessTools.Method(typeof(PartyScreenLogic), nameof(PartyScreenLogic.IsExecutable));
            // TaleWorlds.CampaignSystem.ViewModelCollection
            yield return AccessTools.Method(typeof(ClanLordItemVM), nameof(ClanLordItemVM.UpdateProperties));
            yield return AccessTools.Constructor(typeof(HeroVM), new Type[] { typeof(Hero), typeof(bool) });
            yield return AccessTools.Method(typeof(HeroViewModel), nameof(HeroViewModel.FillFrom),
                new Type[] { typeof(Hero), typeof(int), typeof(bool), typeof(bool) });
            //yield return AccessTools.Method(typeof(PartyCharacterVM), nameof(PartyCharacterVM.ExecuteExecuteTroop));
            // TaleWorlds.MountAndBlade.GauntletUI
            yield return AccessTools.Method(typeof(ImageIdentifierTextureProvider), nameof(ImageIdentifierTextureProvider.CreateImageWithId));
            yield return AccessTools.Method(typeof(ImageIdentifierTextureProvider), nameof(ImageIdentifierTextureProvider.ReleaseCache));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                yield return list[i];
                if (list[i].Matches(OpCodes.Call, AccessTools.Method(typeof(FaceGen), nameof(FaceGen.GetMaturityTypeWithAge))))
                    list[i + 1] = new CodeInstruction(OpCodes.Ldc_I4_0);
            }
        }
    }
}
