using MapsExt;
using GameModeCollection.Extensions;
using Photon.Pun;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using GameModeCollection.GameModes.TRT.Controllers;
using MapEmbiggener.Controllers;
namespace GameModeCollection.GameModes.TRT
{
    public static class TRTMapManager
    {
        // map files must be saved as
        // <mapname>_scale_<scale>_bounds_<x mult>_<y mult>.trtmap
        // scale and ratio are optional and determine what size to embiggen the map to as well as the ratio of the bounds
        // for example:
        //      TRT_Map.trtmap will use the default scaling (set in mod options) and bounds ratio (16:9)
        //      TRT_Map2_scale_3.trtmap will be scaled 3x and use the default bounds for the 3x scaled map
        //      TRT_Map3_bounds_2_1.trtmap will use the default scaling and will double the normal bounds on the x axis so the bounds ratio will be 32:9


        public static Dictionary<string, CustomMap> MapDict { get; internal set; } = new Dictionary<string, CustomMap>() { };
        public static List<CustomMap> Maps => MapDict.Values.ToList();

        public static string CurrentLevel { get; private set; } = null;

        private static PhotonView View => (PhotonView)MapManager.instance.GetFieldValue("view");

        public static string GetFileNameFromMap(CustomMap map)
        {
            try
            {
                return MapDict.First(kv => kv.Value.id == map.id).Key;
            }
            catch
            {
                return "";
            }
        }
        public static CustomMap GetMapFromMapID(string mapID)
        {
            try
            {
                return MapDict.First(kv => kv.Value.id == mapID).Value;
            }
            catch
            {
                return null;
            }
        }
        public static float? GetMapScale(CustomMap map)
        {
            return string.IsNullOrWhiteSpace(GetFileNameFromMap(map)) ? null : GetMapScale(GetFileNameFromMap(map));
        }
        public static Vector2 GetMapBounds(CustomMap map)
        {
            return string.IsNullOrWhiteSpace(GetFileNameFromMap(map)) ? Vector2.one : GetMapBounds(GetFileNameFromMap(map));
        }

        public static float? GetMapScale(string mapName)
        {
            string[] parts = mapName.Replace(".trtmap", "").Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower().Equals("scale"))
                {
                    try
                    {
                        return float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    { }
                }
            }
            return null;
        }
        public static Vector2 GetMapBounds(string mapName)
        {
            Vector2 bounds = Vector2.one;
            string[] parts = mapName.Replace(".trtmap", "").Split('_');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].ToLower().Equals("bounds"))
                {
                    try
                    {
                        bounds[0] = float.Parse(parts[i + 1], System.Globalization.CultureInfo.InvariantCulture);
                        bounds[1] = float.Parse(parts[i + 2], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    { }
                }
            }
            return bounds;
        }

        public static IEnumerator LoadNextTRTLevel(bool callInImmediately = false, bool forceLoad = false)
        {
            if (TRTMapManager.Maps == null || TRTMapManager.Maps.Count == 0)
            {
                MapManager.instance.LoadNextLevel(callInImmediately, forceLoad);
                yield break;
            }

            if (!forceLoad && !PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
            {
                yield return new WaitForSecondsRealtime(1f);
                yield break;
            }

            if (Maps.Count > 1)
            {
                TRTMapManager.CurrentLevel = Maps.Where(m => m.id != TRTMapManager.CurrentLevel).ToList().GetRandom<CustomMap>().id;
            }
            else
            {
                TRTMapManager.CurrentLevel = Maps.GetRandom<CustomMap>().id;
            }

            // tell the map controller of the upcoming map so it can set the mapscale properly, then give it time to sync
            if (ControllerManager.CurrentMapControllerID == TRTMapController.ControllerID)
            {
                ((TRTMapController)ControllerManager.CurrentMapController).SetUpcomingMap(GetMapFromMapID(TRTMapManager.CurrentLevel));
            }
            // only needs 10 frames to sync
            yield return new WaitForSecondsRealtime(1f);

            View.RPC("RPCA_SetCallInNextMap", RpcTarget.All, callInImmediately);
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{TRTMapManager.CurrentLevel}");
            NetworkingManager.RPC(typeof(TRTMapManager), nameof(RPCA_SetCurrentLevel), TRTMapManager.CurrentLevel);
        }
        public static IEnumerator ReLoadTRTLevel(bool callInImmediately = false, bool forceLoad = false)
        {
            if (TRTMapManager.Maps == null || TRTMapManager.Maps.Count == 0)
            {
                MapManager.instance.LoadNextLevel(callInImmediately, forceLoad);
                yield break;
            }

            if (!forceLoad && !PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
            {
                yield return new WaitForSecondsRealtime(1f);
                yield break;
            }
            // tell the map controller of the upcoming map so it can set the mapscale properly, then give it time to sync
            if (ControllerManager.CurrentMapControllerID == TRTMapController.ControllerID)
            {
                ((TRTMapController)ControllerManager.CurrentMapController).SetUpcomingMap(GetMapFromMapID(TRTMapManager.CurrentLevel));
            }
            // only needs 10 frames to sync
            yield return new WaitForSecondsRealtime(1f);

            View.RPC("RPCA_SetCallInNextMap", RpcTarget.All, callInImmediately);
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{TRTMapManager.CurrentLevel}");
            NetworkingManager.RPC(typeof(TRTMapManager), nameof(RPCA_SetCurrentLevel), TRTMapManager.CurrentLevel);
        }
        public static IEnumerator LoadTRTLevelFromID(string ID, bool onlyMaster = false, bool callInImmediately = false)
        {
            if (!TRTMapManager.Maps.Select(m => m.id).Contains(ID))
            {
                TRTMapManager.LoadNextTRTLevel(callInImmediately, onlyMaster);
                yield break;
            }
            if (!PhotonNetwork.IsMasterClient && onlyMaster)
            { 
                yield return new WaitForSecondsRealtime(1f);
                yield break;
            }
            MapManager.instance.SetFieldValue("callInNextMap", callInImmediately);
            TRTMapManager.CurrentLevel = ID;
            // tell the map controller of the upcoming map so it can set the mapscale properly, then give it time to sync
            if (ControllerManager.CurrentMapControllerID == TRTMapController.ControllerID)
            {
                ((TRTMapController)ControllerManager.CurrentMapController).SetUpcomingMap(GetMapFromMapID(TRTMapManager.CurrentLevel));
            }
            // only needs 10 frames to sync
            yield return new WaitForSecondsRealtime(1f);
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
