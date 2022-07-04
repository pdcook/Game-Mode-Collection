using System;
using System.Runtime.CompilerServices;
using InControl;

namespace GameModeCollection.Extensions
{
    // this extension stores ONLY the data for additional player actions
    // additional actions are assigned in the PlayerActions patches
    public class PlayerActionsAdditionalData
    {
        public PlayerAction trt_inspect_body; // inspect a body if near enough
        public PlayerAction interact; // interact with a body if near enough
        public PlayerAction trt_radio_suspect_mod_item3; // reports "<playerColor> is suspicious."
        public PlayerAction trt_radio_imwith_mod_item1; // reports "I'm with <playerColor>."
        public PlayerAction trt_radio_traitor_mod_item2; // reports "<playerColor> is a traitor!"
        public PlayerAction trt_radio_innocent_mod_item4; // reports "<playerColor> is innocent."
        public PlayerAction trt_shop_mod_item5; // open the TRT shop
        public PlayerAction discard_last_card_mod_item0; // discards the player's last card
        public PlayerAction trt_traitor_chat_ptt; // traitor chat push-to-talk key
        public bool try_discard = false;
        public bool try_interact_was_pressed = false;
        public bool try_interact_is_pressed = false;
        public bool try_interact_was_released = false;
        public bool trt_traitor_ptt_is_held = false;
        public PlayerAction modifier; // for controller only, when this button is held it changes the actions of many of the controller buttons

        // TRT items (unassigned on controller, accessible through modifier combos only)
        public PlayerAction trt_item0;
        public PlayerAction trt_item1;
        public PlayerAction trt_item2;
        public PlayerAction trt_item3;
        public PlayerAction trt_item4;
        public PlayerAction trt_item5;
        public bool trt_item0_is_pressed = false;
        public bool trt_item1_is_pressed = false;
        public bool trt_item2_is_pressed = false;
        public bool trt_item3_is_pressed = false;
        public bool trt_item4_is_pressed = false;
        public bool trt_item5_is_pressed = false;
        public bool trt_item0_was_pressed = false;
        public bool trt_item1_was_pressed = false;
        public bool trt_item2_was_pressed = false;
        public bool trt_item3_was_pressed = false;
        public bool trt_item4_was_pressed = false;
        public bool trt_item5_was_pressed = false;

        // TRT round summary toggle
        //public PlayerAction toggle_summary;

        // role help key
        public PlayerAction role_help;
    }
    public static class PlayerActionsExtension
    {
        public static readonly ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData> data =
            new ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData>();

        public static PlayerActionsAdditionalData GetAdditionalData(this PlayerActions playerActions)
        {
            return data.GetOrCreateValue(playerActions);
        }

        public static void AddData(this PlayerActions playerActions, PlayerActionsAdditionalData value)
        {
            try
            {
                data.Add(playerActions, value);
            }
            catch (Exception) { }
        }

        public static bool InteractWasPressed(this PlayerActions playerActions) => playerActions.GetAdditionalData().try_interact_was_pressed;
        public static bool InteractIsPressed(this PlayerActions playerActions) => playerActions.GetAdditionalData().try_interact_is_pressed;
        public static bool InteractWasReleased(this PlayerActions playerActions) => playerActions.GetAdditionalData().try_interact_was_released;
        public static bool Discard(this PlayerActions playerActions) => playerActions.GetAdditionalData().try_discard;
        public static bool TraitorPTTIsHeld(this PlayerActions playerActions) => playerActions.GetAdditionalData().trt_traitor_ptt_is_held;

        public static bool ItemIsPressed(this PlayerActions playerActions, int itemNum)
        {
            switch (itemNum)
            {
                case 0:
                    return playerActions.GetAdditionalData().trt_item0_is_pressed;
                case 1:
                    return playerActions.GetAdditionalData().trt_item1_is_pressed;
                case 2:
                    return playerActions.GetAdditionalData().trt_item2_is_pressed;
                case 3:
                    return playerActions.GetAdditionalData().trt_item3_is_pressed;
                case 4:
                    return playerActions.GetAdditionalData().trt_item4_is_pressed;
                case 5:
                    return playerActions.GetAdditionalData().trt_item5_is_pressed;
                default:
                    return false;
            }
        }
        public static bool ItemWasPressed(this PlayerActions playerActions, int itemNum)
        {
            switch (itemNum)
            {
                case 0:
                    return playerActions.GetAdditionalData().trt_item0_was_pressed;
                case 1:
                    return playerActions.GetAdditionalData().trt_item1_was_pressed;
                case 2:
                    return playerActions.GetAdditionalData().trt_item2_was_pressed;
                case 3:
                    return playerActions.GetAdditionalData().trt_item3_was_pressed;
                case 4:
                    return playerActions.GetAdditionalData().trt_item4_was_pressed;
                case 5:
                    return playerActions.GetAdditionalData().trt_item5_was_pressed;
                default:
                    return false;
            }
        }
    }
}
