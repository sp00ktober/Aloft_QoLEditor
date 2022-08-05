using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aloft_QoLEditor.Patches.Transpiler
{
    [HarmonyPatch(typeof(IslandEditor_CanvasEditor))]
    internal class IslandEditor_CanvasEditor_Transpiler
    {
        delegate void moveObjUpDown(IslandEditor_CanvasEditor __instance, Ray ray, float distance);

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(IslandEditor_CanvasEditor), "Update")]
        public static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // find code when raycast hits x/z plain
            CodeMatcher matcher = new CodeMatcher(instructions, il)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Raycast"))
                .Advance(2);

            // store jmp to end of method
            var jmpIfNoRaycastHit = matcher.Operand;

            // insert own code triggered by holding shift
            matcher
                .Advance(1)
                .CreateLabel(out Label jmpNormalMovement)
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<Func<bool>> (() =>
                    {
                        Keyboard keyboard = Keyboard.current;

                        return keyboard != null && keyboard.shiftKey.isPressed;
                    }),
                    new CodeInstruction(OpCodes.Brfalse, jmpNormalMovement),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    Transpilers.EmitDelegate<moveObjUpDown>((IslandEditor_CanvasEditor __instance, Ray ray, float distance) =>
                    {
                        Vector3 point = ray.GetPoint(distance);
                        TMP_InputField tmp_InputField = __instance.field_posY;
                        IslandEditor_Terrain terrain = (IslandEditor_Terrain)AccessTools.Field(typeof(IslandEditor_CanvasEditor), "terrain").GetValue(__instance);
                        Vector3 vector = terrain.MoveObjSelected(point.y, 1, false);
                        tmp_InputField.SetTextWithoutNotify(vector.y.ToString());
                    }),
                    new CodeInstruction(OpCodes.Br, jmpIfNoRaycastHit));

            // if holding shift we need to create a different plane to get correct positions for up/down movement
            matcher
                .Start()
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_up"))
                .SetInstructionAndAdvance(
                    Transpilers.EmitDelegate<Func<Vector3>>(() =>
                    {
                        Keyboard keyboard = Keyboard.current;

                        if(keyboard != null && keyboard.shiftKey.isPressed)
                        {
                            return Vector3.forward;
                        }
                        return Vector3.up;
                    }));

            return matcher.InstructionEnumeration();
        }
    }
}
