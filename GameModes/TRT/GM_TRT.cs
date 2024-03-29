﻿using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.GameModes.TRT.Controllers;
using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.GameModes.TRT.RoundEvents;
using GameModeCollection.Objects;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.Utils;
using MapEmbiggener.Controllers;
using Photon.Pun;
using Sonigon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerCustomizationUtils.Extensions;

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// 
    /// Trouble In Rounds Town - just like Trouble in Terrorist Town
    /// 
    /// Maps only transition on round end, NOT point end
    /// No pick phase
    /// 
    /// Minimum three (maybe four?) players
    /// </summary>
    public class GM_TRT : MonoBehaviour
    {

        public enum RoundPhase
        {
            PreBattle,
            GracePeriod,
            Starting,
            Active,
            PostBattle,
            Transitioning
        }

        internal static GM_TRT instance;

        private const float RoundTime = 300f; // default 300f
        private const float PrepPhaseTime = 30f; // default 30f
        private const float PostPhaseTime = 30f; // default 30f
        private const float GracePeriodTime = 6f; // default 5f, amount of time at the end of the prep phase during which players cannot shoot or block
        private const float HasteModeAddPerDeath = 30f; // default 30f
        private const float SyncClockEvery = 5f; // sync clock with host every 5 seconds

        private const float DefaultZoom = 60f;
        internal const float DefaultFOV = 361f;
        internal const float DefaultVisibility = 1000f;

        public const float DelayRevivesFor = 1f;

        internal const float TimeBetweenCardDrops = 0.5f;
        private const float CardRandomVelMult = 0.25f;
        private const float CardRandomVelMin = 3f;
        private const float CardAngularVelMult = 10f;
        private const float CardHealth = -1f;

        public const float KarmaPenaltyPerRDM = 0.2f; // you lose 0.2 (20%) karma for each RDM
        public const float KarmaRewardPerPoint = 0.1f; // you gain 0.1 (10%) karma for each clean point
        public const float MinimumKarma = 0.4f; // the minimum karma is 0.4 (40%), players below this will receive a slay
        public const float KarmaFractionForDeath = 0.5f; // if you are dead at the end of a point, you only gain 50% of the 10% you would usuall gain

        public const int BaseMaxCards = 2;
        public const int CardsToSpawnPerPlayer = 3;
        public const float BaseHealth = 100f;
        internal static float Perc_Inno_For_Reward => TRTShopHandler.TRT_Perc_Inno_For_Reward; // what percent of innocents need to be killed for the traitors to be reworded

        public readonly static Color InnocentColor = new Color32(26, 200, 25, 255);
        public readonly static Color DetectiveColor = new Color32(24, 29, 253, 255);
        public readonly static Color TraitorColor = new Color32(199, 25, 24, 255);
        public readonly static Color JesterColor = new Color32(180, 22, 254, 255);
        public readonly static Color GlitchColor = new Color32(244, 105, 0, 255);
        public readonly static Color MercenaryColor = new Color32(246, 200, 0, 255);
        public readonly static Color PhantomColor = new Color32(82, 225, 255, 255);
        public readonly static Color KillerColor = new Color32(46, 1, 68, 255);
        public readonly static Color HypnotistColor = new Color32(255, 80, 235, 255);
        public readonly static Color ZombieColor = new Color32(70, 97, 0, 255);
        public readonly static Color SwapperColor = new Color32(111, 0, 253, 255);
        public readonly static Color AssassinColor = new Color32(112, 50, 1, 255);
        public readonly static Color VampireColor = new Color32(45, 45, 45, 255);

        public readonly static Color DullWhite = new Color32(230, 230, 230, 255);
        public readonly static Color GracePeriodColor = new Color32(230, 115, 0, 255);
        public readonly static Color WarningColor = new Color32(230, 0, 0, 255);
        public readonly static Color DisplayBackgroundColor = new Color32(0, 0, 0, 150);
        public readonly static Color TextBackgroundColor = new Color32(0, 0, 0, 200);

        private readonly ReadOnlyDictionary<int, int> roundCounterValues = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>() { { 0, 0 }, { 1, 0 } }) { };

        internal int pointsPlayedOnCurrentMap = 0;
        internal int roundsPlayed = 0;

        private bool isCheckingWinCondition = false;
        private bool isTransitioning = false;
        private Dictionary<int, string> RoleIDsToAssign = null;
        private int? timeUntilBattleStart = null;

        public RoundPhase CurrentPhase { get; private set; } = RoundPhase.Transitioning;
        public bool BattleOngoing => this.CurrentPhase == RoundPhase.Active;
        /*
        internal bool BattleOngoing = false;
        private bool preBattle = false;
        private bool gracePeriod = false;
        private bool postBattle = false;
        */

        private float clocktime = RoundTime;
        private float syncCounter = -1f;

        private void SetAllPlayersFOV()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                if (player.GetComponentInChildren<ViewSphere>(true) != null)
                {
                    player.GetComponentInChildren<ViewSphere>(true).fov = DefaultFOV;
                    player.GetComponentInChildren<ViewSphere>(true).viewDistance = DefaultVisibility;
                }
            });
        }
        private void HideAllPlayerFaces()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                foreach (CharacterItem item in player.GetComponentsInChildren<CharacterItem>(true))
                {
                    LocalZoom.LocalZoom.MakeObjectHidden(item);
                }
            });
        }
        private void RegisterAllWobbleObjects()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                if (player?.GetComponentInChildren<PlayerWobblePosition>(true) is null) { return; }
                LocalZoom.Extensions.CharacterDataExtension.GetData(player.data).allWobbleImages.AddRange(player.GetComponentInChildren<PlayerWobblePosition>(true).GetComponentsInChildren<UnityEngine.UI.Image>(true));
                LocalZoom.Extensions.CharacterDataExtension.GetData(player.data).allWobbleImages = LocalZoom.Extensions.CharacterDataExtension.GetData(player.data).allWobbleImages.Distinct().ToList();
            });
        }

        private void DoSlays()
        {
            PlayerManager.instance.ForEachAlivePlayer(p =>
            {

                if (p.data.TRT_Karma() < GM_TRT.MinimumKarma)
                {
                    p.data.TRT_ChangeKarma(0.0f, GM_TRT.MinimumKarma);
                    if (!(p?.data?.view?.IsMine ?? false)) { return; }
                    p.data.view.RPC("RPCA_Die", RpcTarget.All, Vector2.up);
                    TRTHandler.SendChat(null, "You have been automatically slain for having too low karma. Avoid killing your teammates.", true);
                    TRTHandler.SendChat(null, $"{p.data.NickName()} was slain for having low karma.", false);
                    RoundSummary.LogEvent(SlayEvent.ID, p.playerID);
                }
            });
        }

        protected void Awake()
        {
            GM_TRT.instance = this;
            RoleManager.Init();
        }

        protected void Start()
        {
            // add RoundSummary input handler
            this.gameObject.GetOrAddComponent<RoundSummary>();

            // register prefabs
            GameObject _ = CardItemPrefabs.CardItem;
            _ = HealthStationPrefab.HealthStation;
            _ = DeathStationPrefab.DeathStation;
            _ = C4Prefab.C4;
            _ = GrenadePrefab.Grenade;
            _ = DiscombobulatorPrefab.Discombobulator;
            _ = IncendiaryGrenadePrefabs.IncendiaryGrenade;
            _ = IncendiaryGrenadePrefabs.IncendiaryGrenadeFragment;
            _ = SmokeGrenadePrefab.SmokeGrenade;
            // spawn handler
            _ = CardItemPrefabs.CardItemHandler;
            this.StartCoroutine(this.Init());
        }
        private IEnumerator Init()
        {
            TRTHandler.EnforceBannedMods();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookInitStart);

            CardItemHandler.Instance.SetCanDiscard(true);
            CardItemHandler.Instance.PlayerDiscardAction += DropCard;

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerAssigner.instance.maxPlayers = RWF.RWFMod.instance.MaxPlayers;

            NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SyncLocalZoomSettings));
            TRTHandler.InitChatGroups();
            BetterChat.BetterChat.SetDeadChat(true);
            BetterChat.BetterChat.UsePlayerColors = false;
            BetterChat.BetterChat.EnableTypingIndicators = false;
            RoundsVC.RoundsVC.DefaultChannelEnabled = false;

            TRTShopHandler.BuildTRTShops();

            ControllerManager.SetMapController(TRTMapController.ControllerID);
            ControllerManager.SetBoundsController(TRTBoundsController.ControllerID);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookInitEnd);
        }
        [UnboundRPC]
        private static void RPCA_SyncLocalZoomSettings()
        {
            LocalZoom.MyCameraController.allowZoomIn = true;
            LocalZoom.MyCameraController.defaultZoomLevel = DefaultZoom;
            LocalZoom.LocalZoom.scaleCamWithBulletSpeed = true;
            LocalZoom.LocalZoom.enableLoSNamePlates = true;
            LocalZoom.LocalZoom.disableLimbClipping = true;
            LocalZoom.LocalZoom.enablePlayerCircles = false;
            LocalZoom.LocalZoom.SetEnableShaderSetting(true);
            LocalZoom.LocalZoom.SetEnableCameraSetting(true);
        }

        private void RandomizePlayerSkins()
        {
            if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode) { return; }
            int[] newColorIDs = Enumerable.Range(0, UnboundLib.Utils.ExtraPlayerSkins.numberOfSkins).OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).Distinct().ToArray();
            for (int i = 0; i < PlayerManager.instance.players.Count(); i++)
            {
                NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SetNewColors), PlayerManager.instance.players[i].playerID, newColorIDs[i]);
            }
        }
        private void RandomizePlayerFaces()
        {
            if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode) { return; }
            PlayerManager.instance.ForEachPlayer(player =>
            {
                player.data.view.RPC("RPCA_SetFace", RpcTarget.All, new object[]
                {
                    CharacterCreatorItemLoader.instance.GetRandomItemID(CharacterItemType.Eyes, null, false),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    CharacterCreatorItemLoader.instance.GetRandomItemID(CharacterItemType.Mouth, null, false),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    CharacterCreatorItemLoader.instance.GetRandomItemID(CharacterItemType.Detail, null, false),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    CharacterCreatorItemLoader.instance.GetRandomItemID(CharacterItemType.Detail, null, false),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1)

                });
            });
        }
        [UnboundRPC]
        public static void RPC_SyncBattleStart(int requestingPlayer, int timeOfBattleStart, Dictionary<int, string> rolesToAssign)
        {

            // calculate the time in milliseconds until the battle starts
            GM_TRT.instance.timeUntilBattleStart = timeOfBattleStart - PhotonNetwork.ServerTimestamp;

            // set the roles to assign
            GM_TRT.instance.RoleIDsToAssign = rolesToAssign;

            NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPC_SyncBattleStartResponse), requestingPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPC_SyncBattleStartResponse(int requestingPlayer, int readyPlayer)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == requestingPlayer)
            {
                GM_TRT.instance.RemovePendingRequest(readyPlayer, nameof(GM_TRT.RPC_SyncBattleStart));
            }
        }

        protected IEnumerator SyncBattleStart()
        {
            // replacing original to be able to assign roles here as well

            if (PhotonNetwork.OfflineMode)
            {
                List<IRoleHandler> roles = RoleManager.GetRoleLineup(PlayerManager.instance.players.Count());
                this.RoleIDsToAssign = roles.Select((r,i) => new { r, i }).ToDictionary(r => r.i, r => r.r.RoleID);
                this.AssignRoles();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                RoleManager.DoRoleDisplay(PlayerManager.instance.players.Find(p => p.data.view.IsMine));
                yield break;
            }

            // only the host will communicate when the battle should start

            if (PhotonNetwork.IsMasterClient)
            {
                // schedule the battle to start 5 times the maximum client ping + host client's ping from now, with a minimum of 1 second
                // 5 because the host and slowest client must:
                // Host 1) send the RPC
                // Host 2) receive ALL clients' responses
                // Host 3) retrieve the server time
                // Client 1) receive the RPC
                // Client 2) respond to the RPC
                // Client 3) retrieve the server time
                // + wiggle room

                // if the host client is the slowest client (very unlikely because of how Photon chooses servers),
                // then this is overkill - but better safe than sorry

                // this is in milliseconds and can overflow, but luckily all overflows will cancel out when a time difference is calculated
                int timeOfBattleStart = PhotonNetwork.ServerTimestamp + UnityEngine.Mathf.Clamp(5 * ((int)PhotonNetwork.LocalPlayer.CustomProperties["Ping"] + PhotonNetwork.CurrentRoom.Players.Select(kv => (int)kv.Value.CustomProperties["Ping"]).Max()), 1000, int.MaxValue);

                // get roles to assign
                List<IRoleHandler> roles = RoleManager.GetRoleLineup(PlayerManager.instance.players.Count());
                Dictionary<int, string> roleIDsToAssign = roles.Select((r,i) => new { r, i }).ToDictionary(r => r.i, r => r.r.RoleID);

                yield return this.SyncMethod(nameof(GM_TRT.RPC_SyncBattleStart), null, PhotonNetwork.LocalPlayer.ActorNumber, timeOfBattleStart, roleIDsToAssign);
            }

            yield return new WaitUntil(() => this.timeUntilBattleStart != null && this.RoleIDsToAssign != null);

            yield return new WaitForSecondsRealtime((float)this.timeUntilBattleStart * 0.001f);

            this.AssignRoles();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            RoleManager.DoRoleDisplay(PlayerManager.instance.players.Find(p => p.data.view.IsMine));

            yield return new WaitForEndOfFrame();

            this.timeUntilBattleStart = null;
            this.RoleIDsToAssign = null;
        }
        private void AssignRoles()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                RoleManager.GetHandler(this.RoleIDsToAssign[player.playerID]).AddRoleToPlayer(player);
            });
        }
        private IEnumerator ClearRoles()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                foreach (var role in player.gameObject.GetComponentsInChildren<TRT_Role>())
                {
                    UnityEngine.GameObject.Destroy(role);
                }
                foreach (var phantomHaunt in player.gameObject.GetComponentsInChildren<PhantomHaunt>())
                {
                    phantomHaunt?.DestroyNow();
                }
            });

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        private IEnumerator ClearRolesAndVisuals()
        {
            // apply karma changes before clearing roles
            this.UpdateKarma();
            yield return new WaitForEndOfFrame();
            yield return this.ClearRoles();
            PlayerManager.instance.ForEachPlayer(p => RoleManager.ClearRoleDisplay(p));
        }
        private void UpdateKarma()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                ITRT_Role role = RoleManager.GetPlayerRole(player);
                if (role is null) { return; }
                float change = role.KarmaChange == 0f ? (player.data.dead ? 0.25f : 1f) * GM_TRT.KarmaRewardPerPoint : role.KarmaChange;
                player.data.TRT_ChangeKarma(change, 0f);
            });
        }

        [UnboundRPC]
        static void RPCA_SetNewColors(int playerID, int colorID)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);

            UnboundLib.Extensions.PlayerExtensions.AssignColorID(player, colorID);
        }

        private void PlayerCorpse(Player player)
        {
            if (player.GetComponent<TRT_Corpse>() != null)
            {
                DestroyImmediate(player.GetComponent<TRT_Corpse>());
            }
            player.gameObject.AddComponent<TRT_Corpse>();
        }
        internal void DropCard(Player player, CardInfo card)
        {
            this.StartCoroutine(this.PlayerDropCard(player, card));
        }
        internal IEnumerator PlayerDropCard(Player player, CardInfo card)
        {
            Vector2 velocty = (Vector2)player.data.playerVel.GetFieldValue("velocity");
            yield return CardItem.MakeCardItem(card,
                                                player.data.playerVel.position,
                                                Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)),
                                                velocty + UnityEngine.Mathf.Clamp(CardRandomVelMult * velocty.magnitude, CardRandomVelMin, float.MaxValue) * new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)),
                                                -CardAngularVelMult * velocty.x,
                                                CardHealth, requireInteract: true);
        }
        private IEnumerator DropCardsOnDeath(Player player, CardInfo[] cardsToDrop)
        {
            foreach (CardInfo card in cardsToDrop.Where(c => !c.categories.Contains(TRTCardCategories.TRT_DoNotDropOnDeath)))
            {
                if (!this.BattleOngoing) { yield break; }
                yield return new WaitForSecondsRealtime(TimeBetweenCardDrops);
                yield return this.PlayerDropCard(player, card);
            }
            yield break;
        }

        public void PlayerJoined(Player player)
        {
            // reset Karma
            player.data.TRT_ResetKarma();

            // set localzoom shader settings
            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();
        }

        public void PlayerDied(Player killedPlayer, int teamsAlive)
        {

            if (this.CurrentPhase != RoundPhase.Active) { return; }
            
            // every time a player dies, time is added to the clock
            this.clocktime += HasteModeAddPerDeath / PlayerManager.instance.players.Select(p => p.data.view.ControllerActorNr).Distinct().Count();

            // handle TRT corpse creation, dropping cards, check win conditions

            // drop cards
            CardInfo[] cardsToDrop = killedPlayer.data.currentCards.ToArray();
            killedPlayer.InvokeMethod("FullReset");
            if (killedPlayer.data.view.IsMine)
            {
                CardUtils.ClientSideClearCardBar(killedPlayer);
            }
            this.StartCoroutine(this.DropCardsOnDeath(killedPlayer, cardsToDrop));

            // corpse creation
            this.PlayerCorpse(killedPlayer);
            
            if (killedPlayer.data.view.IsMine)
            {
                UIHandler.instance.roundCounterSmall.UpdateText(1, "ONGOING", DullWhite, 30, Vector3.one, DisplayBackgroundColor, false);
            }

            float checkAfter = GM_TRT.DelayRevivesFor + 0.5f;

            if (RoleManager.GetPlayerRoleID(killedPlayer) == SwapperRoleHandler.SwapperRoleID)
            {
                checkAfter = 5f;
            }

            // check win condition after a short delay to allow things like phantom spawning and swapper swapping to happen
            if (this.isCheckingWinCondition || !PhotonNetwork.IsMasterClient) { return; }
            this.isCheckingWinCondition = true;
            this.ExecuteAfterSeconds(checkAfter, () =>
            {
                this.isCheckingWinCondition = false;

                string winningRoleID = RoleManager.GetWinningRoleID(PlayerManager.instance.players.ToArray());

                if (winningRoleID != null)
                {

                    if (PhotonNetwork.IsMasterClient)
                    {
                        NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPCA_NextRound), winningRoleID);
                    }
                }
            });
        }

        public void StartGame()
        {
            if (GameManager.instance.isPlaying)
            {
                return;
            }

            PlayerManager.instance.ForEachPlayer(this.PlayerJoined);

            RoundsVC.RoundsVC.DefaultChannelEnabled = false;
            BetterChat.BetterChat.SetDeadChat(true);
            BetterChat.BetterChat.UsePlayerColors = false;
            BetterChat.BetterChat.EnableTypingIndicators = false;
            ControllerManager.SetMapController(TRTMapController.ControllerID);
            ControllerManager.SetBoundsController(TRTBoundsController.ControllerID);

            GameManager.instance.isPlaying = true;
            this.StartCoroutine(this.DoStartGame());
        }

        public IEnumerator DoStartGame()
        {
            // completely replace original method
            RWF.CardBarHandlerExtensions.Rebuild(CardBarHandler.instance);

            // set the roundcounter number of rounds to 1 only so that the round counter is there
            UIHandler.instance.InvokeMethod("SetNumberOfRounds", 1);

            ArtHandler.instance.NextArt();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameStart);

            NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SyncLocalZoomSettings));

            GameManager.instance.battleOngoing = false;

            UIHandler.instance.ShowJoinGameText("Trouble\n\nin\n\nRounds Town", Color.white, TRTHandler.TRTFont);
            yield return new WaitForSecondsRealtime(2f);
            UIHandler.instance.HideJoinGameText();
            yield return this.WaitForSyncUp();

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

            //MapManager.instance.LoadNextLevel(false, false);
            yield return TRTMapManager.LoadNextTRTLevel(false, false);

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            yield return this.ClearRolesAndVisuals();

            // reset karma
            PlayerManager.instance.ResetKarma();

            // reset round summary
            RoundSummary.ResetAll();

            TimeHandler.instance.DoSpeedUp();

            yield return new WaitForSecondsRealtime(1f);
            this.HideAllPlayerFaces();
            yield return this.WaitForSyncUp();
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);
            TimeHandler.instance.DoSpeedUp();
            TimeHandler.instance.StartGame();
            GameManager.instance.battleOngoing = true;
            RWF.UIHandlerExtensions.ShowRoundCounterSmall(UIHandler.instance, this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value), this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value));
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);

            this.StartCoroutine(this.DoRoundStart());

        }
        public IEnumerator DoRoundStart()
        {
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            // players get karma reset on new round
            PlayerManager.instance.ResetKarma();

            // reset round summary
            RoundSummary.ResetAll();

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            yield return TRTCardManager.SpawnCards(CardsToSpawnPerPlayer * PlayerManager.instance.players.Count(), CardHealth, true);

            yield return this.WaitForSyncUp();

            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundStart);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            this.clocktime = PrepPhaseTime;
            //this.preBattle = true;
            //this.gracePeriod = false;
            this.CurrentPhase = RoundPhase.PreBattle;

            UIHandler.instance.roundCounterSmall.UpdateText(1, "PREPARING", DullWhite, 30, Vector3.one, DisplayBackgroundColor, false);

            //yield return new WaitWhile(() => this.preBattle && !this.gracePeriod);
            yield return new WaitWhile(() => this.CurrentPhase == RoundPhase.PreBattle);

            UIHandler.instance.roundCounterSmall.UpdateText(1, "READY", GracePeriodColor, 30, Vector3.one, DisplayBackgroundColor, false);
            PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();

            //yield return new WaitWhile(() => this.preBattle && this.gracePeriod);
            yield return new WaitWhile(() => this.CurrentPhase == RoundPhase.GracePeriod);

            yield return this.SyncBattleStart();

            // reset round summary
            RoundSummary.ResetAll();

            this.HideAllPlayerFaces();

            this.clocktime = RoundTime;
            //this.preBattle = false;
            //this.gracePeriod = false;
            //this.BattleOngoing = true;
            this.CurrentPhase = RoundPhase.Active;

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(false);
            PlayerManager.instance.RevivePlayers();

            this.ExecuteAfterSeconds(1f, this.DoSlays);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);
        }

        public IEnumerator DoPointStart()
        {
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");
            // reset round summary
            RoundSummary.ResetAll();

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            yield return TRTCardManager.SpawnCards(CardsToSpawnPerPlayer * PlayerManager.instance.players.Count(), CardHealth, true);

            //PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();
            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            this.clocktime = PrepPhaseTime;
            //this.preBattle = true;
            //this.gracePeriod = false;
            this.CurrentPhase = RoundPhase.PreBattle;

            UIHandler.instance.roundCounterSmall.UpdateText(1, "PREPARING", DullWhite, 30, Vector3.one, DisplayBackgroundColor, false);

            //yield return new WaitWhile(() => this.preBattle && !this.gracePeriod);
            yield return new WaitWhile(() => this.CurrentPhase == RoundPhase.PreBattle);

            UIHandler.instance.roundCounterSmall.UpdateText(1, "READY", GracePeriodColor, 30, Vector3.one, DisplayBackgroundColor, false);
            PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();

            //yield return new WaitWhile(() => this.preBattle && this.gracePeriod);
            yield return new WaitWhile(() => this.CurrentPhase == RoundPhase.GracePeriod);

            yield return this.SyncBattleStart();

            // reset round summary
            RoundSummary.ResetAll();

            this.HideAllPlayerFaces();

            this.clocktime = RoundTime;
            //this.preBattle = false;
            //this.gracePeriod = false;
            //this.BattleOngoing = true;
            this.CurrentPhase = RoundPhase.Active;

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(false);
            PlayerManager.instance.RevivePlayers();

            this.ExecuteAfterSeconds(1f, this.DoSlays);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);

        }
        public IEnumerator RoundTransition(string winningRoleID)
        {
            //this.BattleOngoing = false;
            //this.preBattle = false;
            //this.gracePeriod = false;
            this.clocktime = PostPhaseTime;
            this.CurrentPhase = RoundPhase.PostBattle;
            IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
            string winMessage = winningRole?.WinMessage ?? "ROUND OVER";
            Color winColor = winningRole?.WinColor ?? DullWhite;
            UIHandler.instance.roundCounterSmall.UpdateText(1, winMessage, winColor, 30, Vector3.one, DisplayBackgroundColor, true);
            
            if (winningRoleID is null)
            {
                //this.StartCoroutine(PointVisualizer.instance.DoSequence("DRAW", DullWhite));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                //IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                //this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendPointOverChat(winningRole); }
            }

            yield return new WaitWhile(() => this.CurrentPhase == RoundPhase.PostBattle);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundEnd);
            
            this.StartCoroutine(this.ClearRolesAndVisuals());
            GameManager.instance.battleOngoing = false;
            PlayerManager.instance.SetPlayersSimulated(false);

            UIHandler.instance.roundCounterSmall.UpdateText(1, "LOADING", DullWhite, 30, Vector3.one, DisplayBackgroundColor, false);

            if (this.roundsPlayed >= (int)GameModeManager.CurrentHandler.Settings["roundsToWinGame"])
            {
                this.GameOver();
                yield break;
            }

            yield return new WaitForSecondsRealtime(1f);
            //MapManager.instance.LoadNextLevel(false, false);
            yield return TRTMapManager.LoadNextTRTLevel(false, false);

            yield return new WaitForSecondsRealtime(1.3f);

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);
            TimeHandler.instance.DoSpeedUp();

            yield return this.StartCoroutine(this.WaitForSyncUp());

            TimeHandler.instance.DoSlowDown();
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);

            PlayerManager.instance.RevivePlayers();

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            yield return this.ClearRolesAndVisuals();

            yield return new WaitForSecondsRealtime(0.3f);
            this.HideAllPlayerFaces();

            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);
            RWF.UIHandlerExtensions.ShowRoundCounterSmall(UIHandler.instance, this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value), this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value));

            this.StartCoroutine(this.DoRoundStart());
        }
        public IEnumerator PointTransition(string winningRoleID)
        {
            //this.BattleOngoing = false;
            //this.preBattle = false;
            //this.gracePeriod = false;
            this.clocktime = PostPhaseTime;
            this.CurrentPhase = RoundPhase.PostBattle;
            IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
            string winMessage = winningRole?.WinMessage ?? "ROUND OVER";
            Color winColor = winningRole?.WinColor ?? DullWhite;
            UIHandler.instance.roundCounterSmall.UpdateText(1, winMessage, winColor, 30, Vector3.one, DisplayBackgroundColor, true);

            if (winningRoleID is null)
            {
                //this.StartCoroutine(PointVisualizer.instance.DoSequence("DRAW", DullWhite));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                //IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                //this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendPointOverChat(winningRole); }
            }

            yield return new WaitWhile(() => this.CurrentPhase == RoundPhase.PostBattle);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);

            this.StartCoroutine(this.ClearRolesAndVisuals());
            GameManager.instance.battleOngoing = false;
            PlayerManager.instance.SetPlayersSimulated(false);

            UIHandler.instance.roundCounterSmall.UpdateText(1, "LOADING", DullWhite, 30, Vector3.one, DisplayBackgroundColor, false);

            yield return new WaitForSecondsRealtime(1f);
            //MapManager.instance.LoadNextLevel(false, false);
            //TRTMapManager.LoadNextTRTLevel(false, false);
            yield return TRTMapManager.ReLoadTRTLevel(false, false);

            yield return new WaitForSecondsRealtime(1.3f);

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);
            TimeHandler.instance.DoSpeedUp();

            yield return this.StartCoroutine(this.WaitForSyncUp());

            TimeHandler.instance.DoSlowDown();
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);

            PlayerManager.instance.RevivePlayers();

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            yield return this.ClearRolesAndVisuals();

            yield return new WaitForSecondsRealtime(0.3f);
            this.HideAllPlayerFaces();

            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);
            RWF.UIHandlerExtensions.ShowRoundCounterSmall(UIHandler.instance, this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value), this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value));

            this.StartCoroutine(this.DoPointStart());
        }
        private void GameOverRematch()
        {
            if (PhotonNetwork.OfflineMode)
            {
                UIHandler.instance.DisplayScreenTextLoop(DullWhite, "REMATCH?");
                UIHandler.instance.popUpHandler.StartPicking(PlayerManager.instance.players.First(), this.GetRematchYesNo);
                //MapManager.instance.LoadNextLevel(false, false);
                this.StartCoroutine(TRTMapManager.LoadNextTRTLevel(false, false));
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values.ToList())
                {
                    PhotonNetwork.DestroyPlayerObjects(player);
                }
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GetRematchYesNo(PopUpHandler.YesNo yesNo)
        {
            if (yesNo == PopUpHandler.YesNo.Yes)
            {
                base.StartCoroutine(this.IDoRematch());
                return;
            }
            this.DoRestart();
        }
        public IEnumerator GameOverTransition()
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameEnd);

            RWF.UIHandlerExtensions.ShowRoundCounterSmall(UIHandler.instance, this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value), this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value));
            //Color color = AverageColor.Average(colors);
            UIHandler.instance.DisplayScreenText(Color.white, "TROUBLE\nIN\nROUNDS TOWN", 1f);
            yield return new WaitForSecondsRealtime(2f);
            this.GameOverRematch();
            yield break;
        }
        protected virtual IEnumerator IDoRematch()
        {
            yield return null;
            this.ResetMatch();
            this.StartCoroutine(this.DoStartGame());
        }

        private void DoRestart()
        {
            GameManager.instance.battleOngoing = false;
            if (PhotonNetwork.OfflineMode)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            NetworkConnectionHandler.instance.NetworkRestart();
        }
        public void ResetMatch()
        {
            UIHandler.instance.StopScreenTextLoop();
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            // reset karma
            PlayerManager.instance.ResetKarma();

            this.pointsPlayedOnCurrentMap = 0;
            this.roundsPlayed = 0;

            this.isTransitioning = false;
            RWF.UIHandlerExtensions.ShowRoundCounterSmall(UIHandler.instance, this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value), this.roundCounterValues.ToDictionary(kv => kv.Key, kv => kv.Value));
            CardBarHandler.instance.ResetCardBards();
            PointVisualizer.instance.ResetPoints();
        }
        private void GameOver()
        {
            base.StartCoroutine(this.GameOverTransition());
        }
        public void RoundOver(string winningRoleID)
        {
            TRTCardManager.RemoveAllCardItems();
            this.StartCoroutine(this.RoundTransition(winningRoleID));
        }

        public void PointOver(string winningRoleID)
        {
            TRTCardManager.RemoveAllCardItems();
            this.StartCoroutine(this.PointTransition(winningRoleID));
        }

        [UnboundRPC]
        public static void RPC_RequestSync(int requestingPlayer)
        {
            NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPC_SyncResponse), requestingPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPC_SyncResponse(int requestingPlayer, int readyPlayer)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == requestingPlayer)
            {
                GM_TRT.instance.RemovePendingRequest(readyPlayer, nameof(GM_TRT.RPC_RequestSync));
            }
        }

        private IEnumerator WaitForSyncUp()
        {
            if (PhotonNetwork.OfflineMode)
            {
                yield break;
            }
            yield return this.SyncMethod(nameof(GM_TRT.RPC_RequestSync), null, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPCA_NextRound(string winningRoleID)
        {
            //TimeHandler.instance.DoSlowDown();

            var instance = GM_TRT.instance;

            if (instance.isTransitioning)
            {
                return;
            }

            instance.CurrentPhase = RoundPhase.PostBattle;
            instance.clocktime = PostPhaseTime;
            IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
            string winMessage = winningRole?.WinMessage ?? "ROUND OVER";
            Color winColor = winningRole?.WinColor ?? DullWhite;
            UIHandler.instance.roundCounterSmall.UpdateText(1, winMessage, winColor, 30, Vector3.one, DisplayBackgroundColor, true);

            RoundSummary.LogWin(winningRoleID);
            RoundSummary.CreateRoundSummary(winningRoleID);

            //instance.StartCoroutine(instance.ClearRolesAndVisuals());

            // dont set battleOngoing to false since it will break localzoom for alive players in the postgame
            //GameManager.instance.battleOngoing = false;
            instance.isTransitioning = true;

            //PlayerManager.instance.SetPlayersSimulated(false);

            instance.pointsPlayedOnCurrentMap++;

            if (instance.pointsPlayedOnCurrentMap < (int)GameModeManager.CurrentHandler.Settings["pointsToWinRound"])
            {
                instance.PointOver(winningRoleID);
                return;
            }
            else
            {
                instance.pointsPlayedOnCurrentMap = 0;
                instance.roundsPlayed++;
                instance.RoundOver(winningRoleID);
            }

        }

        string GetClockString(float time_in_seconds)
        {
            return TimeSpan.FromSeconds(time_in_seconds).ToString(@"mm\:ss");
        }

        void Update()
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            this.syncCounter -= Time.unscaledDeltaTime;

            if (this.syncCounter <= 0f && PhotonNetwork.IsMasterClient)
            {
                this.syncCounter = SyncClockEvery;
                NetworkingManager.RPC_Others(typeof(GM_TRT), nameof(RPCO_SetClockTime), this.clocktime);
            }

            if (this.CurrentPhase == RoundPhase.Transitioning)
            {
                UIHandler.instance.roundCounterSmall.ClearTexts();
                this.clocktime = 0f;
                return;
            }

            this.clocktime -= TimeHandler.deltaTime;
            this.clocktime = UnityEngine.Mathf.Clamp(this.clocktime, 0f, float.PositiveInfinity);

            Color timeColor = DullWhite;
            switch (this.CurrentPhase)
            {
                case RoundPhase.PreBattle:
                    timeColor = DullWhite;
                    break;
                case RoundPhase.GracePeriod:
                    timeColor = GracePeriodColor;
                    break;
                case RoundPhase.Active:
                    if (this.clocktime < HasteModeAddPerDeath)
                    {
                        timeColor = WarningColor;
                    }
                    else
                    {
                        timeColor = DullWhite;
                    }
                    break;
                case RoundPhase.PostBattle:
                    timeColor = DullWhite;
                    break;
                case RoundPhase.Transitioning:
                    timeColor = DullWhite;
                    break;
                default:
                    timeColor = DullWhite;
                    break;
            }

            UIHandler.instance.roundCounterSmall.UpdateText(0, GetClockString(clocktime), timeColor, 30, Vector3.one, DisplayBackgroundColor, false);

            if (this.clocktime < GracePeriodTime && PhotonNetwork.IsMasterClient && this.CurrentPhase == RoundPhase.PreBattle)
            {
                NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SetRoundPhase), (byte)RoundPhase.GracePeriod);
                return;
            }

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && (this.CurrentPhase == RoundPhase.PreBattle || this.CurrentPhase == RoundPhase.GracePeriod))
            {
                NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SetRoundPhase), (byte)RoundPhase.Starting);
                return;
            }

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && this.CurrentPhase == RoundPhase.PostBattle)
            {
                NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SetRoundPhase), (byte)RoundPhase.Transitioning);
                return;
            }

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && this.BattleOngoing)
            {
                // short delay to allow things like phantom spawning and swapper swapping to happen
                if (this.isCheckingWinCondition) { return; }
                this.isCheckingWinCondition = true;
                this.ExecuteAfterFrames(10, () =>
                {
                    this.isCheckingWinCondition = false;

                    // out of time

                    string[] roleIDsAlive = PlayerManager.instance.players.Where(p => !p.data.dead).Select(p => RoleManager.GetPlayerRoleID(p)).ToArray();

                    // if there is a Killer still alive then the round does not end until they either die or win
                    if (roleIDsAlive.Any(rID => rID == KillerRoleHandler.KillerRoleID))
                    {
                        return;
                    }

                    string winningRoleID = null;

                    // if there is no Killer and there are any innocents left, they win
                    if (PlayerManager.instance.players.Any(p => !p.data.dead && RoleManager.GetPlayerAlignment(p) == Alignment.Innocent))
                    {
                        winningRoleID = InnocentRoleHandler.InnocentRoleID;
                    }

                    // if none of the above, (this shouldn't be a valid game state) then it's a draw
                    //NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPCA_DoSlowDown));
                    NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPCA_NextRound), winningRoleID);
                });

            }
        }

        [UnboundRPC]
        private static void RPCO_SetClockTime(float time)
        {
            GM_TRT.instance.clocktime = time;
        }
        [UnboundRPC]
        private static void RPCA_SetRoundPhase(byte phase)
        {
            GM_TRT.instance.CurrentPhase = (RoundPhase)phase;
        }
    }
}
