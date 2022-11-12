﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;

namespace SolastaUnfinishedBusiness.Feats;

internal static class GroupFeats
{
    private static readonly List<FeatDefinition> Groups = new();

    internal static void CreateFeats([NotNull] List<FeatDefinition> feats)
    {
        feats.Add(BuildElementalTouchGroup());
        feats.Add(BuildCreedGroup());
        feats.Add(BuildRangedCombat());
        feats.Add(BuildTwoHandedCombat());
        feats.Add(BuildTwoWeaponCombat()); 
        feats.AddRange(Groups);
    }

#if false
    internal static void MakeGroup(FeatDefinition group, params FeatDefinition[] feats)
    {
        group.SetCustomSubFeatures(new GroupedFeat(feats));
        Groups.Add(group);
    }
#endif

    internal static FeatDefinition MakeGroup(string name, string family, params FeatDefinition[] feats)
    {
        return MakeGroup(name, family, feats.AsEnumerable());
    }

    internal static FeatDefinition MakeGroup(string name, string family, IEnumerable<FeatDefinition> feats)
    {
        var group = FeatDefinitionBuilder
            .Create(name)
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(new GroupedFeat(feats))
            .SetFeatFamily(family)
            .SetFeatures()
            .AddToDB();

        Groups.Add(group);

        return group;
    }

    private static FeatDefinition BuildElementalTouchGroup()
    {
        return FeatDefinitionBuilder
            .Create("FeatGroupElementalTouch")
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(new GroupedFeat(
                FeatDefinitions.BurningTouch,
                FeatDefinitions.ToxicTouch,
                FeatDefinitions.ElectrifyingTouch,
                FeatDefinitions.IcyTouch,
                FeatDefinitions.MeltingTouch
            ))
            .SetFeatFamily(FeatDefinitions.BurningTouch.FamilyTag)
            .SetFeatures()
            .AddToDB();
    }

    private static FeatDefinition BuildCreedGroup()
    {
        return FeatDefinitionBuilder
            .Create("FeatGroupCreed")
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(new GroupedFeat(
                FeatDefinitions.Creed_Of_Arun,
                FeatDefinitions.Creed_Of_Einar,
                FeatDefinitions.Creed_Of_Maraike,
                FeatDefinitions.Creed_Of_Misaye,
                FeatDefinitions.Creed_Of_Pakri,
                FeatDefinitions.Creed_Of_Solasta
            ))
            .SetFeatures()
            .AddToDB();
    }

    private static FeatDefinition BuildRangedCombat()
    {
        return FeatDefinitionBuilder
            .Create("FeatRangedCombat")
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(new GroupedFeat(
                FeatDefinitions.TakeAim,
                FeatDefinitions.UncannyAccuracy,
                CraftyFeats.FeatCraftyFletcher,
                EwFeats.FeatRangedExpert,
                ZappaFeats.FeatDeadEye,
                ZappaFeats.FeatMarksman
            ))
            .SetFeatures()
            .AddToDB();
    }

    private static FeatDefinition BuildTwoHandedCombat()
    {
        return FeatDefinitionBuilder
            .Create("FeatTwoHandedCombat")
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(new GroupedFeat(
                FeatDefinitions.MightyBlow,
                FeatDefinitions.ForestallingStrength,
                FeatDefinitions.FollowUpStrike
            ))
            .SetFeatures()
            .AddToDB();
    }

    private static FeatDefinition BuildTwoWeaponCombat()
    {
        return FeatDefinitionBuilder
            .Create("FeatTwoWeaponCombat")
            .SetGuiPresentation(Category.Feat)
            .SetCustomSubFeatures(new GroupedFeat(
                FeatDefinitions.Ambidextrous,
                ZappaFeats.FeatDualWeaponDefense,
                ElAntoniousFeats.FeatDualFlurry,
                FeatDefinitions.TwinBlade
            ))
            .SetFeatures()
            .AddToDB();
    }
}
