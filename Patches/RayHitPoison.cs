using GameModeCollection.Objects.GameModeObjects;
using UnityEngine;
using HarmonyLib;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(RayHitPoison), nameof(RayHitPoison.DoHitEffect))]
    class RayHitPoison_Patch_DoHitEffect
    {
        // patch to allow damagable items to take damage over time
        static void Postfix(RayHitPoison __instance, HitInfo hit)
        {
            if (!hit.transform)
            {
                return;
            }
            RayHitPoison[] componentsInChildren = __instance.transform.root.GetComponentsInChildren<RayHitPoison>();
            ProjectileHit componentInParent = __instance.GetComponentInParent<ProjectileHit>();
            hit.transform.GetComponent<DeathObjectDamagable>()?.TakeDamageOverTime(componentInParent.damage * __instance.transform.forward / (float)componentsInChildren.Length, __instance.transform.position, __instance.time, __instance.interval, __instance.color, __instance.soundEventDamageOverTime, __instance.GetComponentInParent<ProjectileHit>().ownWeapon, __instance.GetComponentInParent<ProjectileHit>().ownPlayer, true);
        }

    }
}
