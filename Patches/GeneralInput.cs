using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
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
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || !__instance.GetComponent<CharacterData>().view.IsMine || BetterChat.BetterChat.isLockingInput) { return; }

            // TRT inspect body
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_inspect_body.WasPressed)
            {
                TRTHandler.TryInspectBody(__instance.GetComponent<Player>(), false);
            }

            // interact
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().interact.WasPressed)
            {
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().try_interact_was_pressed = true;
                TRTHandler.TryInspectBody(__instance.GetComponent<Player>(), true);
            }
            else
            {
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().try_interact_was_pressed = false;
            }
            __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().try_interact_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().interact.IsPressed;
            __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().try_interact_was_released = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().interact.WasReleased;

            // actions that can be modified by the modifier button/key
            if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().modifier.IsPressed)
            {
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item0_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().discard_last_card_mod_item0.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item0_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().discard_last_card_mod_item0.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item1_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_imwith_mod_item1.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item1_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_imwith_mod_item1.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item2_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_traitor_mod_item2.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item2_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_traitor_mod_item2.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item3_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_suspect_mod_item3.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item3_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_suspect_mod_item3.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item4_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_innocent_mod_item4.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item4_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_innocent_mod_item4.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item5_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_shop_mod_item5.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item5_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_shop_mod_item5.WasPressed;
            }
            else
            {
                // TRT radios
                if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_imwith_mod_item1.WasPressed)
                {
                    TRTHandler.ImWith(__instance.GetComponent<Player>());
                }
                if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_traitor_mod_item2.WasPressed)
                {
                    TRTHandler.IsTraitor(__instance.GetComponent<Player>());
                }
                if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_suspect_mod_item3.WasPressed)
                {
                    TRTHandler.IsSuspect(__instance.GetComponent<Player>());
                }
                if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_radio_innocent_mod_item4.WasPressed)
                {
                    TRTHandler.IsInnocent(__instance.GetComponent<Player>());
                }

                // discard cards
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().try_discard = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().discard_last_card_mod_item0.WasPressed;

                // direct item keybinds
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item0_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item0.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item0_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item0.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item1_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item1.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item1_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item1.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item2_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item2.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item2_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item2.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item3_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item3.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item3_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item3.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item4_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item4.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item4_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item4.WasPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item5_is_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item5.IsPressed;
                __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item5_was_pressed = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_item5.WasPressed;

                // item shop
                if (__instance.GetComponent<CharacterData>().playerActions.GetAdditionalData().trt_shop_mod_item5.WasPressed)
                {
                    __instance.GetComponent<ITRT_Role>()?.TryShop();
                }
            }
        }
    }
}
