﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Behaviors.Specific;

internal static class GuardianAura
{
    private static readonly FeatureDefinitionPower DummyAuraGuardianPower = FeatureDefinitionPowerBuilder
        .Create("GuardianAura")
        .SetGuiPresentation(DatabaseHelper.SpellDefinitions.ShieldOfFaith.guiPresentation)
        .AddToDB();

    internal static readonly object AuraGuardianConditionMarker = new GuardianAuraCondition();
    internal static readonly object AuraGuardianUserMarker = new GuardianAuraUser();

    internal static IEnumerator ProcessOnCharacterAttackHitFinished(
        GameLocationBattleManager battleManager,
        GameLocationCharacter attacker,
        GameLocationCharacter defender,
        RulesetAttackMode attackerAttackMode,
        RulesetEffect rulesetEffect,
        int damageAmount)
    {
        if (Gui.Battle == null)
        {
            yield break;
        }

        var units = Gui.Battle
            .GetContenders(defender, isOppositeSide: false);

        foreach (var unit in units)
        {
            yield return ActiveHealthSwap(
                unit, attacker, defender, battleManager, attackerAttackMode, rulesetEffect, damageAmount);
        }
    }

    private static IEnumerator ActiveHealthSwap(
        [NotNull] GameLocationCharacter unit,
        [NotNull] GameLocationCharacter attacker,
        GameLocationCharacter defender,
        GameLocationBattleManager battleManager,
        RulesetAttackMode attackerAttackMode,
        RulesetEffect rulesetEffect,
        int damageAmount)
    {
        if (!unit.RulesetActor.HasSubFeatureOfType<GuardianAuraUser>()
            || !defender.RulesetActor.HasSubFeatureOfType<GuardianAuraCondition>())
        {
            yield break;
        }

        if (!unit.CanReact(true))
        {
            yield break;
        }

        if (defender.RulesetActor is not { IsDeadOrDyingOrUnconscious: false })
        {
            yield break;
        }

        if (damageAmount == 0)
        {
            yield break;
        }

        yield return unit.MyReactToDoNothing(
            ExtraActionId.DoNothingReaction,
            attacker,
            "GuardianAura",
            "CustomReactionGuardianAuraDescription".Formatted(Category.Reaction, defender.Name, damageAmount),
            ReactionValidated,
            battleManager: battleManager,
            resource: ReactionResourceChannelDivinity.Instance);

        yield break;

        void ReactionValidated()
        {
            DamageForm damage = null;

            if (attackerAttackMode != null)
            {
                damage = attackerAttackMode.EffectDescription.FindFirstDamageForm();
            }

            if (rulesetEffect != null)
            {
                damage = rulesetEffect.EffectDescription.FindFirstDamageForm();
            }

            defender.RulesetCharacter.ReceiveHealing(damageAmount, true, unit.Guid);
            defender.RulesetCharacter.ForceSetHealth(damageAmount, true);

            if (damage != null)
            {
                var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
                {
                    sourceCharacter = attacker.RulesetCharacter,
                    targetCharacter = unit.RulesetCharacter,
                    position = unit.LocationPosition
                };

                RulesetActor.InflictDamage(
                    damageAmount,
                    damage,
                    damage.DamageType,
                    applyFormsParams,
                    unit.RulesetCharacter,
                    false,
                    attacker.Guid,
                    false,
                    [],
                    new RollInfo(DieType.D1, [], damageAmount),
                    false,
                    out _);
            }

            unit.RulesetCharacter.LogCharacterUsedPower(DummyAuraGuardianPower, "Feedback/&GuardianAuraHeal");
        }
    }

    private sealed class GuardianAuraCondition;

    private sealed class GuardianAuraUser;
}
