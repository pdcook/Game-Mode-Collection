using System.Reflection;
using BepInEx;
using TRTCardSpots;
using MapsExt;
using MapsExt.Editor;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace TRTCardSpotsEditor
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInDependency("com.bosssloth.rounds.TRTCardSpots")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class TRTCardSpotsEditor : BaseUnityPlugin
    {
        private const string ModId = "com.bosssloth.rounds.TRTCardSpotsEditor";
        private const string ModName = "TRTCardSpotsEditor";
        public const string Version = TRTCardSpots.TRTCardSpots.Version;
        
        public void Start()
        {
            MapsExtended.instance.RegisterMapObjects();
        }
    }
}