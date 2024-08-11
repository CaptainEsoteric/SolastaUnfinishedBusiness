﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionCombatAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;

namespace SolastaUnfinishedBusiness.Subclasses;

//Paladin Oath inspired from 5e Oath of Vengeance
[UsedImplicitly]
public sealed class OathOfHatred : AbstractSubclass
{
    public OathOfHatred()
    {
        //
        // LEVEL 03
        //

        //Paladins subclass spells based off 5e Oath of Vengeance
        var autoPreparedSpellsHatred = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsHatred")
            .SetGuiPresentation("Subclass/&OathOfHatredTitle", "Feature/&DomainSpellsDescription")
            .SetAutoTag("Oath")
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, Bane, HuntersMark),
                BuildSpellGroup(5, HoldPerson, MistyStep),
                BuildSpellGroup(9, Haste, ProtectionFromEnergy),
                BuildSpellGroup(13, Banishment, DreadfulOmen),
                BuildSpellGroup(17, HoldMonster, DispelEvilAndGood))
            .SetSpellcastingClass(CharacterClassDefinitions.Paladin)
            .AddToDB();


        //Elevated Hate allowing at level 3 to select a favored foe
        var featureSetHatredElevatedHate = FeatureDefinitionFeatureSetBuilder
            .Create(FeatureDefinitionFeatureSets.AdditionalDamageRangerFavoredEnemyChoice,
                "FeatureSetHatredElevatedHate")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .AddToDB();

        //Hateful Gaze ability causing fear
        var powerHatredHatefulGaze = FeatureDefinitionPowerBuilder
            .Create("PowerHatredHatefulGaze")
            .SetGuiPresentation(Category.Feature, PowerSorcererHauntedSoulSpiritVisage)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ChannelDivinity)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFrightenedFear,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddToDB();

        var powerHatredScornfulPrayerFeature = FeatureDefinitionPowerBuilder
            .Create("PowerHatredScornfulPrayerFeature")
            .SetGuiPresentation(ConditionEnfeebled.GuiPresentation)
            .SetUsesFixed(ActivationTime.Action)
            .AddToDB();

        var conditionScornfulPrayer = ConditionDefinitionBuilder
            .Create(ConditionCursedByBestowCurseAttackRoll, "ConditionScornfulPrayer")
            .SetGuiPresentation(Category.Condition, ConditionCursedByBestowCurseAttackRoll)
            .AddFeatures(CombatAffinityEnfeebled)
            .AddFeatures(powerHatredScornfulPrayerFeature)
            .AddToDB();

        //Scornful Prayer cursing attack rolls and enfeebling the foe off a wisdom saving throw
        var powerHatredScornfulPrayer = FeatureDefinitionPowerBuilder
            .Create("PowerHatredScornfulPrayer")
            .SetGuiPresentation(Category.Feature, PowerMartialCommanderInvigoratingShout)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.ChannelDivinity)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionScornfulPrayer,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .SetDurationData(DurationType.Round, 3)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 12, TargetType.IndividualsUnique)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Wisdom,
                        true,
                        EffectDifficultyClassComputation.FixedValue,
                        AttributeDefinitions.Wisdom,
                        14)
                    .Build())
            .AddToDB();

        //
        // LEVEL 7
        //

        var conditionDauntlessPursuer = ConditionDefinitionBuilder
            .Create(ConditionCarriedByWind, "ConditionDauntlessPursuer")
            .SetGuiPresentation(Category.Condition, ConditionCarriedByWind)
            .AddFeatures(FeatureDefinitionMovementAffinitys.MovementAffinityCarriedByWind)
            .AddToDB();

        //Dauntless Pursuer being a carried by the wind that only processes on successful reaction hit
        var featureDauntlessPursuer = FeatureDefinitionBuilder
            .Create("FeatureHatredDauntlessPursuer")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(new PhysicalAttackFinishedByMeDauntlessPursuer(conditionDauntlessPursuer))
            .AddToDB();

        //
        // Level 15
        //

        // TODO: implement Soul of Vengeance instead
        var featureSetHatredResistance = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetHatredResistance")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionDamageAffinitys.DamageAffinityBludgeoningResistanceTrue,
                FeatureDefinitionDamageAffinitys.DamageAffinityPiercingResistanceTrue,
                FeatureDefinitionDamageAffinitys.DamageAffinitySlashingResistanceTrue)
            .AddToDB();

        //
        // Level 20
        //

        var savingThrowAffinityHatredArdentHate = FeatureDefinitionSavingThrowAffinityBuilder
            .Create("SavingThrowAffinityHatredArdentHate")
            .SetGuiPresentation("PowerHatredArdentHate", Category.Feature)
            .SetAffinities(CharacterSavingThrowAffinity.Advantage, false,
                AttributeDefinitions.Strength,
                AttributeDefinitions.Dexterity,
                AttributeDefinitions.Constitution,
                AttributeDefinitions.Intelligence,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Charisma)
            .AddToDB();

        var conditionHatredArdentHate = ConditionDefinitionBuilder
            .Create("ConditionHatredArdentHate")
            .SetGuiPresentation(Category.Condition, ConditionDispellingEvilAndGood)
            .SetPossessive()
            .AddFeatures(savingThrowAffinityHatredArdentHate)
            .AddToDB();

        var powerHatredArdentHate = FeatureDefinitionPowerBuilder
            .Create("PowerHatredArdentHate")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerArdentHate", Resources.PowerArdentHate, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetDurationData(DurationType.Minute, 1)
                    .SetParticleEffectParameters(PowerFighterActionSurge)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionHatredArdentHate, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddToDB();

        conditionHatredArdentHate.AddCustomSubFeatures(new CustomBehaviorArdentHate(powerHatredArdentHate));

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("OathOfHatred")
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite("OathOfHatred", Resources.OathOfHatred, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpellsHatred,
                featureSetHatredElevatedHate,
                powerHatredHatefulGaze,
                powerHatredScornfulPrayer)
            .AddFeaturesAtLevel(7, featureDauntlessPursuer)
            .AddFeaturesAtLevel(15, featureSetHatredResistance)
            .AddFeaturesAtLevel(20, powerHatredArdentHate)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Paladin;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice => FeatureDefinitionSubclassChoices
        .SubclassChoicePaladinSacredOaths;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private sealed class PhysicalAttackFinishedByMeDauntlessPursuer(
        ConditionDefinition conditionDauntlessPursuerAfterAttack) : IPhysicalAttackFinishedByMe
    {
        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            if (rollOutcome is RollOutcome.Failure or RollOutcome.CriticalFailure)
            {
                yield break;
            }

            if (action.ActionType is not ActionDefinitions.ActionType.Reaction ||
                attackMode.AttackTags.Contains(AttacksOfOpportunity.NotAoOTag))
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            rulesetAttacker.InflictCondition(
                conditionDauntlessPursuerAfterAttack.Name,
                DurationType.Round,
                1,
                TurnOccurenceType.StartOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetAttacker.guid,
                rulesetAttacker.CurrentFaction.Name,
                1,
                conditionDauntlessPursuerAfterAttack.Name,
                0,
                0,
                0);
        }
    }

    private sealed class CustomBehaviorArdentHate(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatureDefinitionPower power)
        : IModifyDamageAffinity, ITryAlterOutcomeAttack
    {
        public void ModifyDamageAffinity(RulesetActor defender, RulesetActor attacker, List<FeatureDefinition> features)
        {
            features.RemoveAll(x =>
                x is IDamageAffinityProvider { DamageAffinityType: DamageAffinityType.Resistance });
        }

        public int HandlerPriority => -10;

        public IEnumerator OnTryAlterOutcomeAttack(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect)
        {
            if (action.AttackRollOutcome is not (RollOutcome.Failure or RollOutcome.CriticalFailure) ||
                helper != attacker ||
                !helper.OncePerTurnIsValid(power.Name))
            {
                yield break;
            }

            var guiAttacker = new GuiCharacter(attacker);
            var guiDefender = new GuiCharacter(defender);

            yield return attacker.MyReactToDoNothing(
                ExtraActionId.DoNothingFree,
                attacker,
                "HatredArdentHate",
                "CustomReactionHatredArdentHateDescription".Formatted(
                    Category.Reaction, guiAttacker.Name, guiDefender.Name),
                ReactionValidated,
                battleManager: battleManager);

            yield break;

            void ReactionValidated()
            {
                attacker.UsedSpecialFeatures.TryAdd(power.Name, 1);

                var delta = -action.AttackSuccessDelta;

                action.AttackRollOutcome = RollOutcome.Success;
                action.AttackSuccessDelta = 0;
                action.AttackRoll += delta;
                attackModifier.AttackRollModifier += delta;
                attackModifier.AttacktoHitTrends.Add(new TrendInfo(delta, FeatureSourceType.Power, power.Name, power));
            }
        }
    }
}
