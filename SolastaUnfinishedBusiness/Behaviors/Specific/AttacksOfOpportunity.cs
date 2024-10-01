﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Validators;
using TA;
using static ActionDefinitions;

namespace SolastaUnfinishedBusiness.Behaviors.Specific;

internal static class AttacksOfOpportunity
{
    internal const string NotAoOTag = "NotAoO"; //Used to distinguish reaction attacks from AoO
    internal static readonly IIgnoreAoOImmunity IgnoreDisengage = new IgnoreDisengage();
    internal static readonly object SentinelFeatMarker = new SentinelFeatMarker();

    internal static IEnumerator ProcessOnCharacterAttackFinished(
        GameLocationBattleManager battleManager,
        GameLocationCharacter attacker,
        GameLocationCharacter defender)
    {
        if (Gui.Battle == null)
        {
            yield break;
        }

        //Process features on attacker or defender
        var actionManager = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

        var units = Gui.Battle.AllContenders
            .Where(u => u.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            .ToList(); // avoid changing enumerator

        //Process other participants of the battle
        foreach (var unit in units
                     .Where(unit => attacker != defender &&
                                    unit != attacker &&
                                    unit != defender &&
                                    defender.Side == unit.Side &&
                                    attacker.IsOppositeSide(unit.Side) &&
                                    unit.IsWithinRange(attacker, 1)))
        {
            foreach (var reaction in unit.RulesetCharacter.GetSubFeaturesByType<SentinelFeatMarker>()
                         .Where(feature => feature.IsValid(unit, attacker)))
            {
                yield return reaction.Process(unit, attacker, null, battleManager, actionManager, false);
            }
        }
    }

    internal static IEnumerator ProcessOnCharacterMoveEnd(GameLocationCharacter mover)
    {
        if (Gui.Battle == null ||
            mover.RulesetCharacter is not { IsDeadOrDyingOrUnconscious: false })
        {
            yield break;
        }

        var actionManager = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
        var battleManager = ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;
        var units = Gui.Battle.AllContenders
            .Where(u => u.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
            .ToList(); // avoid changing enumerator

        //Process other participants of the battle
        foreach (var unit in units)
        {
            if (mover == unit ||
                mover.Side == unit.Side ||
                !MovementTracker.TryGetMovement(mover.Guid, out var movement))
            {
                continue;
            }

            foreach (var canMakeAoOOnReachEntered in unit.RulesetActor.GetSubFeaturesByType<CanMakeAoOOnReachEntered>()
                         .Where(feature => feature.IsValid(unit, mover)))
            {
                yield return canMakeAoOOnReachEntered.Process(
                    unit, mover, movement, battleManager, actionManager, canMakeAoOOnReachEntered.AllowRange);
            }
        }
    }

    internal static bool IsSubjectToAttackOfOpportunity(
        RulesetCharacter defender,
        RulesetCharacter attacker,
        bool def,
        float distance)
    {
        return
            attacker.GetSubFeaturesByType<IIgnoreAoOImmunity>()
                .Any(f => f.CanIgnoreAoOImmunity(defender, attacker, distance)) ||
            (!defender.GetSubFeaturesByType<IIgnoreAoOOnMe>()
                .Any(f => f.CanIgnoreAoOOnSelf(defender, attacker)) && def);
    }
}

internal sealed class IgnoreDisengage : IIgnoreAoOImmunity
{
    public bool CanIgnoreAoOImmunity([NotNull] RulesetCharacter character, RulesetCharacter attacker, float distance)
    {
        var featuresToBrowse = new List<FeatureDefinition>();

        character.EnumerateFeaturesToBrowse<ICombatAffinityProvider>(featuresToBrowse);

        var service = ServiceRepository.GetService<IRulesetImplementationService>();
        var disengaging = DatabaseHelper.FeatureDefinitionCombatAffinitys.CombatAffinityDisengaging;

        foreach (var feature in featuresToBrowse)
        {
            if (feature != disengaging
                && feature is ICombatAffinityProvider affinityProvider
                && service.IsSituationalContextValid(
                    new RulesetImplementationDefinitions.SituationalContextParams(
                        affinityProvider.SituationalContext, attacker,
                        character, service.FindSourceIdOfFeature(character, feature),
                        affinityProvider.RequiredCondition, false, null))
                && affinityProvider.IsImmuneToOpportunityAttack(character, attacker, distance))
            {
                return false;
            }
        }

        return true;
    }
}

internal sealed class SentinelFeatMarker : CustomReactionAttack
{
    public SentinelFeatMarker()
    {
        Name = "ReactionAttackSentinel";
    }
}

internal class CanMakeAoOOnReachEntered : CustomReactionAttack
{
    public CanMakeAoOOnReachEntered()
    {
        Name = "ReactionAttackAoOEnter";
    }

    public bool AllowRange { get; set; }
}

internal class CustomReactionAttack
{
    public delegate IEnumerator Handler([NotNull] GameLocationCharacter attacker,
        [NotNull] GameLocationCharacter defender, GameLocationBattleManager battleManager,
        GameLocationActionManager actionManager, ReactionRequest request);

    protected string Name { get; set; }
    protected IsCharacterValidHandler ValidateAttacker { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private IsCharacterValidHandler ValidateMover { get; set; }
    public IsWeaponValidHandler WeaponValidator { get; set; }
    public Handler BeforeReaction { get; set; }
    public Handler AfterReaction { get; set; }
    [UsedImplicitly] public bool IgnoreReactionUses { get; set; }
    public bool AccountAoOImmunity { get; set; }

    internal bool IsValid(GameLocationCharacter attacker, GameLocationCharacter mover)
    {
        var rulesetAttacker = attacker.RulesetCharacter;
        var rulesetMover = mover.RulesetCharacter;

        return rulesetAttacker != null
               && rulesetMover != null
               && (ValidateAttacker?.Invoke(rulesetAttacker) ?? true)
               && attacker.CanReact(IgnoreReactionUses)
               && (ValidateMover?.Invoke(rulesetMover) ?? true);
    }

    public IEnumerator Process(
        [NotNull] GameLocationCharacter attacker,
        [NotNull] GameLocationCharacter mover,
        (int3 from, int3 to)? movement,
        GameLocationBattleManager battleManager,
        GameLocationActionManager actionManager,
        bool allowRange)
    {
        if (!attacker.CanPerformOpportunityAttackOnCharacter(mover, movement?.to, movement?.from,
                out var mode, out var attackModifier, allowRange, battleManager, AccountAoOImmunity, WeaponValidator))
        {
            yield break;
        }

        var attackMode = RulesetAttackMode.AttackModesPool.Get();
        attackMode.Copy(mode);
        attackMode.actionType = ActionType.Reaction;

        var reactionRequest = MakeReactionRequest(attacker, mover, attackMode, attackModifier);

        if (BeforeReaction != null)
        {
            yield return BeforeReaction(attacker, mover, battleManager, actionManager, reactionRequest);
        }

        var count = actionManager.PendingReactionRequestGroups.Count;

        actionManager.AddInterruptRequest(reactionRequest);

        yield return battleManager.WaitForReactions(mover, actionManager, count);

        if (AfterReaction != null)
        {
            yield return AfterReaction(attacker, mover, battleManager, actionManager, reactionRequest);
        }

        RulesetAttackMode.AttackModesPool.Return(attackMode);
    }

    protected virtual ReactionRequest MakeReactionRequest(GameLocationCharacter attacker,
        GameLocationCharacter defender, RulesetAttackMode attackMode, ActionModifier attackModifier)
    {
        var reactionParams = new CharacterActionParams(
            attacker,
            Id.AttackOpportunity,
            attackMode,
            defender,
            attackModifier) { StringParameter2 = Name };

        return new ReactionRequestWarcaster(Name, reactionParams);
    }
}
