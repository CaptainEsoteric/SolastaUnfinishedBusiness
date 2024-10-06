﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Interception : AbstractFightingStyle
{
    private const string Name = "Interception";

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create(Name)
        .SetGuiPresentation(Category.FightingStyle, Sprites.GetSprite(Name, Resources.Interception, 256))
        .SetFeatures(
            FeatureDefinitionPowerBuilder
                .Create($"Power{Name}")
                .SetGuiPresentation(Name, Category.FightingStyle)
                .SetUsesFixed(ActivationTime.NoCost)
                .AddCustomSubFeatures(
                    ModifyPowerVisibility.Hidden,
                    new CustomBehaviorInterception(
                        ConditionDefinitionBuilder
                            .Create($"Condition{Name}")
                            .SetGuiPresentationNoContent(true)
                            .SetSilent(Silent.WhenAddedOrRemoved)
                            .SetSpecialInterruptions(ExtraConditionInterruption.AfterWasAttacked)
                            .SetAmountOrigin(ConditionDefinition.OriginOfAmount.Fixed)
                            .AddFeatures(
                                FeatureDefinitionReduceDamageBuilder
                                    .Create($"ReduceDamage{Name}")
                                    .SetGuiPresentation(Name, Category.FightingStyle)
                                    .SetAlwaysActiveReducedDamage(
                                        (_, defender) =>
                                            defender.RulesetActor.ConditionsByCategory
                                                .SelectMany(x => x.Value)
                                                .FirstOrDefault(
                                                    x => x.ConditionDefinition.Name == $"Condition{Name}")!.Amount)
                                    .AddToDB())
                            .AddToDB()))
                .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice =>
    [
        CharacterContext.FightingStyleChoiceBarbarian,
        CharacterContext.FightingStyleChoiceMonk,
        CharacterContext.FightingStyleChoiceRogue,
        FightingStyleChampionAdditional,
        FightingStyleFighter,
        FightingStylePaladin,
        FightingStyleRanger
    ];

    private sealed class CustomBehaviorInterception(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionDefinition) : ITryAlterOutcomeAttack
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
            if (helper == defender ||
                helper.IsOppositeSide(defender.Side) ||
                !helper.CanReact() ||
                !helper.CanPerceiveTarget(defender) ||
                !helper.IsWithinRange(defender, 1))
            {
                yield break;
            }

            var rulesetHelper = helper.RulesetCharacter;
            var mainHand = rulesetHelper.GetMainWeapon();
            var offHand = rulesetHelper.GetOffhandWeapon();

            if (ValidatorsWeapon.IsUnarmed(mainHand) &&
                ValidatorsWeapon.IsUnarmed(offHand))
            {
                yield break;
            }

            yield return helper.MyReactToDoNothing(
                ExtraActionId.DoNothingReaction,
                attacker,
                Name,
                "CustomReactionInterceptionDescription".Formatted(Category.Reaction, defender.Name, attacker.Name),
                ReactionValidated,
                battleManager: battleManager);

            yield break;

            void ReactionValidated()
            {
                var roll = rulesetHelper.RollDie(DieType.D10, RollContext.None, true, AdvantageType.None, out _,
                    out _);
                var reducedDamage = roll + rulesetHelper.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);

                defender.RulesetActor.InflictCondition(
                    conditionDefinition.Name,
                    DurationType.Round,
                    0,
                    TurnOccurenceType.StartOfTurn,
                    AttributeDefinitions.TagEffect,
                    rulesetHelper.guid,
                    rulesetHelper.CurrentFaction.Name,
                    1,
                    conditionDefinition.Name,
                    reducedDamage,
                    0,
                    0);
            }
        }
    }
}
