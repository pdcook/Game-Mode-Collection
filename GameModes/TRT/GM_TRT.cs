using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.GameModes.TRT.Controllers;
using GameModeCollection.GameModes.TRT.Roles;
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

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// 
    /// Trouble In Rounds Town - just like Trouble in Terrorist Town
    /// 
    /// 
    /// Maps only transition on round end, NOT point end
    /// No pick phase
    /// 
    /// Minimum three (maybe four?) players
    /// 
    /// 
    /// Notes:
    /// 
    /// - [X] There are no game winners / losers. the game is 4 maps with 4 battles each, the game ends after all 16 have been played
    /// 
    /// - [X] Each client flips over cards ONLY they walk near, and they stay flipped (large circular trigger collider)
    /// - [X] Cards can be collected by walking near them and clicking interact (F by default) (smaller box trigger collider just barely larger than card's box collider)
    /// - [X] Cards have health (possibly proportional to their card health stat) and can be shot and permanently destroyed
    /// - [X] Need to patch cards healing players when taken
    /// - [X] Player skins are randomized each round (sorry)
    /// - [X] Player faces are psuedo-randomized (double sorry)
    /// - [X] Player nicknames are removed entirely (triple sorry)
    /// - [X] Players are completely hidden during the skin randomization time
    /// - [X] Local zoom is ON. optionally (by host settings in mod options) with the dark shader
    /// - [X] local zoom scales with bullet speed instead of player size
    /// - [X] RDM is punished (innocent killing innocent) somehow
    /// - [X] Clock in upper left corner (with round counter) that counts down.
    ///         --> Haste mode:
    ///         --> The timer starts at some initial value (5 mins?), counting down
    ///         --> For each death, of any kind, time is added to the clock (30 seconds?)
    /// - [X] below the clock (also with the round counter) is the player's current role
    /// - [X] Each client sees ONLY their own card bar
    /// - [X] Players can have a max of one card
    /// - [X] Dead player's bodies remain on the map (maybe without limbs?) by a patch in HealthHandler::RPCA_Die that freezes them and places them on the nearest ground straight down
    /// - [X] Dead players have a separate text chat
    /// - [X] Players can discard cards by clicking on the square in the card bar
    ///     --> Or by pressing Q to discard their most recent card
    /// - [X] If a non-detective player crouches over a body, it will report it (in the chat?) to the detective [EX: Pykess found the body of Ascyst, they were an innocent!]
    /// - [X] If a detective crouches over a body it will report the approximate color [orang-ish, redd-ish, blue-ish, or green-ish] of the killer (in the chat?) [EX: Pykess inspected the body of Ascyst, the were a traitor killed by a blue-ish player!]
    /// - [X] Add hotkeys for quick chats like: (E -> "[nearest player] is suspicious") (F -> "I'm with [nearest player]") (R -> "Kill [nearest player]!!!")
    /// - [X] custom maps specifically for this mode, not available in normal rotation
    ///   [X] --> custom map object mod for card spawn points
    /// - [X] card random spawning
    /// - [X] Remove screenshake entirely, or make it dependent on distance (if possible)
    /// - [ ] Low karma punishment: slaying. the player is killed AFTER the next round starts and is forced to sit out the round
    /// - [ ] LaTeX document with a short guide to each role
    /// - [ ] Round summaries in chat
    /// - [~] T and D shops...
    /// - [~] Custom cards specifically for certain roles
    ///     - [~] (T) C4 - TODO: beeping, explosion, sound, diffusal
    ///     - [~] (T) Knife - TODO: knife asset to replace gun
    ///     - [X] (D) Golden Gun - change layer and color of gun handle/barrel to gold
    ///         --> One time use, once it hits any player, it is destroyed
    ///         --> If it shoots a traitor/killer, they will die instantly, no phoenix revives either
    ///         --> If it shoots an innocent, the shooter will die instantly, no phoenix revives
    ///         --> If it shoots a jester/swapper, BOTH players will be killed instantly, no phoenix revives AND the jester/swapper will NOT win
    ///     - [X] (T + D) Radar
    ///     - [~] (D) Health Station - TODO: gmod sound effect
    ///     - [X] (T) Death Station
    ///     - [ ] (Z) Claw (same as knife) for zombies to infect others
    ///     - [ ] (D) Diffuser
    ///     - [X] (T + D) Body Armor
    /// 
    /// Roles:
    /// - [X] Innocent
    /// - [X] Traitor (red name, sees other traitors' names as red with a "[T]" in front, notified of other traitors and jesters at start of round) [can have two cards instead of one]
    /// - [~] Detective (blue name visible to everyone with a "[D]" in front, spawns with HealingField, or Huge if healing field is unavailable)
    /// Roles for more than four players:
    /// - [X] Jester (own team) (pink name, visible to the traitors only with a "[J]" in front) [deals no damage]
    /// - [X] Glitch (is innocent, but appears as a traitor to the traitors)
    /// - [X] Mercenary (innocent) [can have two cards instead of one]
    /// - [X] Phantom (innocent) [haunts their killer with a smoke trail in their color. when their killer dies, they revive with 50% health]
    /// - [X] Killer (own team, can only ever be one at a time, traitors are notified that there is a killer) [has 150% health and can have up to four cards (two more than traitors)]
    /// - [X] Hypnotist (traitor) [the first corpse they interact2 with will respawn as a traitor]
    /// - [X] Zombie (has a chance to spawn instead of all traitors) (cannot have ANY cards) [players killed by any zombie will immediately revive as zombies]
    /// - [X] Swapper ("innocent") (appears to traitors as a jester) [cannot deal damage, when killed, their attacker dies instead and they instantly respawn with the role of the attacker, when the attacker's body is searched they report as a swapper]
    /// - [X] Assassin (traitor) [gets a "target" (never detective unless that is the only option) to which they deal double damage, and half damage to all other players. killing the wrong player results in them dealing half damage for the rest of the round]
    /// - [X] Vampire (traitor) [can interact2 with a dead body to eat it (completely destroying the body) and healing 50 HP, though it freezes them in place for a few seconds]
    /// </summary>
    public class GM_TRT : MonoBehaviour
    {
        internal static GM_TRT instance;

        private const float RoundTime = 300f; // default 300f
        private const float PrepPhaseTime = 1f;//30f; // default 30f
        private const float HasteModeAddPerDeath = 30f; // default 30f
        private const float SyncClockEvery = 5f; // sync clock with host every 5 seconds

        private const float DefaultZoom = 40f;

        private const float TimeBetweenCardDrops = 0.5f;
        private const float CardRandomVelMult = 0.25f;
        private const float CardRandomVelMin = 3f;
        private const float CardAngularVelMult = 10f;
        private const float CardHealth = -1f;

        public const float KarmaPenaltyPerRDM = 0.2f; // you lose 0.2 (20%) karma for each RDM
        public const float KarmaRewardPerPoint = 0.1f; // you gain 0.1 (10%) karma for each clean point
        public const float MinimumKarma = 0.1f; // the minimum karma is 0.1 (10%)
        public const float KarmaFractionForDeath = 0.25f; // if you are dead at the end of a point, you only gain 25% of the 10% you would usuall gain

        public const int BaseMaxCards = 1;
        public const float BaseHealth = 100f;

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
        public readonly static Color WarningColor = new Color32(230, 0, 0, 255);
        public readonly static Color DisplayBackgroundColor = new Color32(0, 0, 0, 150);
        public readonly static Color NameBackgroundColor = new Color32(0, 0, 0, 200);

        private readonly ReadOnlyDictionary<int, int> roundCounterValues = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>() { { 0, 0 }, { 1, 0 } }) { };

        internal int pointsPlayedOnCurrentMap = 0;
        internal int roundsPlayed = 0;

        private bool isCheckingWinCondition = false;
        private bool isTransitioning = false;
        private Dictionary<int, string> RoleIDsToAssign = null;
        private int? timeUntilBattleStart = null;

        private bool battleOngoing = false;
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
                if (player?.GetComponentInChildren<PlayerWobblePosition>() is null) { return; }
                LocalZoom.Extensions.CharacterDataExtension.GetData(player.data).allWobbleImages.AddRange(player.GetComponentInChildren<PlayerWobblePosition>().GetComponentsInChildren<UnityEngine.UI.Image>(true));
                LocalZoom.Extensions.CharacterDataExtension.GetData(player.data).allWobbleImages = LocalZoom.Extensions.CharacterDataExtension.GetData(player.data).allWobbleImages.Distinct().ToList();
            });
        }

        protected void Awake()
        {
            GM_TRT.instance = this;
            RoleManager.Init();
        }

        protected void Start()
        {
            // register prefabs
            GameObject _ = CardItemPrefabs.CardItem;
            _ = HealthStationPrefab.HealthStation;
            _ = DeathStationPrefab.DeathStation;
            _ = C4Prefab.C4;
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
            TRTHandler.InitChatGroups();
            BetterChat.BetterChat.SetDeadChat(true);
            BetterChat.BetterChat.UsePlayerColors = true;

            ControllerManager.SetMapController(TRTMapController.ControllerID);
            ControllerManager.SetBoundsController(TRTBoundsController.ControllerID);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookInitEnd);
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
                    UnityEngine.Random.Range(0, CharacterCreatorItemLoader.instance.eyes.Count()),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    UnityEngine.Random.Range(0, CharacterCreatorItemLoader.instance.mouths.Count()),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    CharacterCreatorItemLoader.instance.GetRandomItemID(CharacterItemType.Detail, new string[] { "TRT_Detective_Hat" }),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    CharacterCreatorItemLoader.instance.GetRandomItemID(CharacterItemType.Detail, new string[] { "TRT_Detective_Hat" }),
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
                if (PhotonNetwork.IsMasterClient) { GameModeCollection.Log($"PLAYER {player.playerID} | {this.RoleIDsToAssign[player.playerID]}"); }
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
                player.data.TRT_ChangeKarma(change, GM_TRT.MinimumKarma);
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
        private IEnumerator PlayerDropCard(Player player, CardInfo card)
        {
            if (!this.battleOngoing) { yield break; }
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
            // every time a player dies, time is added to the clock
            this.clocktime += HasteModeAddPerDeath / PlayerManager.instance.players.Select(p => p.data.view.ControllerActorNr).Distinct().Count();

            // handle TRT corpse creation, dropping cards, check win conditions

            // drop cards
            GameModeCollection.Log($"Player {killedPlayer.playerID} dropping cards...");

            CardInfo[] cardsToDrop = killedPlayer.data.currentCards.ToArray();
            killedPlayer.InvokeMethod("FullReset");
            this.StartCoroutine(this.DropCardsOnDeath(killedPlayer, cardsToDrop));

            // corpse creation
            this.PlayerCorpse(killedPlayer);
            
            if (killedPlayer.data.view.IsMine)
            {
                UIHandler.instance.roundCounterSmall.UpdateText(1, "ONGOING", DullWhite, 30, Vector3.one, DisplayBackgroundColor);
            }

            // check win condition after a short delay to allow things like phantom spawning and swapper swapping to happen
            if (this.isCheckingWinCondition || !PhotonNetwork.IsMasterClient) { return; }
            this.isCheckingWinCondition = true;
            this.ExecuteAfterFrames(10, () =>
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

            BetterChat.BetterChat.SetDeadChat(true);
            BetterChat.BetterChat.UsePlayerColors = true;
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
            yield return TRTMapManager.LoadNextTRTLevel(false, false);

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            yield return this.ClearRolesAndVisuals();

            // reset karma
            PlayerManager.instance.ResetKarma();

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
            PlayerManager.instance.SetPlayersInvulnerable(true);

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

            yield return TRTCardManager.SpawnCards(2 * PlayerManager.instance.players.Count(), CardHealth, true);

            yield return this.WaitForSyncUp();

            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerable(true);

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
            //UIHandler.instance.DisplayRoundStartText("INNOCENT", InnocentColor, new Vector3(0.5f, 0.8f, 0f));
            PlayerManager.instance.SetPlayersInvulnerable(false);
            PlayerManager.instance.RevivePlayers();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);
        }

        public IEnumerator DoPointStart()
        {
            PlayerManager.instance.SetPlayersInvulnerable(true);

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

            yield return TRTCardManager.SpawnCards(2 * PlayerManager.instance.players.Count(), CardHealth, true);

            //PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();
            PlayerManager.instance.SetPlayersSimulated(true);
            PlayerManager.instance.SetPlayersInvulnerable(true);

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
            //UIHandler.instance.DisplayRoundStartText("TRAITOR", TraitorColor, new Vector3(0.5f, 0.8f, 0f));
            PlayerManager.instance.SetPlayersInvulnerable(false);
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
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendPointOverChat(winningRole); }
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
            this.battleOngoing = false;
            this.prebattle = false;
            this.clocktime = 0f;

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);

            if (winningRoleID is null)
            {
                this.StartCoroutine(PointVisualizer.instance.DoSequence("DRAW", DullWhite));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendPointOverChat(winningRole); }
            }

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
            /*
            this.battleOngoing = false;
            this.prebattle = false;
            this.clocktime = 0f;

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);

            if (winningRoleID is null)
            {
                this.StartCoroutine(PointVisualizer.instance.DoSequence("DRAW", DullWhite));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendChat(null, "<b>DRAW - NOBODY WINS</b>", false); }
            }
            else
            {
                IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
                this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
                if (PhotonNetwork.IsMasterClient) { TRTHandler.SendPointOverChat(winningRole); }
            }

            yield return new WaitForSecondsRealtime(1f);

            //MapManager.instance.LoadLevelFromID(MapManager.instance.currentLevelID, false, false);
            yield return TRTMapManager.LoadTRTLevelFromID(TRTMapManager.CurrentLevel, false, false);

            yield return new WaitForSecondsRealtime(1.3f);

            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

            yield return this.WaitForSyncUp();

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
            RWF.UIHandlerExtensions.ShowRoundCounterSmall(UIHandler.instance, this.roundCounterValues, this.roundCounterValues);

            this.StartCoroutine(this.DoPointStart());
            */
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
            TimeHandler.instance.DoSlowDown();

            var instance = GM_TRT.instance;

            if (instance.isTransitioning)
            {
                return;
            }

            instance.StartCoroutine(instance.ClearRolesAndVisuals());

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
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            this.syncCounter -= Time.unscaledDeltaTime;

            if (this.syncCounter <= 0f && PhotonNetwork.IsMasterClient)
            {
                this.syncCounter = SyncClockEvery;
                NetworkingManager.RPC_Others(typeof(GM_TRT), nameof(RPCO_SetClockTime), this.clocktime);
            }

            if (!this.prebattle && !this.battleOngoing)
            {
                UIHandler.instance.roundCounterSmall.ClearTexts();
                this.clocktime = 0f;
                return;
            }

            this.clocktime -= TimeHandler.deltaTime;
            this.clocktime = UnityEngine.Mathf.Clamp(this.clocktime, 0f, float.PositiveInfinity);

            Color timeColor = this.prebattle ? DullWhite : (this.clocktime < HasteModeAddPerDeath ? WarningColor : DullWhite);

            UIHandler.instance.roundCounterSmall.UpdateText(0, GetClockString(clocktime), timeColor, 30, Vector3.one, DisplayBackgroundColor);

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && this.prebattle)
            {
                NetworkingManager.RPC(typeof(GM_TRT), nameof(RPCA_SetPreBattle), false);
                return;
            }

            if (this.clocktime == 0f && PhotonNetwork.IsMasterClient && this.battleOngoing)
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
                    NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPCA_DoSlowDown));
                    NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPCA_NextRound), winningRoleID);
                });

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
            GM_TRT.instance.clocktime = time;
        }
        [UnboundRPC]
        private static void RPCA_SetPreBattle(bool prebattle)
        {
            GM_TRT.instance.prebattle = prebattle;
        }
    }
}
