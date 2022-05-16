using BepInEx;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.Murder;
using HarmonyLib;
using MapsExt;
using System.IO;
using System.Linq;
namespace GameModeCollection.Patches
{
    // patching another mod is a stupid idea - but I don't have another option here
    [HarmonyPatch(typeof(MapsExtended), nameof(MapsExtended.UpdateMapFiles))]
    class MapsExtended_UpdateMapFiles
    {
        static void Postfix(MapsExtended __instance)
        {
            string[] trt_files = Directory.GetFiles(Paths.PluginPath, "*.trtmap", SearchOption.AllDirectories);
            TRTMapManager.MapDict = trt_files.ToDictionary(f => f, f => (MapsExt.CustomMap)typeof(MapsExt.MapsExtended).GetMethod("LoadMapData", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(obj: null, parameters: new object[] { f }));
            MapsExt.MapsExtended.instance.maps.AddRange(TRTMapManager.Maps);
            GameModeCollection.Log($"Loaded {TRTMapManager.Maps.Count()} TRT maps.");

            /*
            string[] mrdr_files = Directory.GetFiles(Paths.PluginPath, "*.mrdrmap", SearchOption.AllDirectories);
            MurderMapManager.MapDict = mrdr_files.ToDictionary(f => f, f => (MapsExt.CustomMap)typeof(MapsExt.MapsExtended).GetMethod("LoadMapData", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(obj: null, parameters: new object[] { f }));
            MapsExt.MapsExtended.instance.maps.AddRange(MurderMapManager.Maps);
            GameModeCollection.Log($"Loaded {MurderMapManager.Maps.Count()} Murder maps.");
            */
        }
    }
}
