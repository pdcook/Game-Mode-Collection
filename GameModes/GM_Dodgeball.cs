using RWF.GameModes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects;
using System;
using UnboundLib.GameModes;

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// 
    /// Can be played FFA or as teams
    /// 
    /// Points awarded to either: the last team standing, or the team who deals the final blow to the dodgeball
    /// Guns, bullets, explosives, etc do not do damage to players
    /// 
    /// As the team in first place gets closer to winnining, it becomes more and more likely for multiple dodgeballs to spawn
    /// 
    /// </summary>
    public class GM_Dodgeball : RWFGameMode
    {
        public enum Dodgable
        {
            Ball,
            Box,
            Rod,
            None
        }

        internal static GM_Dodgeball instance;

        private Coroutine ExtraObjectsCO = null;

        internal static readonly Vector2 objSpawn = new Vector2(0.5f, 1f);

        private Dodgable _currentDodgableType = Dodgable.None;
        public Dodgable CurrentDodgableType
        {
            get
            {
                return this._currentDodgableType;
            }
            private set
            {
                this._currentDodgableType = value;
            }
        }
        public GameObject CurrentDodgableObject
        {
            get
            {
                switch (this.CurrentDodgableType)
                {
                    case Dodgable.Ball:
                        return this.deathBall.gameObject;
                    case Dodgable.Box:
                        return this.deathBox.gameObject;
                    case Dodgable.Rod:
                        return this.deathRod.gameObject;
                    case Dodgable.None:
                        return null;
                    default:
                        return null;
                }
            }
        }
        public DeathObjectHealth CurrentDodgableHealth
        {
            get
            {
                return this.CurrentDodgableObject?.GetComponent<DeathObjectHealth>();
            }
        }
        public bool CurrentDodgableAlive
        {
            get
            {
                //return !(this.CurrentDodgableHealth?.Dead ?? true);
                return !this.CurrentDodgableHealth.Dead;
            }
        }
        public Player PlayerLastDamagedDodgable
        {
            get
            {
                return this.CurrentDodgableHealth.LastSourceOfDamage; 
            }
        }

        private DeathBall deathBall;
        private DeathBox deathBox;
        private DeathRod deathRod;
        public void SetDeathObject(DeathBall deathObjectHandler)
        {
                this.deathBall = deathObjectHandler;
        }
        public void SetDeathObject(DeathBox deathObjectHandler)
        {
                this.deathBox = deathObjectHandler;
        }
        public void SetDeathObject(DeathRod deathObjectHandler)
        {
                this.deathRod = deathObjectHandler;
        }
        public void DestroyDeathBall()
        {
            if (this.deathBall != null)
            {
                UnityEngine.GameObject.DestroyImmediate(this.deathBall);
            }
        }
        public void DestroyDeathBox()
        {
            if (this.deathBox != null)
            {
                UnityEngine.GameObject.DestroyImmediate(this.deathBox);
            }
        }
        public void DestroyDeathRod()
        {
            if (this.deathRod != null)
            {
                UnityEngine.GameObject.DestroyImmediate(this.deathRod);
            }
        }

        protected override void Awake()
        {
            GM_Dodgeball.instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            // register prefabs
            GameObject _ = DeathObjectPrefabs.DeathBall;
            _ = DeathObjectPrefabs.DeathBox;
            _ = DeathObjectPrefabs.DeathRod;
            base.Start();
        }
        public override void PlayerDied(Player killedPlayer, int teamsAlive)
        {
            base.PlayerDied(killedPlayer, teamsAlive);
        }
        public void PlayerKilledDodgable(Player player)
        {
            // point ends only if the player killed the "main" dodgable
            if (this.CurrentDodgableAlive) { return; }
            TimeHandler.instance.DoSlowDown();
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(GM_Dodgeball.RPCA_NextRound), new int[] { player.teamID }, this.teamPoints, this.teamRounds);
            }
        }
        public override IEnumerator DoStartGame()
        {
            // these will wait until the objects exist
            GameModeCollection.Log("[GM_DodgeBall] Destroying Objects...");
            DeathBall.DestroyDeathBall();
            yield return this.WaitForSyncUp();
            yield return new WaitForEndOfFrame();
            DeathBox.DestroyDeathBox();
            yield return this.WaitForSyncUp();
            yield return new WaitForEndOfFrame();
            DeathRod.DestroyDeathRod();
            yield return this.WaitForSyncUp();
            yield return new WaitForEndOfFrame();

            GameModeCollection.Log("[GM_DodgeBall] Creating Objects...");
            yield return DeathBall.MakeDeathBall();
            yield return DeathBox.MakeDeathBox();
            yield return DeathRod.MakeDeathRod();
            GameModeCollection.Log("[GM_DodgeBall] Objects Created.");

            yield return this.WaitForSyncUp();

            this.ResetAllObjects();
            this.CurrentDodgableType = Dodgable.Box;

            this.deathBall.GetComponent<DeathObjectHealth>().AddPlayerKilledAction(this.PlayerKilledDodgable);
            this.deathBox.GetComponent<DeathObjectHealth>().AddPlayerKilledAction(this.PlayerKilledDodgable);
            this.deathRod.GetComponent<DeathObjectHealth>().AddPlayerKilledAction(this.PlayerKilledDodgable);

            yield return base.DoStartGame();
        }

        public void ResetAllObjects()
        {
            this.deathBall.Reset();
            this.deathBox.Reset();
            this.deathRod.Reset();
        }
        public void SetRandomObject()
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SetObject), (byte)((Enum.GetValues(typeof(Dodgable))).Cast<Dodgable>().ToList().Where(d => d!=Dodgable.None).OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).First()));
            }
        }
        [UnboundRPC]
        private static void RPCA_SetObject(byte obj)
        {
            GM_Dodgeball.instance.CurrentDodgableType = (Dodgable)obj;
        }
        public IEnumerator SpawnExtraObjects()
        {
            if (!PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
            {
                yield break;
            }

            List<Dodgable> nextUp = ((Enum.GetValues(typeof(Dodgable))).Cast<Dodgable>().ToList().Where(d => d != Dodgable.None && d != this.CurrentDodgableType).OrderBy(_ => UnityEngine.Random.Range(0f, 1f))).ToList();

            for (int i = 0; i < nextUp.Count(); i++)
            {
                yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1f, 3f));

                if (UnityEngine.Random.Range(0f, 1f) <= (float)this.teamRounds.Values.Max()/(float)(int)GameModeManager.CurrentHandler.Settings["roundsToWinGame"])
                {
                    this.SpawnDodgable(nextUp[i]);                
                }
            }
        }

        public override IEnumerator DoRoundStart()
        {
            this.ResetAllObjects();
            this.SetRandomObject();
            yield return base.DoRoundStart();
            this.SpawnDodgable(this.CurrentDodgableType);
            if (this.ExtraObjectsCO != null)
            {
                this.StopCoroutine(this.ExtraObjectsCO);
            }
            this.ExtraObjectsCO = this.StartCoroutine(this.SpawnExtraObjects());
        }
        public override IEnumerator DoPointStart()
        {
            this.ResetAllObjects();
            this.SetRandomObject();
            yield return base.DoPointStart();
            this.SpawnDodgable(this.CurrentDodgableType);
            if (this.ExtraObjectsCO != null)
            {
                this.StopCoroutine(this.ExtraObjectsCO);
            }
            this.ExtraObjectsCO = this.StartCoroutine(this.SpawnExtraObjects());
        }
        public override void RoundOver(int[] winningTeamIDs)
        {
            if (this.ExtraObjectsCO != null)
            {
                this.StopCoroutine(this.ExtraObjectsCO);
            }
            this.ResetAllObjects();
            base.RoundOver(winningTeamIDs);
        }
        public override void PointOver(int[] winningTeamIDs)
        {
            if (this.ExtraObjectsCO != null)
            {
                this.StopCoroutine(this.ExtraObjectsCO);
            }
            this.ResetAllObjects();
            base.PointOver(winningTeamIDs);
        }
        private void SpawnDodgable(Dodgable dodgable)
        {
            if (dodgable == Dodgable.Ball && (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SpawnBall), GM_Dodgeball.objSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
            }
            else if (dodgable == Dodgable.Box && (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SpawnBox), GM_Dodgeball.objSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
            }
            else if (dodgable == Dodgable.Rod && (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SpawnRod), GM_Dodgeball.objSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
            }
        }

        [UnboundRPC]
        private static void RPCA_SpawnBall(Vector2 spawnPos)
        {
            GM_Dodgeball.instance.deathBall.Spawn(spawnPos);
        }
        [UnboundRPC]
        private static void RPCA_SpawnBox(Vector2 spawnPos)
        {
            GM_Dodgeball.instance.deathBox.Spawn(spawnPos);
        }
        [UnboundRPC]
        private static void RPCA_SpawnRod(Vector2 spawnPos)
        {
            GM_Dodgeball.instance.deathRod.Spawn(spawnPos);
        }
    }
}
