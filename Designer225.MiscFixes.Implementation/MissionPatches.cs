using System.Collections.Generic;
using System.Reflection.Emit;
using Designer225.MiscFixes.Util;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Designer225.MiscFixes
{
    public static class MissionPatches
    {
        [HarmonyPatch(typeof(Mission), nameof(Mission.SpawnAgent))]
        public static class SpawnAgentPatch
        {
            public static bool Prepare()
            {
                if (Settings.Instance!.PatchAgentSpawning)
                    return true;
                Debug.Print("[Designer225.MiscFixes] Will NOT attempt tp bypass age checker in Mission.SpawnAgent()");
                return false;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions);
                codeMatcher.MatchStartForward(
                    CodeMatch.Calls(AccessTools.Method(typeof(MBBodyProperties),
                        nameof(MBBodyProperties.GetMaturityType))),
                    CodeMatch.LoadsConstant(3));
                if (codeMatcher.IsValid)
                    codeMatcher.Advance().RemoveInstruction().InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_M1));
                return codeMatcher.InstructionEnumeration();
            }
        }
    }
}