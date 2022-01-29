using HarmonyLib;
using UnityEngine;

namespace GameModeCollection.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandlerPatchDoDamage
    {
        // prefix to disable damage for friendly fire and self damage
        private static bool Prefix(HealthHandler __instance, Vector2 damage, Vector2 position, Color blinkColor, GameObject damagingWeapon = null, Player damagingPlayer = null, bool healthRemoval = false, bool lethal = true, bool ignoreBlock = false)
        {
            Player ownPlayer = __instance.GetComponent<Player>();
            if (damagingPlayer != null && ownPlayer != null)
            {
                // self-damage
                if (damagingPlayer.playerID == ownPlayer.playerID && !GameModeCollection.SelfDamageAllowed)
                {
                    return false;
                }
                // friendly fire
                else if (damagingPlayer.playerID != ownPlayer.playerID && damagingPlayer.teamID == ownPlayer.teamID && !GameModeCollection.TeamDamageAllowed)
                {
                    return false;
                }
                // enemy fire
                else if (damagingPlayer.playerID != ownPlayer.playerID && damagingPlayer.teamID != ownPlayer.teamID && !GameModeCollection.EnemyDamageAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
