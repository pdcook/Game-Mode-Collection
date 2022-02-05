using HarmonyLib;
using UnityEngine;
using Photon.Pun;

namespace GameModeCollection.Patches
{

    [HarmonyPatch(typeof(ProjectileHit), "Hit")]
    [HarmonyPriority(Priority.First)]
    class ProjectileHitPatchHit
    {
        // disable friendly fire or self damage and hit effects if that setting is enabled
        private static bool Prefix(ProjectileHit __instance, HitInfo hit, bool forceCall)
        {
            HealthHandler healthHandler = null;
            if (hit?.transform)
            {
                healthHandler = hit.transform.GetComponent<HealthHandler>();
            }
            if (healthHandler)
            {
                Player hitPlayer = healthHandler.GetComponent<Player>();
                // if the hit player is not null
                if (hitPlayer != null && __instance?.ownPlayer != null)
                {
                    // self-damage
                    if (hitPlayer.playerID == __instance.ownPlayer.playerID && !GameModeCollection.SelfDamageAllowed)
                    {
                        return false;
                    }
                    // friendly fire
                    else if (hitPlayer.playerID != __instance.ownPlayer.playerID && hitPlayer.teamID == __instance.ownPlayer.teamID && !GameModeCollection.TeamDamageAllowed)
                    {
                        return false;
                    }
                    // enemy fire
                    else if (hitPlayer.playerID != __instance.ownPlayer.playerID && hitPlayer.teamID != __instance.ownPlayer.teamID && !GameModeCollection.EnemyDamageAllowed)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

}