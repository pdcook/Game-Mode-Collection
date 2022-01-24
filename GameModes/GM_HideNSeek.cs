using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModdingUtils.MonoBehaviours;
using Photon.Pun;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameModeCollection.GameModes
{
    /// <summary>
    ///
    /// A game mode where 1/3 of the players (rounded down) are randomly selected to be the seekers of the round the rest are hiders.
    /// The seekers will be bright red and have 90 seconds to kill all hiders.
    /// The hiders will get a hidden hider card which makes them weaker than the seekers.
    ///
    /// The hiders get 1 point for every seeker they kill if the hiders win the round.
    /// The seekers get (amount of seekers) points * (percentage of hiders killed) if the seekers win the round.
    ///
    /// </summary>
    public class GM_HideNSeek : RWFGameMode
    {

        public List<int> seekerIDs = new List<int>();
        public List<int> hiderIDs = new List<int>();
        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        public Dictionary<int, int> otherTeamKills = new Dictionary<int, int>();
        public Dictionary<int, int> pointsToGive = new Dictionary<int, int>();
        public List<int> deathPlayers = new List<int>();
        public int[] lastPointWinningTeamIDs;

        public static GM_HideNSeek instance;
        
        
        //TODO:
        // Make the seeker stronger than the hider with a card on the hider.
        // Add the 90 second timer.

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public override void StartGame()
        {
            if (GameManager.instance.isPlaying) { return; }
            this.players.Clear();
            // this.ResetGame();

            base.StartGame();
        }

        public void ResetGame()
        {
            // Reset the game mode
            this.seekerIDs.Clear();
            this.hiderIDs.Clear();
            this.deathPlayers.Clear();

            // Reset things
            foreach (var player in this.players.Values)
            {
                this.otherTeamKills[player.playerID] = 0;
                var colorEffect = player.gameObject.GetOrAddComponent<ReversibleColorEffect>();
                // colorEffect.SetColor(Color.white);
                colorEffect.SetLivesToEffect(100);
                colorEffect.SetColor(player.GetTeamColors().color);
                colorEffect.ApplyColor();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                var seed = Random.Range(0, int.MaxValue);
                NetworkingManager.RPC(
                    typeof(GM_HideNSeek),
                    nameof(GM_HideNSeek.RPCA_GenerateTeams),
                    seed
                );
            }
        }

        [UnboundRPC]
        public static void RPCA_GenerateTeams(int seed)
        {
            var instance = GM_HideNSeek.instance;
            
            Random.InitState(seed);
            
            var amountOfSeekers = Mathf.Floor(instance.players.Count / 3);
            // Set the seeker IDs
            for (int i = 0; i < amountOfSeekers; i++)
            {
                var randomIndex = Random.Range(0, instance.players.Count);
                // If the ID is already in the list, try again
                while (instance.seekerIDs.Contains(instance.players[randomIndex].playerID))
                {
                    randomIndex = Random.Range(0, instance.players.Count);
                }
                instance.seekerIDs.Add(instance.players[randomIndex].playerID);
                var colorEffect = instance.players[randomIndex].gameObject.GetOrAddComponent<ReversibleColorEffect>();
                colorEffect.SetColor(Color.red);
                colorEffect.ApplyColor();
                GameModeCollection.Log("[Hide&Seek] Seeker ID: " + instance.players[randomIndex].playerID);
            }

            // Set the hider IDs
            foreach (var player in instance.players.Values.Where(p=>!instance.seekerIDs.Contains(p.playerID)))
            {
                instance.hiderIDs.Add(player.playerID);

                // GameModeCollection.Log("Hider ID: " + player.playerID);
            }
        }

        public override void PlayerJoined(Player player)
        {
            if (!this.players.ContainsKey(player.playerID))
            {
                this.players.Add(player.playerID, player);
            }
            
            if (!this.otherTeamKills.ContainsKey(player.playerID))
            {
                this.otherTeamKills.Add(player.playerID, 0);
            }


            base.PlayerJoined(player);
        }

        public override void PlayerDied(Player killedPlayer, int teamsAlive)
        {
            if (this.deathPlayers.Contains(killedPlayer.playerID))
            {
                return;
            }
            
            // PlayerDied is called twice for a player
            UnityEngine.Debug.Log("PlayerDied: " + killedPlayer);
            
            var playerKiller = PlayerManager.instance.players.FirstOrDefault(p => p.data.lastDamagedPlayer == killedPlayer &&
                                                                         (float)p.data.stats.GetFieldValue("sinceDealtDamage") <= 1.5f && !p.data.dead);
            if (playerKiller != null && playerKiller != killedPlayer)
            {
                // If seeker killed hider
                if(this.seekerIDs.Contains(playerKiller.playerID) && this.hiderIDs.Contains(killedPlayer.playerID))
                {
                    this.otherTeamKills[playerKiller.playerID]++;
                }
                // If the hider killed a seeker
                else if (this.hiderIDs.Contains(playerKiller.playerID) && this.seekerIDs.Contains(killedPlayer.playerID))
                {
                    this.otherTeamKills[playerKiller.playerID]++;
                } 
                // If a team kill
                // else if (this.seekerIDs.Contains(playerKiller.playerID) && this.seekerIDs.Contains(killedPlayer.playerID))
                // {
                //     this.otherTeamKills[playerKiller.playerID]--;
                // }
            }
            
            this.deathPlayers.Add(killedPlayer.playerID);

            // Check if all hiders are dead
            if(this.hiderIDs.Select(id => this.players[id]).All(p => p.data.dead))
            {
                // All hiders are dead, seekers win
                TimeHandler.instance.DoSlowDown();
                
                //Calculate points
                var seekerPoints = new Dictionary<int, int>();
                
                foreach (var seekerId in this.seekerIDs)
                {
                    seekerPoints[seekerId] = Mathf.RoundToInt(3f * (this.otherTeamKills[seekerId] / (float)this.hiderIDs.Count));
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    NetworkingManager.RPC(
                        typeof(GM_HideNSeek),
                        nameof(GM_HideNSeek.RPCA_MyNextRound),
                        seekerPoints
                    );
                }
            } 
            
            // Check if all seekers are dead
            else if(this.seekerIDs.Select(id => this.players[id]).All(p => p.data.dead))
            {
                // All seekers are dead, hiders win
                TimeHandler.instance.DoSlowDown();

                //Calculate points
                //Calculate points
                var seekerPoints = new Dictionary<int, int>();
                
                foreach (var seekerId in this.seekerIDs)
                {
                    seekerPoints[seekerId] = Mathf.RoundToInt(3f * (this.otherTeamKills[seekerId] / (float)this.hiderIDs.Count));
                }

                var hiderOtherTeamKills = this.otherTeamKills.Where(k => this.hiderIDs.Contains(k.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);

                foreach (var point in seekerPoints)
                {
                    hiderOtherTeamKills.Add(point.Key, point.Value);
                }
 
                if (PhotonNetwork.IsMasterClient)
                {
                    NetworkingManager.RPC(
                        typeof(GM_HideNSeek),
                        nameof(GM_HideNSeek.RPCA_MyNextRound),
                        hiderOtherTeamKills
                    );
                }
            }
        }

        [UnboundRPC]
        public static void RPCA_MyNextRound(Dictionary<int, int> _winnerPoints)
        {
            var instance = GM_HideNSeek.instance;

            if (instance.isTransitioning)
            {
                return;
            }
            
            GameManager.instance.battleOngoing = false;
            instance.isTransitioning = true;
            PlayerManager.instance.SetPlayersSimulated(false);
            
            var winnerPoints = _winnerPoints.Where(v=> v.Value != 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            
            // If no points
            if (!winnerPoints.Any())
            {
                instance.PointOver(winnerPoints.Select(v => v.Key).ToArray());
                return;
            }

            foreach (var point in winnerPoints)
            {
                instance.teamPoints[point.Key] += point.Value;
            }
            
            if (winnerPoints.Select(p => instance.teamPoints[p.Key]).All(p => p < (int) GameModeManager.CurrentHandler.Settings["pointsToWinRound"]))
            {
                instance.PointOver(winnerPoints.Select(v=> v.Key).ToArray());
                return;
            }

            int[] roundWinningTeamIDs = winnerPoints.Select(v => v.Key).Where(tID =>
                instance.teamPoints[tID] >= (int)GameModeManager.CurrentHandler.Settings["pointsToWinRound"]).ToArray();
            foreach (int winningTeamID in roundWinningTeamIDs)
            {
                instance.teamRounds[winningTeamID] += 1;
            }
            instance.RoundOver(roundWinningTeamIDs);
        }

        public override IEnumerator DoRoundStart()
        {
            this.ResetGame();
            yield return base.DoRoundStart();
        }
        public override IEnumerator DoPointStart()
        {
            this.ResetGame();
            yield return base.DoPointStart();
        }
    }
}
