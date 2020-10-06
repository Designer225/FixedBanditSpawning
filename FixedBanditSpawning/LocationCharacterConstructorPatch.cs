using HarmonyLib;
using Helpers;
using SandBox.Source.Towns;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace FixedBanditSpawning
{
    [HarmonyPatch(typeof(LocationCharacter), MethodType.Constructor, new[] { typeof(AgentData), typeof(LocationCharacter.AddBehaviorsDelegate), typeof(string),
        typeof(bool), typeof(LocationCharacter.CharacterRelations), typeof(string), typeof(bool), typeof(bool), typeof(ItemObject), typeof(bool), typeof(bool), typeof(bool)
    })]
    static class LocationCharacterConstructorPatch
    {
        public const int InfantAge = 3, ChildAge = 6, TweenAge = 10, TeenAge = 13, AdultAge = 16;

        public const float WorkerGenderRatio = 0.3f;

        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance == null || !D225MiscFixesSettings.Instance.TownAndVillageVariety) return false;
            Debug.Print("[FixedBanditSpawning] Adding a little bit variety to towns and villages...");
            return true;
        }

        public static void Prefix(AgentData agentData)
        {
            try
            {
                AgeModel ageModel = Campaign.Current?.Models?.AgeModel;
                if (ageModel == default) return;

                BasicCharacterObject character = agentData.AgentCharacter;
                if (character is CharacterObject)
                {
                    CultureObject culture = (character as CharacterObject).Culture;
                    if (culture == default) return;

                    int randMin = agentData.AgentAge;
                    int randMax = randMin;

                    // Rather spaghetti-ish, but should do better performance wise
                    if (character == culture.Barber || character == culture.ShopWorker || character == culture.TavernGamehost || character == culture.Tavernkeeper
                        || character == culture.Musician || character == culture.Armorer || character == culture.Blacksmith || character == culture.HorseMerchant
                        || character == culture.Merchant || character == culture.Weaponsmith || character == culture.GangleaderBodyguard)
                    {
                        randMin = TweenAge;
                        randMax = ageModel.BecomeOldAge;
                        agentData.IsFemale(MBRandom.RandomFloat <
                            (D225MiscFixesSettings.Instance != null ? D225MiscFixesSettings.Instance.WorkerGenderRatio : WorkerGenderRatio));
                    }
                    else if (character == culture.ArtisanNotary || character == culture.MerchantNotary || character == culture.PreacherNotary
                        || character == culture.RuralNotableNotary || character == culture.RansomBroker)
                    {
                        randMin = TweenAge;
                        randMax = ageModel.MaxAge;
                        agentData.IsFemale(MBRandom.RandomFloat <
                            (D225MiscFixesSettings.Instance != null ? D225MiscFixesSettings.Instance.WorkerGenderRatio : WorkerGenderRatio));
                    }
                    else if (character == culture.MeleeMilitiaTroop || character == culture.RangedMilitiaTroop
                        || character == culture.MilitiaSpearman || character == culture.MilitiaArcher)
                    {
                        randMin = TeenAge;
                        randMax = ageModel.BecomeOldAge;
                        agentData.IsFemale(MBRandom.RandomFloat <
                            (D225MiscFixesSettings.Instance != null ? D225MiscFixesSettings.Instance.WorkerGenderRatio : WorkerGenderRatio));
                    }
                    else if (character == culture.MeleeEliteMilitiaTroop || character == culture.RangedEliteMilitiaTroop
                        || character == culture.MilitiaVeteranSpearman || character == culture.MilitiaVeteranArcher)
                    {
                        randMin = AdultAge;
                        randMax = ageModel.BecomeOldAge;
                        agentData.IsFemale(MBRandom.RandomFloat <
                            (D225MiscFixesSettings.Instance != null ? D225MiscFixesSettings.Instance.WorkerGenderRatio : WorkerGenderRatio));
                    }
                    else if (character == culture.TavernWench || character == culture.FemaleDancer)
                    {
                        randMin = TweenAge;
                        randMax = ageModel.BecomeOldAge;
                    }
                    else if (character == culture.Townsman || character == culture.Townswoman || character == culture.Villager || character == culture.VillageWoman)
                    {
                        randMin = TweenAge;
                        randMax = ageModel.MaxAge;
                    }
                    else if (character == culture.Beggar || character == culture.FemaleBeggar)
                    {
                        randMin = ChildAge;
                        randMax = ageModel.MaxAge;
                    }
                    else if (character == culture.TownsmanInfant || character == culture.TownswomanInfant)
                    {
                        randMin = InfantAge;
                        randMax = ChildAge;
                    }
                    else if (character == culture.TownsmanChild || character == culture.TownswomanChild
                        || character == culture.VillagerMaleChild || character == culture.VillagerFemaleChild)
                    {
                        randMin = ChildAge;
                        randMax = TeenAge;
                    }
                    else if (character == culture.TownsmanTeenager || character == culture.TownswomanTeenager
                        || character == culture.VillagerMaleTeenager || character == culture.VillagerFemaleTeenager)
                    {
                        randMin = TeenAge;
                        randMax = AdultAge;
                    }

                    if (agentData.AgeOverriden || randMin != agentData.AgentAge || randMin != randMax)
                        agentData.Age(MBRandom.RandomInt(randMin, randMax));
                }
            }
            catch (Exception e)
            {
                Debug.Print($"[FixedBanditSpawning] Error attempting to modify location character agent data.\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(CommonVillagersCampaignBehavior), "conversation_children_rhymes_on_condition")]
    static class CommonVillagersCampaignBehaviorPatch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance == null || !D225MiscFixesSettings.Instance.TownAndVillageVariety) return false;
            Debug.Print("[FixedBanditSpawning] Making some children less innocent...");
            return true;
        }

        public static void Postfix(ref bool __result)
        {

            BasicCharacterObject character = Campaign.Current.ConversationManager.OneToOneConversationAgent.Character;
            if (character is CharacterObject)
            {
                CultureObject culture = (character as CharacterObject).Culture;
                __result = character == culture.TownsmanInfant || character == culture.TownswomanInfant
                    || character == culture.TownsmanChild || character == culture.TownswomanChild
                    || character == culture.VillagerMaleChild || character == culture.VillagerFemaleChild;
            }
        }
    }

    // Yes, there is already a patch on the same method. No, they do not do the same thing.
    [HarmonyPatch(typeof(HeroCreator), "CreateNewHero")]
    static class CreateNewHero_NotablesPatch
    {
        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance == null || !D225MiscFixesSettings.Instance.TownAndVillageVariety) return false;
            Debug.Print("[FixedBanditSpawning] Patching hero creation method to allow female notables (on top of existing female templates... who instead can be male)...");
            return true;
        }

        public static void Postfix(ref Hero __result)
        {
            // It appears that gang leaders, merchants, and preachers have some female representation. Will skip them in that case.
            if ((__result.IsArtisan || __result.IsRuralNotable || __result.IsHeadman || __result.IsOutlaw) && !__result.IsFemale)
                __result.UpdatePlayerGender(MBRandom.RandomFloat < (D225MiscFixesSettings.Instance != null
                    ? D225MiscFixesSettings.Instance.WorkerGenderRatio : LocationCharacterConstructorPatch.WorkerGenderRatio));

            AgeModel ageModel = Campaign.Current.Models.AgeModel;
            int baseAge = Math.Max(ageModel.HeroComesOfAge, LocationCharacterConstructorPatch.TweenAge);
            if (__result.IsNotable || __result.IsOutlaw)
                __result.BirthDay = HeroHelper.GetRandomBirthDayForAge(baseAge + MBRandom.RandomInt(ageModel.BecomeOldAge - baseAge + 1));
        }
    }
}