using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;

namespace FixedBanditSpawning
{
    public partial class D225MiscFixesSettings
    {
        private const string ModNameText = "{=D225MiscFixes_ModNameText}Designer225's Miscellaneous Fixes",

            PatchEncyclopediaEntryName = "{=D225MiscFixes_PatchEncyclopediaEntryName}Patch Encyclopedia Entry",
            PatchEncyclopediaEntryHint = "{=D225MiscFixes_PatchEncyclopediaEntryHint}Fixes some of TaleWorld's black magic that prevents children under 10 from being rendered in their encyclopedia entries. Also give children over 3 portraits (Baby portraits for 8 year olds? Seriously?). Disable if another mod also does this.",
            PatchBanditSpawningName = "{=D225MiscFixes_PatchBanditSpawningName}Patch Bandit Spawning",
            PatchBanditSpawningHint = "{=D225MiscFixes_PatchBanditSpawningHint}Enable this to patch bandit spawning to allow more than 3 types of troops. Disable if another mod also does this.",
            PatchAgentSpawningName = "{=D225MiscFixes_PatchAgentSpawningName}Patch Agent Spawning",
            PatchAgentSpawningHint = "{=D225MiscFixes_PatchAgentSpawningHint}Enable this to bypass age check that makes underage agents look adult-ish. Disable if another mod also does this.",
            PatchInvincibleChildrenName = "{=D225MiscFixes_PatchInvincibleChildrenName}Patch Invincible Children",
            PatchInvincibleChildrenHint = "{=D225MiscFixes_PatchInvincibleChildrenHint}Enable this to make children less overpowered. Disable if another mod also does this.",
            PatchSavePreviewGenderBugName = "{=D225MiscFixes_PatchSavePreviewGenderBugName}Fix Incorrect Save Preview",
            PatchSavePreviewGenderBugHint = "{=D225MiscFixes_PatchSavePreviewGenderBugHint}Enable this to fix heroes in save preview having incorrect morphs for certain edge cases. Disable if another mod also does this.",
            FixMachineGunCrosshairName = "{=D225MiscFixes_FixMachineGunCrosshairName}Fix Machine Gun/Multi-Round Crossbow Crosshair",
            FixMachineGunCrosshairHint = "{=D225MiscFixes_FixMachineGunCrosshairHint}Enable to fix crosshair disappearing when there are still rounds in a 'crossbow' (machine gun?). Disable if another mod also does this.",
            
            PatchWandererSpawningName = "{=D225MiscFixes_PatchWandererSpawningName}Patch Wander Spawning",
            PatchWandererSpawningHint = "{=D225MiscFixes_PatchWandererSpawningHint}Enable this to get rid of the arbitrary minimum of 20 years for wanderer spawning and also to increase the age range to from adult age to adult age + 32. Disable if another mod also does this.",
            WanderSpawningRngMaxName = "{=D225MiscFixes_WanderSpawningRngMaxName}Wanderer Max RNG Age Increase",
            WanderSpawningRngMaxHint = "{=D225MiscFixes_WanderSpawningRngMaxHint}Sets the maximum age increase of wanderers during game start. Default is 32 years (originally 5 + randomized max 27). Does not require restart unlike other options.",
            
            TownAndVillageVarietyName = "{=D225MiscFixes_TownAndVillageVarietyName}Add Variety to Villagers and Townsfolk",
            TownAndVillageVarietyHint = "{=D225MiscFixes_TownAndVillageVarietyHint}Make villagers and townsfolk slightly more varied and gendered. Also fixes an issue with modified age models. Disable if another mod also does this.",
            WorkerGenderRatioName = "{=D225MiscFixes_WorkerMaleToFemaleRatioName}Worker Gender Ratio",
            WorkerGenderRatioHint = "{=D225MiscFixes_WorkerMaleToFemaleRatioHint}Affects the ratio of workers (not just any generic townsfolk and villagers) in towns and villages that are female instead of being male. Default is 30%. Game default is 0%. Does not require restart unlike other options.";

        private static readonly TextObject ModNameTextObject = new TextObject(ModNameText);
    }
}
