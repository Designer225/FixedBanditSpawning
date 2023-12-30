using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace FixedBanditSpawning
{
    static class D225MiscFixesSettingsUtil
    {
        private static ID225MiscFixesSettings? instance;

        private static FileInfo ConfigFile { get; } = new FileInfo(Path.Combine(BasePath.Name, "Modules", "D225.MiscFixes.config.xml"));

        public static int ExceptionCount { get; internal set; }

        public static ID225MiscFixesSettings Instance
        {
            get
            {
                // attempt to load MCM config
                try
                {
                    instance = D225MiscFixesSettings.Instance ?? instance;
                }
                catch (Exception e)
                {
                    if (ExceptionCount < 100) // again, don't want to throw too many exceptions
                    {
                        ExceptionCount++;
                        Debug.Print(string.Format("[FixedBanditSpawning] Failed to obtain MCM config, defaulting to config file.",
                            ConfigFile.FullName, e.Message, e.StackTrace));
                    }
                }

                // load config file if MCM config load fails
                if (instance == default)
                {
                    var serializer = new XmlSerializer(typeof(D225MiscFixesDefaultSettings));
                    if (ConfigFile.Exists)
                    {
                        try
                        {
                            using (var stream = ConfigFile.OpenText())
                                instance = serializer.Deserialize(stream) as D225MiscFixesDefaultSettings;
                        }
                        catch (Exception e)
                        {
                            Debug.Print(string.Format("[FixedBanditSpawning] Failed to load file {0}\n\nError: {1}\n\n{2}",
                                ConfigFile.FullName, e.Message, e.StackTrace));
                        }
                    }

                    if (instance == default) instance = new D225MiscFixesDefaultSettings();
                    using (var stream = ConfigFile.Open(FileMode.Create))
                    {
                        var xmlWritter = new XmlTextWriter(stream, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented,
                            Indentation = 4
                        };
                        serializer.Serialize(xmlWritter, instance);
                    }
                }
                return instance;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reset_exception_count", "d225_misc")]
        public static string ResetExceptionCount(List<string> strings)
        {
            ExceptionCount = 0;
            return "Success";
        }
    }

    interface ID225MiscFixesSettings
    {
        bool PatchHeroEncyclopediaEntries { get; set; }

        bool PatchBanditSpawning { get; set; }

        bool PatchAgentSpawning { get; set; }

        bool PatchInvincibleChildren { get; set; }

        bool PatchSavePreviewGenderBug { get; set; }

        bool FixMachineGunCrosshair { get; set; }

        bool PatchWandererSpawning { get; set; }

        int WanderSpawningRngMax { get; set; }

        bool TownAndVillageVariety { get; set; }

        float WorkerGenderRatio { get; set; }
    }

    [XmlRoot("D225MiscFixes", IsNullable = false)]
    public class D225MiscFixesDefaultSettings : ID225MiscFixesSettings
    {
        [XmlElement(DataType = "boolean")]
        public bool PatchHeroEncyclopediaEntries { get; set; } = false;

        [XmlElement(DataType = "boolean")]
        public bool PatchBanditSpawning { get; set; } = false;

        [XmlElement(DataType = "boolean")]
        public bool PatchAgentSpawning { get; set; } = false;

        [XmlElement(DataType = "boolean")]
        public bool PatchInvincibleChildren { get; set; } = false;

        [XmlElement(DataType = "boolean")]
        public bool PatchSavePreviewGenderBug { get; set; } = false;

        [XmlElement(DataType = "boolean")]
        public bool FixMachineGunCrosshair { get; set; } = false;

        [XmlElement(DataType = "boolean")]
        public bool PatchWandererSpawning { get; set; } = false;

        [XmlElement(DataType = "int")]
        public int WanderSpawningRngMax { get; set; } = 32;

        [XmlElement(DataType = "boolean")]
        public bool TownAndVillageVariety { get; set; } = false;

        [XmlElement(DataType = "float")]
        public float WorkerGenderRatio { get; set; } = LocationCharacterConstructorPatch.WorkerGenderRatio;
    }

    partial class D225MiscFixesSettings : AttributeGlobalSettings<D225MiscFixesSettings>, ID225MiscFixesSettings
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
        [SettingPropertyBool(PatchWandererSpawningName, HintText = PatchWandererSpawningHint, IsToggle = true, RequireRestart = true)]
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
        public float WorkerGenderRatio { get; set; } = LocationCharacterConstructorPatch.WorkerGenderRatio;
        #endregion
    }
}
