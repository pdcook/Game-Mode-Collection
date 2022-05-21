using System.Linq;
using BetterChat;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using UnboundLib.GameModes;

namespace GameModeCollection.Patches
{
    // patch to replace BetterChat's chat handler with one that uses custom player names for TRT
    
    [HarmonyPatch(typeof(Photon.Realtime.Player), nameof(Photon.Realtime.Player.NickName), MethodType.Getter)]
    class PhotonPlayerPatchNickNameGetter
    {
        private static bool Prefix(Photon.Realtime.Player __instance, ref string __result)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return true; }
            Player player = PlayerManager.instance.GetPlayerWithActorID(__instance.ActorNumber);
            if (player?.data is null || string.IsNullOrEmpty(player.data.GetData().forcedNickName)) { return true; }
            __result = player.data.GetData().forcedNickName;
            return false;
        }
    }
}
