using HarmonyLib;
using Photon.Pun;
namespace GameModeCollection.Patches
{
    /*
    [HarmonyPatch(typeof(DevConsole), "RPCA_SendChat")]
    [HarmonyPriority(Priority.First)]
    [HarmonyBefore("com.bosssloth.rounds.BetterChat")]
    class DevConsole_Patch_RPCA_SendChat
    {
        static bool Prefix(string message, int playerViewID)
        {
            var player = PhotonNetwork.GetPhotonView(playerViewID);
            if (GameModeCollection.SeparateChatForDeadPlayers && (player?.GetComponent<Player>()?.data?.dead ?? false) && !player.IsMine)
            {
                MenuControllerHandler.instance.GetComponent<PhotonView>().RPC("RPCA_CreateMessage", RpcTarget.All, player.Owner.NickName, player.GetComponent<Player>().teamID, $"{message}{GameModeCollection.SecretDeadPlayerChatKey}");
                return false;
            }
            return true;
        }
    }*/
}
