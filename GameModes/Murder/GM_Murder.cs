using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.Murder.Cards;
using GameModeCollection.GameModes.Murder.Controllers;
using GameModeCollection.Objects;
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

namespace GameModeCollection.GameModes.Murder
{
    /// <summary>
    /// Murder - Just like GMod Murder
    /// 
    /// [ ] - Blocking is disabled
    /// [ ] - Players have no weapons by default
    /// [ ] - Players (all roles) can collect clues (cards) 5 clues results in a revolver if they don't already have one and aren't the murderer
    /// 
    /// Roles:
    /// [ ] - Murderer (only one), has a knife and must kill everyone to win
    /// [ ] - Detective (only one), has a revolver. drops the revolver and is blinded if they kill a bystander
    /// [ ] - Bystander (everyone else), can pick up the revolver and clues. have no weapon
    /// </summary>
    public class GM_Murder : MonoBehaviour
    {
        internal static GM_Murder instance;

        private const float RoundTime = 300f; // default 300f
        private const float PrepPhaseTime = 5f; // default 5f
        private const float SyncClockEvery = 5f; // sync clock with host every 5 seconds

        private const float DefaultZoom = 40f;

        internal const float TimeBetweenCardDrops = 0.5f;
        private const float CardRandomVelMult = 0.25f;
        private const float CardRandomVelMin = 3f;
        private const float CardAngularVelMult = 10f;
        private const float CardHealth = -1f;

        internal enum Role
        {
            Bystander,
            Detective,
            Murderer
        }

        public readonly static Color BystanderColor = new Color32(26, 200, 25, 255);
        public readonly static Color DetectiveColor = new Color32(24, 29, 253, 255);
        public readonly static Color MurdererColor = new Color32(199, 25, 24, 255);

        public readonly static Color DullWhite = new Color32(230, 230, 230, 255);
        public readonly static Color WarningColor = new Color32(230, 0, 0, 255);
        public readonly static Color DisplayBackgroundColor = new Color32(0, 0, 0, 150);
        public readonly static Color NameBackgroundColor = new Color32(0, 0, 0, 200);

        private readonly ReadOnlyDictionary<int, int> roundCounterValues = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>() { { 0, 0 }, { 1, 0 } }) { };

        internal int pointsPlayedOnCurrentMap = 0;
        internal int roundsPlayed = 0;

        private bool isTransitioning = false;
        private Dictionary<int, Role> RolesToAssign = null;
        private int? timeUntilBattleStart = null;

        internal bool battleOngoing = false;
        private bool prebattle = false;

        private float clocktime = RoundTime;
        private float syncCounter = -1f;

        private void SetAllPlayersFOV()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                if (player.GetComponentInChildren<ViewSphere>(true) != null)
                {
                    player.GetComponentInChildren<ViewSphere>(true).fov = 361f;
                    player.GetComponentInChildren<ViewSphere>(true).viewDistance = 1000f;
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

        protected void Awake()
        {
            GM_Murder.instance = this;
        }

        protected void Start()
        {
            // register prefabs
            GameObject _ = CardItemPrefabs.CardItem;
            // spawn handler
            _ = CardItemPrefabs.CardItemHandler;
            this.StartCoroutine(this.Init());
        }
        private IEnumerator Init()
        {

            yield return GameModeManager.TriggerHook(GameModeHooks.HookInitStart);

            CardItemHandler.Instance.SetCanDiscard(true);
            CardItemHandler.Instance.PlayerDiscardAction += DropCard;

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerAssigner.instance.maxPlayers = RWF.RWFMod.instance.MaxPlayers;

            LocalZoom.MyCameraController.allowZoomIn = true;
            LocalZoom.MyCameraController.defaultZoomLevel = DefaultZoom;
            LocalZoom.LocalZoom.scaleCamWithBulletSpeed = true;
            LocalZoom.LocalZoom.enableLoSNamePlates = true;
            LocalZoom.LocalZoom.SetEnableShaderSetting(true);
            LocalZoom.LocalZoom.SetEnableCameraSetting(true);
            BetterChat.BetterChat.SetDeadChat(true);
            BetterChat.BetterChat.UsePlayerColors = false;

            ControllerManager.SetMapController(MurderMapController.ControllerID);
            ControllerManager.SetBoundsController(MurderBoundsController.ControllerID);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookInitEnd);
        }

        private void RandomizePlayerSkins()
        {
            if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode) { return; }
            int[] newColorIDs = Enumerable.Range(0, UnboundLib.Utils.ExtraPlayerSkins.numberOfSkins).OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).Distinct().ToArray();
            for (int i = 0; i < PlayerManager.instance.players.Count(); i++)
            {
                NetworkingManager.RPC(typeof(GM_Murder), nameof(RPCA_SetNewColors), PlayerManager.instance.players[i].playerID, newColorIDs[i]);
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
        public static void RPC_SyncBattleStart(int requestingPlayer, int timeOfBattleStart, Dictionary<int, byte> rolesToAssign)
        {

            // calculate the time in milliseconds until the battle starts
            GM_Murder.instance.timeUntilBattleStart = timeOfBattleStart - PhotonNetwork.ServerTimestamp;

            // set the roles to assign
            GM_Murder.instance.RolesToAssign = rolesToAssign.ToDictionary(kv => kv.Key, kv => (Role)kv.Value);

            NetworkingManager.RPC(typeof(GM_Murder), nameof(GM_Murder.RPC_SyncBattleStartResponse), requestingPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPC_SyncBattleStartResponse(int requestingPlayer, int readyPlayer)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == requestingPlayer)
            {
                GM_Murder.instance.RemovePendingRequest(readyPlayer, nameof(GM_Murder.RPC_SyncBattleStart));
            }
        }

        private Dictionary<int, Role> GetRoleLineup(int players)
        {
            return (new List<Role>() { Role.Murderer, Role.Detective }).Concat(Enumerable.Repeat(Role.Bystander, players - 2)).Shuffled().Select((r, i) => new { r, i }).ToDictionary(r => r.i, r => r.r);
        }

        protected IEnumerator SyncBattleStart()
        {
            // replacing original to be able to assign roles here as well

            if (PhotonNetwork.OfflineMode)
            {
                this.RolesToAssign = this.GetRoleLineup(PlayerManager.instance.players.Count());
                this.AssignRoles();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
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
                Dictionary<int, byte> serializableRolesToAssign = this.GetRoleLineup(PlayerManager.instance.players.Count()).ToDictionary(kv => kv.Key, kv => (byte)kv.Value);

                yield return this.SyncMethod(nameof(GM_Murder.RPC_SyncBattleStart), null, PhotonNetwork.LocalPlayer.ActorNumber, timeOfBattleStart, serializableRolesToAssign);
            }

            yield return new WaitUntil(() => this.timeUntilBattleStart != null && this.RolesToAssign != null);

            yield return new WaitForSecondsRealtime((float)this.timeUntilBattleStart * 0.001f);

            this.AssignRoles();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            this.timeUntilBattleStart = null;
            this.RolesToAssign = null;
        }
        private void AssignRoles()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                if (PhotonNetwork.IsMasterClient) { GameModeCollection.Log($"PLAYER {player.playerID} | {this.RolesToAssign[player.playerID]}"); }
                // TODO: add role cards here
            });
        }

        private Role GetPlayerRole(Player player)
        {
            return (player?.data?.currentCards?.Any(c => c.categories.Contains(MurderCardCategories.Murder_Murderer)) ?? false) ? Role.Murderer : Role.Bystander;
        }

        /// <summary>
        /// Returns the role which won the round.
        /// The muderer wins if they are the only player remaining
        /// The bystanders win if the murderer is no longer alive
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private Role? GetWinningRole(Player[] players)
        {
            bool murdererAlive = players.Where(p => !p.data.dead).Any(p => p.data.currentCards.Any(c => c.categories.Contains(MurderCardCategories.Murder_Murderer)));
            bool bystanderAlive = players.Where(p => !p.data.dead && !p.data.currentCards.Any(c => c.categories.Contains(MurderCardCategories.Murder_Murderer))).Any();
            
            if (murdererAlive && bystanderAlive) { return null; }
            else
            {
                return murdererAlive ? Role.Murderer : Role.Bystander;
            }
        }

        [UnboundRPC]
        static void RPCA_SetNewColors(int playerID, int colorID)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);

            UnboundLib.Extensions.PlayerExtensions.AssignColorID(player, colorID);
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
            foreach (CardInfo card in cardsToDrop.Where(c => !c.categories.Contains(MurderCardCategories.Murder_DoNotDropOnDeath)))
            {
                if (!this.battleOngoing) { yield break; }
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
            // handle Murder corpse creation, dropping cards, check win conditions

            // drop cards
            GameModeCollection.Log($"Player {killedPlayer.playerID} dropping cards...");

            CardInfo[] cardsToDrop = killedPlayer.data.currentCards.ToArray();
            killedPlayer.InvokeMethod("FullReset");
            this.StartCoroutine(this.DropCardsOnDeath(killedPlayer, cardsToDrop));

            if (killedPlayer.data.view.IsMine)
            {
                UIHandler.instance.roundCounterSmall.UpdateText(1, "ONGOING", DullWhite, 30, Vector3.one, DisplayBackgroundColor);
            }

            Role? winningRole = GetWinningRole(PlayerManager.instance.players.ToArray());

            if (winningRole != null)
            {

                if (PhotonNetwork.IsMasterClient)
                {
                    NetworkingManager.RPC(typeof(GM_Murder), nameof(GM_Murder.RPCA_NextRound), (byte)winningRole);
                }
            }
        }

        public void StartGame()
        {
            if (GameManager.instance.isPlaying)
            {
                return;
            }

            PlayerManager.instance.ForEachPlayer(this.PlayerJoined);

            BetterChat.BetterChat.SetDeadChat(true);
            BetterChat.BetterChat.UsePlayerColors = false;
            ControllerManager.SetMapController(MurderMapController.ControllerID);
            ControllerManager.SetBoundsController(MurderBoundsController.ControllerID);

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

            LocalZoom.MyCameraController.defaultZoomLevel = DefaultZoom;
            LocalZoom.MyCameraController.allowZoomIn = true;

            GameManager.instance.battleOngoing = false;

            UIHandler.instance.ShowJoinGameText("TROUBLE\nIN\nROUNDS TOWN", Color.white);
            yield return new WaitForSecondsRealtime(2f);
            UIHandler.instance.HideJoinGameText();
            yield return this.WaitForSyncUp();

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

            //MapManager.instance.LoadNextLevel(false, false);
            yield return MurderMapManager.LoadNextMurderLevel(false, false);

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            PlayerManager.instance.ForEachPlayer(p => p.InvokeMethod("FullReset"));

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

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            this.SetAllPlayersFOV();
            this.HideAllPlayerFaces();
            this.RegisterAllWobbleObjects();

            //yield return MurderCardManager.SpawnClues(CardsToSpawnPerPlayer * PlayerManager.instance.players.Count(), CardHealth, true);

            yield return this.WaitForSyncUp();

            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundStart);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            this.clocktime = PrepPhaseTime;
            this.prebattle = true;

            var sounds = GameObject.Find("/SonigonSoundEventPool");

            UIHandler.instance.roundCounterSmall.UpdateText(1, "PREPARING", DullWhite, 30, Vector3.one, DisplayBackgroundColor);

            yield return new WaitWhile(() => this.prebattle);

            yield return this.SyncBattleStart();
            this.HideAllPlayerFaces();

            this.clocktime = RoundTime;
            this.prebattle = false;
            this.battleOngoing = true;

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(false);
            PlayerManager.instance.RevivePlayers();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);
        }

        public IEnumerator DoPointStart()
        {
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");

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

            //yield return MurderCardManager.SpawnClues(CardsToSpawnPerPlayer * PlayerManager.instance.players.Count(), CardHealth, true);

            //PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();
            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            this.clocktime = PrepPhaseTime;
            this.prebattle = true;

            var sounds = GameObject.Find("/SonigonSoundEventPool");

            UIHandler.instance.roundCounterSmall.UpdateText(1, "PREPARING", DullWhite, 30, Vector3.one, DisplayBackgroundColor);

            yield return new WaitWhile(() => this.prebattle);

            yield return this.SyncBattleStart();
            this.HideAllPlayerFaces();

            this.clocktime = RoundTime;
            this.prebattle = false;
            this.battleOngoing = true;

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            PlayerManager.instance.SetPlayersInvulnerableAndIntangible(false);
            PlayerManager.instance.RevivePlayers();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);

        }
        public IEnumerator RoundTransition(string winningRoleID)
        {
            this.battleOngoing = false;
            this.prebattle = false;
            this.clocktime = 0f;

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundEnd);

            if (this.roundsPlayed >= (int)GameModeManager.CurrentHandler.Settings["roundsToWinGame"])
            {
                this.GameOver();
                yield break;
            }

            if (winningRoleID is null)
            {
                this.StartCoroutine(PointVisualizer.instance.DoSequence("DRAW", DullWhite));
                if (PhotonNetwork.IsMasterClient) { MurderHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                // TODO
                //IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                //this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                //if (PhotonNetwork.IsMasterClient) { MurderHandler.SendPointOverChat(winningRole); }
            }

            yield return new WaitForSecondsRealtime(1f);
            //MapManager.instance.LoadNextLevel(false, false);
            yield return MurderMapManager.LoadNextMurderLevel(false, false);

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
            PlayerManager.instance.ForEachPlayer(p => p.InvokeMethod("FullReset"));

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
            this.battleOngoing = false;
            this.prebattle = false;
            this.clocktime = 0f;

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);

            if (winningRoleID is null)
            {
                this.StartCoroutine(PointVisualizer.instance.DoSequence("DRAW", DullWhite));
                if (PhotonNetwork.IsMasterClient) { MurderHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                // TODO
                /*
                IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                if (PhotonNetwork.IsMasterClient) { MurderHandler.SendPointOverChat(winningRole); }
                */
            }

            yield return new WaitForSecondsRealtime(1f);
            //MapManager.instance.LoadNextLevel(false, false);
            //MurderMapManager.LoadNextMurderLevel(false, false);
            yield return MurderMapManager.ReLoadMurderLevel(false, false);

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
            PlayerManager.instance.ForEachPlayer(p => p.InvokeMethod("FullReset"));

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
                this.StartCoroutine(MurderMapManager.LoadNextMurderLevel(false, false));
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
            UIHandler.instance.DisplayScreenText(Color.white, "MURDER", 1f);
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
            MurderCardManager.RemoveAllCardItems();
            this.StartCoroutine(this.RoundTransition(winningRoleID));
        }

        public void PointOver(string winningRoleID)
        {
            MurderCardManager.RemoveAllCardItems();
            this.StartCoroutine(this.PointTransition(winningRoleID));
        }

        [UnboundRPC]
        public static void RPC_RequestSync(int requestingPlayer)
        {
            NetworkingManager.RPC(typeof(GM_Murder), nameof(GM_Murder.RPC_SyncResponse), requestingPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPC_SyncResponse(int requestingPlayer, int readyPlayer)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == requestingPlayer)
            {
                GM_Murder.instance.RemovePendingRequest(readyPlayer, nameof(GM_Murder.RPC_RequestSync));
            }
        }

        private IEnumerator WaitForSyncUp()
        {
            if (PhotonNetwork.OfflineMode)
            {
                yield break;
            }
            yield return this.SyncMethod(nameof(GM_Murder.RPC_RequestSync), null, PhotonNetwork.LocalPlayer.ActorNumber);
        }

        [UnboundRPC]
        public static void RPCA_NextRound(string winningRoleID)
        {
            TimeHandler.instance.DoSlowDown();

            var instance = GM_Murder.instance;

            if (instance.isTransitioning)
            {
                return;
            }

            PlayerManager.instance.ForEachPlayer(p => p.InvokeMethod("FullReset"));

            GameManager.instance.battleOngoing = false;
            instance.isTransitioning = true;

            PlayerManager.instance.SetPlayersSimulated(false);

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
            if (GameModeManager.CurrentHandlerID != MurderHandler.GameModeID) { return; }

            this.syncCounter -= Time.unscaledDeltaTime;

            if (this.syncCounter <= 0f && PhotonNetwork.IsMasterClient)
            {
                this.syncCounter = SyncClockEvery;
                NetworkingManager.RPC_Others(typeof(GM_Murder), nameof(RPCO_SetClockTime), this.clocktime);
            }

            if (!this.prebattle && !this.battleOngoing)
            {
                UIHandler.instance.roundCounterSmall.ClearTexts();
                this.clocktime = 0f;
                return;
            }

            this.clocktime -= TimeHandler.deltaTime;
            this.clocktime = UnityEngine.Mathf.Clamp(this.clocktime, 0f, float.PositiveInfinity);

            Color timeColor = this.prebattle ? DullWhite : (this.clocktime < 30f ? WarningColor : DullWhite);

            UIHandler.instance.roundCounterSmall.UpdateText(0, GetClockString(clocktime), timeColor, 30, Vector3.one, DisplayBackgroundColor);

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && this.prebattle)
            {
                NetworkingManager.RPC(typeof(GM_Murder), nameof(RPCA_SetPreBattle), false);
                return;
            }

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && this.battleOngoing)
            {
                // TODO
                /*
                // out of time
                string[] roleIDsAlive = PlayerManager.instance.players.Where(p => !p.data.dead).Select(p => RoleManager.GetPlayerRoleID(p)).ToArray();

                string winningRoleID = null;

                // if there is no Killer and there are any innocents left, they win
                if (PlayerManager.instance.players.Any(p => !p.data.dead && RoleManager.GetPlayerAlignment(p) == Alignment.Innocent))
                {
                    winningRoleID = InnocentRoleHandler.InnocentRoleID;
                }*/
                string winningRoleID = null;

                // if none of the above, (this shouldn't be a valid game state) then it's a draw
                NetworkingManager.RPC(typeof(GM_Murder), nameof(GM_Murder.RPCA_DoSlowDown));
                NetworkingManager.RPC(typeof(GM_Murder), nameof(GM_Murder.RPCA_NextRound), winningRoleID);
            }
        }
        [UnboundRPC]
        private static void RPCA_DoSlowDown()
        {
            TimeHandler.instance.DoSlowDown();
        }

        [UnboundRPC]
        private static void RPCO_SetClockTime(float time)
        {
            GM_Murder.instance.clocktime = time;
        }
        [UnboundRPC]
        private static void RPCA_SetPreBattle(bool prebattle)
        {
            GM_Murder.instance.prebattle = prebattle;
        }
    }
}
