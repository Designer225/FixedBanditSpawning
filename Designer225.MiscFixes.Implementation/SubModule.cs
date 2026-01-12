using System.Linq;
using Designer225.MiscFixes.Implementation.Models;
using Designer225.MiscFixes.Implementation.Util;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace Designer225.MiscFixes.Implementation
{
    public class SubModule : MBSubModuleBase
    {
        //internal static Dictionary<Agent, float> AgentAgeDict { get; set; } = new Dictionary<Agent, float>();
        private bool _isLoaded;
        
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (_isLoaded) return;
            new Harmony("d225.fixedbanditspawning").PatchAll();
            _isLoaded = true;
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            
            if (!(game.GameType is Campaign) || !(gameStarterObject is CampaignGameStarter gameStarter))
                return;
            
            // add game models
            if (Settings.Instance!.PatchWandererSpawning)
                gameStarter.AddModel(new D225MiscFixesHeroCreationModel(gameStarter.Models
                    .WhereQ(x => x is HeroCreationModel).Cast<HeroCreationModel>().Last()));
        }
    }
}