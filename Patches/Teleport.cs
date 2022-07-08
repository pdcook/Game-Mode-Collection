using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using UnboundLib;

namespace GameModeCollection.Patches
{
    /*
    [HarmonyPatch]
    class TeleportPatchDelayMove
    {
        /// <summary>
        /// patch to conditionally prevent players teleporting through walls
        /// </summary>
        /// <returns></returns>
        static Type GetNestedMoveType()
        {
            var nestedTypes = typeof(Teleport).GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic);
            Type nestedType = null;

            foreach (var type in nestedTypes)
            {
                if (type.Name.Contains("DelayMove"))
                {
                    nestedType = type;
                    break;
                }
            }

            return nestedType;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(GetNestedMoveType(), "MoveNext");
        }

        static bool IsNOTValidTeleportLocation(bool colliderWasHit, Vector3 trialPosition, CharacterData data)
        {
            if (colliderWasHit || !GameModeCollection.PreventTeleportThroughWalls)
            {
                return colliderWasHit;
            }
            return !PlayerManager.instance.CanSeePlayer(trialPosition, data.player).canSee;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {*/
            /*
             *  We want to replace:
             * 
            /////
            Vector3 vector = base.transform.position;
	        Vector3 position = base.transform.position;
            int num = 10;
            float d = this.distance * (float)this.level.attackLevel / (float)num;
            for (int i = 0; i < num; i++)
            {
                vector += d * this.data.aimDirection;
                if (!Physics2D.OverlapCircle(vector, 0.5f))
                {
                    position = vector;
                }
            }
            /////

            * with this:
            
            /////
            Vector3 vector = base.transform.position;
	        Vector3 position = base.transform.position;
            int num = 10;
            float d = this.distance * (float)this.level.attackLevel / (float)num;
            for (int i = 0; i < num; i++)
            {
                vector += d * this.data.aimDirection;
                if (!Physics2D.OverlapCircle(vector, 0.5f) && (!GameModeCollection.PreventTeleportThroughWalls || PlayerManager.instance.CanSeePlayer(vector, this.data.player))) // add check for LoS to be able to teleport
                {
                    position = vector;
                }
            }
            /////

            The pattern to look for is:
            
            brtrue_s (or brtrue) // skip the next two lines if a collider was found
            ldloc_2 // if a collider wasn't found, load the temporary vector to teleport to
            stloc_3 // and store it in the final vector to teleport to
            
            */
            /*
            MethodInfo m_isNOTValidTeleportLocation = ExtensionMethods.GetMethodInfo(typeof(TeleportPatchDelayMove), nameof(IsNOTValidTeleportLocation));
            FieldInfo m_data = ExtensionMethods.GetFieldInfo(typeof(Teleport), "data");

            List<CodeInstruction> codes = instructions.ToList();

            int index = -1;
            
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if ((codes[i].opcode == OpCodes.Brtrue_S || codes[i].opcode == OpCodes.Brtrue) && codes[i+1].opcode == OpCodes.Ldloc_2 && codes[i+2].opcode == OpCodes.Stloc_3)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                GameModeCollection.LogError("[TeleportPatchDelayMove] INSTRUCTION NOT FOUND.");
            }

            // before our transpiler, the bool colliderWasHit is already loaded,
            // so we just need to load the trial position and the character data,
            // then call our method
            codes.Insert(index, new CodeInstruction(OpCodes.Ldloc_2)); // load the trial vector
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldloc_1)); // load the Teleport instance
            codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldfld, m_data)); // load the CharacterData field from the Teleport instance
            codes.Insert(index + 3, new CodeInstruction(OpCodes.Call, m_isNOTValidTeleportLocation)); // call our method

            return codes.AsEnumerable();
        }
    }
    */
}
