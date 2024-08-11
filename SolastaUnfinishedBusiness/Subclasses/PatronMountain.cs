﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public class PatronMountain : AbstractSubclass
{
    private const string Name = "PatronMountain";

    public PatronMountain()
    {
        // LEVEL 01

        var spellList = SpellListDefinitionBuilder
            .Create(SpellListDefinitions.SpellListWizard, $"SpellList{Name}")
            .SetGuiPresentationNoContent(true)
            .ClearSpells()
            .SetSpellsAtLevel(1, SpellsContext.EarthTremor, Sleep)
            .SetSpellsAtLevel(2, HeatMetal, LesserRestoration)
            .SetSpellsAtLevel(3, ProtectionFromEnergy, SleetStorm)
            .SetSpellsAtLevel(4, FreedomOfMovement, IceStorm)
            .SetSpellsAtLevel(5, ConeOfCold, GreaterRestoration)
            .FinalizeSpells(true, 9)
            .AddToDB();

        var magicAffinityExpandedSpells = FeatureDefinitionMagicAffinityBuilder
            .Create($"MagicAffinity{Name}ExpandedSpells")
            .SetGuiPresentation("MagicAffinityPatronExpandedSpells", Category.Feature)
            .SetExtendedSpellList(spellList)
            .AddToDB();

        // Barrier of Stone

        var reduceDamageBarrierOfStone = FeatureDefinitionReduceDamageBuilder
            .Create($"ReduceDamage{Name}BarrierOfStone")
            .SetGuiPresentation($"Power{Name}BarrierOfStone", Category.Feature)
            .SetAlwaysActiveReducedDamage((_, defender) =>
            {
                var rulesetCharacter = defender.RulesetActor;

                if (rulesetCharacter is not { IsDeadOrDyingOrUnconscious: false })
                {
                    return 0;
                }

                if (!rulesetCharacter.TryGetConditionOfCategoryAndType(
                        AttributeDefinitions.TagEffect, $"Condition{Name}BarrierOfStone",
                        out var activeCondition))
                {
                    return 0;
                }

                var rulesetCaster = EffectHelpers.GetCharacterByGuid(activeCondition.SourceGuid);

                if (rulesetCaster == null)
                {
                    return 0;
                }

                var levels = rulesetCaster.GetClassLevel(CharacterClassDefinitions.Warlock);
                var charismaModifier = AttributeDefinitions.ComputeAbilityScoreModifier(
                    rulesetCaster.TryGetAttributeValue(AttributeDefinitions.Charisma));

                return (2 * levels) + charismaModifier;
            })
            .AddToDB();

        var conditionBarrierOfStone = ConditionDefinitionBuilder
            .Create($"Condition{Name}BarrierOfStone")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(reduceDamageBarrierOfStone)
            .SetSpecialInterruptions(ExtraConditionInterruption.AfterWasAttacked)
            .AddToDB();

        var powerBarrierOfStone = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}BarrierOfStone")
            .SetGuiPresentation(Category.Feature)
            .SetUsesAbilityBonus(ActivationTime.NoCost, RechargeRate.LongRest, AttributeDefinitions.Charisma)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 0, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionBarrierOfStone))
                    .SetParticleEffectParameters(Banishment)
                    .Build())
            .AddToDB();

        // Knowledge of Aeons

        var proficiencyNatureSurvival = FeatureDefinitionProficiencyBuilder
            .Create($"Proficiency{Name}NatureSurvival")
            .SetGuiPresentationNoContent(true)
            .SetProficiencies(ProficiencyType.Skill, SkillDefinitions.Nature, SkillDefinitions.Survival)
            .AddToDB();

        var featureSetKnowledgeOfAeons = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}KnowledgeOfAeons")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                proficiencyNatureSurvival,
                FeatureDefinitionTerrainTypeAffinitys.TerrainTypeAffinityRangerNaturalExplorerMountain)
            .AddToDB();

        // LEVEL 06

        // Clinging Strength

        var conditionClingingStrength = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionLongstrider, $"Condition{Name}ClingingStrength")
            .SetGuiPresentation($"Power{Name}ClingingStrength", Category.Feature, Gui.EmptyContent,
                ConditionDefinitions.ConditionLongstrider.GuiPresentation.SpriteReference)
            .AddFeatures(FeatureDefinitionMovementAffinitys.MovementAffinitySpiderClimb)
            .AddToDB();

        var powerClingingStrength = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}ClingingStrength")
            .SetGuiPresentation(Category.Feature, Sprites.GetSprite(Name, Resources.PowerClingeStrength, 128, 64))
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Hour, 1)
                    .SetTargetingData(Side.Ally, RangeType.Touch, 0, TargetType.IndividualsUnique)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionClingingStrength))
                    .Build())
            .AddToDB();

        // Eternal Guardian

        var powerEternalGuardian = FeatureDefinitionPowerBuilder
            .Create(powerBarrierOfStone, $"Power{Name}EternalGuardian")
            .SetGuiPresentation(Category.Feature, hidden: true)
            .SetUsesAbilityBonus(ActivationTime.NoCost, RechargeRate.ShortRest, AttributeDefinitions.Charisma)
            .SetOverriddenPower(powerBarrierOfStone)
            .AddToDB();

        powerBarrierOfStone.AddCustomSubFeatures(
            ModifyPowerVisibility.Hidden,
            new CustomBehaviorBarrierOfStone(powerBarrierOfStone, powerEternalGuardian));

        var featureSetEternalGuardian = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}EternalGuardian")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(powerEternalGuardian)
            .AddToDB();

        // LEVEL 10

        // The Mountain Wakes

        var powerTheMountainWakes = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}TheMountainWakes")
            .SetGuiPresentation(Category.Feature, IceStorm)
            .SetUsesProficiencyBonus(ActivationTime.Action)
            .SetEffectDescription(IceStorm.EffectDescription)
            .AddToDB();

        // LEVEL 14

        // Icebound Soul

        var additionalDamageIceboundSoul = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}IceboundSoul")
            .SetGuiPresentation($"FeatureSet{Name}IceboundSoul", Category.Feature)
            .SetFrequencyLimit(FeatureLimitedUsage.OnceInMyTurn)
            .SetAttackOnly()
            .SetSavingThrowData()
            .AddConditionOperation(new ConditionOperationDescription
            {
                operation = ConditionOperationDescription.ConditionOperation.Add,
                conditionDefinition = ConditionDefinitions.ConditionBlindedEndOfNextTurn,
                hasSavingThrow = true,
                saveAffinity = EffectSavingThrowType.Negates
            })
            .AddToDB();

        var featureSetIceboundSoul = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}IceboundSoul")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                additionalDamageIceboundSoul,
                FeatureDefinitionDamageAffinitys.DamageAffinityColdImmunity)
            .AddToDB();

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PatronMountain, 256))
            .AddFeaturesAtLevel(1,
                magicAffinityExpandedSpells,
                featureSetKnowledgeOfAeons,
                powerBarrierOfStone)
            .AddFeaturesAtLevel(6,
                powerClingingStrength,
                featureSetEternalGuardian)
            .AddFeaturesAtLevel(10,
                powerTheMountainWakes)
            .AddFeaturesAtLevel(14,
                featureSetIceboundSoul)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Warlock;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceWarlockOtherworldlyPatrons;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private class CustomBehaviorBarrierOfStone(
        FeatureDefinitionPower powerBarrierOfStone,
        FeatureDefinitionPower powerEternalGuardian) : ITryAlterOutcomeAttack
    {
        public int HandlerPriority => 20;

        public IEnumerator OnTryAlterOutcomeAttack(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier actionModifier,
            RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect)
        {
            var rulesetHelper = helper.RulesetCharacter;
            var levels = rulesetHelper.GetClassLevel(CharacterClassDefinitions.Warlock);
            var power = levels < 6 ? powerBarrierOfStone : powerEternalGuardian;
            var usablePower = PowerProvider.Get(power, rulesetHelper);

            if (helper == defender ||
                helper.IsOppositeSide(defender.Side) ||
                !helper.CanReact() ||
                !helper.CanPerceiveTarget(attacker) ||
                !helper.CanPerceiveTarget(defender) ||
                !helper.IsWithinRange(defender, 7) ||
                rulesetHelper.GetRemainingUsesOfPower(usablePower) == 0)
            {
                yield break;
            }

            yield return helper.MyReactToUsePower(
                ActionDefinitions.Id.PowerReaction,
                usablePower,
                [defender],
                attacker,
                "BarrierOfStone",
                battleManager: battleManager);
        }
    }
}
