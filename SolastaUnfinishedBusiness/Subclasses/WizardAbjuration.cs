﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Interfaces;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDamageAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSavingThrowAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSubclassChoices;

using Resources = SolastaUnfinishedBusiness.Properties.Resources;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class WizardAbjuration : AbstractSubclass
{
    private const string Name = "WizardAbjuration";

    public WizardAbjuration()
    {
        // Lv.2 Abjuration Savant
        var magicAffinityAbjurationScriber = FeatureDefinitionMagicAffinityBuilder
            .Create($"MagicAffinity{Name}Scriber")
            .SetGuiPresentation($"MagicAffinity{Name}Scriber", Category.Feature)
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .SetSpellLearnAndPrepModifiers(0.5f, 0.5f, 0, AdvantageType.Advantage,
                PreparedSpellsModifier.None)
            .AddToDB();
        

        // LV.2 Arcane Ward
        // initialize power point pool with INT mod points
        var powerArcaneWard = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}ArcaneWard")
            .SetGuiPresentation($"Power{Name}ArcaneWard", Category.Feature, GlobeOfInvulnerability.GuiPresentation.SpriteReference)
            .SetUsesAbilityBonus(ActivationTime.NoCost, RechargeRate.LongRest, AttributeDefinitions.Intelligence)
            .SetShowCasting(false)
            .AddToDB();

        // + 2 * Wiz Lv. points in the pool max
        powerArcaneWard.AddCustomSubFeatures(
            HasModifiedUses.Marker,
            new ModifyPowerPoolAmount
            {
                PowerPool = powerArcaneWard,
                Type = PowerPoolBonusCalculationType.ClassLevel,
                Attribute = WizardClass,
                Value = 2
            });

        // create the feature that actual reduces damage (before DamageAffinitys) based on the arcaneWardPointsPool
        var powerArcaneWardReduceDamage = FeatureDefinitionReduceDamageBuilder
            .Create($"Power{Name}ArcaneWardReduceDamage")
            .SetGuiPresentation(Category.Feature, $"Feature/&Power{Name}ArcaneWardDescription")
            .SetFeedbackPowerReducedDamage(
                (_, defender) =>
                    defender.RulesetCharacter.GetRemainingPowerUses(powerArcaneWard),
                powerArcaneWard)
            .AddToDB();

        // create a condition that gives powerArcaneWardReduceDamage feature to a creature
        var conditionArcaneWard = ConditionDefinitionBuilder
            .Create($"Condition{Name}ArcaneWard")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionShielded)
            .SetConditionType(ConditionType.Beneficial)
            .SetSpecialDuration(DurationType.UntilLongRest)
            .SetSilent(Silent.WhenRefreshedOrRemoved)
            .AddFeatures(powerArcaneWardReduceDamage)
            .SetPossessive()
            .AddToDB();

        // handle applying the condition or refunding points to the pool when casting Abjuration spells
        powerArcaneWard.AddCustomSubFeatures(new CustomBehaviorArcaneWard(powerArcaneWard, conditionArcaneWard));

        ////////
        // Lv.6 Projected Ward
        var conditionProjectedWard = ConditionDefinitionBuilder
            .Create($"Condition{Name}ProjectedWard")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionShielded)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetConditionType(ConditionType.Beneficial)
            .SetSpecialInterruptions((ConditionInterruption)ExtraConditionInterruption.AfterWasAttacked, ConditionInterruption.Damaged, ConditionInterruption.AnyBattleTurnEnd)
            .SetPossessive()
            .AddToDB();

        var powerProjectedWardReduceDamage = FeatureDefinitionReduceDamageBuilder
            .Create($"Power{Name}ProjectedWardReduceDamage")
            .SetGuiPresentationNoContent(true)
            .SetFeedbackPowerReducedDamage(
                (_, defender) => {
                    return defender.RulesetCharacter.TryGetConditionOfCategoryAndType(
                        AttributeDefinitions.TagEffect, conditionProjectedWard.Name, out var activeCondition)
                        ? EffectHelpers
                            .GetCharacterByGuid(activeCondition.SourceGuid)
                            .GetRemainingPowerUses(powerArcaneWard)
                        : 0;},
                powerArcaneWard,
                conditionProjectedWard)
            .AddToDB();

        conditionProjectedWard.Features.Add(powerProjectedWardReduceDamage);

        // Can only use when Arcane is both Active and has non-zero points remaining
        var powerProjectedWard = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}ProjectedWard")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .SetShowCasting(false)
            .AddToDB();

        powerProjectedWard.AddCustomSubFeatures(new CustomBehaviorProjectedWard(
                                                    powerArcaneWard,
                                                    conditionArcaneWard,
                                                    powerProjectedWard,
                                                    conditionProjectedWard));

        ////////
        // Lv.10 Improved Abjuration
        var magicAffinityCounterspellAffinity = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}CounterSpell")
            .SetGuiPresentationNoContent(true)
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .SetUsesFixed(ActivationTime.Reaction,RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Instantaneous, 1)
                .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.IndividualsUnique)
                .SetParticleEffectParameters(SpellDefinitions.Counterspell)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetCounterForm(CounterForm.CounterType.InterruptSpellcasting, 3, 10, true, true)
                        .Build())
                .Build())
            .AddToDB();

        var magicAffinityDispelMagicAffinity = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}CounterDispel")
            .SetGuiPresentationNoContent(true)
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Instantaneous, 1)
                .SetTargetingData(
                    Side.All,
                    RangeType.Distance, 24,
                    TargetType.IndividualsUnique, 1, 2,
                    ActionDefinitions.ItemSelectionType.Equiped)
                .SetParticleEffectParameters(SpellDefinitions.DispelMagic)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetCounterForm(CounterForm.CounterType.DissipateSpells, 3, 10, true, true)
                        .Build())
                .Build())
            .AddToDB();

        var featureSetImprovedAbjuration = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}ImprovedAbjuration")
            .SetGuiPresentation($"Power{Name}ImprovedAbjuration", Category.Feature)
            .SetFeatureSet(magicAffinityCounterspellAffinity, magicAffinityDispelMagicAffinity)
            .AddToDB();

        magicAffinityDispelMagicAffinity.EffectDescription.targetFilteringMethod = TargetFilteringMethod.CharacterGadgetEffectProxyItems;
        magicAffinityDispelMagicAffinity.EffectDescription.targetFilteringTag = TargetFilteringTag.No;

        ////////
        // Lv.14 Spell Resistance
        // Adv. on saves against magic
        var savingThrowAffinitySpellResistance = FeatureDefinitionSavingThrowAffinityBuilder
            .Create(SavingThrowAffinitySpellResistance, $"SavingThrowAffinity{Name}SpellResistance")
            .SetGuiPresentationNoContent(true)
            .AddToDB();

        
        // Resist damage from spells
        var conditionSpellResistance = ConditionDefinitionBuilder
            .Create($"Condition{Name}SpellResistance")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddFeatures(
                DamageAffinityAcidResistance,
                DamageAffinityBludgeoningResistanceTrue,
                DamageAffinityColdResistance,
                DamageAffinityFireResistance,
                DamageAffinityForceDamageResistance,
                DamageAffinityLightningResistance,
                DamageAffinityNecroticResistance,
                DamageAffinityPiercingResistanceTrue,
                DamageAffinityPoisonResistance,
                DamageAffinityPsychicResistance,
                DamageAffinityRadiantResistance,
                DamageAffinitySlashingResistanceTrue,
                DamageAffinityThunderResistance)
            .SetSpecialInterruptions(ConditionInterruption.Damaged)
            .AddToDB();

        var powerSpellResistance = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}SpellResistance")
            .SetGuiPresentation($"Power{Name}SpellResistance", Category.Feature, Sprites.GetSprite("CircleOfMagicalNegation", Resources.CircleOfMagicalNegation, 128))
            .SetUsesFixed(ActivationTime.Permanent)
            .AddCustomSubFeatures(new MagicEffectBeforeHitConfirmedOnMeSpellResistance(conditionSpellResistance))
            .AddToDB();        

        var featureSetSpellResistance = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}SpellResistance")
            .SetGuiPresentation($"Power{Name}SpellResistance", Category.Feature)
            .SetFeatureSet(savingThrowAffinitySpellResistance, powerSpellResistance)
            .AddToDB();


        // Assemble the subclass
        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.WizardWarMagic, 256))
            .AddFeaturesAtLevel(2, magicAffinityAbjurationScriber, powerArcaneWard)
            .AddFeaturesAtLevel(6, powerProjectedWard)
            .AddFeaturesAtLevel(10, magicAffinityCounterspellAffinity, magicAffinityDispelMagicAffinity, featureSetImprovedAbjuration)
            .AddFeaturesAtLevel(14, featureSetSpellResistance)
            .AddToDB();
        
    }


    // Handle Behaviour related to Arcane Ward
    private sealed class CustomBehaviorArcaneWard(FeatureDefinitionPower arcaneWard, ConditionDefinition conditionArcaneWard)
        : IMagicEffectFinishedByMe
    {
        public IEnumerator OnMagicEffectFinishedByMe(CharacterAction action,
            GameLocationCharacter attacker,
            List<GameLocationCharacter> targets)
        {
            if (action is not CharacterActionCastSpell actionCastSpell ||
                actionCastSpell.Countered ||
                actionCastSpell.ExecutionFailed)
            {
                yield break;
            }

            var rulesetCharacter = attacker.RulesetCharacter;
            var spellCast = actionCastSpell.ActiveSpell;            

            if (GetDefinition<SchoolOfMagicDefinition>(spellCast.SchoolOfMagic) == SchoolOfMagicDefinitions.SchoolAbjuration)
            {
                if (rulesetCharacter.HasConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect, conditionArcaneWard.Name))
                {
                    // if ward already exists, update pool
                    var usablePowerArcaneWard = PowerProvider.Get(arcaneWard, rulesetCharacter);
                    rulesetCharacter.UpdateUsageForPowerPool(-2 * spellCast.EffectLevel, usablePowerArcaneWard);
                }
                else
                {
                    // if no ward condition, add condition (which should last until long rest)
                    rulesetCharacter.AddConditionOfCategory(
                        AttributeDefinitions.TagEffect,
                        RulesetCondition.CreateCondition(rulesetCharacter.Guid, conditionArcaneWard), refresh:false);
                }

            }
        }
    }

    private sealed class CustomBehaviorProjectedWard(FeatureDefinitionPower arcaneWard, ConditionDefinition conditionArcaneWard, FeatureDefinitionPower projectedWard, ConditionDefinition conditionProjectedWard)
        : ITryAlterOutcomeAttack, ITryAlterOutcomeSavingThrow
    {
        public int HandlerPriority => 25;

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
            if (action.AttackRollOutcome == RollOutcome.Failure)
            {
                yield break;
            }

            yield return HandleReactionProjectedWard(battleManager, attacker, defender, helper);
        }

        public IEnumerator OnTryAlterOutcomeSavingThrow(
            GameLocationBattleManager battleManager,
            [CanBeNull] GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            SavingThrowData savingThrowData,
            bool hasHitVisual)
        {
            /*
            if (//Check if save was failed and effect causes damage
                //Check if save was successful or critically successful and effect still causes damage
                //**A)** ||
                //(SavingThrowData.SaveOutcome == RollOutcome.Failure && **B)**)
                )
            {
                yield break;
            }
            */

            // any reaction within a saving flow must use the yielder as waiter
            yield return HandleReactionProjectedWard(battleManager, helper, defender, helper);
        }

        private IEnumerator HandleReactionProjectedWard(
            GameLocationBattleManager battleManager,
            [CanBeNull] GameLocationCharacter waiter,
            GameLocationCharacter defender,
            GameLocationCharacter helper)
        {
            var rulesetHelper = helper.RulesetCharacter;
            var usableArcaneWard = PowerProvider.Get(arcaneWard, rulesetHelper);

            if (defender == helper ||
                !helper.CanReact() ||
                helper.IsOppositeSide(defender.Side) ||
                !helper.IsWithinRange(defender, 6) ||
                !helper.CanPerceiveTarget(defender) ||
                !rulesetHelper.HasConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect, conditionArcaneWard.Name) ||
                rulesetHelper.GetRemainingUsesOfPower(usableArcaneWard) == 0)
            {
                yield break;
            }
            
            var usableProjectedWard = PowerProvider.Get(projectedWard, rulesetHelper);

            // any reaction within an attack flow must use the attacker as waiter
            yield return helper.MyReactToSpendPower(
                usableProjectedWard,
                waiter,
                "ProjectedWard",
                "SpendPowerProjectedWardDescription".Formatted(
                    Category.Reaction, defender.Name, helper.Name),
                ReactionValidated,
                battleManager: battleManager
                );

            yield break;

            void ReactionValidated()
            {
                helper.SpendActionType(ActionDefinitions.ActionType.Reaction);
                var activeCondition = RulesetCondition.CreateCondition(defender.Guid, conditionProjectedWard);
                activeCondition.sourceGuid = helper.RulesetCharacter.Guid;

                defender.RulesetCharacter.AddConditionOfCategory(
                        AttributeDefinitions.TagEffect,
                        activeCondition, refresh:false);

                helper.RulesetCharacter.LogCharacterUsedPower(
                    projectedWard,
                    "Feedback/&ProjectedWard",
                    extra:
                        [
                             (ConsoleStyleDuplet.ParameterType.Player, defender.Name)
                        ] );
            }
        }
    }

    private sealed class MagicEffectBeforeHitConfirmedOnMeSpellResistance(ConditionDefinition conditionSpellResistance)
        : IMagicEffectBeforeHitConfirmedOnMe
    {
        public IEnumerator OnMagicEffectBeforeHitConfirmedOnMe(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier actionModifier,
            RulesetEffect rulesetEffect,
            List<EffectForm> actualEffectForms,
            bool firstTarget,
            bool criticalHit)
        {
            var rulesetDefender = defender.RulesetCharacter;

            rulesetDefender.InflictCondition(
                conditionSpellResistance.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetDefender.guid,
                rulesetDefender.CurrentFaction.Name,
                14,
                conditionSpellResistance.Name,
                0,
                0,
                0);

            yield break;
        }
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Wizard;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice => SubclassChoiceWizardArcaneTraditions;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }
}

