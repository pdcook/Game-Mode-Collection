using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using GameModeCollection.Extensions;
using GameModeCollection.Objects;
using UnboundLib.GameModes;
using RWF;
using Sonigon;
using UnboundLib.Extensions;
using GameModeCollection.Utils;
using GameModeCollection.GameModes.TRT;

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
    /// - [ ] Local zoom is ON. optionally (how?) with the dark shader
    /// - [ ] RDM is punished (innocent killing innocent) somehow
    /// - [ ] Clock in upper left corner (with round counter) that counts down. when the timer reaches 0, it turns red, signaling haste mode
    /// - [ ]   --> Figure out what to do for Haste Mode
    /// - [ ] below the clock (also with the round counter) is the player's current role
    /// - [X] Each client sees ONLY their own card bar
    /// - [ ]   --> until they die and enter spectator mode
    /// - [~] Players can have a max of one card
    /// - [X] Dead player's bodies remain on the map (maybe without limbs?) by a patch in HealthHandler::RPCA_Die that freezes them and places them on the nearest ground straight down
    /// - [ ] Dead players have a separate text chat
    /// - [ ] Players can discard cards by clicking on the square in the card bar
    /// - [ ] If a non-detective player crouches over a body, it will report it (in the chat?) to the detective [EX: Pykess found the body of Ascyst, they were an innocent!]
    /// - [ ] If a detective crouches over a body it will report the approximate color [orang-ish, redd-ish, blue-ish, or green-ish] of the killer (in the chat?) [EX: Pykess inspected the body of Ascyst, the were a traitor killed by a blue-ish player!]
    /// - [ ] Add hotkeys for quick chats like: (E -> "[nearest player] is suspicious") (F -> "I'm with [nearest player]") (R -> "Kill [nearest player]!!!")
    /// - [ ] custom maps specifically for this mode, not available in normal rotation - can utilize either custom map objects or spawn points for weapon/item spawns
    /// 
    /// Roles:
    /// - Innocent
    /// - Traitor (red name, sees other traitors' names as red with a "[T]" in front, notified of other traitors and jesters at start of round) [can have two cards instead of one]
    /// - Detective (blue name visible to everyone with a "[D]" in front)
    /// Roles for more than four players:
    /// - Jester (own team) (pink name, visible to the traitors only with a "[J]" in front) [deals no damage]
    /// - Glitch (is innocent, but appears as a traitor to the traitors)
    /// - Mercenary (innocent) [can have two cards instead of one]
    /// - Phantom (innocent) [haunts their killer with a smoke trail in their color. when their killer dies, they revive with 50% health]
    /// - Killer (own team, can only ever be one at a time, traitors are notified that there is a killer) [has 150% health, starts with a random card (respecting rarity) and can have up to four cards (two more than traitors)]
    /// - Hypnotist (traitor) [the first corpse they interact with will respawn as a traitor]
    /// - Zombie (has a chance to spawn instead of all traitors) (cannot have ANY cards) [players killed by any zombie will immediately revive as zombies]
    /// - Swapper ("innocent") (appears to traitors as a jester) [cannot deal damage, when killed, their attacker dies instead and they instantly respawn with the role of the attacker, when the attacker's body is searched they report as a swapper]
    /// - Assassin (traitor) [gets a "target" (never detective unless that is the only option) to which they deal double damage, and half damage to all other players. killing the wrong player results in them dealing half damage for the rest of the round]
    /// - Vampire (traitor) [can block while on top of a dead body to eat it (completely destroying the body) and healing 50 HP, though it freezes them in place for a few seconds]
    /// </summary>
    public class GM_TRT : RWFGameMode
    {
        internal static GM_TRT instance;

        private const float PrepPhaseTime = 5f;
        private const float TimeBetweenCardDrops = 0.5f;
        private const float CardRandomVelMult = 0.25f;
        private const float CardRandomVelMin = 3f;
        private const float CardAngularVelMult = 10f;
        private const float CardHealth = 100f;

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

        internal int pointsPlayedOnCurrentMap = 0;
        internal int roundsPlayed = 0;

        protected override void Awake()
        {
            GM_TRT.instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            // register prefab
            GameObject _ = CardItemPrefabs.CardItem;
            // spawn handler
            _ = CardItemPrefabs.CardItemHandler;
            base.Start();
        }
        
        public void IdentifyBody(TRT_Corpse corpse, bool detective)
        {
            // ID a body

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
            foreach (Player player in PlayerManager.instance.players)
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
            }

        }
        [UnboundRPC]
        static void RPCA_SetNewColors(int playerID, int colorID)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);

            player.AssignColorID(colorID);

        }

        private void PlayerCorpse(Player player)
        {
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
                                                    CardHealth);
            }
            yield break;
        }

        public override void PlayerJoined(Player player)
        {
            // completely replace original, since we don't need teamPoints or teamRounds
        }

        public override void PlayerDied(Player killedPlayer, int teamsAlive)
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

            // check win condition
            bool win = true;
            string winningGroup = "TRAITORS";
            if (win)
            {
                TimeHandler.instance.DoSlowDown();
                if (PhotonNetwork.IsMasterClient)
                {
                    NetworkingManager.RPC(typeof(GM_TRT), nameof(GM_TRT.RPCA_NextRound), winningGroup);
                }
            }
        }

        public override void StartGame()
        {
            base.StartGame();
        }

        public override IEnumerator DoStartGame()
        {
            // completely replace original method
            RWF.CardBarHandlerExtensions.Rebuild(CardBarHandler.instance);
            UIHandler.instance.InvokeMethod("SetNumberOfRounds", (int) GameModeManager.CurrentHandler.Settings["roundsToWinGame"]);
            ArtHandler.instance.NextArt();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameStart);

            GameManager.instance.battleOngoing = false;

            UIHandler.instance.ShowJoinGameText("TROUBLE\nIN\nROUNDS TOWN", PlayerSkinBank.GetPlayerSkinColors(1).winText);
            yield return new WaitForSecondsRealtime(2f);
            UIHandler.instance.HideJoinGameText();
            yield return this.WaitForSyncUp();

            PlayerManager.instance.SetPlayersSimulated(false);
            PlayerManager.instance.InvokeMethod("SetPlayersVisible", false);

            MapManager.instance.LoadNextLevel(false, false);

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
        public override IEnumerator DoRoundStart()
        {
            // completely replace original method

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();

            // TODO: REMOVE THIS
            yield return CardItem.MakeCardItem(CardChoice.instance.cards.GetRandom<CardInfo>(), Vector3.zero, Quaternion.identity, maxHealth: 100f);

            yield return this.WaitForSyncUp();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundStart);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            var sounds = GameObject.Find("/SonigonSoundEventPool");

            yield return new WaitForSecondsRealtime(PrepPhaseTime);

            yield return this.SyncBattleStart();

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            UIHandler.instance.DisplayRoundStartText("INNOCENT", InnocentColor, MapEmbiggener.OutOfBoundsUtils.GetPoint(new Vector3(0.5f, 0.8f, 0f)));
            PlayerManager.instance.SetPlayersSimulated(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);

            this.ExecuteAfterSeconds(0.5f, () => {
                UIHandler.instance.HideRoundStartText();
            });
        }

        public override IEnumerator DoPointStart()
        {
            // completely replace original

            // reset players completely
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            // Wait for MapManager to set all players to playing after map transition
            while (PlayerManager.instance.players.ToList().Any(p => !(bool)p.data.isPlaying))
            {
                yield return null;
            }

            this.RandomizePlayerSkins();
            this.RandomizePlayerFaces();

            // TODO: REMOVE THIS
            yield return CardItem.MakeCardItem(CardChoice.instance.cards.GetRandom<CardInfo>(), Vector3.zero, Quaternion.identity, maxHealth: 100f);

            //PlayerManager.instance.SetPlayersSimulated(false);
            yield return this.WaitForSyncUp();

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointStart);

            var sounds = GameObject.Find("/SonigonSoundEventPool");

            yield return new WaitForSecondsRealtime(PrepPhaseTime);

            yield return this.SyncBattleStart();

            SoundManager.Instance.Play(PointVisualizer.instance.sound_UI_Arms_Race_C_Ball_Pop_Shake, this.transform);
            UIHandler.instance.DisplayRoundStartText("TRAITOR", TraitorColor, MapEmbiggener.OutOfBoundsUtils.GetPoint(new Vector3(0.5f, 0.8f, 0f)));
            PlayerManager.instance.SetPlayersSimulated(true);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookBattleStart);

            this.ExecuteAfterSeconds(0.5f, () => {
                UIHandler.instance.HideRoundStartText();
            });
        }
        public override IEnumerator RoundTransition(int[] winningTeamIDs)
        {
            // completely replace original

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookRoundEnd);

            if (this.roundsPlayed >= (int)GameModeManager.CurrentHandler.Settings["roundsToWinGame"])
            {
                this.GameOver(winningTeamIDs);
                yield break;
            }

            this.StartCoroutine(PointVisualizer.instance.DoSequence("TRAITORS WIN", TraitorColor));

            yield return new WaitForSecondsRealtime(1f);
            MapManager.instance.LoadNextLevel(false, false);

            yield return new WaitForSecondsRealtime(1.3f);

            PlayerManager.instance.SetPlayersSimulated(false);
            TimeHandler.instance.DoSpeedUp();

            yield return this.StartCoroutine(this.WaitForSyncUp());

            TimeHandler.instance.DoSlowDown();
            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);
            PlayerManager.instance.RevivePlayers();

            yield return new WaitForSecondsRealtime(0.3f);

            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);

            this.StartCoroutine(this.DoRoundStart());
        }
        public override IEnumerator PointTransition(int[] winningTeamIDs)
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPointEnd);

            this.StartCoroutine(PointVisualizer.instance.DoSequence("TRAITORS WIN", TraitorColor));
            yield return new WaitForSecondsRealtime(1f);

            MapManager.instance.LoadLevelFromID(MapManager.instance.currentLevelID, false, false);

            yield return new WaitForSecondsRealtime(0.5f);
            yield return this.WaitForSyncUp();

            MapManager.instance.CallInNewMapAndMovePlayers(MapManager.instance.currentLevelID);

            PlayerManager.instance.RevivePlayers();

            yield return new WaitForSecondsRealtime(0.3f);

            TimeHandler.instance.DoSpeedUp();
            GameManager.instance.battleOngoing = true;
            this.isTransitioning = false;
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);

            this.StartCoroutine(this.DoPointStart());
        }

        public override IEnumerator GameOverTransition(int[] winningTeamIDs)
        {
            yield return GameModeManager.TriggerHook(GameModeHooks.HookGameEnd);

            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);
            //List<Color> colors = winningTeamIDs.Select(tID => PlayerManager.instance.GetPlayersInTeam(tID).First().GetTeamColors().color).ToList();
            //Color color = AverageColor.Average(colors);
            UIHandler.instance.DisplayScreenText(Color.white, "TROUBLE\nIN\nROUNDS TOWN", 1f);
            yield return new WaitForSecondsRealtime(2f);
            this.GameOverRematch(winningTeamIDs);
            yield break;
        }
        public override void ResetMatch()
        {
            UIHandler.instance.StopScreenTextLoop();
            PlayerManager.instance.InvokeMethod("ResetCharacters");

            foreach (var player in PlayerManager.instance.players)
            {
                this.teamPoints[player.teamID] = 0;
                this.teamRounds[player.teamID] = 0;
            }
            this.pointsPlayedOnCurrentMap = 0;
            this.roundsPlayed = 0;

            this.isTransitioning = false;
            //UIHandler.instance.ShowRoundCounterSmall(this.teamPoints, this.teamRounds);
            CardBarHandler.instance.ResetCardBards();
            PointVisualizer.instance.ResetPoints();
        }

        [UnboundRPC]
        public static void RPCA_NextRound(string winningRole)
        {
            var instance = GM_TRT.instance;

            if (instance.isTransitioning)
            {
                return;
            }

            GameManager.instance.battleOngoing = false;
            instance.isTransitioning = true;

            PlayerManager.instance.SetPlayersSimulated(false);

            instance.pointsPlayedOnCurrentMap++;


            if (instance.pointsPlayedOnCurrentMap < (int)GameModeManager.CurrentHandler.Settings["pointsToWinRound"])
            {
                instance.PointOver(new int[] { });
                return;
            }
            else
            {
                instance.pointsPlayedOnCurrentMap = 0;
                instance.roundsPlayed++;
                instance.RoundOver(new int[] { });
            }

        }
    }
}
