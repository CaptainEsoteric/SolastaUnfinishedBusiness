﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class RangerHellWalker : AbstractSubclass
{
    private const string Name = "RangerHellWalker";

    public RangerHellWalker()
    {
        // LEVEL 03

        // Hellwalker Magic

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetAutoTag("Ranger")
            .SetSpellcastingClass(CharacterClassDefinitions.Ranger)
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, HellishRebuke),
                BuildSpellGroup(5, Invisibility),
                BuildSpellGroup(9, BestowCurse),
                BuildSpellGroup(13, WallOfFire),
                BuildSpellGroup(17, SpellsContext.FarStep))
            .AddToDB();

        var powerFirebolt = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}Firebolt")
            .SetGuiPresentation(SpellsContext.EnduringSting.GuiPresentation)
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, additionalDicePerIncrement: 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 1, 1, 6, 5)
                            .SetDamageForm(DamageTypeNecrotic, 1, DieType.D4)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.FallProne)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .Build())
                    .SetParticleEffectParameters(SpellsContext.EnduringSting)
                    .Build())
            .AddToDB();

        var featureSetFirebolt = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Firebolt")
            .SetGuiPresentationNoContent(true)
            .AddFeatureSet(powerFirebolt)
            .AddToDB();

        // Damning Strike

        var conditionDammingStrike = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionOnFire, $"Condition{Name}DammingStrike")
            .SetParentCondition(ConditionDefinitions.ConditionOnFire)
            .SetFeatures()
            .AddToDB();

        var additionalDamageDammingStrike = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}DammingStrike")
            .SetGuiPresentation(Category.Feature)
            .SetRequiredProperty(RestrictedContextRequiredProperty.Weapon)
            .SetAttackModeOnly()
            .SetSavingThrowData()
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetIgnoreCriticalDoubleDice(true)
            .AddConditionOperation(
                new ConditionOperationDescription
                {
                    operation = ConditionOperationDescription.ConditionOperation.Add,
                    conditionDefinition = conditionDammingStrike,
                    canSaveToCancel = true,
                    hasSavingThrow = true,
                    saveAffinity = EffectSavingThrowType.Negates,
                    saveOccurence = TurnOccurenceType.StartOfTurn
                })
            .AddToDB();

        // Cursed Tongue

        var proficiencyCursedTongue = FeatureDefinitionProficiencyBuilder
            .Create($"Proficiency{Name}CursedTongue")
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(ProficiencyType.Language, "Language_Abyssal", "Language_Infernal")
            .AddToDB();

        // LEVEL 07

        // Burning Constitution

        var featureSetBurningConstitution = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}BurningConstitution")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionDamageAffinitys.DamageAffinityFireResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityNecroticResistance)
            .AddToDB();

        // LEVEL 11

        // Mark of the Dammed

        var conditionMarkOfTheDammed = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionFrightened, $"Condition{Name}MarkOfTheDammed")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetParentCondition(ConditionDefinitions.ConditionFrightened)
            .SetPossessive()
            .SetFeatures()
            .SetRecurrentEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeNecrotic, 1, DieType.D6)
                    .SetCreatedBy()
                    .Build())
            .AddToDB();

        var powerMarkOfTheDammed = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}MarkOfTheDammed")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("MarkOfTheDammed", Resources.PowerMarkOfTheDammed, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionMarkOfTheDammed, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(new CustomBehaviorMarkOfTheDammed(conditionMarkOfTheDammed))
            .AddToDB();

        conditionDammingStrike.AddCustomSubFeatures(
            new OnConditionAddedOrRemovedDammingStrike(conditionMarkOfTheDammed));

        // LEVEL 15

        // Fiendish Spawn

        var fiendMonsters = new List<MonsterDefinition>
        {
            MonsterDefinitions.Hezrou_MonsterDefinition, MonsterDefinitions.Marilith_MonsterDefinition
        };

        var powerFiendishSpawnPool = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}FiendishSpawn")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerFiendishSpawn", Resources.PowerFiendishSpawn, 256, 128))
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .AddToDB();

        var powerFiendishSpawnList = fiendMonsters
            .Select(monsterDefinition => new
            {
                monsterDefinition, monsterName = monsterDefinition.Name.Replace("_MonsterDefinition", string.Empty)
            })
            .Select(t => new
            {
                t,
                newMonsterDefinition = MonsterDefinitionBuilder
                    .Create(t.monsterDefinition, $"Monster{t.monsterName}")
                    .SetOrUpdateGuiPresentation(Category.Monster)
                    .SetDefaultFaction(FactionDefinitions.Party)
                    .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
                    .SetDungeonMakerPresence(MonsterDefinition.DungeonMaker.None)
                    .SetFullyControlledWhenAllied(true)
                    .SetDroppedLootDefinition(null)
                    .AddToDB()
            })
            .Select(
                t => FeatureDefinitionPowerSharedPoolBuilder
                    .Create($"Power{Name}FiendishSpawn{t.t.monsterName}")
                    .SetGuiPresentation(t.newMonsterDefinition.GuiPresentation)
                    .SetSharedPool(ActivationTime.Action, powerFiendishSpawnPool)
                    .SetEffectDescription(
                        EffectDescriptionBuilder
                            .Create()
                            .SetDurationData(DurationType.Minute, 1)
                            .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.Position)
                            .SetEffectForms(
                                EffectFormBuilder
                                    .Create()
                                    .SetSummonCreatureForm(1, t.newMonsterDefinition.Name)
                                    .Build())
                            .Build())
                    .AddToDB())
            .Cast<FeatureDefinitionPower>()
            .ToList();

        PowerBundle.RegisterPowerBundle(powerFiendishSpawnPool, true, powerFiendishSpawnList);

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.RangerHellWalker, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpells,
                featureSetFirebolt,
                additionalDamageDammingStrike,
                proficiencyCursedTongue)
            .AddFeaturesAtLevel(7,
                featureSetBurningConstitution)
            .AddFeaturesAtLevel(11,
                powerMarkOfTheDammed)
            .AddFeaturesAtLevel(15,
                powerFiendishSpawnPool)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Ranger;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRangerArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // DammingStrike
    //

    private sealed class OnConditionAddedOrRemovedDammingStrike(
        ConditionDefinition conditionMarkOfTheDammed) : IOnConditionAddedOrRemoved
    {
        public void OnConditionAdded(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            // empty
        }

        // should only remove the condition from the same source
        public void OnConditionRemoved(RulesetCharacter target, RulesetCondition rulesetCondition)
        {
            if (target.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect, conditionMarkOfTheDammed.Name, out var activeCondition) &&
                activeCondition.SourceGuid == rulesetCondition.SourceGuid)
            {
                target.RemoveCondition(activeCondition);
            }
        }
    }

    //
    // Mark of the Dammed
    //

    private sealed class CustomBehaviorMarkOfTheDammed(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionDefinition)
        : IModifyDamageAffinity, IPowerOrSpellFinishedByMe, IFilterTargetingCharacter
    {
        public bool EnforceFullSelection => false;

        public bool IsValid(CursorLocationSelectTarget __instance, GameLocationCharacter target)
        {
            if (target.RulesetCharacter == null)
            {
                return false;
            }

            var isValid = target.RulesetCharacter.HasConditionOfType("ConditionRangerHellWalkerDammingStrike");

            if (!isValid)
            {
                __instance.actionModifier.FailureFlags.Add("Tooltip/&MustHaveDammingStrikeCondition");
            }

            return isValid;
        }

        public void ModifyDamageAffinity(RulesetActor defender, RulesetActor attacker, List<FeatureDefinition> features)
        {
            if (!attacker.HasConditionOfType(conditionDefinition.Name))
            {
                return;
            }

            features.RemoveAll(x =>
                x is IDamageAffinityProvider
                {
                    DamageAffinityType: DamageAffinityType.Immunity, DamageType: DamageTypeFire
                });
        }

        public IEnumerator OnPowerOrSpellFinishedByMe(CharacterActionMagicEffect action, BaseDefinition power)
        {
            if (Gui.Battle == null)
            {
                yield break;
            }

            var gameLocationDefender = action.actionParams.targetCharacters[0];

            // remove this condition from all other enemies
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var gameLocationCharacter in Gui.Battle
                         .GetContenders(gameLocationDefender, isOppositeSide: false))
            {
                var rulesetDefender = gameLocationCharacter.RulesetCharacter;

                // should only check the condition from the same source
                if (rulesetDefender.TryGetConditionOfCategoryAndType(
                        AttributeDefinitions.TagEffect, conditionDefinition.Name, out var activeCondition) &&
                    activeCondition.SourceGuid == action.ActingCharacter.Guid)
                {
                    rulesetDefender.RemoveCondition(activeCondition);
                }
            }
        }
    }
}
