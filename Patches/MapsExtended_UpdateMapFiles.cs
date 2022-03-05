using BepInEx;
using GameModeCollection.GameModes.TRT;
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
            string[] files = Directory.GetFiles(Paths.PluginPath, "*.trtmap", SearchOption.AllDirectories);
            TRTMapManager.MapDict = files.ToDictionary(f => f, f => (MapsExt.CustomMap)typeof(MapsExt.MapsExtended).GetMethod("LoadMapData", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(obj: null, parameters: new object[] { f }));
            MapsExt.MapsExtended.instance.maps.AddRange(TRTMapManager.Maps);
            GameModeCollection.Log($"Loaded {TRTMapManager.Maps.Count()} TRT maps.");
        }
    }
}
