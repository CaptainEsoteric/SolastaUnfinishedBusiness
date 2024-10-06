﻿using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
internal class FeatureDefinitionGrantInvocations : FeatureDefinition
{
    // ReSharper disable once CollectionNeverUpdated.Local
    internal List<InvocationDefinition> Invocations { get; } = [];

    internal static void GrantInvocations(
        RulesetCharacterHero hero,
        string tag,
        IEnumerable<FeatureDefinition> grantedFeatures)
    {
        var features = grantedFeatures
            .OfType<FeatureDefinitionGrantInvocations>()
            .ToArray();

        if (features.Length == 0)
        {
            return;
        }

        var command = ServiceRepository.GetService<IHeroBuildingCommandService>();

        foreach (var invocation in features.SelectMany(f => f.Invocations))
        {
            command.TrainCharacterFeature(hero, tag, invocation.Name, HeroDefinitions.PointsPoolType.Invocation);
        }
    }

    internal static void RemoveInvocations(
        RulesetCharacterHero hero,
        string tag,
        IEnumerable<FeatureDefinition> removedFeatures)
    {
        var features = removedFeatures
            .OfType<FeatureDefinitionGrantInvocations>()
            .ToArray();

        if (features.Length == 0)
        {
            return;
        }

        var command = ServiceRepository.GetService<IHeroBuildingCommandService>();

        foreach (var invocation in features.SelectMany(f => f.Invocations))
        {
            command.UntrainCharacterFeature(hero, tag, invocation.Name, HeroDefinitions.PointsPoolType.Invocation);
        }
    }
}
