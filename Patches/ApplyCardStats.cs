using HarmonyLib;
using UnityEngine;
using UnboundLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(ApplyCardStats), "ApplyStats")]
    public class ApplyCardStats_Patch_ApplyStats
    {
        static void ReviveOrCompensate(ApplyCardStats instance)
        {
            if (instance == null || ((Player)instance.GetFieldValue("playerToUpgrade")) == null)
            {
                return;
            }

            if (GameModeCollection.ReviveOnCardAdd)
            {
                // vanilla behaviour
                ((Player)instance.GetFieldValue("playerToUpgrade")).GetComponent<HealthHandler>().Revive(true);
                return;
            }
            else
            {
                // compensate for health and respawns change immediately
                ((Player)instance.GetFieldValue("playerToUpgrade")).GetComponent<CharacterData>().health *= ((CharacterStatModifiers)instance.GetFieldValue("myPlayerStats")).health;
                ((Player)instance.GetFieldValue("playerToUpgrade")).GetComponent<CharacterData>().stats.remainingRespawns += ((CharacterStatModifiers)instance.GetFieldValue("myPlayerStats")).respawns;
                return;
            }

        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_revive = ExtensionMethods.GetMethodInfo(typeof(HealthHandler), nameof(HealthHandler.Revive));
            var m_newRevive = ExtensionMethods.GetMethodInfo(typeof(ApplyCardStats_Patch_ApplyStats), nameof(ReviveOrCompensate));
            foreach (var code in instructions)
            {
                if (code.Calls(m_revive))
                {
                    // instead of calling HealthHandler::Revive(true), call ReviveOrCompensate(this)
                    yield return new CodeInstruction(OpCodes.Pop); // pop `true` off the stack
                    yield return new CodeInstruction(OpCodes.Pop); // pop `healthHandler` off the stack
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // load the ApplyCardStats instance
                    yield return new CodeInstruction(OpCodes.Call, m_newRevive); // call the new revive method
                }
                else
                {
                    yield return code;
                }
            }
        }
    }
}
