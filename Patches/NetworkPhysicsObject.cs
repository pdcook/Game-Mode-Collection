using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnboundLib;
using UnityEngine;
namespace GameModeCollection.Patches
{
    // patches to remove collision damage if the gamemode option is set
    [HarmonyPatch(typeof(NetworkPhysicsObject))]
    class NetworkPhysicsObjectPatch
    {
        static void CallTakeDamageIfEnabled(HealthHandler healthHandler, Vector2 damage, Vector2 damagePosition, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            if (!GameModeCollection.DisableColliderDamage)
            {
                healthHandler.CallTakeDamage(damage, damagePosition, damagingWeapon, damagingPlayer, lethal);
            }
        }
        // patch to remove initial collision damage
        [HarmonyTranspiler]
        [HarmonyPatch("OnPlayerCollision")]
        static IEnumerable<CodeInstruction> OnPlayerCollisionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_callTakeDamage = ExtensionMethods.GetMethodInfo(typeof(Damagable), nameof(Damagable.CallTakeDamage), new System.Type[] {typeof(Vector2), typeof(Vector2), typeof(GameObject), typeof(Player), typeof(bool)} );
            var m_callTakeDamageIfEnabled = ExtensionMethods.GetMethodInfo(typeof(NetworkPhysicsObjectPatch), nameof(NetworkPhysicsObjectPatch.CallTakeDamageIfEnabled));
            
            // replace all occurances of CallTakeDamage with CallTakeDamageIfEnabled
            foreach (var code in instructions)
            {
                if (code.Calls(m_callTakeDamage))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_callTakeDamageIfEnabled);
                }
                else
                {
                    yield return code;
                }
            }
        }
    }
}
