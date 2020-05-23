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
        private const string D225MiscFixesModNameText = "{=D225MiscFixesModNameText}Designer225's Miscellaneous Fixes",

            PatchBanditSpawningName = "{=PatchBanditSpawningName}Patch Bandit Spawning",
            PatchBanditSpawningHint = "{=PatchBanditSpawningHint}Enable this to patch bandit spawning to allow more than 3 types of troops. Disable if another mod also does this.",
            PatchAgentSpawningName = "{=PatchAgentSpawningName}Patch Agent Spawning",
            PatchAgentSpawningHint = "{=PatchAgentSpawningHint}Enable this to bypass age check that makes underage agents look adult-ish. Disable if another mod also does this.",
            PatchInvincibleChildrenName = "{=PatchInvincibleChildrenName}Patch Invincible Children",
            PatchInvincibleChildrenHint = "{=PatchInvincibleChildrenHint}Enable this to make children less overpowered. Disable if another mod also does this.",
            PatchWandererSpawningName = "{=PatchWandererSpawningName}Patch Wander Spawning",
            PatchWandererSpawningHint = "{=PatchWandererSpawningHint}Enable this to get rid of the arbitrary minimum of 20 years for wanderer spawning and also to increase the age range to from adult age to adult age + 32. Disable if another mod also does this.",
            WanderSpawningRngMaxName = "{=WanderSpawningRngMaxName}Wanderer Max RNG Age Increase",
            WanderSpawningRngMaxHint = "{=WanderSpawningRngMaxHint}Sets the maximum age increase of wanderers during game start. Default is 32 years (originally 5 + randomized max 27). Does not require restart unlike other options.";

        private static readonly TextObject ModNameTextObject = new TextObject(D225MiscFixesModNameText);
    }
}
