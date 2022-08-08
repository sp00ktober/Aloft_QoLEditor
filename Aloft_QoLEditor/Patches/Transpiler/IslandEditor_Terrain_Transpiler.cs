using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Aloft_QoLEditor.Patches.Transpiler
{
    [HarmonyPatch(typeof(IslandEditor_Terrain))]
    internal class IslandEditor_Terrain_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(IslandEditor_Terrain), "MoveObjSelected")]
        public static IEnumerable<CodeInstruction> MoveObjSelected_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Vector3), "x")),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Clamp"));

            int indexStart = matcher.Pos;

            matcher
                .MatchForward(false,
                    new CodeMatch(OpCodes.Div),
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_propSelected"))
                .Advance(2);

            int indexEnd = matcher.Pos;

            matcher
                .Start()
                .Advance(indexStart);

            for(; matcher.Pos < indexEnd;)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            return matcher.InstructionEnumeration();
        }
    }
}
