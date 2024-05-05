﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Interfaces;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttributeModifiers;

namespace SolastaUnfinishedBusiness.Feats;

internal static class CraftyFeats
{
    internal static void CreateFeats([NotNull] List<FeatDefinition> feats)
    {
        // skill

        var proficiencyCraftyArcana = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyArcana")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Skill, SkillDefinitions.Arcana)
            .AddToDB();

        var proficiencyCraftyAnimalHandling = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyAnimalHandling")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Skill, SkillDefinitions.AnimalHandling)
            .AddToDB();

        var proficiencyCraftyMedicine = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyMedicine")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Skill, SkillDefinitions.Medecine)
            .AddToDB();

        var proficiencyCraftyNature = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyNature")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Skill, SkillDefinitions.Nature)
            .AddToDB();

        var proficiencyCraftyHerbalismKit = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyHerbalismKit")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Tool, ToolTypeDefinitions.HerbalismKitType.Name)
            .AddToDB();

        var proficiencyCraftyPoisonersKit = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyPoisonersKit")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Tool, ToolTypeDefinitions.PoisonersKitType.Name)
            .AddToDB();

        var proficiencyCraftyScrollKit = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyScrollKit")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Tool, ToolTypeDefinitions.ScrollKitType.Name)
            .AddToDB();

        var proficiencyCraftyArcanaExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyArcanaExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, SkillDefinitions.Arcana)
            .AddToDB();

        var proficiencyCraftyAnimalHandlingExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyAnimalHandlingExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, SkillDefinitions.AnimalHandling)
            .AddToDB();

        var proficiencyCraftyMedicineExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyMedicineExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, SkillDefinitions.Medecine)
            .AddToDB();

        var proficiencyCraftyNatureExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyNatureExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, SkillDefinitions.Nature)
            .AddToDB();

        var proficiencyCraftyHerbalismKitExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyHerbalismKitExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, ToolTypeDefinitions.HerbalismKitType.Name)
            .AddToDB();

        var proficiencyCraftyPoisonersKitExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyPoisonersKitExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, ToolTypeDefinitions.PoisonersKitType.Name)
            .AddToDB();

        var proficiencyCraftyScrollKitExpertise = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyCraftyScrollKitExpertise")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Expertise, ToolTypeDefinitions.ScrollKitType.Name)
            .AddToDB();

        //
        // Apothecary
        //

        var featApothecaryInt = FeatDefinitionBuilder
            .Create("FeatApothecaryInt")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Pakri)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.HerbalismKitType, proficiencyCraftyHerbalismKit,
                    proficiencyCraftyHerbalismKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.Arcana, proficiencyCraftyArcana,
                    proficiencyCraftyArcanaExpertise))
            .SetFeatFamily("Apothecary")
            .AddToDB();

        var featApothecaryWis = FeatDefinitionBuilder
            .Create("FeatApothecaryWis")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Maraike)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.HerbalismKitType, proficiencyCraftyHerbalismKit,
                    proficiencyCraftyHerbalismKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.Medecine, proficiencyCraftyMedicine,
                    proficiencyCraftyMedicineExpertise))
            .SetFeatFamily("Apothecary")
            .AddToDB();

        var featApothecaryCha = FeatDefinitionBuilder
            .Create("FeatApothecaryCha")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Solasta)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.HerbalismKitType, proficiencyCraftyHerbalismKit,
                    proficiencyCraftyHerbalismKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.Medecine, proficiencyCraftyMedicine,
                    proficiencyCraftyMedicineExpertise))
            .SetFeatFamily("Apothecary")
            .AddToDB();

        var featGroupApothecary = GroupFeats.MakeGroup("FeatGroupApothecary", "Apothecary",
            featApothecaryInt,
            featApothecaryWis,
            featApothecaryCha);

        //
        // Toxicologist
        //

        var featToxicologistInt = FeatDefinitionBuilder
            .Create("FeatToxicologistInt")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Pakri)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.PoisonersKitType, proficiencyCraftyPoisonersKit,
                    proficiencyCraftyPoisonersKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.Nature, proficiencyCraftyNature,
                    proficiencyCraftyNatureExpertise))
            .SetFeatFamily("Toxicologist")
            .AddToDB();

        var featToxicologistWis = FeatDefinitionBuilder
            .Create("FeatToxicologistWis")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Maraike)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.PoisonersKitType, proficiencyCraftyPoisonersKit,
                    proficiencyCraftyPoisonersKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.Medecine, proficiencyCraftyMedicine,
                    proficiencyCraftyMedicineExpertise))
            .SetFeatFamily("Toxicologist")
            .AddToDB();

        var featToxicologistCha = FeatDefinitionBuilder
            .Create("FeatToxicologistCha")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Solasta)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.PoisonersKitType, proficiencyCraftyPoisonersKit,
                    proficiencyCraftyPoisonersKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.AnimalHandling, proficiencyCraftyAnimalHandling,
                    proficiencyCraftyAnimalHandlingExpertise))
            .SetFeatFamily("Toxicologist")
            .AddToDB();

        var featGroupToxicologist = GroupFeats.MakeGroup("FeatGroupToxicologist", "Toxicologist",
            featToxicologistInt,
            featToxicologistWis,
            featToxicologistCha);

        //
        // Scriber
        //

        var featCraftyScriber = FeatDefinitionBuilder
            .Create("FeatCraftyScriber")
            .SetGuiPresentation(Category.Feat)
            .SetMustCastSpellsPrerequisite()
            .SetFeatures(AttributeModifierCreed_Of_Pakri)
            .AddCustomSubFeatures(
                new ToolOrExpertise(ToolTypeDefinitions.ScrollKitType, proficiencyCraftyScrollKit,
                    proficiencyCraftyScrollKitExpertise),
                new SkillOrExpertise(DatabaseHelper.SkillDefinitions.Arcana, proficiencyCraftyArcana,
                    proficiencyCraftyArcanaExpertise))
            .AddToDB();

        //
        // MAIN
        //

        feats.AddRange(
            featApothecaryInt,
            featApothecaryWis,
            featApothecaryCha,
            featToxicologistInt,
            featToxicologistWis,
            featToxicologistCha,
            featCraftyScriber);

        GroupFeats.FeatGroupSkills.AddFeats(
            featGroupApothecary,
            featGroupToxicologist,
            featCraftyScriber);

        GroupFeats.FeatGroupTools.AddFeats(
            featGroupToxicologist,
            featCraftyScriber,
            FeatDefinitions.InitiateAlchemist,
            FeatDefinitions.MasterAlchemist,
            FeatDefinitions.InitiateEnchanter,
            FeatDefinitions.MasterEnchanter);
    }

    private sealed class SkillOrExpertise(
        SkillDefinition skillDefinition,
        FeatureDefinitionProficiency skill,
        FeatureDefinitionProficiency expertise) : ICustomLevelUpLogic
    {
        public void ApplyFeature(RulesetCharacterHero hero, string tag)
        {
            hero.ActiveFeatures[tag].TryAdd(hero.TrainedSkills.Contains(skillDefinition)
                ? expertise
                : skill);
        }

        public void RemoveFeature(RulesetCharacterHero hero, string tag)
        {
            // empty
        }
    }

    private sealed class ToolOrExpertise(
        ToolTypeDefinition toolTypeDefinition,
        FeatureDefinitionProficiency tool,
        FeatureDefinitionProficiency expertise) : ICustomLevelUpLogic
    {
        public void ApplyFeature(RulesetCharacterHero hero, string tag)
        {
            hero.ActiveFeatures[tag].TryAdd(hero.TrainedToolTypes.Contains(toolTypeDefinition)
                ? expertise
                : tool);
        }

        public void RemoveFeature(RulesetCharacterHero hero, string tag)
        {
            // empty
        }
    }
}
