﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using TA.AI;
using TA.AI.Considerations;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches.Considerations;

[UsedImplicitly]
public static class InfluenceFearSourceProximityPatcher
{
    //PATCH: allows this influence to be reverted if enemy has StringParameter condition name
    //used on Command Spell, approach command
    [HarmonyPatch(typeof(InfluenceFearSourceProximity), nameof(InfluenceFearSourceProximity.Score))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Score_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(
            DecisionContext context,
            ConsiderationDescription consideration,
            DecisionParameters parameters,
            ScoringResult scoringResult)
        {
            Score(context, consideration, parameters, scoringResult);

            return false;
        }

        // mainly vanilla code except for BEGIN/END blocks
        private static void Score(
            DecisionContext context,
            ConsiderationDescription consideration,
            DecisionParameters parameters,
            ScoringResult scoringResult)
        {
            var character = parameters.character.GameLocationCharacter;
            var rulesetCharacter = character.RulesetCharacter;
            var denominator = consideration.IntParameter > 0 ? consideration.IntParameter : 1;
            var floatParameter = consideration.FloatParameter;
            var position = consideration.BoolParameter ? context.position : character.LocationPosition;
            var numerator = 0.0f;

            // prefer enumerate first to save some cycles
            rulesetCharacter.GetAllConditions(rulesetCharacter.AllConditionsForEnumeration);

            var approachSourceGuid = rulesetCharacter.AllConditionsForEnumeration
                .FirstOrDefault(x =>
                    x.ConditionDefinition.Name == consideration.StringParameter)?.SourceGuid ?? 0;

            foreach (var rulesetCondition in rulesetCharacter.AllConditionsForEnumeration
                         .Where(rulesetCondition =>
                             rulesetCondition.ConditionDefinition.ForceBehavior &&
                             rulesetCondition.ConditionDefinition.FearSource))
            {
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var relevantEnemy in parameters.situationalInformation.RelevantEnemies)
                {
                    if (relevantEnemy.Guid != rulesetCondition.SourceGuid)
                    {
                        continue;
                    }

                    var distance =
                        parameters.situationalInformation.PositioningService
                            .ComputeDistanceBetweenCharactersApproximatingSize(
                                parameters.character.GameLocationCharacter, position,
                                relevantEnemy, relevantEnemy.LocationPosition);

                    //BEGIN PATCH
                    if (relevantEnemy.Guid == approachSourceGuid)
                    {
                        numerator += Mathf.Lerp(0.0f, 1f, Mathf.Clamp(distance / floatParameter, 0.0f, 1f));
                        break;
                    }
                    //END PATCH

                    numerator += Mathf.Lerp(1f, 0.0f, Mathf.Clamp(distance / floatParameter, 0.0f, 1f));
                    break;
                }

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var relevantAlly in parameters.situationalInformation.RelevantAllies)
                {
                    if (relevantAlly.Guid != rulesetCondition.SourceGuid)
                    {
                        continue;
                    }

                    var distance =
                        parameters.situationalInformation.PositioningService
                            .ComputeDistanceBetweenCharactersApproximatingSize(
                                parameters.character.GameLocationCharacter, position, relevantAlly,
                                relevantAlly.LocationPosition);

                    //BEGIN PATCH
                    if (relevantAlly.Guid == approachSourceGuid)
                    {
                        numerator += Mathf.Lerp(0.0f, 1f, Mathf.Clamp(distance / floatParameter, 0.0f, 1f));
                        break;
                    }
                    //END PATCH

                    numerator += Mathf.Lerp(1f, 0.0f, Mathf.Clamp(distance / floatParameter, 0.0f, 1f));
                    break;
                }
            }

            scoringResult.Score = numerator / denominator;
        }
    }
}
