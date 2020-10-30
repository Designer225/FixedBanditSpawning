using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCM.Abstractions.Settings.Base.Global;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;

namespace FixedBanditSpawning
{
    static class D225MiscFixesSettingsUtil
    {
        private static ID225MiscFixesSettings instance;

        public static ID225MiscFixesSettings Instance
        {
            get
            {
                instance = D225MiscFixesSettings.Instance ?? instance ?? new D225MiscFixesDefaultSettings();
                return instance;
            }
        }

        public static int WandererRngMaxAge => Instance.WanderSpawningRngMax;
    }

    interface ID225MiscFixesSettings
    {
        bool PatchBanditSpawning { get; set; }

        bool PatchAgentSpawning { get; set; }

        bool PatchInvincibleChildren { get; set; }

        bool PatchWandererSpawning { get; set; }

        int WanderSpawningRngMax { get; set; }

        bool TownAndVillageVariety { get; set; }

        float WorkerGenderRatio { get; set; }
    }

    class D225MiscFixesDefaultSettings : ID225MiscFixesSettings
    {
        public bool PatchBanditSpawning { get; set; } = true;

        public bool PatchAgentSpawning { get; set; } = true;

        public bool PatchInvincibleChildren { get; set; } = true;

        public bool PatchWandererSpawning { get; set; } = true;

        public int WanderSpawningRngMax { get; set; } = 32;

        public bool TownAndVillageVariety { get; set; } = true;

        public float WorkerGenderRatio { get; set; } = LocationCharacterConstructorPatch.WorkerGenderRatio;
    }

    partial class D225MiscFixesSettings : AttributeGlobalSettings<D225MiscFixesSettings>, ID225MiscFixesSettings
    {
        public override string Id => "D225.MiscFixes";

        public override string FolderName => "D225.MiscFixes";

        public override string FormatType => "json2";

        public override string DisplayName => ModNameTextObject.ToString();

        [SettingPropertyBool(PatchBanditSpawningName, HintText = PatchBanditSpawningHint, Order = 0, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText, GroupOrder = 0)]
        public bool PatchBanditSpawning { get; set; } = true;

        [SettingPropertyBool(PatchAgentSpawningName, HintText = PatchAgentSpawningHint, Order = 1, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool PatchAgentSpawning { get; set; } = true;

        [SettingPropertyBool(PatchInvincibleChildrenName, HintText = PatchInvincibleChildrenHint, Order = 2, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool PatchInvincibleChildren { get; set; } = true;

        [SettingPropertyBool(PatchWandererSpawningName, HintText = PatchWandererSpawningHint, IsToggle = true, Order = 3, RequireRestart = true)]
        [SettingPropertyGroup(PatchWandererSpawningName, GroupOrder = 1)]
        public bool PatchWandererSpawning { get; set; } = true;

        [SettingPropertyInteger(WanderSpawningRngMaxName, 0, 50, HintText = WanderSpawningRngMaxHint, Order = 4, RequireRestart = false)]
        [SettingPropertyGroup(PatchWandererSpawningName)]
        public int WanderSpawningRngMax { get; set; } = 32;

        [SettingPropertyBool(TownAndVillageVarietyName, HintText = TownAndVillageVarietyHint, IsToggle = true, Order = 5, RequireRestart = true)]
        [SettingPropertyGroup(TownAndVillageVarietyName, GroupOrder = 2)]
        public bool TownAndVillageVariety { get; set; } = true;

        [SettingPropertyFloatingInteger(WorkerGenderRatioName, 0, 1, HintText = WorkerGenderRatioHint, Order = 6, RequireRestart = false)]
        [SettingPropertyGroup(TownAndVillageVarietyName)]
        public float WorkerGenderRatio { get; set; } = LocationCharacterConstructorPatch.WorkerGenderRatio;
    }
}
