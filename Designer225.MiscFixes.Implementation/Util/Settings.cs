using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using static Designer225.MiscFixes.Implementation.Util.Localization;

namespace Designer225.MiscFixes.Implementation.Util
{
    public sealed class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "D225.MiscFixes";

        public override string FolderName => "D225.MiscFixes";

        public override string FormatType => "json2";

        public override string DisplayName => ModNameTextObject.ToString();

        #region Patches
        [SettingPropertyBool(PatchHeroEncyclopediaEntriesName, HintText = PatchHeroEncyclopediaEntriesHint, Order = 0, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText, GroupOrder = 0)]
        public bool PatchHeroEncyclopediaEntries { get; set; } = false;

        [SettingPropertyBool(PatchBanditSpawningName, HintText = PatchBanditSpawningHint, Order = 1, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool PatchBanditSpawning { get; set; } = false;

        [SettingPropertyBool(PatchAgentSpawningName, HintText = PatchAgentSpawningHint, Order = 2, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool PatchAgentSpawning { get; set; } = false;

        [SettingPropertyBool(PatchInvincibleChildrenName, HintText = PatchInvincibleChildrenHint, Order = 3, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool PatchInvincibleChildren { get; set; } = false;

        [SettingPropertyBool(PatchSavePreviewGenderBugName, HintText = PatchSavePreviewGenderBugHint, Order = 4, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool PatchSavePreviewGenderBug { get; set; } = false;

        [SettingPropertyBool(FixMachineGunCrosshairName, HintText = FixMachineGunCrosshairHint, Order = 5, RequireRestart = true)]
        [SettingPropertyGroup(ModNameText)]
        public bool FixMachineGunCrosshair { get; set; } = false;
        #endregion

        #region Wanderer Spawning
        [SettingPropertyBool(PatchWandererSpawningName, HintText = PatchWandererSpawningHint, IsToggle = true, RequireRestart = false)]
        [SettingPropertyGroup(PatchWandererSpawningName, GroupOrder = 1)]
        public bool PatchWandererSpawning { get; set; } = false;

        [SettingPropertyInteger(WanderSpawningRngMaxName, 0, 50, HintText = WanderSpawningRngMaxHint, Order = 0, RequireRestart = false)]
        [SettingPropertyGroup(PatchWandererSpawningName)]
        public int WanderSpawningRngMax { get; set; } = 32;
        #endregion

        #region Town and Village Variety
        [SettingPropertyBool(TownAndVillageVarietyName, HintText = TownAndVillageVarietyHint, IsToggle = true, RequireRestart = true)]
        [SettingPropertyGroup(TownAndVillageVarietyName, GroupOrder = 2)]
        public bool TownAndVillageVariety { get; set; } = false;

        [SettingPropertyFloatingInteger(WorkerGenderRatioName, 0, 1, HintText = WorkerGenderRatioHint, Order = 0, RequireRestart = false)]
        [SettingPropertyGroup(TownAndVillageVarietyName)]
        public float WorkerGenderRatio { get; set; } = 0.3f;
        #endregion
    }
}
