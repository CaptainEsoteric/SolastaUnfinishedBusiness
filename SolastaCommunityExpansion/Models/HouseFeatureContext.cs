﻿using SolastaCommunityExpansion.Api;
using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.CustomDefinitions;

namespace SolastaCommunityExpansion.Models;

public static class HouseFeatureContext
{
    internal const int DEFAULT_VISION_RANGE = 16;
    internal const int MAX_VISION_RANGE = 120;

    public static void LateLoad()
    {
        FixDivineSmiteRestrictions();
        FixMountaineerBonusShoveRestrictions();
        FixRecklessAttckForReachWeapons();
    }

    /**
     * Makes Divine Smite trigger only from melee attacks.
     * This wasn't relevant until we changed how SpendSpellSlot trigger works.
     */
    private static void FixDivineSmiteRestrictions()
    {
        DatabaseHelper.FeatureDefinitionAdditionalDamages.AdditionalDamagePaladinDivineSmite.attackModeOnly = true;
        DatabaseHelper.FeatureDefinitionAdditionalDamages.AdditionalDamagePaladinDivineSmite.requiredProperty =
            RuleDefinitions.AdditionalDamageRequiredProperty.MeleeWeapon;
    }

    /**
     * Makes Mountaineer's `Shield Push` bonus shove work only with shield equipped.
     * This wasn't relevant until we removed forced shield check in the `GameLocationCharacter.GetActionStatus`.
     */
    private static void FixMountaineerBonusShoveRestrictions()
    {
        DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinityMountaineerShieldCharge
            .SetCustomSubFeatures(new FeatureApplicationValidator(CharacterValidators.HasShield));
    }

    /**
     * Makes `Reckless` context check if main hand weapon is melee, instead of if character is next to target.
     * Required for it to work on reach weapons.
     */
    private static void FixRecklessAttckForReachWeapons()
    {
        DatabaseHelper.FeatureDefinitionCombatAffinitys.CombatAffinityReckless
            .situationalContext = (RuleDefinitions.SituationalContext)ExtendedSituationalContext.MainWeaponIsMelee;
    }
}
