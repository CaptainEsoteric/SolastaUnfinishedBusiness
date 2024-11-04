﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class GuiInvocationDefinitionPatcher
{
    //NOTE: There is a typo on official method name
    // ReSharper disable once StringLiteralTypo
    [HarmonyPatch(typeof(GuiInvocationDefinition), nameof(GuiInvocationDefinition.IsInvocationMacthingPrerequisites))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    // ReSharper disable once IdentifierTypo
    public static class IsInvocationMacthingPrerequisites_Patch
    {
        //PATCH: return class level instead of char level on invocations selection (MULTICLASS)
        private static int TryGetAttributeValue(
            RulesetCharacterHero hero,
            string attributeName)
        {
            return hero.GetClassLevel(DatabaseHelper.CharacterClassDefinitions.Warlock);
        }

        //PATCH: allow warlocks to bypass spell knowledge on level 1 if using new warlock 2024 invocations progression
        private static bool HasKnowledgeOfSpell(RulesetSpellRepertoire spellRepertoire, SpellDefinition spellDefinition)
        {
            if (!Main.Settings.EnableWarlockToUseOneDndInvocationProgression)
            {
                return spellRepertoire.HasKnowledgeOfSpell(spellDefinition);
            }

            var screen = Gui.GuiService.GetScreen<CharacterCreationScreen>();

            if (!screen)
            {
                return spellRepertoire.HasKnowledgeOfSpell(spellDefinition);
            }

            return screen.Visible || spellRepertoire.HasKnowledgeOfSpell(spellDefinition);
        }

        [NotNull]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .ReplaceCalls(typeof(RulesetEntity).GetMethod("TryGetAttributeValue"),
                    // ReSharper disable once StringLiteralTypo
                    "GuiInvocationDefinition.IsInvocationMacthingPrerequisites1",
                    new CodeInstruction(OpCodes.Call,
                        new Func<RulesetCharacterHero, string, int>(TryGetAttributeValue).Method))
                .ReplaceCalls(typeof(RulesetSpellRepertoire).GetMethod("HasKnowledgeOfSpell"),
                    // ReSharper disable once StringLiteralTypo
                    "GuiInvocationDefinition.IsInvocationMacthingPrerequisites2",
                    new CodeInstruction(OpCodes.Call,
                        new Func<RulesetSpellRepertoire, SpellDefinition, bool>(HasKnowledgeOfSpell).Method));
        }

        [UsedImplicitly]
        public static void Postfix(
            ref bool __result,
            InvocationDefinition invocation,
            RulesetCharacterHero hero,
            ref string prerequisiteOutput)
        {
            //PATCH: Enforces Invocations With PreRequisites
            if (invocation is not InvocationDefinitionWithPrerequisites invocationDefinitionWithPrerequisites
                || invocationDefinitionWithPrerequisites.Validators.Count == 0)
            {
                return;
            }

            var (result, output) =
                invocationDefinitionWithPrerequisites.Validate(invocationDefinitionWithPrerequisites, hero);

            __result = __result && result;
            prerequisiteOutput += '\n' + output;
        }
    }

    [HarmonyPatch(typeof(GuiInvocationDefinition), nameof(GuiInvocationDefinition.Subtitle), MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Subtitle_Getter_Patch
    {
        [UsedImplicitly]
        public static void Postfix(GuiInvocationDefinition __instance, ref string __result)
        {
            //PATCH: show custom subtitle for custom invocations
            var feature = __instance.InvocationDefinition as InvocationDefinitionCustom;

            if (!feature || feature.PoolType == null)
            {
                return;
            }

            __result = $"UI/&CustomFeatureSelectionTooltipType{feature.PoolType.Name}";
        }
    }
}
