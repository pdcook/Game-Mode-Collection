using UnityEngine;
using HarmonyLib;
using UnboundLib;
using GameModeCollection.Utils.UI;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(ProjectileHit), nameof(ProjectileHit.Hit))]
    class ProjectileHit_Patch_Hit
    {
        static void Postfix(ProjectileHit __instance, HitInfo hit)
        {
            CrownHandler crownHandler = hit?.collider?.GetComponent<CrownHandler>();
            if (crownHandler != null)
            {
                crownHandler.TakeForce(hit.point, __instance.force * (Vector2)__instance.transform.forward);
            }
        }
    }
}
