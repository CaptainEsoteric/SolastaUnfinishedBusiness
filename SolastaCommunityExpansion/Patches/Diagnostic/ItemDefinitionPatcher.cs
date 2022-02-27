﻿using System;
using System.IO;
using HarmonyLib;
using SolastaCommunityExpansion.Models;
using SolastaModApi.Diagnostics;

namespace SolastaCommunityExpansion.Patches.Diagnostic
{
#if DEBUG
    internal static class ItemDefinitionVerification
    {
        [Flags]
        public enum Verification
        {
            None,
            ReturnNull = 1,
            Log = 2,
            Throw = 4
        }

        public static Verification Mode { get; set; } = Verification.Log;

        public static void VerifyUsage<T>(ItemDefinition definition, bool hasFlag, ref T __result) where T : class
        {
            if (Mode == Verification.None)
            {
                return;
            }

            if (hasFlag)
            {
                return;
            }

            var msg = $"ItemDefinition {definition.Name}[{definition.GUID}] property {typeof(T)} does not have the matching flag set.";

            if (Mode.HasFlag(Verification.Log))
            {
                Main.Log(msg);

                if (DiagnosticsContext.HasDiagnosticsFolder)
                {
                    var path = Path.Combine(DiagnosticsContext.DiagnosticsOutputFolder, "ItemDefinition.txt");
                    File.AppendAllLines(path, new string[] {
                        $"{Environment.NewLine}",
                        $"------------------------------------------------------------------------------------", 
                        msg
                    });
                    File.AppendAllText(path, Environment.StackTrace);
                }
            }

            if (Mode.HasFlag(Verification.ReturnNull))
            {
                __result = null;
            }

            if (Mode.HasFlag(Verification.Throw))
            {
                throw new SolastaModApiException(msg);
            }
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "ArmorDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_ArmorDescription
    {
        public static void Postfix(ItemDefinition __instance, ref ArmorDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsArmor, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "WeaponDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_WeaponDescription
    {
        public static void Postfix(ItemDefinition __instance, ref WeaponDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsWeapon, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "AmmunitionDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_AmmunitionDescription
    {
        public static void Postfix(ItemDefinition __instance, ref AmmunitionDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsAmmunition, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "UsableDeviceDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_UsableDeviceDescription
    {
        public static void Postfix(ItemDefinition __instance, ref UsableDeviceDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsUsableDevice, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "ToolDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_ToolDescription
    {
        public static void Postfix(ItemDefinition __instance, ref ToolDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsTool, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "StarterPackDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_StarterPackDescription
    {
        public static void Postfix(ItemDefinition __instance, ref StarterPackDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsStarterPack, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "ContainerItemDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_ContainerItemDescription
    {
        public static void Postfix(ItemDefinition __instance, ref ContainerItemDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsContainerItem, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "LightSourceItemDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_LightSourceItemDescription
    {
        public static void Postfix(ItemDefinition __instance, ref LightSourceItemDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsLightSourceItem, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "FocusItemDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_FocusItemDescription
    {
        public static void Postfix(ItemDefinition __instance, ref FocusItemDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsFocusItem, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "WealthPileDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_WealthPileDescription
    {
        public static void Postfix(ItemDefinition __instance, ref WealthPileDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsWealthPile, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "SpellbookDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_SpellbookDescription
    {
        public static void Postfix(ItemDefinition __instance, ref SpellbookDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsSpellbook, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "FoodDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_FoodDescription
    {
        public static void Postfix(ItemDefinition __instance, ref FoodDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsFood, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "FactionRelicDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_FactionRelicDescription
    {
        public static void Postfix(ItemDefinition __instance, ref FactionRelicDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsFactionRelic, ref __result);
        }
    }

    [HarmonyPatch(typeof(ItemDefinition), "DocumentDescription", MethodType.Getter)]
    internal static class ItemDefinitionPatch_DocumentDescription
    {
        public static void Postfix(ItemDefinition __instance, ref DocumentDescription __result)
        {
            ItemDefinitionVerification.VerifyUsage(__instance, __instance.IsDocument, ref __result);
        }
    }
#endif
}
