using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBOptionScreen.Settings;
using MBOptionScreen.Attributes;
using MBOptionScreen.Attributes.v2;

namespace FixedBanditSpawning
{
    public partial class D225MiscFixesSettings : AttributeSettings<D225MiscFixesSettings>
    {
        public override string Id { get; set; } = "D225.MiscFixes";

        public override string ModuleFolderName => "D225.MiscFixes";

        public override string ModName => ModNameTextObject.ToString();

        [SettingPropertyBool(PatchBanditSpawningName, HintText = PatchBanditSpawningHint, Order = 0, RequireRestart = true)]
        public bool PatchBanditSpawning { get; set; } = true;

        [SettingPropertyBool(PatchAgentSpawningName, HintText = PatchAgentSpawningHint, Order = 1, RequireRestart = true)]
        public bool PatchAgentSpawning { get; set; } = true;

        [SettingPropertyBool(PatchInvincibleChildrenName, HintText = PatchInvincibleChildrenHint, Order = 2, RequireRestart = true)]
        public bool PatchInvincibleChildren { get; set; } = true;

        [SettingPropertyBool(PatchWandererSpawningName, HintText = PatchWandererSpawningHint, Order = 3, RequireRestart = true)]
        public bool PatchWandererSpawning { get; set; } = true;

        [SettingPropertyInteger(WanderSpawningRngMaxName, 0, 50, HintText = WanderSpawningRngMaxHint, Order = 4, RequireRestart = false)]
        public int WanderSpawningRngMax { get; set; } = 32;

        public static int WandererRngMaxAge => Instance != null ? Instance.WanderSpawningRngMax : 32;
    }
}
