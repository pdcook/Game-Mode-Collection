using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using GameModeCollection.Objects;
using System;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(PlayerCollision),"FixedUpdate")]
    class PlayerCollision_Patch_FixedUpdate
    {
        static void DoPhysicsItemPush(RaycastHit2D raycast, CharacterData data)
        {
            raycast.transform.GetComponent<PhysicsItem>()?.Push(data);
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var m_clamp = UnboundLib.ExtensionMethods.GetMethodInfo(typeof(UnityEngine.Mathf), nameof(UnityEngine.Mathf.Clamp), new System.Type[] {typeof(float), typeof(float), typeof(float) });
            var m_push = UnboundLib.ExtensionMethods.GetMethodInfo(typeof(PlayerCollision_Patch_FixedUpdate), nameof(DoPhysicsItemPush));
            var f_data = UnboundLib.ExtensionMethods.GetFieldInfo(typeof(PlayerCollision), "data");

            /// Some notes:
            /// - ldloc.2 loads the array of RaycastHit2D's `array` (local variable)
            /// - ldloc.s 6 loads the loop index `j` (local variable)
            /// - preceeded by the previous two, ldelem RaycastHit2D loads `array[j]`

            List<CodeInstruction> codes = instructions.ToList();

            int index = -1;

            for (int i = 3; i < codes.Count(); i++)
            {
                // looking for:
                /*
                 *  num5 = Mathf.Clamp(num5, 0f, 10f);
                 * 
                 */
                if (codes[i].opcode == OpCodes.Stloc_S && codes[i].operand.ToString().EndsWith("(11)")
                    && codes[i - 1].Calls(m_clamp)
                    && codes[i - 2].opcode == OpCodes.Ldc_R4 && (float)codes[i - 2].operand == 10f
                    && codes[i - 3].opcode == OpCodes.Ldc_R4 && (float)codes[i - 3].operand == 0.0f
                   )
                {
                    index = i + 1;
                    break;
                }
            }

            if (index == -1) { GameModeCollection.LogError("[PlayerCollisionPatch] INSTRUCTION NOT FOUND."); }

            codes.Insert(index, new CodeInstruction(OpCodes.Ldloc_2));
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldloc_S, 6));
            codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldelem, typeof(UnityEngine.RaycastHit2D)));
            codes.Insert(index + 3, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(index + 4, new CodeInstruction(OpCodes.Ldfld, f_data));
            codes.Insert(index + 5, new CodeInstruction(OpCodes.Call, m_push));

            return codes.AsEnumerable();
        }
    }
}
