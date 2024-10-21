﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomUI;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatDefinitions;

namespace SolastaUnfinishedBusiness.Feats;

internal static class GroupFeats
{
    internal const string Slasher = "Slasher";
    internal const string Piercer = "Piercer";
    internal const string Crusher = "Crusher";
    internal const string DevastatingStrikes = "DevastatingStrikes";
    internal const string OldTactics = "OldTactics";
    internal const string FightingStyle = "FightingStyle";

    internal static List<FeatDefinition> Groups { get; } = [];

    internal static FeatDefinition FeatGroupBodyResilience { get; } = MakeGroup("FeatGroupBodyResilience", null,
        BadlandsMarauder,
        BlessingOfTheElements,
        Enduring_Body,
        FocusedSleeper,
        HardToKill,
        Hauler,
        Robust,
        MakeGroup("FeatGroupCreed", null,
            Creed_Of_Einar,
            Creed_Of_Misaye,
            Creed_Of_Arun,
            Creed_Of_Pakri,
            Creed_Of_Maraike,
            Creed_Of_Solasta));

    private static FeatDefinition FeatGroupElementalTouch { get; } = MakeGroup("FeatGroupElementalTouch",
        BurningTouch.FamilyTag,
        BurningTouch,
        ToxicTouch,
        ElectrifyingTouch,
        IcyTouch,
        MeltingTouch);

    internal static FeatDefinition FeatGroupFightingStyle { get; } = MakeGroup("FeatGroupFightingStyle", FightingStyle);

    //
    // Crusher & Piercer
    //

    internal static FeatDefinition FeatGroupCrusher { get; } = MakeGroup("FeatGroupCrusher", Crusher);

    internal static FeatDefinition FeatGroupPiercer { get; } = MakeGroup("FeatGroupPiercer", Piercer);

    //
    // Skill & Tools
    //

    internal static FeatDefinition FeatGroupSkills { get; } = MakeGroup("FeatGroupSkills", null,
        ArcaneAppraiser,
        InitiateEnchanter,
        Manipulator);

    internal static FeatDefinition FeatGroupTools { get; } = MakeGroup("FeatGroupTools", null,
        InitiateAlchemist,
        MasterAlchemist,
        InitiateEnchanter,
        MasterEnchanter,
        Lockbreaker);

    //
    // Combat Groups
    //

    internal static FeatDefinition FeatGroupAgilityCombat { get; } = MakeGroup("FeatGroupAgilityCombat", null,
        EagerForBattle,
        ForestRunner,
        ReadyOrNot,
        RushToBattle);

    internal static FeatDefinition FeatGroupDefenseCombat { get; } = MakeGroup("FeatGroupDefenseCombat", null,
        CloakAndDagger,
        RaiseShield,
        TwinBlade);

    internal static FeatDefinition FeatGroupMeleeCombat { get; } = MakeGroup("FeatGroupMeleeCombat", null,
        FeatGroupElementalTouch,
        FeatGroupCrusher,
        FeatGroupPiercer,
        DauntingPush,
        DistractingGambit,
        TripAttack);

    internal static FeatDefinition FeatGroupRangedCombat { get; } = MakeGroup("FeatGroupRangedCombat", null,
        FeatGroupPiercer,
        TakeAim,
        DiscretionOfTheCoedymwarth,
        UncannyAccuracy);

    internal static FeatDefinition FeatGroupSpellCombat { get; } = MakeGroup("FeatGroupSpellCombat", null,
        FlawlessConcentration,
        PowerfulCantrip);

    internal static FeatDefinition FeatGroupSupportCombat { get; } = MakeGroup("FeatGroupSupportCombat", null,
        Mender);

    internal static FeatDefinition FeatGroupTwoHandedCombat { get; } = MakeGroup("FeatGroupTwoHandedCombat", null,
        MightyBlow,
        ForestallingStrength,
        FollowUpStrike);

    internal static FeatDefinition FeatGroupTwoWeaponCombat { get; } = MakeGroup("FeatGroupTwoWeaponCombat", null,
        Ambidextrous,
        TwinBlade);

    internal static FeatDefinition FeatGroupUnarmoredCombat { get; } = MakeGroup("FeatGroupUnarmoredCombat", null,
        FeatGroupCrusher,
        FeatGroupElementalTouch);

    internal static void Load(Action<FeatDefinition> loader)
    {
        MakeFeatGroupHalfAttributes();
        Groups.ForEach(ApplyDynamicDescription);
        Groups.ForEach(loader);
    }

    private static void MakeFeatGroupHalfAttributes()
    {
        var featGroupHalfStrength = MakeGroup("FeatGroupHalfStrength", null);
        var featGroupHalfDexterity = MakeGroup("FeatGroupHalfDexterity", null);
        var featGroupHalfConstitution = MakeGroup("FeatGroupHalfConstitution", null);
        var featGroupHalfIntelligence = MakeGroup("FeatGroupHalfIntelligence", null);
        var featGroupHalfWisdom = MakeGroup("FeatGroupHalfWisdom", null);
        var featGroupHalfCharisma = MakeGroup("FeatGroupHalfCharisma", null);

        foreach (var featDefinition in DatabaseRepository.GetDatabase<FeatDefinition>())
        {
            var attributeModifiers = featDefinition.Features
                .OfType<FeatureDefinitionAttributeModifier>()
                .Where(y =>
                    AttributeDefinitions.AbilityScoreNames.Contains(y.ModifiedAttribute) &&
                    y.ModifierOperation == AttributeModifierOperation.Additive &&
                    y.ModifierValue == 1)
                .ToArray();

            if (attributeModifiers.Length != 1)
            {
                continue;
            }

            switch (attributeModifiers[0].ModifiedAttribute)
            {
                case AttributeDefinitions.Strength:
                    featGroupHalfStrength.AddFeats(featDefinition);
                    break;

                case AttributeDefinitions.Dexterity:
                    featGroupHalfDexterity.AddFeats(featDefinition);
                    break;

                case AttributeDefinitions.Constitution:
                    featGroupHalfConstitution.AddFeats(featDefinition);
                    break;

                case AttributeDefinitions.Intelligence:
                    featGroupHalfIntelligence.AddFeats(featDefinition);
                    break;

                case AttributeDefinitions.Wisdom:
                    featGroupHalfWisdom.AddFeats(featDefinition);
                    break;

                case AttributeDefinitions.Charisma:
                    featGroupHalfCharisma.AddFeats(featDefinition);
                    break;
            }
        }

        MakeGroup("FeatGroupHalfAttributes", null,
            featGroupHalfStrength,
            featGroupHalfDexterity,
            featGroupHalfConstitution,
            featGroupHalfIntelligence,
            featGroupHalfWisdom,
            featGroupHalfCharisma);
    }

    private static void ApplyDynamicDescription(FeatDefinition groupDefinition)
    {
        var groupedFeat = groupDefinition.GetFirstSubFeatureOfType<GroupedFeat>();

        if (groupedFeat == null)
        {
            return;
        }

        var titles = groupedFeat.GetSubFeats(true)
            .Select(x => x.FormatTitle())
            .OrderBy(x => x)
            .ToArray();
        var title = string.Join(", ", titles);

        groupDefinition.guiPresentation.description = Gui.Format(groupDefinition.guiPresentation.description, title);
    }

    //
    // Group Builders
    //

    internal static FeatDefinition MakeGroup(string name, string family, params FeatDefinition[] feats)
    {
        return MakeGroup(name, family, feats.AsEnumerable());
    }

    internal static FeatDefinition MakeGroup(string name, string family, IEnumerable<FeatDefinition> feats)
    {
        var group = FeatDefinitionBuilder
            .Create(name)
            .SetGuiPresentation(Category.Feat)
            .AddCustomSubFeatures(new GroupedFeat(feats))
            .SetFeatFamily(family)
            .SetFeatures()
            .AddToDB();

        Groups.Add(group);

        return group;
    }

    internal static FeatDefinition MakeGroupWithPreRequisite(
        string name,
        string family,
        Func<FeatDefinitionWithPrerequisites, RulesetCharacterHero, (bool result, string output)> validator,
        params FeatDefinition[] feats)
    {
        var group = FeatDefinitionWithPrerequisitesBuilder
            .Create(name)
            .SetGuiPresentation(Category.Feat)
            .AddCustomSubFeatures(new GroupedFeat(feats))
            .SetFeatFamily(family)
            .SetFeatures()
            .SetValidators(validator)
            .AddToDB();

        Groups.Add(group);

        return group;
    }

    internal static void AddFeats(this FeatDefinition groupDefinition, params FeatDefinition[] feats)
    {
        var groupedFeat = groupDefinition.GetFirstSubFeatureOfType<GroupedFeat>();

        groupedFeat?.AddFeats(feats);
    }

    internal static void RemoveFeats(this FeatDefinition groupDefinition, params FeatDefinition[] feats)
    {
        var groupedFeat = groupDefinition.GetFirstSubFeatureOfType<GroupedFeat>();

        groupedFeat?.RemoveFeats(feats);
    }
}
