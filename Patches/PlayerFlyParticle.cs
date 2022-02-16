using UnityEngine;
using HarmonyLib;
using UnboundLib;
using GameModeCollection.GameModes.TRT;
namespace GameModeCollection.Patches
{
    class PlayerFlyParticle_Patch_Update
    {
        static bool Prefix(PlayerFlyParticle __instance)
        {
            if (__instance != null && __instance.GetComponentInParent<PhantomHaunt>())
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
