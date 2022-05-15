using GameModeCollection.Extensions;
using HarmonyLib;
using SoundImplementation;
using UnboundLib;
using UnityEngine;
using GameModeCollection.Objects;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(SoundGun))]
    class SoundGunPatch
    {
        /// <summary>
        /// patches for silenced weapons
        /// </summary>

        const float eps = 0.1f;

        static bool CanSeePlayer(Player player, Vector2 position)
        {

            RaycastHit2D[] array = Physics2D.RaycastAll(position, (player.data.playerVel.position - position).normalized, Vector2.Distance(position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].transform
                    && !array[i].transform.root.GetComponent<SpawnedAttack>()
                    && !array[i].transform.root.GetComponent<Player>()
                    && !array[i].transform.root.GetComponentInChildren<PhysicsItem>()
                    && !(Vector2.Distance(array[i].point, position) < eps)
                    )
                {
                    return false;
                }
            }
            return true;
        }

        static bool ShouldPlaySound(SoundGun soundGun, Vector3? position = null)
        {
            Gun gun = (Gun)soundGun.GetFieldValue("parentGun");
            if (gun is null || !gun.GetData().silenced)
            {
                // if the gun is not silenced, or if the gun is null, just play the sound normally
                return true;
            }
            else
            {
                Vector3 soundPos = position ?? gun.transform.position;
                // if the gun is silenced, check to see if the local player has LoS
                Player localPlayer = PlayerManager.instance.GetLocalPlayer();
                if (localPlayer is null || localPlayer.data.dead || CanSeePlayer(localPlayer, soundPos))
                {
                    GameModeCollection.Log("PLAY SOUND");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(SoundGun.PlayImpact))]
        static bool PlayImpact(SoundGun __instance, HitInfo hit)
        {
            return ShouldPlaySound(__instance, hit.point);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayImpactModifiers")]
        static bool PlayImpactModifiers(SoundGun __instance, Vector2 position)
        {
            return ShouldPlaySound(__instance, position);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(SoundGun.PlayShot))]
        static bool PlayShot(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayShotgun")]
        static bool PlayShotgun(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayShotgunAuto")]
        static bool PlayShotgunAuto(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlaySingle")]
        static bool PlaySingle(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlaySingleAuto")]
        static bool PlaySingleAuto(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }            
    }
}
