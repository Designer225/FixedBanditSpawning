using HarmonyLib;
using TaleWorlds.Core;

namespace Designer225.MiscFixes
{
    public abstract class MiscFixesEntryPoint
    {
        public readonly Harmony HarmonyInstance = new Harmony("d225.fixedbanditspawning");

        public abstract void OnBeforeInitialModuleScreenSetAsRoot();

        public abstract void OnGameStart(Game game, IGameStarter gameStarterObject);
    }
}