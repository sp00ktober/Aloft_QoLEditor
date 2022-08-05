using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Aloft_QoLEditor.Patches.Transpiler
{
    [HarmonyPatch(typeof(IslandEditor_Input))]
    internal class IslandEditor_Input_Transpiler
    {
        private delegate void additionalUpDownKeys(IslandEditor_Input __instance, ref Vector3 speed);

        private static float speedIncrease = 1f;

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(IslandEditor_Input), "Update")]
        public static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, il)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(IslandEditor_Input), "camMovement")),
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_deltaTime"),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "op_Multiply"));

            // store index where we insert our delegate to create a jmp for it later
            int indexAddAdditionalUpDownKeys = matcher.Pos;

            matcher
                // space and ctrl move camera up and down too (default is e and q)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloca_S, 0),
                    Transpilers.EmitDelegate<additionalUpDownKeys>((IslandEditor_Input __instance, ref Vector3 speed) =>
                    {
                        if (__instance._keyboard.spaceKey.isPressed)
                        {
                            speed += Vector3.up;
                        }
                        if (__instance._keyboard.ctrlKey.isPressed)
                        {
                            speed -= Vector3.up;
                        }
                    }))
                .Advance(5)
                // increases camera movement speed over time until stopped
                .Insert(Transpilers.EmitDelegate<Func<Vector3, Vector3>>(speed =>
                {
                    if (speed == Vector3.zero && speedIncrease > 1f)
                    {
                        speedIncrease = 1f;
                    }
                    else
                    {
                        speedIncrease += Time.deltaTime * 3;
                    }

                    return speed * speedIncrease;
                }));

            // get addr to jump to if camera rotation should not be changed
            matcher
                .Start()
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_rightButton"))
                .Advance(2);

            var jmpIfNoCameraRotation = matcher.Operand;

            // change brfalse to brtrue to construct an OR
            matcher
                .CreateLabelAt(matcher.Pos + 1, out Label jmpIfCameraRotation)
                .Set(OpCodes.Brtrue, jmpIfCameraRotation);

            // insert code to rotate camera by moving the mouse while camera is moving
            matcher
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    Transpilers.EmitDelegate<Func<Vector3, bool>>(speed =>
                    {
                        return speed == Vector3.zero;
                    }),
                    new CodeInstruction(OpCodes.Brtrue, jmpIfNoCameraRotation));

            // create jmp for up down keys delegate, else game would skip our delegate
            matcher
                .Start()
                .Advance(indexAddAdditionalUpDownKeys)
                .CreateLabel(out Label jmpAdditionalUpDownKeys)
                .Advance(-5)
                .SetInstruction(new CodeInstruction(OpCodes.Brfalse, jmpAdditionalUpDownKeys));

            return matcher.InstructionEnumeration();
        }
    }
}
