using BepInEx;
using HarmonyLib;
using UnboundLib;
using UnityEngine;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using TMPro;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes;
using UnboundLib.Cards;
using UnboundLib.Utils;

namespace GameModeCollection
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.mapembiggener", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class GameModeCollection : BaseUnityPlugin
    {
        private const string ModId = "pykessandbosssloth.rounds.plugins.gamemodecollection";
        private const string ModName = "Game Mode Collection";
        public const string Version = "0.0.0";
        private static string CompatibilityModName => ModName.Replace(" ", "");

        public static GameModeCollection instance;

        private Harmony harmony;

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif
        internal static void Log(object msg)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.Log($"[{ModName}] {msg}");
            }
        }
        internal static void LogWarning(object msg)
        {
            if (DEBUG)
            {
                UnityEngine.Debug.LogWarning($"[{ModName}] {msg}");
            }
        }
        internal static void LogError(object msg)
        {
            UnityEngine.Debug.LogError($"[{ModName}] {msg}");
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
            Unbound.RegisterCredits(ModName, new string[] { "Pykess", "BossSloth" }, new string[] { "github", "Support Pykess", "Support BossSloth" }, new string[] { "https://github.com/pdcook/Game-Mode-Collection", "https://ko-fi.com/pykess", "https://www.buymeacoffee.com/BossSloth" });

            // add GUI to modoptions menu
            //Unbound.RegisterMenu(ModName, () => { }, GUI, null, false);

            GameModeManager.AddHandler<GM_CrownControl>(CrownControlHandler.GameModeID, new CrownControlHandler());
            GameModeManager.AddHandler<GM_CrownControl>(TeamCrownControlHandler.GameModeID, new TeamCrownControlHandler());
            GameModeManager.AddHandler<GM_Dodgeball>(DodgeballHandler.GameModeID, new DodgeballHandler());
            GameModeManager.AddHandler<GM_Dodgeball>(TeamDodgeballHandler.GameModeID, new TeamDodgeballHandler());
            
            CustomCard.BuildCard<HiderCard>(card => { HiderCard.instance = card; ModdingUtils.Utils.Cards.instance.AddHiddenCard(HiderCard.instance); });
            GameModeManager.AddHandler<GM_HideNSeek>(HideNSeekHandler.GameModeID, new HideNSeekHandler());
            GameModeManager.AddHandler<GM_BombDefusal>(BombDefusalHandler.GameModeID, new BombDefusalHandler());
        }

        private void OnDestroy()
        {
            harmony.UnpatchAll(GameModeCollection.ModId);
        }

        internal static string GetConfigKey(string key) => $"{GameModeCollection.CompatibilityModName}_{key}";

        internal static string AllowEnemyDamageKey => "allowEnemyDamage";
        internal static string AllowTeamDamageKey => "allowTeamDamage";
        internal static string AllowSelfDamageKey => "allowSelfDamage";

        internal static bool EnemyDamageAllowed
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return true;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(AllowEnemyDamageKey, out object allow) && !(bool)allow)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        internal static bool TeamDamageAllowed
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return true;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(AllowTeamDamageKey, out object allow) && !(bool)allow)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        internal static bool SelfDamageAllowed
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return true;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(AllowSelfDamageKey, out object allow) && !(bool)allow)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }
    }
}