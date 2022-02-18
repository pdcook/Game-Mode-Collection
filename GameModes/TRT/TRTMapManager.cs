using MapsExt;
using GameModeCollection.Extensions;
using Photon.Pun;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using System.Collections.Generic;
namespace GameModeCollection.GameModes.TRT
{
    public static class TRTMapManager
    {
        public static List<CustomMap> Maps { get; internal set; }

        public static string CurrentLevel { get; private set; } = null;

        private static PhotonView View => (PhotonView)MapManager.instance.GetFieldValue("view");

        public static void LoadNextTRTLevel(bool callInImmediately = false, bool forceLoad = false)
        {
            if (TRTMapManager.Maps == null || TRTMapManager.Maps.Count == 0)
            {
                MapManager.instance.LoadNextLevel(callInImmediately, forceLoad);
                return;
            }

            if (!forceLoad && !PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
            {
                return;
            }

            TRTMapManager.CurrentLevel = Maps.GetRandom<CustomMap>().id;

            View.RPC("RPCA_SetCallInNextMap", RpcTarget.All, callInImmediately);
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{TRTMapManager.CurrentLevel}");
            NetworkingManager.RPC(typeof(TRTMapManager), nameof(RPCA_SetCurrentLevel), TRTMapManager.CurrentLevel);
        }
        public static void LoadTRTLevelFromID(string ID, bool onlyMaster = false, bool callInImmediately = false)
        {
            if (!TRTMapManager.Maps.Select(m => m.id).Contains(ID))
            {
                TRTMapManager.LoadNextTRTLevel(callInImmediately, onlyMaster);
                return;
            }
            if (!PhotonNetwork.IsMasterClient && onlyMaster) { return; }
            MapManager.instance.SetFieldValue("callInNextMap", callInImmediately);
            TRTMapManager.CurrentLevel = ID;
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{ID}");
            NetworkingManager.RPC(typeof(TRTMapManager), nameof(RPCA_SetCurrentLevel), TRTMapManager.CurrentLevel);
            
        }
        [UnboundRPC]
        private static void RPCA_SetCurrentLevel(string currentLevel)
        {
            TRTMapManager.CurrentLevel = currentLevel;
        }
    }
}
