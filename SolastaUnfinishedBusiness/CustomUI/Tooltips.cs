﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using TMPro;
using UnityEngine;
using static RuleDefinitions.EffectDifficultyClassComputation;
using Object = UnityEngine.Object;

namespace SolastaUnfinishedBusiness.CustomUI;

internal static class Tooltips
{
    private static GameObject _tooltipInfoCharacterDescription;
    private static GameObject _distanceTextObject;
    private static TextMeshProUGUI _tmpUGui;

    internal static void AddContextToRecoveredFeature(RecoveredFeatureItem item, RulesetCharacterHero character)
    {
        item.GuiTooltip.Context = character;
    }

    internal static void UpdatePowerUses(ITooltip tooltip, TooltipFeaturePowerParameters parameters)
    {
        if (tooltip.DataProvider is not GuiPowerDefinition guiPowerDefinition)
        {
            return;
        }

        if (tooltip.Context is not RulesetCharacter character)
        {
            return;
        }

        var power = guiPowerDefinition.PowerDefinition;
        var usesLabel = parameters.usesLabel;

        usesLabel.Text = FormatUses(power, character, usesLabel.Text);
    }

    private static string FormatUses(FeatureDefinitionPower power, RulesetCharacter character, string def)
    {
        if (power.UsesDetermination != RuleDefinitions.UsesDetermination.Fixed)
        {
            return def;
        }

        if (power.RechargeRate == RuleDefinitions.RechargeRate.AtWill)
        {
            return def;
        }

        if (power.CostPerUse == 0)
        {
            return def;
        }

        var usablePower = PowerProvider.Get(power, character);
        var maxUses = character.GetMaxUsesOfPower(usablePower);
        // must use GetRemainingPowerUses as power could be a Shared Pool
        var remainingUses = character.GetRemainingPowerUses(power);

        return $"{remainingUses}/{maxUses}";
    }

    internal static void UpdatePowerSaveDC(ITooltip tooltip, TooltipFeaturePowerParameters parameters)
    {
        if (tooltip.DataProvider is not GuiPowerDefinition guiPowerDefinition)
        {
            return;
        }

        if (!guiPowerDefinition.HasSavingThrow)
        {
            return;
        }

        var effectDescription = guiPowerDefinition.EffectDescription;
        if (effectDescription.DifficultyClassComputation
            is not (AbilityScoreAndProficiency or SpellCastingFeature))
        {
            return;
        }

        if (tooltip.Context is not RulesetCharacter character)
        {
            return;
        }

        var attribute = DatabaseRepository.GetDatabase<SmartAttributeDefinition>()
            .GetElement(effectDescription.SavingThrowAbility);

        var power = guiPowerDefinition.PowerDefinition;
        var classDefinition = character.FindClassHoldingFeature(power);
        var saveDC = EffectHelpers.CalculateSaveDc(character, effectDescription, classDefinition);

        parameters.savingThrowLabel.Text = Gui.Format("{0} {1}", attribute.GuiPresentation.Title, saveDC.ToString());
    }

    internal static void UpdateCraftingTooltip(TooltipFeatureDescription description, ITooltip tooltip)
    {
        if (!Main.Settings.ShowCraftingRecipeInDetailedTooltips)
        {
            return;
        }

        if (tooltip.DataProvider is not IItemDefinitionProvider itemDefinitionProvider)
        {
            return;
        }

        var item = itemDefinitionProvider.ItemDefinition;

        if (!item.IsDocument || item.DocumentDescription.LoreType != RuleDefinitions.LoreType.CraftingRecipe)
        {
            return;
        }

        var guiWrapperService = ServiceRepository.GetService<IGuiWrapperService>();

        foreach (var contentFragmentDescription in item.DocumentDescription.ContentFragments
                     .Where(x => x.Type == ContentFragmentDescription.FragmentType.Body))
        {
            var guiRecipeDefinition =
                guiWrapperService.GetGuiRecipeDefinition(item.DocumentDescription.RecipeDefinition.Name);

            description.DescriptionLabel.Text = Gui.Format(contentFragmentDescription.Text,
                guiRecipeDefinition.Title, guiRecipeDefinition.IngredientsText);
        }
    }

    internal static void AddDistanceToTooltip(EntityDescription entityDescription)
    {
        _tooltipInfoCharacterDescription ??= GameObject.Find("TooltipFeatureCharacterDescription");

        if (_tooltipInfoCharacterDescription is null)
        {
            return;
        }

        var battleService = ServiceRepository.GetService<IGameLocationBattleService>();

        if (Main.Settings.EnableDistanceOnTooltip && battleService.Battle is not null)
        {
            entityDescription.header += "<br><br>";

            GameLocationCharacter characterToMeasureFrom = null;
            var distance = GetDistanceFromCharacter(ref characterToMeasureFrom, battleService);

            if (characterToMeasureFrom is null)
            {
                return;
            }

            // don't use ? on a type deriving from an unity object
            if (_tooltipInfoCharacterDescription)
            {
                _tmpUGui ??= _tooltipInfoCharacterDescription.transform.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (!_distanceTextObject)
            {
                GenerateDistanceText(distance, _tmpUGui, characterToMeasureFrom);
            }
            else
            {
                UpdateDistanceText(distance, characterToMeasureFrom);
            }

            // don't use ? on a type deriving from an unity object
#pragma warning disable IDE0031
            if (_distanceTextObject)
#pragma warning restore IDE0031
            {
                _distanceTextObject.SetActive(true);
            }
        }
        else if (!Main.Settings.EnableDistanceOnTooltip || battleService.Battle is null)
        {
            // don't use ? on a type deriving from an unity object
#pragma warning disable IDE0031
            if (_distanceTextObject)
#pragma warning restore IDE0031
            {
                _distanceTextObject.SetActive(false);
            }
        }
    }

    private static void GenerateDistanceText(int distance, TextMeshProUGUI tmpUGui,
        GameLocationCharacter characterToMeasureFrom)
    {
        var anchorObject = new GameObject();

        anchorObject.transform.SetParent(tmpUGui.transform);
        anchorObject.transform.localPosition = Vector3.zero;
        _distanceTextObject = Object.Instantiate(tmpUGui).gameObject;
        _distanceTextObject.name = "DistanceTextObject";
        _distanceTextObject.transform.SetParent(anchorObject.transform);
        _distanceTextObject.transform.position = Vector3.zero;
        _distanceTextObject.transform.localPosition = new Vector3(0, -10, 0);

        UpdateDistanceText(distance, characterToMeasureFrom);
    }

    private static int GetDistanceFromCharacter(
        ref GameLocationCharacter characterToMeasureFrom,
        IGameLocationBattleService battleService)
    {
        var gameLocationSelectionService = ServiceRepository.GetService<IGameLocationSelectionService>();

        if (gameLocationSelectionService.HoveredCharacters.Count is 0)
        {
            return 0;
        }

        var hoveredCharacter = gameLocationSelectionService.HoveredCharacters[0];
        var initiativeSortedContenders = battleService.Battle.InitiativeSortedContenders;
        var activePlayerController = ServiceRepository.GetService<IPlayerControllerService>().ActivePlayerController;
        var activePlayerControlledCharacters = activePlayerController.ControlledCharacters;
        var actingCharacter = battleService.Battle?.activeContender;

        if (actingCharacter is null)
        {
            return 0;
        }

        characterToMeasureFrom = activePlayerControlledCharacters.Contains(actingCharacter)
            ? actingCharacter
            : GetNextControlledCharacterInInitiative(
                initiativeSortedContenders, activePlayerController, actingCharacter);

        return (int)Math.Round(DistanceCalculation.GetDistanceFromCharacters(characterToMeasureFrom, hoveredCharacter));
    }

    private static GameLocationCharacter GetNextControlledCharacterInInitiative(
        List<GameLocationCharacter> initiativeSortedContenders,
        PlayerController activePlayerController,
        GameLocationCharacter actingCharacter)
    {
        return initiativeSortedContenders.Find(character =>
            character.controllerId == activePlayerController.controllerId
            && character.lastInitiative < actingCharacter.lastInitiative) ?? initiativeSortedContenders.Find(
            character =>
                character.controllerId == activePlayerController.controllerId);
    }

    private static void UpdateDistanceText(int distance, GameLocationCharacter characterToMeasureFrom)
    {
        _distanceTextObject.GetComponent<TextMeshProUGUI>().text =
            Gui.Format("UI/&DistanceFormat", Gui.FormatDistance(distance))
            + $" {Gui.Localize("UI/&From")} "
            + GetReducedName(characterToMeasureFrom.Name);
    }

    private static string GetReducedName(string characterName)
    {
        return characterName.Length >= 12
            ? characterName.Substring(0, 9) + "..."
            : characterName;
    }
}
