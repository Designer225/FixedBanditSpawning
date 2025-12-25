using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Designer225.MiscFixes
{
    public class SubModule : MBSubModuleBase
    {
        //internal static Dictionary<Agent, float> AgentAgeDict { get; set; } = new Dictionary<Agent, float>();
        private bool _isLoaded;
        private readonly MiscFixesEntryPoint _entryPoint;

        public SubModule()
        {
            if (!new MiscFixesAssemblyLoader().TryLoad(out var result, out var error))
                throw error!;

            _entryPoint = result.Instance;
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (_isLoaded) return;
            _entryPoint.OnBeforeInitialModuleScreenSetAsRoot();
            _isLoaded = true;
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            _entryPoint.OnGameStart(game, gameStarterObject);
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
}
