using HarmonyLib;
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
        private const int InfantAge = 3, ChildAge = 6, TweenAge = 10, TeenAge = 13, AdultAge = 16;

        // War is hell.
        private const int MinBegAge = ChildAge;
        // Apprenticeships are real things in the medieval era. Though this patch may have exaggerated it.
        private const int MinWorkAge = TweenAge;

        public const float WorkerGenderRatio = 0.3f;

        public static bool Prepare()
        {
            if (D225MiscFixesSettings.Instance == null || !D225MiscFixesSettings.Instance.TownAndVillageVariety) return false;
            Debug.Print("[FixedBanditSpawning] Adding a little bit variety to towns and villages...");
            return true;
        }

        public static void Prefix(ref AgentData agentData)
        {
            AgeModel ageModel = Campaign.Current.Models.AgeModel;
            BasicCharacterObject character = agentData.AgentCharacter;
            if (character is CharacterObject)
            {
                CultureObject culture = (character as CharacterObject).Culture;

                if (character == culture.Beggar || character == culture.FemaleBeggar)
                {
                    agentData.Age(MBRandom.RandomInt(MinBegAge, ageModel.MaxAge));
                }
                else
                {
                    int randMin = agentData.AgentAge;
                    int randMax = randMin;

                    if (character == culture.Barber || character == culture.ShopWorker || character == culture.TavernGamehost
                        || character == culture.Tavernkeeper || character == culture.Musician || character == culture.RansomBroker
                        || character == culture.Armorer || character == culture.Blacksmith || character == culture.HorseMerchant
                        || character == culture.Merchant || character == culture.Weaponsmith || character == culture.ArtisanNotary
                        || character == culture.MerchantNotary || character == culture.PreacherNotary || character == culture.RuralNotableNotary)
                    {
                        randMin = MinWorkAge;
                        randMax = ageModel.MaxAge;
                        agentData.IsFemale(MBRandom.RandomFloat <
                            (D225MiscFixesSettings.Instance != null ? D225MiscFixesSettings.Instance.WorkerGenderRatio : WorkerGenderRatio));
                    }
                    else if (character == culture.Townsman || character == culture.Townswoman || character == culture.Villager
                        || character == culture.VillageWoman || character == culture.TavernWench)
                    {
                        randMin = MinWorkAge;
                        randMax = ageModel.MaxAge;
                    }
                    else if (character == culture.FemaleDancer)
                    {
                        randMin = MinWorkAge;
                        randMax = ageModel.BecomeOldAge;
                    }
                    else if (character == culture.Beggar || character == culture.FemaleBeggar)
                    {
                        randMin = MinBegAge;
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
}