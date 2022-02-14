using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using HarmonyLib;
using UnityEngine;
using UnboundLib.GameModes;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(GeneralInput), "Update")]
    class GeneralInputPatchUpdate
    {
        private static void Postfix(GeneralInput __instance)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || !__instance.GetComponent<CharacterData>().view.IsMine) { return; }

            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_inspect_body.WasPressed)
            {
                TRTHandler.TryInspectBody(__instance.GetComponent<Player>(), false);
            }
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_interact_with_body.WasPressed)
            {
                TRTHandler.TryInspectBody(__instance.GetComponent<Player>(), true);
            }
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_imwith.WasPressed)
            {
                TRTHandler.ImWith(__instance.GetComponent<Player>());
            }
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_traitor.WasPressed)
            {
                TRTHandler.IsTraitor(__instance.GetComponent<Player>());
            }
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_suspect.WasPressed)
            {
                TRTHandler.IsSuspect(__instance.GetComponent<Player>());
            }
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_innocent.WasPressed)
            {
                TRTHandler.IsInnocent(__instance.GetComponent<Player>());
            }
        }
    }
}
