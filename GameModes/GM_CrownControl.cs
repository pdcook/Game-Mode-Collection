using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    /// - If the crown goes OOB off the right, left, or bottom while it is not controlled by a player, it is respawned at a random point above ground on the battlefield
    /// - If the crown goes untouched for too long, it is respawned at the center of the battlefield
    /// 
    /// </summary>
    public class GM_CrownControl : RWFGameMode
    {

        private const float delayPenaltyPerDeath = 1f;
        private const float baseRespawnDelay = 1f;

        private CrownHandler crown;

        private Dictionary<int, int> deathsThisBattle = new Dictionary<int, int>() { };

        private void ResetDeathCounter()
        {
            this.deathsThisBattle.Clear();
            for (int i = 0; i < PlayerManager.instance.players.Count(); i++)
            {
                this.deathsThisBattle[i] = 0;
            }
        }

        private Vector3 GetFarthestSpawn(int playerID)
        {
            Vector3[] spawns = MapManager.instance.GetSpawnPoints().Select(s => s.localStartPos).ToArray();
            float dist = -1f;
            Vector3 best = Vector3.zero;
            foreach (Vector3 spawn in spawns)
            {
                float thisDist = PlayerManager.instance.players.Where(p => !p.data.dead && p.playerID != playerID).Select(p => Vector3.Distance(p.transform.position, spawn)).Sum();
                if (thisDist > dist)
                {
                    dist = thisDist;
                    best = spawn;
                }
            }
            return best;
        }

        public override void StartGame()
        {

            if (GameManager.instance.isPlaying) { return; }

            this.crown = CrownHandler.MakeCrownHandler(this.transform);
            this.ResetDeathCounter();

            base.StartGame();
        }

        public override void PlayerJoined(Player player)
        {
            this.deathsThisBattle[player.playerID] = 0;
            base.PlayerJoined(player);
        }

        public override void PlayerDied(Player killedPlayer, int teamsAlive)
        {
            this.deathsThisBattle[killedPlayer.playerID]++;

            if (killedPlayer.playerID == this.crown.CrownHolder)
            {
                this.crown.GiveCrownToPlayer(-1);
            }


            this.StartCoroutine(this.IRespawnPlayer(killedPlayer, delayPenaltyPerDeath*this.deathsThisBattle[killedPlayer.playerID] + baseRespawnDelay));
        }

        public IEnumerator IRespawnPlayer(Player player, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            player.transform.position = this.GetFarthestSpawn(player.playerID);
            player.data.healthHandler.Revive(true);
            player.GetComponent<GeneralInput>().enabled = true;
        }

        public override IEnumerator DoRoundStart()
        {
            yield return this.StartCoroutine(base.DoRoundStart());
        }
        public override IEnumerator DoPointStart()
        {
            GameModeCollection.Log("POINT START");
            //yield return this.StartCoroutine(base.DoPointStart());
            GameModeCollection.Log("POINT START, RESET CROWN");
            this.crown.SetPos(100f * Vector2.up);
            this.crown.SetVel(Vector2.zero);
            this.crown.SetAngularVel(0f);
            yield break;
        }
    }
}
