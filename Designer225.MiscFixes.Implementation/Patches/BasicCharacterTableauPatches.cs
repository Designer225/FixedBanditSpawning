using System.Collections.Generic;
using System.Reflection;
using Designer225.MiscFixes.Implementation.Util;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace Designer225.MiscFixes.Implementation.Patches
{
    public class BasicCharacterTableauPatches
    {
        [HarmonyPatch(typeof(BasicCharacterTableau), "RefreshCharacterTableau")]
        public static class RefreshCharacterTableauPatch
        {
            public static bool Prepare(MethodBase original)
            {
                if (Settings.Instance!.PatchSavePreviewGenderBug)
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
    }
}