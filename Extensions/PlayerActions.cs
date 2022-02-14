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
        public PlayerAction trt_interact_with_body; // interact with a body if near enough
        public PlayerAction trt_radio_suspect; // reports "<playerColor> is suspicious."
        public PlayerAction trt_radio_imwith; // reports "I'm with <playerColor>."
        public PlayerAction trt_radio_traitor; // reports "<playerColor> is a traitor!"
        public PlayerAction trt_radio_innocent; // reports "<playerColor> is innocent."
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
    }
}
