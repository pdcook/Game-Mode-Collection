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
            int num = -1;
            if (hit.transform)
            {
                PhotonView component = hit.transform.root.GetComponent<PhotonView>();
                if (component)
                {
                    num = component.ViewID;
                }
            }
            int num2 = -1;
            if (num == -1)
            {
                Collider2D[] componentsInChildren = MapManager.instance.currentMap.Map.GetComponentsInChildren<Collider2D>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    if (componentsInChildren[i] == hit.collider)
                    {
                        num2 = i;
                    }
                }
            }
            HealthHandler healthHandler = null;
            if (hit.transform)
            {
                healthHandler = hit.transform.GetComponent<HealthHandler>();
            }
            if (healthHandler)
            {
                Player hitPlayer = healthHandler.GetComponent<Player>();
                // if the hit player is not null
                if (hitPlayer != null)
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