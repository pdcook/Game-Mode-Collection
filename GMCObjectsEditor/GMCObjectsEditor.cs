using System.Reflection;
using BepInEx;
using MapsExt;
using MapsExt.Editor;
using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace GMCObjectsEditor
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInDependency("com.pykessandbosssloth.rounds.GMCObjects")]
    [BepInPlugin(GMCObjectsEditor.ModId, GMCObjectsEditor.ModName, GMCObjectsEditor.Version)]
    [BepInProcess("Rounds.exe")]
    public class GMCObjectsEditor : BaseUnityPlugin
    {
        private const string ModId = "com.bosssloth.rounds.TRTCardSpotsEditor";
        private const string ModName = "TRTCardSpotsEditor";
        public const string Version = GameModeCollection.GMCObjects.GMCObjects.Version;
        
        public void Start()
        {
            MapsExtended.instance.RegisterMapObjects();
        }
    }
}