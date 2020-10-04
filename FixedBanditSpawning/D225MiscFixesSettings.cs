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
    public partial class D225MiscFixesSettings : AttributeGlobalSettings<D225MiscFixesSettings>
    {
        public override string Id => "D225.MiscFixes";

        public override string FolderName => "D225.MiscFixes";

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

        [SettingPropertyBool(PatchWandererSpawningName, HintText = PatchWandererSpawningHint, Order = 3, RequireRestart = true)]
        [SettingPropertyGroup(PatchWandererSpawningName, GroupOrder = 1, IsMainToggle = true)]
        public bool PatchWandererSpawning { get; set; } = true;

        [SettingPropertyInteger(WanderSpawningRngMaxName, 0, 50, HintText = WanderSpawningRngMaxHint, Order = 4, RequireRestart = false)]
        [SettingPropertyGroup(PatchWandererSpawningName)]
        public int WanderSpawningRngMax { get; set; } = 32;

        [SettingPropertyBool(TownAndVillageVarietyName, HintText = TownAndVillageVarietyHint, Order = 5, RequireRestart = true)]
        [SettingPropertyGroup(TownAndVillageVarietyName, GroupOrder = 2, IsMainToggle = true)]
        public bool TownAndVillageVariety { get; set; } = true;

        [SettingPropertyFloatingInteger(WorkerGenderRatioName, 0, 1, HintText = WorkerGenderRatioHint, Order = 6, RequireRestart = false)]
        [SettingPropertyGroup(TownAndVillageVarietyName)]
        public float WorkerGenderRatio { get; set; } = LocationCharacterConstructorPatch.WorkerGenderRatio;

        public static int WandererRngMaxAge => Instance != null ? Instance.WanderSpawningRngMax : 32;
    }
}
