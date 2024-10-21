﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Models;
using static SolastaUnfinishedBusiness.Models.SaveByLocationContext;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class TacticalAdventuresApplicationPatcher
{
    private static bool EnableSaveByLocation(ref string __result)
    {
        //PATCH: EnableSaveByLocation
        if (!SettingsContext.GuiModManagerInstance.EnableSaveByLocation)
        {
            return true;
        }

        // Modify the value returned by TacticalAdventuresApplication.SaveGameDirectory so that saves
        // end up where we want them (by location/campaign)
        var selectedCampaignService = ServiceRepository.GetService<SelectedCampaignService>();

        // handle exception when saving from world map or encounters on a user campaign
        if (Gui.GameCampaign?.campaignDefinition?.IsUserCampaign == true &&
            selectedCampaignService is { LocationType: LocationType.StandardCampaign })
        {
            __result = GetMostRecentPlace().Path;

            return false;
        }

        __result = selectedCampaignService?.SaveGameDirectory ?? DefaultSaveGameDirectory;

        return false;
    }

    [HarmonyPatch(typeof(TacticalAdventuresApplication), nameof(TacticalAdventuresApplication.SaveGameDirectory),
        MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class SaveGameDirectory_Getter_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(ref string __result)
        {
            return EnableSaveByLocation(ref __result);
        }
    }
}
