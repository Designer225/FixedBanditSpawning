using Designer225.MiscFixes.Util;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Designer225.MiscFixes
{
    public static class AgentPatches
    {
        private static void DisableInvulnerability(Agent agent, bool prepareImmediately)
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

        private static SkinGenerationParams GenerateSkinGenParams(Agent agent)
        {
            return new SkinGenerationParams((int)SkinMask.NoneVisible,
                agent.SpawnEquipment.GetUnderwearType(agent.IsFemale && agent.Age >= 14),
                (int)agent.SpawnEquipment.BodyMeshType, (int)agent.SpawnEquipment.HairCoverType,
                (int)agent.SpawnEquipment.BeardCoverType, (int)agent.SpawnEquipment.BodyDeformType, agent == Agent.Main,
                agent.Character.FaceDirtAmount, agent.IsFemale ? 1 : 0, agent.Character.Race, false, false);
        }
        
        [HarmonyPatch(typeof(Agent), nameof(Agent.EquipItemsFromSpawnEquipment))]
        public static class EquipItemsFromSpawnEquipmentPatch
        {
            public static bool Prepare()
            {
                if (Settings.Instance!.PatchInvincibleChildren)
                {
                    Debug.Print("[Designer225.MiscFixes] Will patch invincible children");
                    return true;
                }
                return false;
            }

            // ReSharper disable once InconsistentNaming
            public static void Postfix(Agent __instance, bool neededBatchedItems, bool prepareImmediately)
            {
                if (__instance.IsHuman && __instance.Age < 18f)
                    DisableInvulnerability(__instance, !neededBatchedItems | prepareImmediately);
            }
        }

        [HarmonyPatch(typeof(Agent), nameof(Agent.EquipItemsFromSpawnEquipment))]
        public static class EquipItemFromSpawnEquipmentPatch
        {
            public static bool Prepare()
            {
                if (Settings.Instance!.PatchInvincibleChildren)
                {
                    Debug.Print("[Designer225.MiscFixes] Will patch invincible children");
                    return true;
                }
                return false;
            }

            // ReSharper disable once InconsistentNaming
            public static void Postfix(Agent __instance, bool neededBatchedItems, bool prepareImmediately)
            {
                if (__instance.IsHuman && __instance.Age < 18f)
                    DisableInvulnerability(__instance, !neededBatchedItems | prepareImmediately);
            }
        }
    }
}