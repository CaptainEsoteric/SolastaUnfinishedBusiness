﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class BaseBlueprintPatcher
{
    [HarmonyPatch(typeof(BaseBlueprint), nameof(BaseBlueprint.GetAssetKey))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class GetAssetKey_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(
            BaseBlueprint __instance,
            ref string __result,
            BaseBlueprint.PrefabByEnvironmentDescription prefabByEnvironmentDescription,
            EnvironmentDefinition environmentDefinition,
            bool perspective)
        {
            //PATCH: ensures custom props display the proper icon (DMP)
            return DungeonMakerCustomRooms.ExtendedGetAssetKey(
                __instance, ref __result, prefabByEnvironmentDescription, environmentDefinition, perspective);
        }
    }
}
