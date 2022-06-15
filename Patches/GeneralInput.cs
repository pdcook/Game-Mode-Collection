using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using HarmonyLib;
using UnityEngine;
using UnboundLib.GameModes;
using LocalZoom;
using System.Collections.Generic;
using System;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(GeneralInput), "Update")]
    class GeneralInputPatchUpdate
    {
        private static void Postfix(GeneralInput __instance)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || !__instance.GetComponent<CharacterData>().view.IsMine || BetterChat.BetterChat.isLockingInput) { return; }

            PlayerActionsAdditionalData data = __instance.GetComponent<CharacterData>().playerActions.GetAdditionalData();

            // TRT traitor VC push-to-talk
            data.trt_traitor_ptt_is_held = data.trt_traitor_chat_ptt.IsPressed;

            // TRT inspect body
            if (data.trt_inspect_body.WasPressed)
            {
                TRTHandler.TryInspectBody(__instance.GetComponent<Player>(), false);
            }

            // interact
            if (data.interact.WasPressed)
            {
                data.try_interact_was_pressed = true;
                TRTHandler.TryInspectBody(__instance.GetComponent<Player>(), true);
            }
            else
            {
                data.try_interact_was_pressed = false;
            }
            data.try_interact_is_pressed = data.interact.IsPressed;
            data.try_interact_was_released = data.interact.WasReleased;

            // actions that can be modified by the modifier button/key
            if (data.modifier.IsPressed)
            {
                data.trt_item0_is_pressed = data.discard_last_card_mod_item0.IsPressed;
                data.trt_item0_was_pressed = data.discard_last_card_mod_item0.WasPressed;
                data.trt_item1_is_pressed = data.trt_radio_imwith_mod_item1.IsPressed;
                data.trt_item1_was_pressed = data.trt_radio_imwith_mod_item1.WasPressed;
                data.trt_item2_is_pressed = data.trt_radio_traitor_mod_item2.IsPressed;
                data.trt_item2_was_pressed = data.trt_radio_traitor_mod_item2.WasPressed;
                data.trt_item3_is_pressed = data.trt_radio_suspect_mod_item3.IsPressed;
                data.trt_item3_was_pressed = data.trt_radio_suspect_mod_item3.WasPressed;
                data.trt_item4_is_pressed = data.trt_radio_innocent_mod_item4.IsPressed;
                data.trt_item4_was_pressed = data.trt_radio_innocent_mod_item4.WasPressed;
                data.trt_item5_is_pressed = data.trt_shop_mod_item5.IsPressed;
                data.trt_item5_was_pressed = data.trt_shop_mod_item5.WasPressed;

                // traitor PTT is LB + RB for controller players
                try
                {
                    PlayerActions actions = __instance.GetComponent<CharacterData>().playerActions;
                    data.trt_traitor_ptt_is_held = actions[LocalZoom.Extensions.PlayerActionsExtension.GetAdditionalData(actions).modifier.Name].IsPressed;
                }
                catch (KeyNotFoundException e)
                {
                    GameModeCollection.LogError($"LocalZoom modifier keybind not found. Full error: {e}");
                }
                catch (Exception e)
                {
                    GameModeCollection.LogError($"Unknown exception while accessing LocalZoom modifier keybind. Full error: {e}");
                }
            }
            else
            {
                // TRT radios
                if (data.trt_radio_imwith_mod_item1.WasPressed)
                {
                    TRTHandler.ImWith(__instance.GetComponent<Player>());
                }
                if (data.trt_radio_traitor_mod_item2.WasPressed)
                {
                    TRTHandler.IsTraitor(__instance.GetComponent<Player>());
                }
                if (data.trt_radio_suspect_mod_item3.WasPressed)
                {
                    TRTHandler.IsSuspect(__instance.GetComponent<Player>());
                }
                if (data.trt_radio_innocent_mod_item4.WasPressed)
                {
                    TRTHandler.IsInnocent(__instance.GetComponent<Player>());
                }

                // discard cards
                data.try_discard = data.discard_last_card_mod_item0.WasPressed;

                // direct item keybinds
                data.trt_item0_is_pressed = data.trt_item0.IsPressed;
                data.trt_item0_was_pressed = data.trt_item0.WasPressed;
                data.trt_item1_is_pressed = data.trt_item1.IsPressed;
                data.trt_item1_was_pressed = data.trt_item1.WasPressed;
                data.trt_item2_is_pressed = data.trt_item2.IsPressed;
                data.trt_item2_was_pressed = data.trt_item2.WasPressed;
                data.trt_item3_is_pressed = data.trt_item3.IsPressed;
                data.trt_item3_was_pressed = data.trt_item3.WasPressed;
                data.trt_item4_is_pressed = data.trt_item4.IsPressed;
                data.trt_item4_was_pressed = data.trt_item4.WasPressed;
                data.trt_item5_is_pressed = data.trt_item5.IsPressed;
                data.trt_item5_was_pressed = data.trt_item5.WasPressed;

                // item shop
                if (data.trt_shop_mod_item5.WasPressed && __instance.GetComponent<CharacterData>().GetData().playerCanAccessShop)
                {
                    __instance.GetComponent<ITRT_Role>()?.TryShop();
                }
            }
        }
    }
}
