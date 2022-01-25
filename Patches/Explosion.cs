using HarmonyLib;
using UnityEngine;
using GameModeCollection.Objects;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using Photon.Pun;
using UnboundLib;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Explosion), nameof(Explosion.Explode))]
    class Explosion_Patch_Explode
    {

        static void DoPhysicsItemExplosions(Explosion instance, Collider2D[] colliders)
        {
            // this patch is only necessary if items are on the layer ignored by explosions, layer 19
            if (PhysicsItem.Layer != 19) { return; }

            float radius = instance.scaleRadius ? instance.transform.localScale.x : 1f;
            foreach (Collider2D collider in colliders.Where(c => c.gameObject.layer == PhysicsItem.Layer))
            {
                if (collider?.GetComponentInParent<PhysicsItem>() != null && ((bool)collider?.GetComponentInParent<PhotonView>()?.IsMine || PhotonNetwork.OfflineMode))
                {
                    float distance = Vector2.Distance(instance.transform.position, collider.bounds.ClosestPoint(instance.transform.position));
                    float rangeMultiplier = 1f - distance / (instance.range * radius);
                    if (instance.staticRangeMultiplier)
                    {
                        rangeMultiplier = 1f;
                    }
                    rangeMultiplier = Mathf.Clamp(rangeMultiplier, 0f, 1f);

                    instance.InvokeMethod("DoExplosionEffects", collider, collider?.GetComponentInParent<PhysicsItem>().PhysicalProperties.PhysicsImpulseMult * rangeMultiplier, distance);
                }
            }
        }

        // patch to allow explosions to effect PhysicsItems
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            /* NOTES:
             * the "array" of Collider2Ds can be accessed with ldloc.1
             * the current index of the loop can be access with ldloc.2
             * the sequence: ldloc.1, ldloc.2, ldelem.ref will load a reference to the collider at index i in the array, as type "object"
             */

            // we could use a postfix, but it would be inefficient since it would require multiple Physics2D::OverlapCircleAll calls
            // it would be even more efficient to just add an else block to the `if <...>.layer != 19`, but this is pretty complicated in CIL

            var m_DoPhysicsItemExplosions = ExtensionMethods.GetMethodInfo(typeof(Explosion_Patch_Explode), nameof(Explosion_Patch_Explode.DoPhysicsItemExplosions));

            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ret)
                {
                    // add call to our method
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, m_DoPhysicsItemExplosions);
                }
                yield return code;
            }

        }
    }
}
