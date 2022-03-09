using HarmonyLib;
using GameModeCollection.GameModes.TRT.Cards;
using UnityEngine;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Player), "FullReset")]
    class PlayerPatchFullReset
    {
        static void Prefix(Player __instance)
        {
            // hide claw just to be sure
            try
            {
                A_Claw.MakeGunClaw(__instance.playerID, false);
            }
            catch { }
            // hide knife just to be sure
            try
            {
                A_Knife.MakeGunKnife(__instance.playerID, false);
            }
            catch { }
            try
            {
                if (__instance.gameObject.GetComponent<GoldenDeagleGun>() != null)
                {
                    GameObject.Destroy(__instance.gameObject.GetComponent<GoldenDeagleGun>());
                }
            }
            catch { }

        }
    }
}
