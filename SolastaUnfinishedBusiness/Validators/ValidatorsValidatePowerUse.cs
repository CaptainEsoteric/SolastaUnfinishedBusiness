﻿using System.Linq;
using System.Runtime.CompilerServices;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Interfaces;

namespace SolastaUnfinishedBusiness.Validators;

internal delegate bool IsPowerUseValidHandler(RulesetCharacter character, FeatureDefinitionPower power);

internal sealed class ValidatorsValidatePowerUse : IValidatePowerUse
{
    public static readonly IValidatePowerUse NotInCombat = new ValidatorsValidatePowerUse(_ => Gui.Battle == null);

    public static readonly IValidatePowerUse InCombat = new ValidatorsValidatePowerUse(_ => Gui.Battle != null);

    public static readonly IValidatePowerUse HasTacticalMovesAvailable =
        new ValidatorsValidatePowerUse(ValidatorsCharacter.HasTacticalMovesAvailable);

    public static readonly IValidatePowerUse HasBonusAttackAvailable =
        new ValidatorsValidatePowerUse(ValidatorsCharacter.HasBonusAttackAvailable);

    public static readonly IValidatePowerUse HasMainAttackAvailable =
        new ValidatorsValidatePowerUse(ValidatorsCharacter.HasMainAttackAvailable);

    private readonly IsPowerUseValidHandler[] _validators;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValidatorsValidatePowerUse(params IsCharacterValidHandler[] validators)
    {
        _validators = validators.Select(v => new IsPowerUseValidHandler((character, _) => v(character))).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValidatorsValidatePowerUse(params IsPowerUseValidHandler[] validators)
    {
        _validators = validators;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUsePower(RulesetCharacter character, FeatureDefinitionPower power)
    {
        return _validators.All(v => v(character, power));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IValidatePowerUse UsedLessTimesThan(int limit)
    {
        return new ValidatorsValidatePowerUse((character, power) =>
        {
            var user = GameLocationCharacter.GetFromActor(character);

            if (user == null)
            {
                return false;
            }

            user.UsedSpecialFeatures.TryGetValue(power.Name, out var uses);
            return uses < limit;
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IValidatePowerUse HasNoneOfConditions(params string[] types)
    {
        return new ValidatorsValidatePowerUse(ValidatorsCharacter.HasNoneOfConditions(types));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsPowerNotValid(RulesetCharacter character, RulesetUsablePower power)
    {
        return !character.CanUsePower(power.PowerDefinition, false);
    }
}
