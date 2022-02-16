using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using GameModeCollection.Extensions;
using GameModeCollection.Objects;
using UnboundLib.GameModes;
using Sonigon;
using GameModeCollection.Utils;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.GameModeHandlers;

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
    /// - [X] Cards can be collected by walking near them (smaller box trigger collider just barely larger than card's box collider)
    /// - [X] Cards have health (possibly proportional to their card health stat) and can be shot and permanently destroyed
    /// - [X] Need to patch cards healing players when taken
    /// - [X] Player skins are randomized each round (sorry)
    /// - [X] Player faces are psuedo-randomized (double sorry)
    /// - [X] Player nicknames are removed entirely (triple sorry)
    /// - [X] Players are completely hidden during the skin randomization time
    /// - [ ] Local zoom is ON. optionally (how?) with the dark shader
    /// - [ ] local zoom scales with bullet speed instead of player size
    /// - [X] RDM is punished (innocent killing innocent) somehow
    /// - [ ] Clock in upper left corner (with round counter) that counts down. when the timer reaches 0, it turns red, signaling haste mode
    /// - [ ]   --> Figure out what to do for Haste Mode
    /// - [ ] below the clock (also with the round counter) is the player's current role
    /// - [X] Each client sees ONLY their own card bar
    /// - [ ]   --> until they die and enter spectator mode
    /// - [~] Players can have a max of one card
    /// - [X] Dead player's bodies remain on the map (maybe without limbs?) by a patch in HealthHandler::RPCA_Die that freezes them and places them on the nearest ground straight down
    /// - [~] Dead players have a separate text chat
    /// - [ ] Players can discard cards by clicking on the square in the card bar
    /// - [X] If a non-detective player crouches over a body, it will report it (in the chat?) to the detective [EX: Pykess found the body of Ascyst, they were an innocent!]
    /// - [X] If a detective crouches over a body it will report the approximate color [orang-ish, redd-ish, blue-ish, or green-ish] of the killer (in the chat?) [EX: Pykess inspected the body of Ascyst, the were a traitor killed by a blue-ish player!]
    /// - [~] Add hotkeys for quick chats like: (E -> "[nearest player] is suspicious") (F -> "I'm with [nearest player]") (R -> "Kill [nearest player]!!!")
    /// - [~] custom maps specifically for this mode, not available in normal rotation - can utilize either custom map objects or spawn points for weapon/item spawns
    /// - [ ] card random spawning
    /// - [ ] LaTeX document with a short guide to each role
    /// - [ ] Round summaries in chat
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
    /// - [~] Killer (own team, can only ever be one at a time, traitors are notified that there is a killer) [has 150% health, starts with a random card (respecting rarity) and can have up to four cards (two more than traitors)]
    /// - [X] Hypnotist (traitor) [the first corpse they interact2 with will respawn as a traitor]
    /// - [X] Zombie (has a chance to spawn instead of all traitors) (cannot have ANY cards) [players killed by any zombie will immediately revive as zombies]
    /// - [X] Swapper ("innocent") (appears to traitors as a jester) [cannot deal damage, when killed, their attacker dies instead and they instantly respawn with the role of the attacker, when the attacker's body is searched they report as a swapper]
    /// - [X] Assassin (traitor) [gets a "target" (never detective unless that is the only option) to which they deal double damage, and half damage to all other players. killing the wrong player results in them dealing half damage for the rest of the round]
    /// - [X] Vampire (traitor) [can interact2 with a dead body to eat it (completely destroying the body) and healing 50 HP, though it freezes them in place for a few seconds]
    /// </summary>
    public class GM_TRT : MonoBehaviour
    {
        internal static GM_TRT instance;

        private const float PrepPhaseTime = 1f;
        private const float TimeBetweenCardDrops = 0.5f;
        private const float CardRandomVelMult = 0.25f;
        private const float CardRandomVelMin = 3f;
        private const float CardAngularVelMult = 10f;
        private const float CardHealth = 100f;

        public const float KarmaPenaltyPerRDM = 0.1f; // you lose 0.1 (10%) karma for each RDM
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

        internal int pointsPlayedOnCurrentMap = 0;
        internal int roundsPlayed = 0;

        private bool isCheckingWinCondition = false;
        private bool isTransitioning = false;
        private Dictionary<int, string> RoleIDsToAssign = null;
        private int? timeUntilBattleStart = null;

        protected void Awake()
        {
            GM_TRT.instance = this;
            RoleManager.Init();
        }

        protected void Start()
        {
            // register prefab
            GameObject _ = CardItemPrefabs.CardItem;
            // spawn handler
            _ = CardItemPrefabs.CardItemHandler;
            this.StartCoroutine(this.Init());
        }
        private IEnumerator Init()
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookInitStart);

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerAssigner.instance.maxPlayers = RWF.RWFMod.instance.MaxPlayers;

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
                    UnityEngine.Random.Range(0, CharacterCreatorItemLoader.instance.accessories.Count()),
                    RandomUtils.ClippedGaussianVector2(-1, -1, 1, 1),
                    UnityEngine.Random.Range(0, CharacterCreatorItemLoader.instance.accessories.Count()),
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

            this.timeUntilBattleStart = null;
            this.RoleIDsToAssign = null;
        }
        private void AssignRoles()
        {
            PlayerManager.instance.ForEachPlayer(player =>
            {
                GameModeCollection.Log($"PLAYER {player.playerID} | {this.RoleIDsToAssign[player.playerID]}");
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
        private IEnumerator DropCardsOnDeath(Player player, CardInfo[] cardsToDrop)
        {
            foreach (CardInfo card in cardsToDrop)
            {
                yield return new WaitForSecondsRealtime(TimeBetweenCardDrops);
                Vector2 velocty = (Vector2)player.data.playerVel.GetFieldValue("velocity");
                yield return CardItem.MakeCardItem(card,
                                                    player.data.playerVel.position,
                                                    Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)),
                                                    velocty + UnityEngine.Mathf.Clamp(CardRandomVelMult * velocty.magnitude, CardRandomVelMin, float.MaxValue) * new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)),
                                                    -CardAngularVelMult * velocty.x,
                                                    CardHealth, true);
            }
            yield break;
        }

        public void PlayerJoined(Player player)
        {
            // completely replace original, since we don't need teamPoints or teamRounds

            // reset Karma
            player.data.TRT_ResetKarma();
        }

        public void PlayerDied(Player killedPlayer, int teamsAlive)
        {
            // completely replace original method

            // handle TRT corpse creation, dropping cards, check win conditions

            // drop cards
            GameModeCollection.Log($"Player {killedPlayer.playerID} dropping cards...");

            CardInfo[] cardsToDrop = killedPlayer.data.currentCards.ToArray();
            killedPlayer.data.currentCards.Clear();
            this.StartCoroutine(this.DropCardsOnDeath(killedPlayer, cardsToDrop));

            // corpse creation
            this.PlayerCorpse(killedPlayer);

            // check win condition after a short delay to allow things like phantom spawning and swapper swapping to happen
            if (this.isCheckingWinCondition) { return; }
            this.isCheckingWinCondition = true;
            this.ExecuteAfterFrames(10, () =>
            {
                this.isCheckingWinCondition = false;

                string winningRoleID = RoleManager.GetWinningRoleID(PlayerManager.instance.players.ToArray());

                if (winningRoleID != null)
                {

                    TimeHandler.instance.DoSlowDown();
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

            GameManager.instance.isPlaying = true;
            this.StartCoroutine(this.DoStartGame());
        }

        public IEnumerator DoStartGame()
        {
            // completely replace original method
            RWF.CardBarHandlerExtensions.Rebuild(CardBarHandler.instance);
            //UIHandler.instance.InvokeMethod("SetNumberOfRounds", (int) GameModeManager.CurrentHandler.Settings["roundsToWinGame"]);
            ArtHandler.instance.NextArt();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameStart);

            GameManager.instance.battleOngoing = false;

            UIHandler.instance.ShowJoinGameText("TROUBLE\nIN\nROUNDS TOWN", Color.white);
            yield return new WaitForSecondsRealtime(2f);
            UIHandler.instance.HideJoinGameText();
            yield return this.WaitForSyncUp();

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

            MapManager.instance.LoadNextLevel(false, false);

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            yield return this.ClearRolesAndVisuals();

            // reset karma
            PlayerManager.instance.ResetKarma();

            TimeHandler.instance.DoSpeedUp();

            yield return new WaitForSecondsRealtime(1f);
            yield return this.WaitForSyncUp();
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);
            TimeHandler.instance.DoSpeedUp();
            TimeHandler.instance.StartGame();
            GameManager.instance.battleOngoing = true;
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);

            this.StartCoroutine(this.DoRoundStart());

        }
        public IEnumerator DoRoundStart()
        {
            // completely replace original method

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            // players get karma reset on new round
            PlayerManager.instance.ResetKarma();

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            // TODO: REMOVE THIS
            yield return CardItem.MakeCardItem(CardChoice.instance.cards.GetRandom<CardInfo>(), Vector3.zero, Quaternion.identity, maxHealth: 100f, requireInteract: true);

            yield return this.WaitForSyncUp();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundStart);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            var sounds = GameObject.Find("/SonigonSoundEventPool");

            yield return new WaitForSecondsRealtime(PrepPhaseTime);

            yield return this.SyncBattleStart();

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            //UIHandler.instance.DisplayRoundStartText("INNOCENT", InnocentColor, new Vector3(0.5f, 0.8f, 0f));
            PlayerManager.instance.SetPlayersSimulated(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);
        }

        public IEnumerator DoPointStart()
        {
            // completely replace original

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            // TODO: REMOVE THIS
            yield return CardItem.MakeCardItem(CardChoice.instance.cards.GetRandom<CardInfo>(), Vector3.zero, Quaternion.identity, maxHealth: 100f, requireInteract: true);

            //PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            var sounds = GameObject.Find("/SonigonSoundEventPool");

            yield return new WaitForSecondsRealtime(PrepPhaseTime);

            yield return this.SyncBattleStart();

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            //UIHandler.instance.DisplayRoundStartText("TRAITOR", TraitorColor, new Vector3(0.5f, 0.8f, 0f));
            PlayerManager.instance.SetPlayersSimulated(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);

        }
        public IEnumerator RoundTransition(string winningRoleID)
        {
            // completely replace original

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundEnd);

            if (this.roundsPlayed >= (int)GameModeManager.CurrentHandler.Settings["roundsToWinGame"])
            {
                this.GameOver();
                yield break;
            }

            IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);
            this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));

            yield return new WaitForSecondsRealtime(1f);
            MapManager.instance.LoadNextLevel(false, false);

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

            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);

            this.StartCoroutine(this.DoRoundStart());
        }
        public IEnumerator PointTransition(string winningRoleID)
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);

            IRoleHandler winningRole = RoleManager.GetHandler(winningRoleID);

            this.StartCoroutine(PointVisualizer.instance.DoSequence(winningRole.WinMessage, winningRole.WinColor));
            yield return new WaitForSecondsRealtime(1f);

            MapManager.instance.LoadLevelFromID(MapManager.instance.currentLevelID, false, false);

            yield return new WaitForSecondsRealtime(1.3f);

            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

            yield return this.WaitForSyncUp();

            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);

            PlayerManager.instance.RevivePlayers();

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();
            yield return this.ClearRolesAndVisuals();

            yield return new WaitForSecondsRealtime(0.3f);

            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", true);
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);

            this.StartCoroutine(this.DoPointStart());
        }
        private void GameOverRematch()
        {
            if (PhotonNetwork.OfflineMode)
            {
                UIHandler.instance.DisplayScreenTextLoop(DullWhite, "REMATCH?");
                UIHandler.instance.popUpHandler.StartPicking(PlayerManager.instance.players.First(), this.GetRematchYesNo);
                MapManager.instance.LoadNextLevel(false, false);
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

            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);
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
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);
            CardBarHandler.instance.ResetCardBards();
            PointVisualizer.instance.ResetPoints();
        }
        private void GameOver()
        {
            base.StartCoroutine(this.GameOverTransition());
        }
        public void RoundOver(string winningRoleID)
        {
            this.StartCoroutine(this.RoundTransition(winningRoleID));
        }

        public void PointOver(string winningRoleID)
        {
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
    }
}
