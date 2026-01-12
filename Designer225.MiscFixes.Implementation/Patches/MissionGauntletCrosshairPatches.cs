using System;
using System.Reflection;
using Designer225.MiscFixes.Implementation.Util;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;

namespace Designer225.MiscFixes.Implementation.Patches
{
    public class MissionGauntletCrosshairPatches
    {
        private static MethodInfo? _getShouldArrowsBeVisibleMethod;
        
        private static Func<bool>? _getShouldArrowsBeVisibleDelegate;

        public static bool Prepare()
        {
            if (Settings.Instance!.FixMachineGunCrosshair)
            {
                _getShouldArrowsBeVisibleMethod = AccessTools.Method(typeof(MissionGauntletCrosshair), "GetShouldArrowsBeVisible");
                if (_getShouldArrowsBeVisibleMethod is null) return false;
                Debug.Print("[Designer225.MiscFixes] Fixing crossbow crosshairs");
                return true;
            }
            return false;
        }

        [HarmonyPatch("OnCreateView")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void OnCreateViewPostfix(MissionGauntletCrosshair __instance)
        {
            if (_getShouldArrowsBeVisibleMethod is null) return;
            _getShouldArrowsBeVisibleDelegate =
                AccessTools.MethodDelegate<Func<bool>>(_getShouldArrowsBeVisibleMethod, __instance);
        }

        [HarmonyPatch("OnDestroyView")]
        [HarmonyPrefix]
        public static void OnCreateViewPrefix() => _getShouldArrowsBeVisibleDelegate = null;

        [HarmonyPatch("GetShouldCrosshairBeVisible")]
        [HarmonyPostfix]
        // ReSharper disable twice InconsistentNaming
        public static void GetShouldCrosshairBeVisiblePostfix(MissionGauntletCrosshair __instance, ref bool __result)
        {
            if (_getShouldArrowsBeVisibleDelegate is null) return;
            if (__instance.Mission.MainAgent == null) return;

            if (!_getShouldArrowsBeVisibleDelegate() || !BannerlordConfig.DisplayTargetingReticule) return;
            
            var wieldedWeapon = __instance.Mission.MainAgent.WieldedWeapon;
            if (wieldedWeapon.IsEmpty) return;
            if (!wieldedWeapon.CurrentUsageItem.IsRangedWeapon) return;
            if (wieldedWeapon.CurrentUsageItem.WeaponClass != WeaponClass.Crossbow) return;
            __result = __result || wieldedWeapon.Ammo > 0;
        }
    }
}