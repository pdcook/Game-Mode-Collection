using BepInEx;
using HarmonyLib;
using UnboundLib;
using UnityEngine;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnboundLib.Networking;
using Photon.Pun;

namespace GameModeCollection
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class GameModeCollection : BaseUnityPlugin
    {
        private const string ModId = "pykessandbosssloth.rounds.plugins.gamemodecollection";
        private const string ModName = "Game Mode Collection";
        public const string Version = "0.0.0";
        private string CompatibilityModName => ModName.Replace(" ", "");

        public static GameModeCollection instance;

        private Harmony harmony;

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif
        internal static void Log(string str)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.Log($"[{ModName}] {str}");
            }
        }


        private void Awake()
        {
            instance = this;
            
            harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        private void Start()
        {
            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess", "BossSloth" }, new string[] { "github", "Support Pykess", "Support BossSloth" }, new string[] { "REPLACE WITH LINK", "https://ko-fi.com/pykess", "https://www.buymeacoffee.com/BossSloth" });

            // add GUI to modoptions menu
            //Unbound.RegisterMenu(ModName, () => { }, GUI, null, false);

        }

        private void OnDestroy()
        {
            harmony.UnpatchAll();
        }

        internal static string GetConfigKey(string key) => $"{GameModeCollection.ModName}_{key}";

        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
    }
}