using BepInEx;
using BepInEx.Configuration;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.GameModes.TRT.Controllers;
using GameModeCollection.Objects;
using HarmonyLib;
using Jotunn.Utils;
using MapEmbiggener.Controllers;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.GameModes;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace GameModeCollection
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)] // necessary for most modding stuff here
    [BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)] // specifically, requires RWEMF
    [BepInDependency("pykess.rounds.plugins.mapembiggener", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bosssloth.rounds.LocalZoom", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bosssloth.rounds.BetterChat", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.olavim.rounds.mapsextended", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.gununblockablepatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.demonicpactpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.dk.rounds.plugins.zerogpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.willuwontu.rounds.itemshops", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class GameModeCollection : BaseUnityPlugin
    {
        private const string ModId = "pykessandbosssloth.rounds.plugins.gamemodecollection";
        private const string ModName = "Game Mode Collection";
        public const string Version = "0.0.0";
        private static string CompatibilityModName => ModName.Replace(" ", "");

        internal static ConfigEntry<float> TRTDefaultMapScale;

        public static GameModeCollection instance;

        internal static AssetBundle TRT_Assets;
        internal static AssetBundle TRT_Card_Assets;

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

            TRTDefaultMapScale = Config.Bind(CompatibilityModName + "_TRT", "TRT Default Map Scale", 1f);

            On.MainMenuHandler.Awake += (orig, self) =>
            {
                this.ExecuteAfterFrames(10, () =>
                {
                    // custom face items for TRT
                    GameObject TRT_Detective_Hat = GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_Detective_Hat");
                    TRT_Detective_Hat.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
                    TRT_Detective_Hat.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    CharacterItem characterItem = TRT_Detective_Hat.AddComponent<CharacterItem>();
                    characterItem.itemType = CharacterItemType.Detail;
                    characterItem.scale = 1.2f;
                    characterItem.moveHealthBarUp = 1f;
                    CharacterCreatorItemLoader.instance.UpdateItems(CharacterItemType.Detail, CharacterCreatorItemLoader.instance.accessories.Concat(new CharacterItem[] { characterItem }).ToArray());
                });

                orig(self);
            };
        }
        private void Start()
        {

            try
            {
                TRT_Assets = AssetUtils.LoadAssetBundleFromResources("trt_assets", typeof(GameModeCollection).Assembly);
                if (TRT_Assets == null)
                {
                    LogError("TRT Assets failed to load.");
                }
            }
            catch
            {
                // ignored
            }
            try
            {
                TRT_Card_Assets = AssetUtils.LoadAssetBundleFromResources("trt_card_art", typeof(GameModeCollection).Assembly);
                if (TRT_Card_Assets == null)
                {
                    LogError("TRT Card Assets failed to load.");
                }
            }
            catch
            {
                // ignored
            }


            // add credits
            Unbound.RegisterCredits(ModName, new string[] { "Pykess (Crown Control, Trouble In Rounds Town, Dodgeball, Physics Items, MapEmbiggener)", "BossSloth (Hide & Seek, LocalZoom, MapEmbiggener)", " ", "Special Thanks To", "Willuwontu (TRT shop, sound effects, TRT Map)","TheCoconutDream (TRT Card Art)","LMS (TRT Maps)", "Ascyst (TRT Maps)" }, new string[] { "github", "Support Pykess", "Support BossSloth" }, new string[] { "https://github.com/pdcook/Game-Mode-Collection", "https://ko-fi.com/pykess", "https://www.buymeacoffee.com/BossSloth" });

            // add GUI to modoptions menu
            Unbound.RegisterMenu(ModName, () => { }, GUI, null, false);

            // register callback to enable vanilla cards in TRT
            Unbound.AddAllCardsCallback(TRTCardManager.SetTRTEnabled);

            // hooks
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, PhysicsItemRemover.RemoveItemsOnPointEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, A_Radar.DestroyAllPointsOnPointEnd);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, A_Knife.RemoveAllKnives);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, A_Knife.RemoveAllKnives);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, Cuffed.RemoveAllCuffsFromPlayers);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, Cuffed.RemoveAllCuffsFromPlayers);

            // Pykess game mode stuff
            GameModeManager.AddHandler<GM_CrownControl>(CrownControlHandler.GameModeID, new CrownControlHandler());
            GameModeManager.AddHandler<GM_CrownControl>(TeamCrownControlHandler.GameModeID, new TeamCrownControlHandler());
            GameModeManager.AddHandler<GM_Dodgeball>(DodgeballHandler.GameModeID, new DodgeballHandler());
            GameModeManager.AddHandler<GM_Dodgeball>(TeamDodgeballHandler.GameModeID, new TeamDodgeballHandler());
            GameModeManager.AddHandler<GM_TRT>(TRTHandler.GameModeID, new TRTHandler());
            ControllerManager.AddMapController(TRTMapController.ControllerID, new TRTMapController());
            ControllerManager.AddBoundsController(TRTBoundsController.ControllerID, new TRTBoundsController());
            CustomCard.BuildCard<C4Card>(C4Card.Callback);
            CustomCard.BuildCard<VSSCard>(VSSCard.Callback);
            CustomCard.BuildCard<ClawCard>(ClawCard.Callback);
            CustomCard.BuildCard<KnifeCard>(KnifeCard.Callback);
            CustomCard.BuildCard<RadarCard>(RadarCard.Callback);
            CustomCard.BuildCard<RifleCard>(RifleCard.Callback);
            CustomCard.BuildCard<DefuserCard>(DefuserCard.Callback);
            CustomCard.BuildCard<BodyArmorCard>(BodyArmorCard.Callback);
            CustomCard.BuildCard<HandcuffsCard>(HandcuffsCard.Callback);
            CustomCard.BuildCard<GoldenDeagleCard>(GoldenDeagleCard.Callback);
            CustomCard.BuildCard<DeathStationCard>(DeathStationCard.Callback);
            CustomCard.BuildCard<HealthStationCard>(HealthStationCard.Callback);

           
            // BossSloth game mode stuff
            CustomCard.BuildCard<HiderCard>(card => { HiderCard.instance = card; ModdingUtils.Utils.Cards.instance.AddHiddenCard(HiderCard.instance); });
            GameModeManager.AddHandler<GM_HideNSeek>(HideNSeekHandler.GameModeID, new HideNSeekHandler());
            //GameModeManager.AddHandler<GM_BombDefusal>(BombDefusalHandler.GameModeID, new BombDefusalHandler());
        }

        private void OnDestroy()
        {
            harmony.UnpatchAll(GameModeCollection.ModId);
        }

        internal static string GetConfigKey(string key) => $"{GameModeCollection.CompatibilityModName}_{key}";

        public static string AllowEnemyDamageKey => GetConfigKey("allowEnemyDamage");
        public static string AllowTeamDamageKey => GetConfigKey("allowTeamDamage");
        public static string AllowSelfDamageKey => GetConfigKey("allowSelfDamage");
        public static string ReviveOnCardAddKey => GetConfigKey("reviveOnCardAdd");
        public static string CreatePlayerCorpsesKey => GetConfigKey("createPlayerCorpses");
        public static string UsePlayerColorsInsteadOfNamesInChatKey => GetConfigKey("usePlayerColorsInsteadOfNamesInChat");
        public static string IgnoreGameFeelKey => GetConfigKey("ignoreGameFeel");

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
        public static bool SelfDamageAllowed
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
        public static bool ReviveOnCardAdd
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return true;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(ReviveOnCardAddKey, out object revive) && !(bool)revive)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
        public static bool CreatePlayerCorpses
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return false;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(CreatePlayerCorpsesKey, out object create) && (bool)create)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public static bool UsePlayerColorsInsteadOfNamesInChat
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return false;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(UsePlayerColorsInsteadOfNamesInChatKey, out object use) && (bool)use)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public static bool IgnoreGameFeel
        {
            get
            {
                if (GameModeManager.CurrentHandler is null || GameModeManager.CurrentHandler.Settings is null)
                {
                    return false;
                }
                if (GameModeManager.CurrentHandler.Settings.TryGetValue(IgnoreGameFeelKey, out object ignore) && (bool)ignore)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        private static void GUI(GameObject menu)
        {
            MenuHandler.CreateText(ModName, menu, out TextMeshProUGUI _, 60);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            GameObject TRTMenu = MenuHandler.CreateMenu("TROUBLE IN ROUNDS TOWN", () => { }, menu, 60, true, true, menu.transform.parent.gameObject);
            TRTHandler.TRTMenu(TRTMenu);
        }
    }
}