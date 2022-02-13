using HarmonyLib;
using UnityEngine;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;
using System;
namespace GameModeCollection.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.TakeDamage), new Type[] {typeof(Vector2), typeof(Vector2), typeof(GameObject), typeof(Player), typeof(bool), typeof(bool)})]
    class Destructible_Patch_TakeDamage_TRT_NoDamage
    {
        // patch for TRT roles that cannot deal damage
        private static void Prefix(ref Vector2 damage, Player damagingPlayer = null)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer?.GetComponent<ITRT_Role>() is null) { return; }

            if (!damagingPlayer.GetComponent<ITRT_Role>().CanDealDamage)
            {
                damage = Vector2.zero;
            }
        }
    }
}
