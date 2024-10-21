﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class CursorLocationGeometricShapePatcher
{
    //PATCH: UseHeightOneCylinderEffect
    [HarmonyPatch(typeof(CursorLocationGeometricShape), nameof(CursorLocationGeometricShape.UpdateGeometricShape))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class UpdateGeometricShape_Patch
    {
        [UsedImplicitly]
        public static void MyUpdateCubePosition_Regular(
            [NotNull] GeometricShape __instance,
            Vector3 origin,
            float edgeSize,
            int targetSize,
            bool adaptToGroundLevel,
            bool isValid,
            int height)
        {
            __instance.UpdateCubePosition_Regular(origin, edgeSize, targetSize, adaptToGroundLevel, isValid);

            if (!Main.Settings.UseHeightOneCylinderEffect)
            {
                return;
            }

            if (height == 0)
            {
                return;
            }

            var vector3 = new Vector3();

            if (!adaptToGroundLevel)
            {
                if (edgeSize % 2.0 == 0.0)
                {
                    vector3 = new Vector3(0.5f, 0.0f, 0.5f);
                }

                if (height % 2.0 == 0.0)
                {
                    vector3.y = 0.5f;
                }
            }
            else
            {
                vector3.y = (float)((0.5 * height) - 0.5);

                if (edgeSize % 2.0 == 0.0)
                {
                    vector3 += new Vector3(0.5f, 0.0f, 0.5f);
                }
            }

            var transform = __instance.cubeRenderer.transform;

            transform.SetPositionAndRotation(origin + vector3, Quaternion.identity);
            transform.localScale = new Vector3(edgeSize, height, edgeSize);
        }

        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var targetParameter2Field =
                typeof(CursorLocationGeometricShape).GetField("targetParameter2",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var updateCubePositionRegularMethod = typeof(GeometricShape).GetMethod("UpdateCubePosition_Regular");
            var myUpdateCubePositionRegularMethod =
                typeof(UpdateGeometricShape_Patch).GetMethod("MyUpdateCubePosition_Regular");

            return instructions.ReplaceCalls(updateCubePositionRegularMethod,
                "CursorLocationGeometricShape.UpdateGeometricShape",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, targetParameter2Field),
                new CodeInstruction(OpCodes.Call, myUpdateCubePositionRegularMethod));
        }
    }

    //PATCH: supports `IModifyTeleportEffectBehavior`
    [HarmonyPatch(typeof(CursorLocationGeometricShape), nameof(CursorLocationGeometricShape.Activate))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Activate_Patch
    {
        [UsedImplicitly]
        public static void Postfix(CursorLocationGeometricShape __instance)
        {
            CursorMotionHelper.Activate(__instance);
        }
    }

    //PATCH: supports `IModifyTeleportEffectBehavior`
    [HarmonyPatch(typeof(CursorLocationGeometricShape), nameof(CursorLocationGeometricShape.Deactivate))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Deactivate_Patch
    {
        [UsedImplicitly]
        public static void Prefix(CursorLocationGeometricShape __instance)
        {
            CursorMotionHelper.Deactivate(__instance);
        }
    }

    //PATCH: supports `IModifyTeleportEffectBehavior`
    [HarmonyPatch(typeof(CursorLocationGeometricShape), nameof(CursorLocationGeometricShape.RefreshHover))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class RefreshHover_Patch
    {
        [UsedImplicitly]
        public static void Postfix(CursorLocationGeometricShape __instance)
        {
            CursorMotionHelper.RefreshHover(__instance);
        }

        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            const int LOAD_SHIFT = 10;
            const int COMPUTE_SHIFT = 25;

            var code = instructions.ToList();
            var loadParam =
                code.FindIndex(i => i.Calls(nameof(IGameLocationTargetingService.ComputeTargetingParameters)));
            var callCompute = code.FindIndex(i =>
                i.Calls(nameof(IGameLocationTargetingService.ComputeTargetsOfAreaOfEffect)));

            if (loadParam >= LOAD_SHIFT && callCompute >= COMPUTE_SHIFT)
            {
                code[callCompute - COMPUTE_SHIFT] = code[loadParam - LOAD_SHIFT];
            }
            else
            {
                Main.Error("Failed to apply transpiler patch [CursorLocationGeometricShape.RefreshHover.1]!");
                Main.Error($"Couldn't find some insertion points load:{loadParam} compute:{callCompute}");
            }

            var magnetic = code.FindIndex(i =>
                i.opcode == OpCodes.Stfld && i.operand.ToString().Contains("hasMagneticTargeting"));
            if (magnetic >= 0)
            {
                var newMagnetic = new Action<CursorLocationGeometricShape, bool>(InitMagnetic).Method;
                code[magnetic] = new CodeInstruction(OpCodes.Call, newMagnetic);
            }
            else
            {
                Main.Error("Failed to apply transpiler patch [CursorLocationGeometricShape.RefreshHover.2]!");
                Main.Error($"Couldn't find magnetic:{loadParam} point");
            }

            var oldGetter = typeof(CursorLocation).GetProperty(nameof(CursorLocation.HoveredPosition))!.GetGetMethod();
            var newGetter = new Func<CursorLocation, Vector3>(GetHoveredPosition).Method;

            return code.ReplaceCall(oldGetter, 1, "CursorLocationGeometricShape.RefreshHover.3",
                new CodeInstruction(OpCodes.Call, newGetter));
        }

        private static Vector3 GetHoveredPosition(CursorLocation cursor)
        {
            return cursor.HoveredPosition + CursorMotionHelper.CursorHoverShift;
        }

        private static void InitMagnetic(CursorLocationGeometricShape cursor, bool _)
        {
            cursor.hasMagneticTargeting = SettingsContext.InputModManagerInstance.EnableShiftToSnapLineSpells
                                          && Global.IsShiftPressed
                                          && cursor.shapeType == MetricsDefinitions.GeometricShapeType.Line;
        }
    }
}
