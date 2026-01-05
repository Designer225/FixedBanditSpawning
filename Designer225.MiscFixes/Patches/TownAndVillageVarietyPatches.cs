using System;
using Designer225.MiscFixes.Util;
using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Designer225.MiscFixes.Patches
{
    public static class TownAndVillageVarietyPatches
    {
        public const int InfantAge = 3, ChildAge = 6, TweenAge = 10, TeenAge = 13, AdultAge = 16;
        
        [HarmonyPatch(typeof(LocationCharacter), MethodType.Constructor, typeof(AgentData),
            typeof(LocationCharacter.AddBehaviorsDelegate), typeof(string), typeof(bool),
            typeof(LocationCharacter.CharacterRelations), typeof(string), typeof(bool), typeof(bool),
            typeof(ItemObject), typeof(bool), typeof(bool), typeof(bool),
            typeof(LocationCharacter.AfterAgentCreatedDelegate), typeof(bool))]
        public static class LocationCharacterConstructorPatch
        {
            public static bool Prepare()
            {
                if (!Settings.Instance!.TownAndVillageVariety) return false;
                Debug.Print("[Designer225.MiscFixes] Adding a little bit variety to towns and villages...");
                return true;
            }

            public static void Prefix(AgentData agentData)
            {
                try
                {
                    var ageModel = Campaign.Current?.Models?.AgeModel;
                    if (ageModel == null) return;

                    var character = agentData.AgentCharacter;
                    if (character is CharacterObject characterObject)
                    {
                        var culture = characterObject.Culture;
                        if (culture == null) return;

                        var randMin = agentData.AgentAge;
                        var randMax = randMin;

                        // Rather spaghetti-ish, but should do better performance wise
                        if (characterObject == culture.Barber || characterObject == culture.ShopWorker ||
                            characterObject == culture.TavernGamehost || characterObject == culture.Tavernkeeper ||
                            characterObject == culture.Musician || characterObject == culture.Armorer ||
                            characterObject == culture.Blacksmith || characterObject == culture.HorseMerchant ||
                            characterObject == culture.Merchant || characterObject == culture.Weaponsmith ||
                            characterObject == culture.GangleaderBodyguard)
                        {
                            randMin = TeenAge;
                            randMax = ageModel.BecomeOldAge;
                            agentData.IsFemale(MBRandom.RandomFloat < Settings.Instance!.WorkerGenderRatio);
                        }
                        else if (characterObject == culture.ArtisanNotary || characterObject == culture.MerchantNotary ||
                                 characterObject == culture.PreacherNotary ||
                                 characterObject == culture.RuralNotableNotary || characterObject == culture.RansomBroker)
                        {
                            randMin = AdultAge;
                            randMax = ageModel.MaxAge;
                            agentData.IsFemale(MBRandom.RandomFloat < Settings.Instance!.WorkerGenderRatio);
                        }
                        else if (characterObject == culture.MeleeMilitiaTroop ||
                                 characterObject == culture.RangedMilitiaTroop)
                        {
                            randMin = TeenAge;
                            randMax = ageModel.BecomeOldAge;
                            agentData.IsFemale(MBRandom.RandomFloat < Settings.Instance!.WorkerGenderRatio);
                        }
                        else if (characterObject == culture.MeleeEliteMilitiaTroop ||
                                 characterObject == culture.RangedEliteMilitiaTroop)
                        {
                            randMin = AdultAge;
                            randMax = ageModel.BecomeOldAge;
                            agentData.IsFemale(MBRandom.RandomFloat < Settings.Instance!.WorkerGenderRatio);
                        }
                        else if (characterObject == culture.TavernWench || characterObject == culture.FemaleDancer)
                        {
                            randMin = TweenAge;
                            randMax = ageModel.BecomeOldAge;
                        }
                        else if (characterObject == culture.Townsman || characterObject == culture.Townswoman ||
                                 characterObject == culture.Villager || characterObject == culture.VillageWoman)
                        {
                            randMin = TweenAge;
                            randMax = ageModel.MaxAge;
                        }
                        else if (characterObject == culture.Beggar || characterObject == culture.FemaleBeggar)
                        {
                            randMin = ChildAge;
                            randMax = ageModel.MaxAge;
                        }
                        else if (characterObject == culture.TownsmanInfant || characterObject == culture.TownswomanInfant)
                        {
                            randMin = InfantAge;
                            randMax = ChildAge;
                        }
                        else if (characterObject == culture.TownsmanChild || characterObject == culture.TownswomanChild ||
                                 characterObject == culture.VillagerMaleChild ||
                                 characterObject == culture.VillagerFemaleChild)
                        {
                            randMin = ChildAge;
                            randMax = TeenAge;
                        }
                        else if (characterObject == culture.TownsmanTeenager ||
                                 characterObject == culture.TownswomanTeenager ||
                                 characterObject == culture.VillagerMaleTeenager ||
                                 characterObject == culture.VillagerFemaleTeenager)
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
                    Debug.Print($"[Designer225.MiscFixes] Error attempting to modify location character agent data.\n{e}");
                }
            }
        }

        [HarmonyPatch(typeof(CommonVillagersCampaignBehavior), "conversation_children_rhymes_on_condition")]
        public static class CommonVillagersCampaignBehaviorPatch
        {
            public static bool Prepare()
            {
                if (!Settings.Instance!.TownAndVillageVariety) return false;
                Debug.Print("[Designer225.MiscFixes] Replacing hardcoded dialogue checks...");
                return true;
            }

            // ReSharper disable once InconsistentNaming
            public static void Postfix(ref bool __result)
            {

                var character = Campaign.Current.ConversationManager.OneToOneConversationAgent.Character;
                if (character is CharacterObject characterObject)
                {
                    var culture = characterObject.Culture;
                    if (culture is null) return;
                    __result = characterObject == culture.TownsmanInfant || characterObject == culture.TownswomanInfant ||
                               characterObject == culture.TownsmanChild || characterObject == culture.TownswomanChild ||
                               characterObject == culture.VillagerMaleChild ||
                               characterObject == culture.VillagerFemaleChild;
                }
            }
        }

        [HarmonyPatch(typeof(HeroCreator), nameof(HeroCreator.CreateSpecialHero))]
        public static class CreateSpecialHeroPatch
        {
            public static bool Prepare()
            {
                if (!Settings.Instance!.TownAndVillageVariety) return false;
                Debug.Print("[Designer225.MiscFixes] Patching notable spawning...");
                return true;
            }
        
            // ReSharper disable once InconsistentNaming
            public static void Postfix(ref Hero __result)
            {
                var ageModel = Campaign.Current.Models.AgeModel;
                var baseAge = Math.Max(ageModel.HeroComesOfAge, TeenAge);
                if (__result.IsNotable)
                    __result.SetBirthDay(HeroHelper.GetRandomBirthDayForAge(
                        MathF.Lerp(baseAge, ageModel.MaxAge, (__result.Age - ageModel.HeroComesOfAge) / (ageModel.MaxAge - ageModel.HeroComesOfAge))));
            }
        }
    }
}