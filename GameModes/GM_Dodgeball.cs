﻿using RWF.GameModes;
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

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// 
    /// Can be played FFA or as teams
    /// 
    /// Points awarded to either: the last team standing, or the team who deals the final blow to the dodgeball
    /// 
    /// </summary>
    public class GM_Dodgeball : RWFGameMode
    {

        public enum Dodgeable
        {
            Ball,
            Box,
            Rod,
            None
        }

        internal static GM_Dodgeball instance;

        internal static readonly Vector2 ballSpawn = new Vector2(0.5f, 1f);

        private Dodgeable _currentDodgeable = Dodgeable.None;
        public Dodgeable CurrentDodgeable
        {
            get
            {
                return this._currentDodgeable;
            }
            private set
            {
                this._currentDodgeable = value;
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
        public override IEnumerator DoStartGame()
        {
            // these will wait until the objects exist
            yield return DeathBall.MakeDeathBall();
            yield return DeathBox.MakeDeathBox();
            yield return DeathRod.MakeDeathRod();

            this.ResetAllObjects();
            this.CurrentDodgeable = Dodgeable.Box;

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
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SetObject), (byte)((Enum.GetValues(typeof(Dodgeable))).Cast<Dodgeable>().ToList().Where(d => d!=Dodgeable.None).OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).First()));
            }
        }
        [UnboundRPC]
        private static void RPCA_SetObject(byte obj)
        {
            GM_Dodgeball.instance.CurrentDodgeable = (Dodgeable)obj;
        }

        public override IEnumerator DoRoundStart()
        {
            this.SetRandomObject();
            yield return base.DoRoundStart();
            this.SpawnDodgeable();
        }
        public override IEnumerator DoPointStart()
        {
            this.SetRandomObject();
            yield return base.DoPointStart();
            this.SpawnDodgeable();
        }
        public override void RoundOver(int[] winningTeamIDs)
        {
            this.ResetAllObjects();
            base.RoundOver(winningTeamIDs);
        }
        public override void PointOver(int[] winningTeamIDs)
        {
            this.ResetAllObjects();
            base.PointOver(winningTeamIDs);
        }
        private void SpawnDodgeable()
        {
            if (this.CurrentDodgeable == Dodgeable.Ball && (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SpawnBall), GM_Dodgeball.ballSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
            }
            else if (this.CurrentDodgeable == Dodgeable.Box && (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SpawnBox), GM_Dodgeball.ballSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
            }
            else if (this.CurrentDodgeable == Dodgeable.Rod && (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
            {
                NetworkingManager.RPC(typeof(GM_Dodgeball), nameof(RPCA_SpawnRod), GM_Dodgeball.ballSpawn + new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), 0f));
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
