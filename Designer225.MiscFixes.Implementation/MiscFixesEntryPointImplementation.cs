using System.Linq;
using Designer225.MiscFixes.Util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;

namespace Designer225.MiscFixes
{
    public class MiscFixesEntryPointImplementation : MiscFixesEntryPoint
    {
        public override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            HarmonyInstance.PatchAll();
        }

        public override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {

            if (!(game.GameType is Campaign) || !(gameStarterObject is CampaignGameStarter gameStarter))
                return;
            
            // add game models
            if (Settings.Instance!.PatchWandererSpawning)
                gameStarter.AddModel(new D225MiscFixesHeroCreationModel(gameStarter.Models
                    .WhereQ(x => x is HeroCreationModel).Cast<HeroCreationModel>().Last()));
        }
    }
}