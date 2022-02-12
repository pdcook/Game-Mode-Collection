using HarmonyLib;
using UnityEngine;
using UnboundLib.Utils;
namespace GameModeCollection.Patches
{
    
    [HarmonyPatch(typeof(BetterChat.ChatMonoGameManager), nameof(BetterChat.ChatMonoGameManager.CreateLocalMessage))]
    [HarmonyPriority(Priority.First)]
    [HarmonyBefore("com.bosssloth.rounds.BetterChat")]
    class BetterChat_ChatMonoGameManager
    {
        static bool Prefix(ref string message, ref string playerName, int colorID)
        {
            // WHY DO NEITHER OF THESE PATCHES WORK??
            return true;
            /*
            if (GameModeCollection.UsePlayerColorsInsteadOfNamesInChat)
            {
                playerName = ExtraPlayerSkins.GetTeamColorName(colorID);
            }
            return true;
            */


            /*
            if (!GameModeCollection.SeparateChatForDeadPlayers || !message.Contains(GameModeCollection.SecretDeadPlayerChatKey))
            {
                return true;
            }
            message = message.Replace(GameModeCollection.SecretDeadPlayerChatKey, "");
            playerName = $"<b><color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>[DEAD]</color></b> {playerName}";
            return PlayerManager.instance.players.Find(p => p.data.view.IsMine)?.data?.dead ?? false;
            */
        }
    }
}
