using System.Collections.Generic;
using Designer225.MiscFixes.Util;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace Designer225.MiscFixes
{
    public class D225MiscFixesHeroCreationModel : HeroCreationModel
    {
        private readonly HeroCreationModel _baseModel;
        
        public D225MiscFixesHeroCreationModel(HeroCreationModel baseModel) => _baseModel = baseModel;
        
        public override (CampaignTime, CampaignTime) GetBirthAndDeathDay(CharacterObject character, bool createAlive,
            int age)
        {
            if (!createAlive || age == -1 || age == 0 || character.Occupation != Occupation.Wanderer)
                return _baseModel.GetBirthAndDeathDay(character, createAlive, age);
            age = Campaign.Current.Models.AgeModel.HeroComesOfAge +
                  MBRandom.RandomInt(Settings.Instance!.WanderSpawningRngMax);
            return (HeroHelper.GetRandomBirthDayForAge(age), CampaignTime.Never);
        }

        public override Settlement GetBornSettlement(Hero character) => _baseModel.GetBornSettlement(character);

        public override StaticBodyProperties GetStaticBodyProperties(Hero character, bool isOffspring,
            float variationAmount = 0.35f) =>
            _baseModel.GetStaticBodyProperties(character, isOffspring, variationAmount);

        public override FormationClass GetPreferredUpgradeFormation(Hero character) =>
            _baseModel.GetPreferredUpgradeFormation(character);

        public override Clan GetClan(Hero character) => _baseModel.GetClan(character);

        public override CultureObject GetCulture(Hero hero, Settlement bornSettlement, Clan clan) =>
            _baseModel.GetCulture(hero, bornSettlement, clan);

        public override CharacterObject GetRandomTemplateByOccupation(Occupation occupation,
            Settlement? settlement = null) => _baseModel.GetRandomTemplateByOccupation(occupation, settlement);

        public override List<(TraitObject trait, int level)> GetTraitsForHero(Hero hero) =>
            _baseModel.GetTraitsForHero(hero);

        public override Equipment GetCivilianEquipment(Hero hero) => _baseModel.GetCivilianEquipment(hero);

        public override Equipment GetBattleEquipment(Hero hero) => _baseModel.GetBattleEquipment(hero);

        public override CharacterObject GetCharacterTemplateForOffspring(Hero mother, Hero father,
            bool isOffspringFemale) => _baseModel.GetCharacterTemplateForOffspring(mother, father, isOffspringFemale);

        public override (TextObject firstName, TextObject name) GenerateFirstAndFullName(Hero hero) =>
            _baseModel.GenerateFirstAndFullName(hero);

        public override List<(SkillObject, int)> GetDefaultSkillsForHero(Hero hero) =>
            _baseModel.GetDefaultSkillsForHero(hero);

        public override List<(SkillObject, int)> GetInheritedSkillsForHero(Hero hero) =>
            _baseModel.GetInheritedSkillsForHero(hero);

        public override bool IsHeroCombatant(Hero hero) => _baseModel.IsHeroCombatant(hero);
    }
}