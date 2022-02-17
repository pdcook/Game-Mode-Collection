using HarmonyLib;
using UnityEngine;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
namespace GameModeCollection.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(DamagableEvent), "DoDamage")]
    class DamagableEvent_Patch_DoDamage_TRT_NoDamage
    {
        // patch for TRT roles that cannot deal damage
        private static void Prefix(ref Vector2 damage, Player damagingPlayer = null)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer?.GetComponent<ITRT_Role>() is null) { return; }

            if (!damagingPlayer.GetComponent<ITRT_Role>().CanDealDamageAndTakeEnvironmentalDamage)
            {
                damage = Vector2.zero;
            }
        }
    }
}
