using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using GameModeCollection.Extensions;
using GameModeCollection.Utils.UI;
using UnboundLib.GameModes;

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// 
    /// A game mode which can be played as FFA or in teams, where players fight for control of a crown.
    /// The crown starts in the middle of each battlefield and a player picks it up by walking into it.
    /// When the crowned player dies, they drop the crown - with it keeping the momentum of the now dead player.
    /// A team wins when they have held the crown for a requisite amount of time.
    /// 
    /// - Players respawn after a few seconds during battles, the time they are dead gets longer the more times they die
    /// - If the crown goes OOB off the bottom while it is not controlled by a player, it is respawned at the spawn point farthest from all players 
    /// - If the crown goes untouched for too long, it is respawned at the farthest spawnpoint from all players
    /// 
    /// </summary>
    public class GM_CrownControl : RWFGameMode
    {
        internal static GM_CrownControl instance;

        private const float secondsNeededToWin = 20f;

        private const float crownAngularVelocityMult = 10f;

        private const float delayPenaltyPerDeath = 1f;
        private const float baseRespawnDelay = 1f;
        private static readonly Vector2 crownSpawn = new Vector2(0.5f, 0.999f);
        private List<int> awaitingRespawn = new List<int>() { };

        private CrownHandler crown;

        private Coroutine CrownControlCO;

        private Dictionary<int, int> deathsThisBattle = new Dictionary<int, int>() { };
        private Dictionary<int, float> teamHeldFor = new Dictionary<int, float>() { };

        protected override void Awake()
        {
            GM_CrownControl.instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            // register prefab
            GameObject _ = CrownPrefab.Crown;
            base.Start();
        }

        private void ResetForBattle()
        {
            this.crown.Reset();
            this.ResetDeaths();
            this.ResetHeldFor();
        }

        private void ResetHeldFor()
        {
            this.teamHeldFor.Clear();
            foreach (int tID in PlayerManager.instance.players.Select(p => p.teamID).Distinct())
            {
                this.teamHeldFor[tID] = 0f;
            }
        }

        private void SyncHeldFor()
        {
            if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(GM_CrownControl), nameof(RPCA_SyncHeldFor), this.teamHeldFor);
            }
        }

        [UnboundRPC]
        private static void RPCA_SyncHeldFor(Dictionary<int, float> teamHeldFor)
        {
            GM_CrownControl.instance.teamHeldFor = teamHeldFor.ToDictionary(kv => kv.Key, kv=>kv.Value);
        }

        private void ResetDeaths()
        {
            this.deathsThisBattle.Clear();
            for (int i = 0; i < PlayerManager.instance.players.Count(); i++)
            {
                this.deathsThisBattle[i] = 0;
            }
            this.awaitingRespawn.Clear();
        }

        /// <summary>
        /// get farthest spawn from all non-team players and the crown
        /// </summary>
        /// <param name="teamID"></param>
        /// <returns></returns>
        private Vector3 GetFarthestSpawn(int teamID)
        {
            Vector3[] spawns = MapManager.instance.GetSpawnPoints().Select(s => s.localStartPos).ToArray();
            float dist = -1f;
            Vector3 best = Vector3.zero;
            foreach (Vector3 spawn in spawns)
            {
                float thisDist = PlayerManager.instance.players.Where(p => !p.data.dead && p.teamID != teamID).Select(p => Vector3.Distance(p.transform.position, spawn)).Sum();
                thisDist += Vector2.Distance(this.crown.transform.position, spawn);
                if (thisDist > dist)
                {
                    dist = thisDist;
                    best = spawn;
                }
            }
            return best;
        }

        public void SetCrown(CrownHandler crownHandler)
        {
            this.crown = crownHandler;
        }
        public void DestroyCrown()
        {
            if (this.crown != null)
            {
                UnityEngine.GameObject.DestroyImmediate(this.crown);
            }
        }

        public override void StartGame()
        {
            base.StartGame();
        }

        public override IEnumerator DoStartGame()
        {
            // this will wait until the crown exists
            yield return CrownHandler.MakeCrownHandler();

            this.ResetForBattle();

            yield return base.DoStartGame();
        }

        public override void PlayerJoined(Player player)
        {
            this.deathsThisBattle[player.playerID] = 0;
            base.PlayerJoined(player);
        }

        public override void PlayerDied(Player killedPlayer, int teamsAlive)
        {
            if (this.awaitingRespawn.Contains(killedPlayer.playerID))
            {
                return;
            }

            this.deathsThisBattle[killedPlayer.playerID]++;

            if (killedPlayer.playerID == this.crown.CrownHolder)
            {
                this.teamHeldFor[killedPlayer.teamID] += this.crown.HeldFor;

                this.crown.GiveCrownToPlayer(-1);
                this.crown.SetVel((Vector2)killedPlayer.data.playerVel.GetFieldValue("velocity"));
                this.crown.SetAngularVel(-GM_CrownControl.crownAngularVelocityMult*((Vector2)killedPlayer.data.playerVel.GetFieldValue("velocity")).x);
            }

            this.SyncHeldFor();

            this.awaitingRespawn.Add(killedPlayer.playerID);
            this.StartCoroutine(this.IRespawnPlayer(killedPlayer, delayPenaltyPerDeath*(this.deathsThisBattle[killedPlayer.playerID]-1) + baseRespawnDelay));
        }

        public IEnumerator IRespawnPlayer(Player player, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (this.awaitingRespawn.Contains(player.playerID))
            {
                player.transform.position = this.GetFarthestSpawn(player.teamID);
                player.data.healthHandler.Revive(true);
                player.GetComponent<GeneralInput>().enabled = true;
                this.awaitingRespawn.Remove(player.playerID);
            }
        }
        public override IEnumerator DoRoundStart()
        {
            this.ResetForBattle();
            this.CrownControlCO = this.StartCoroutine(this.DoCrownControl());
            yield return base.DoRoundStart();
            this.SpawnCrown();
        }
        public override IEnumerator DoPointStart()
        {
            this.ResetForBattle();
            this.CrownControlCO = this.StartCoroutine(this.DoCrownControl());
            yield return base.DoPointStart();
            this.SpawnCrown();
        }

        private IEnumerator DoCrownControl()
        {
            while (true)
            {

                int? winningTeamID = null;

                foreach (int tID in this.teamHeldFor.Keys)
                {
                    float time = this.teamHeldFor[tID] + ((this.crown.CrownHolder != -1 && PlayerManager.instance.players[this.crown.CrownHolder].teamID == tID) ? this.crown.HeldFor : 0f);

                    /*
                    UIHandler.instance.roundCounterSmall.UpdateText(tID, 
                        UnityEngine.Mathf.Clamp(time, 0f, GM_CrownControl.secondsNeededToWin).ToString("0.00"),
                        PlayerManager.instance.GetPlayersInTeam(tID).First().GetTeamColors().color);
                    */
                    UIHandler.instance.roundCounterSmall.UpdateClock(tID, 
                        UnityEngine.Mathf.Clamp(time, 0f, GM_CrownControl.secondsNeededToWin)/GM_CrownControl.secondsNeededToWin);

                    if (time > GM_CrownControl.secondsNeededToWin)
                    {
                        winningTeamID = tID;
                        break;
                    }
                }
                if (winningTeamID == null && this.crown.TooManyRespawns)
                {
                    winningTeamID = -1;
                }

                if (winningTeamID != null && PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
                {
                    foreach (Player player in PlayerManager.instance.players.Where(p => !p.data.dead && p.teamID != winningTeamID))
                    {
                        player.data.view.RPC("RPCA_Die", RpcTarget.All, new object[]
                        {
                                new Vector2(0, 1)
                        });
                    }
                    yield return null;
                    this.CallEndRound();
                    NetworkingManager.RPC(
                        typeof(GM_CrownControl),
                        nameof(GM_CrownControl.RPCA_NextRound),
                        winningTeamID != -1 ? new int[] { (int)winningTeamID } : new int[] { },
                        this.teamPoints,
                        this.teamRounds
                    );
                    break;
                }
                yield return null;
            }
            yield break;
        }

        private void CallEndRound()
        {
            if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC(typeof(GM_CrownControl), nameof(RPCA_EndRound));
            }
        }
        [UnboundRPC]
        private static void RPCA_EndRound()
        {
            GM_CrownControl.instance.awaitingRespawn.Clear();
            TimeHandler.instance.DoSlowDown();
            if (GM_CrownControl.instance.CrownControlCO != null)
            {
                GM_CrownControl.instance.StopCoroutine(GM_CrownControl.instance.CrownControlCO);
            }
        }

        private void SpawnCrown()
        {
            if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC(typeof(GM_CrownControl), nameof(RPCA_SpawnCrown), GM_CrownControl.crownSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
            }
        }

        [UnboundRPC]
        private static void RPCA_ResetCrown()
        {
            GM_CrownControl.instance.crown.Reset();
        }

        [UnboundRPC]
        private static void RPCA_SpawnCrown(Vector2 spawnPos)
        {
            GM_CrownControl.instance.crown.Spawn(spawnPos);
        }
    }
}
