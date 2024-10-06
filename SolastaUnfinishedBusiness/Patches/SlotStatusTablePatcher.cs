﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Models;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class SlotStatusTablePatcher
{
    [HarmonyPatch(typeof(SlotStatusTable), nameof(SlotStatusTable.Bind))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Bind_Patch
    {
        private static bool UniqueLevelSlots(
            FeatureDefinitionCastSpell featureDefinitionCastSpell,
            RulesetSpellRepertoire rulesetSpellRepertoire)
        {
            // NPCs cannot be multicasters so try to get a hero here
            var hero = rulesetSpellRepertoire.GetCaster() as RulesetCharacterHero;

            //PATCH: displays slots on any multicaster hero so Warlocks can see their spell slots
            return featureDefinitionCastSpell.UniqueLevelSlots && !SharedSpellsContext.IsMulticaster(hero);
        }

        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var uniqueLevelSlotsMethod = typeof(FeatureDefinitionCastSpell).GetMethod("get_UniqueLevelSlots");
            var myUniqueLevelSlotsMethod =
                new Func<FeatureDefinitionCastSpell, RulesetSpellRepertoire, bool>(UniqueLevelSlots).Method;

            return instructions.ReplaceCalls(uniqueLevelSlotsMethod, "SlotStatusTable.Bind",
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, myUniqueLevelSlotsMethod));
        }

        //PATCH: creates different slots colors and pop up messages depending on slot types
        [UsedImplicitly]
        public static void Postfix(
            SlotStatusTable __instance,
            RulesetSpellRepertoire spellRepertoire,
            List<SpellDefinition> spells,
            int spellLevel)
        {
            var character = spellRepertoire?.GetCaster();

            // spellRepertoire is null during level up...
            if (spellLevel == 0 || character == null)
            {
                return;
            }

            if (spellRepertoire.SpellCastingRace)
            {
                return;
            }

            // NPCs cannot be multicasters so try to get a hero here
            if (character is not RulesetCharacterHero hero ||
                !SharedSpellsContext.IsMulticaster(hero))
            {
                //PATCH: support display cost on spell level blocks (SPELL_POINTS)
                if (!Main.Settings.UseAlternateSpellPointsSystem ||
                    spellRepertoire.spellCastingClass == Warlock)
                {
                    return;
                }

                for (var index = 0; index < __instance.table.childCount; ++index)
                {
                    var component = __instance.table.GetChild(index).GetComponent<SlotStatus>();

                    SpellPointsContext.DisplayCostOnSpellLevelBlocks(
                        __instance, component, spellLevel, spells.Count);
                }

                return;
            }

            spellRepertoire.GetSlotsNumber(spellLevel, out var totalSlotsRemainingCount, out var totalSlotsCount);

            MulticlassGameUiContext.PaintPactSlots(
                hero,
                totalSlotsCount,
                totalSlotsRemainingCount,
                spellLevel,
                spells.Count,
                __instance,
                (Global.InspectedHero != null && spellRepertoire.spellCastingClass == Warlock) ||
                (Global.InspectedHero == null &&
                 !(Main.Settings.DisplayPactSlotsOnSpellSelectionPanel &&
                   !Main.Settings.UseAlternateSpellPointsSystem)));
        }
    }

    //PATCH: ensures slot colors are white before getting back to pool
    [HarmonyPatch(typeof(SlotStatusTable), nameof(SlotStatusTable.Unbind))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Unbind_Patch
    {
        [UsedImplicitly]
        public static void Prefix(SlotStatusTable __instance)
        {
            MulticlassGameUiContext.PaintSlotsWhite(__instance.table);
        }
    }
}
