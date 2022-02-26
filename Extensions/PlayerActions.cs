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
        public PlayerAction trt_radio_suspect; // reports "<playerColor> is suspicious."
        public PlayerAction trt_radio_imwith; // reports "I'm with <playerColor>."
        public PlayerAction trt_radio_traitor; // reports "<playerColor> is a traitor!"
        public PlayerAction trt_radio_innocent; // reports "<playerColor> is innocent."
        public PlayerAction trt_shop; // open the TRT shop
        public PlayerAction discard_last_card; // discards the player's last card
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

        public static PlayerAction Interact(this PlayerActions playerActions) => playerActions.GetAdditionalData().interact;
        public static bool Discard(this PlayerActions playerActions) => playerActions.GetAdditionalData().discard_last_card.WasPressed;
    }
}
