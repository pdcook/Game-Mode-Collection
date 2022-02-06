using System;
using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameModeCollection.Extensions;
using ModdingUtils.Extensions;
using ModdingUtils.MonoBehaviours;
using ModdingUtils.Utils;
using Photon.Pun;
using TMPro;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnboundLib.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameModeCollection.GameModes
{
    /// <summary>
    ///
    /// A game mode where 1/3 of the players (rounded down) are randomly selected to be the seekers of the round the rest are hiders.
    /// The seekers will be bright red and have (amount of hiders *15) seconds to kill all hiders.
    /// The hiders will get a hidden hider card which makes them weaker than the seekers.
    ///
    /// The hiders get 1 point for every seeker they kill if the hiders win the round.
    /// The seekers get 3 points * (percentage of hiders killed) if the seekers win the round.
    /// If the seeker dies by accident so dying to the bounds all hiders get 1 point.
    ///
    /// NOTE: If this gamemode is played with cards that don't remove themself properly some thing WILL  go wrong.
    /// </summary>
    public class GM_HideNSeek : RWFGameMode
    {

        public List<int> seekerIDs = new List<int>();
        public List<int> hiderIDs = new List<int>();
        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        public Dictionary<int, int> otherTeamKills = new Dictionary<int, int>();
        public float timeLimit => this.hiderIDs.Count*15f;
        public float timeLeft;
        public bool startTimer = false;
        private Coroutine HideNSeekCO;

        public static GM_HideNSeek instance;

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

            this.timeLeft = this.timeLimit;

            base.StartGame();
        }

        public void ResetGame()
        {
            // Reset the game mode
            this.seekerIDs.Clear();
            this.hiderIDs.Clear();


            // Reset things
            foreach (var player in this.players.Values)
            {
                this.otherTeamKills[player.playerID] = 0;
                var colorEffect = player.gameObject.GetOrAddComponent<ReversibleColorEffect>();
                // colorEffect.SetColor(Color.white);
                colorEffect.SetLivesToEffect(100);
                colorEffect.SetColor(player.GetTeamColors().color);
                colorEffect.ApplyColor();
                
                player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().color = new Color(0.6132076f, 0.6132076f, 0.6132076f, 1);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                // Get seekerIDs
                var amountOfSeekers = Mathf.Floor(instance.players.Count / 3);
                if(amountOfSeekers == 0) { amountOfSeekers = 1; }
                var localSeekerIDs = new List<int>();
                // Set the seeker IDs
                for (int i = 0; i < amountOfSeekers; i++)
                {
                    var randomIndex = Random.Range(0, instance.players.Count);
                    // If the ID is already in the list, try again
                    while (instance.seekerIDs.Contains(instance.players[randomIndex].playerID))
                    {
                        randomIndex = Random.Range(0, instance.players.Count);
                    }
                    localSeekerIDs.Add(randomIndex);
                }

                // this.ExecuteAfterFrames(1, () =>
                // {
                    NetworkingManager.RPC(
                        typeof(GM_HideNSeek),
                        nameof(GM_HideNSeek.RPCA_GenerateTeams),
                        localSeekerIDs.ToArray()
                    );
                // });
            }
        }

        [UnboundRPC]
        public static void RPCA_GenerateTeams(int[] localSeekerIDs)
        {
            
            var instance = GM_HideNSeek.instance;
            
            if (!PhotonNetwork.IsMasterClient)
            {
                instance.ResetGame();
            }
            // Set the seeker IDs
            foreach (var index in localSeekerIDs)
            {
                instance.seekerIDs.Add(instance.players[index].playerID);
                GameModeCollection.instance.ExecuteAfterFrames(2, () =>
                {
                    var colorEffect = instance.players[index].gameObject.GetOrAddComponent<ReversibleColorEffect>();
                    colorEffect.SetLivesToEffect(100);
                    colorEffect.SetColor(Color.red);
                    colorEffect.ApplyColor();
                    instance.players[index].GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().color = Color.red;
                });
                GameModeCollection.Log("[Hide&Seek] Seeker ID: " + instance.players[index].playerID);
            }

            // Set the hider IDs
            foreach (var player in instance.players.Values.Where(p=>!instance.seekerIDs.Contains(p.playerID)))
            {
                instance.hiderIDs.Add(player.playerID);
                Cards.instance.AddCardToPlayer(player, HiderCard.instance, addToCardBar: false);

                // GameModeCollection.Log("[Hide&Seek] Hider ID: " + player.playerID);
            }
            
            instance.timeLeft = instance.timeLimit;
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

        private IEnumerator DoHideNSeek()
        {
            this.timeLeft = this.timeLimit;
            this.startTimer = true;
            while (true)
            {
                var white = Color.white * 0.9f;
                white.a = 1f;
                UIHandler.instance.roundCounterSmall.UpdateClock(0, this.timeLeft/ this.timeLimit, white, new Vector2(0.175f, 0.175f));
                // foreach (var hiderID in this.hiderIDs)
                // {
                //     UIHandler.instance.roundCounterSmall.UpdateText(hiderID, this.otherTeamKills[hiderID].ToString());
                // }
                // foreach (var seekerID in this.seekerIDs)
                // {
                //     UIHandler.instance.roundCounterSmall.UpdateText(seekerID, Mathf.RoundToInt(3f * (this.otherTeamKills[seekerID] / (float)this.hiderIDs.Count)-0.1f).ToString());
                // }
                
                if(this.timeLeft <= 0 && PhotonNetwork.IsMasterClient)
                {
                    // Give all alive hiders a point
                    this.otherTeamKills.Where(k => this.hiderIDs.Contains(k.Key) && !this.players[k.Key].data.dead).ToList().ForEach(kvp => this.otherTeamKills[kvp.Key] += 1);
                    
                    var hiderOtherTeamKills = this.otherTeamKills.Where(k => this.hiderIDs.Contains(k.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
                    NetworkingManager.RPC(
                        typeof(GM_HideNSeek),
                        nameof(GM_HideNSeek.RPCA_MyNextRound),
                        hiderOtherTeamKills
                    );
                }

                yield return null;
            }
        }

        private void Update()
        {
            if (this.startTimer)
            {
                this.timeLeft -= Time.deltaTime;
            }
        }

        public override void PlayerDied(Player killedPlayer, int teamsAlive)
        {
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

            // Check if all hiders are dead
            if(this.hiderIDs.Select(id => this.players[id]).All(p => p.data.dead))
            {
                // All hiders are dead, seekers win
                TimeHandler.instance.DoSlowDown();
                
                //Calculate points
                var seekerPoints = new Dictionary<int, int>();
                
                foreach (var seekerId in this.seekerIDs)
                {
                    seekerPoints[seekerId] = Mathf.RoundToInt(3f * (this.otherTeamKills[seekerId] / (float)this.hiderIDs.Count)-0.1f);
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
                var seekerPoints = new Dictionary<int, int>();
                
                foreach (var seekerId in this.seekerIDs)
                {
                    seekerPoints[seekerId] = Mathf.RoundToInt(3f * (this.otherTeamKills[seekerId] / (float)this.hiderIDs.Count)-0.1f);
                }
                
                // If nobody killed a seeker and the seeker dies by for example the border then everyone gets a point
                var hiderOtherTeamKills = this.otherTeamKills.Where(k => this.hiderIDs.Contains(k.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
                if (hiderOtherTeamKills.All(k => k.Value == 0))
                {
                    foreach (var hiderId in hiderOtherTeamKills.Keys.ToArray())
                    {
                        hiderOtherTeamKills[hiderId] ++;
                    }
                }

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

        public override void PointOver(int[] winningTeamIDs)
        {
            if(this.HideNSeekCO != null)
            {
                this.StopCoroutine(this.HideNSeekCO);
            }

            this.startTimer = false;
            base.PointOver(winningTeamIDs);
            foreach (var player in this.players.Values)
            {
                Cards.instance.RemoveCardFromPlayer(player, HiderCard.instance, Cards.SelectionType.All);
            }
        }

        public override void RoundOver(int[] winningTeamIDs)
        {
            if(this.HideNSeekCO != null)
            {
                this.StopCoroutine(this.HideNSeekCO);
            }
            
            this.startTimer = false;
            base.RoundOver(winningTeamIDs);
            foreach (var player in this.players.Values)
            {
                Cards.instance.RemoveCardFromPlayer(player, HiderCard.instance, Cards.SelectionType.All);
            }
        }

        public override IEnumerator DoRoundStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                this.ResetGame();
            }
            yield return base.DoRoundStart();
            this.HideNSeekCO = this.StartCoroutine(this.DoHideNSeek());
            
            foreach (var player in this.players.Values)
            {
                if (player.GetComponent<ReversibleColorEffect>())
                {
                    player.GetComponent<ReversibleColorEffect>().ApplyColor();
                }
            }
        }
        public override IEnumerator DoPointStart()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                this.ResetGame();
            }
            yield return base.DoPointStart();
            this.HideNSeekCO = this.StartCoroutine(this.DoHideNSeek());
            
            foreach (var player in this.players.Values)
            {
                if (player.GetComponent<ReversibleColorEffect>())
                {
                    player.GetComponent<ReversibleColorEffect>().ApplyColor();
                }
            }

        }
    }
    
    public class HiderCard : CustomCard
    {
        public static CardInfo instance;
        protected override string GetTitle()
        {
            return "HiderCard";
        }

        protected override string GetDescription()
        {
            return "If you see this something went wrong";
        }
        
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            
        }
        
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;

            gun.damage = 0.85f;
            statModifiers.health = 0.85f;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[] { };
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }

        protected override GameObject GetCardArt()
        {
            return null;
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.FirepowerYellow;
        }
        
        public override bool GetEnabled()
        {
            return false;
        }

        public override void OnRemoveCard()
        {
        }
        
    }
}
