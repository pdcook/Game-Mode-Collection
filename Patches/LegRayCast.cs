using HarmonyLib;
using UnityEngine;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(LegRaycasters))]
    public class LegRayCastPatch
    {
        [HarmonyPatch("Start")]
        public static void Postfix(LegRaycasters __instance)
        {
            __instance.mask |= 1 << LayerMask.NameToLayer("PlayerObjectCollider");
        }
    }
}