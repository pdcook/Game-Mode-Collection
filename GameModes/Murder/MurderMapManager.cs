using MapsExt;
using GameModeCollection.Extensions;
using Photon.Pun;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using GameModeCollection.GameModes.Murder.Controllers;
using MapEmbiggener.Controllers;
namespace GameModeCollection.GameModes.Murder
{
    public static class MurderMapManager
    {
        // map files must be saved as
        // <mapname>_scale_<scale>_bounds_<x mult>_<y mult>.mrdrmap
        // scale and ratio are optional and determine what size to embiggen the map to as well as the ratio of the bounds
        // for example:
        //      Murder_Map.mrdrmap will use the default scaling (set in mod options) and bounds ratio (16:9)
        //      Murder_Map2_scale_3.mrdrmap will be scaled 3x and use the default bounds for the 3x scaled map
        //      Murder_Map3_bounds_2_1.mrdrmap will use the default scaling and will double the normal bounds on the x axis so the bounds ratio will be 32:9


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
            string[] parts = mapName.Replace(".mrdrmap", "").Split('_');
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
            string[] parts = mapName.Replace(".mrdrmap", "").Split('_');
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

        public static IEnumerator LoadNextMurderLevel(bool callInImmediately = false, bool forceLoad = false)
        {
            if (MurderMapManager.Maps == null || MurderMapManager.Maps.Count == 0)
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
                MurderMapManager.CurrentLevel = Maps.Where(m => m.id != MurderMapManager.CurrentLevel).ToList().GetRandom<CustomMap>().id;
            }
            else
            {
                MurderMapManager.CurrentLevel = Maps.GetRandom<CustomMap>().id;
            }

            // tell the map controller of the upcoming map so it can set the mapscale properly, then give it time to sync
            if (ControllerManager.CurrentMapControllerID == MurderMapController.ControllerID)
            {
                ((MurderMapController)ControllerManager.CurrentMapController).SetUpcomingMap(GetMapFromMapID(MurderMapManager.CurrentLevel));
            }
            // only needs 10 frames to sync
            yield return new WaitForSecondsRealtime(1f);

            View.RPC("RPCA_SetCallInNextMap", RpcTarget.All, callInImmediately);
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{MurderMapManager.CurrentLevel}");
            NetworkingManager.RPC(typeof(MurderMapManager), nameof(RPCA_SetCurrentLevel), MurderMapManager.CurrentLevel);
        }
        public static IEnumerator ReLoadMurderLevel(bool callInImmediately = false, bool forceLoad = false)
        {
            if (MurderMapManager.Maps == null || MurderMapManager.Maps.Count == 0)
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
            if (ControllerManager.CurrentMapControllerID == MurderMapController.ControllerID)
            {
                ((MurderMapController)ControllerManager.CurrentMapController).SetUpcomingMap(GetMapFromMapID(MurderMapManager.CurrentLevel));
            }
            // only needs 10 frames to sync
            yield return new WaitForSecondsRealtime(1f);

            View.RPC("RPCA_SetCallInNextMap", RpcTarget.All, callInImmediately);
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{MurderMapManager.CurrentLevel}");
            NetworkingManager.RPC(typeof(MurderMapManager), nameof(RPCA_SetCurrentLevel), MurderMapManager.CurrentLevel);
        }
        public static IEnumerator LoadMurderLevelFromID(string ID, bool onlyMaster = false, bool callInImmediately = false)
        {
            if (!MurderMapManager.Maps.Select(m => m.id).Contains(ID))
            {
                MurderMapManager.LoadNextMurderLevel(callInImmediately, onlyMaster);
                yield break;
            }
            if (!PhotonNetwork.IsMasterClient && onlyMaster)
            { 
                yield return new WaitForSecondsRealtime(1f);
                yield break;
            }
            MapManager.instance.SetFieldValue("callInNextMap", callInImmediately);
            MurderMapManager.CurrentLevel = ID;
            // tell the map controller of the upcoming map so it can set the mapscale properly, then give it time to sync
            if (ControllerManager.CurrentMapControllerID == MurderMapController.ControllerID)
            {
                ((MurderMapController)ControllerManager.CurrentMapController).SetUpcomingMap(GetMapFromMapID(MurderMapManager.CurrentLevel));
            }
            // only needs 10 frames to sync
            yield return new WaitForSecondsRealtime(1f);
            View.RPC("RPCA_LoadLevel", RpcTarget.All, $"MapsExtended:{ID}");
            NetworkingManager.RPC(typeof(MurderMapManager), nameof(RPCA_SetCurrentLevel), MurderMapManager.CurrentLevel);
            
        }
        [UnboundRPC]
        private static void RPCA_SetCurrentLevel(string currentLevel)
        {
            MurderMapManager.CurrentLevel = currentLevel;
        }
    }
}
