using UnityEngine;
using HarmonyLib;
using UnboundLib;
using GameModeCollection.GameModes.TRT;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(PlayerFlyParticle), "Update")]
    [HarmonyPriority(Priority.First)]
    class PlayerFlyParticle_Patch_Update
    {
        static bool Prefix(PlayerFlyParticle __instance)
        {
            if (__instance != null && __instance.transform.root.GetComponentInChildren<PhantomHaunt>() != null)
            {
                if (!(__instance?.GetComponent<ParticleSystem>()?.isPlaying ?? true))
                {
                    __instance.GetComponent<ParticleSystem>().Play();
                }
                return false;
            }
            return true;
        }
    }
}
