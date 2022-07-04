using System.Reflection;
using BepInEx;
using MapsExt;
using MapsExt.UI;
using UnboundLib;
using UnityEngine;
using HarmonyLib;

namespace GMCObjectsEditor
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInDependency("com.pykess.rounds.GMCObjects")]
    [BepInPlugin(GMCObjectsEditor.ModId, GMCObjectsEditor.ModName, GMCObjectsEditor.Version)]
    [BepInProcess("Rounds.exe")]
    public class GMCObjectsEditor : BaseUnityPlugin
    {
        private const string ModId = "com.pykess.rounds.GMCObjectsEditor";
        private const string ModName = "GMCObjectsEditor";
        public const string Version = GameModeCollection.GMCObjects.GMCObjects.Version;
        private Harmony harmony;
        void Awake()
        {
            harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        public void Start()
        {
            MapsExtended.instance.RegisterMapObjects();
        }
    }
    [HarmonyPatch(typeof(Toolbar), "Start")]
    [HarmonyPriority(Priority.First)]
    class CharacterItem_Patch_Start
    {
        static void Prefix()
        {
            // patch to decrease the gridsize
            var field = typeof(Toolbar).GetField("gridStep", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, 0.1f);
        }
    }
}