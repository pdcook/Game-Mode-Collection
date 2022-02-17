using System.Reflection;
using BepInEx;
using MapsExt;
using MapsExt.Editor;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace TRTCardSpotsEditor
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInDependency("com.pykessandbosssloth.rounds.GMCObjects")]
    [BepInPlugin(TRTCardSpotsEditor.ModId, TRTCardSpotsEditor.ModName, TRTCardSpotsEditor.Version)]
    [BepInProcess("Rounds.exe")]
    public class TRTCardSpotsEditor : BaseUnityPlugin
    {
        private const string ModId = "com.bosssloth.rounds.TRTCardSpotsEditor";
        private const string ModName = "TRTCardSpotsEditor";
        public const string Version = GMCObjects.GMCObjects.Version;
        
        public void Start()
        {
            MapsExtended.instance.RegisterMapObjects();
        }
    }
}