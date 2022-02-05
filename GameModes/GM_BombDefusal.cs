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

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// it is a 2 teams 1v1 team based gamemode where there will be a bomb that one team will need to protect
    /// while the other team needs to "defuse" the bomb. If the time runs out or the defenders kill all
    /// attackers the defenders win and the bomb might "blow up".
    /// If the attackers kill all defenders or "defuse" the bomb they win.
    /// The defenders will all spawn close around the bomb and the attackers will spawn furthers away from the bomb
    /// </summary>
    public class GM_BombDefusal : RWFGameMode
    {
        
        internal static GM_BombDefusal instance;

        public BombObjectHandler bomb;

        protected override void Awake()
        {
            GM_BombDefusal.instance = this;
            base.Awake();
        }
        
        public void SetBomb(BombObjectHandler bombHandler)
        {
            this.bomb = bombHandler;
        }
        
        public override IEnumerator DoStartGame()
        {
            yield return new WaitForEndOfFrame();

            yield return BombObjectHandler.MakeBombHandler();
            
            yield return base.DoStartGame();
        }
        
        private void ResetForBattle()
        {
            this.bomb.Reset();
        }

        public override void PointOver(int winningTeamID)
        {
            base.PointOver(winningTeamID);
            this.ResetForBattle();
        }

        public IEnumerator DoStartPoint()
        {
            this.bomb.Spawn();
            yield break;
        }

        public override IEnumerator DoRoundStart()
        {
            this.ResetForBattle();
            yield return base.DoRoundStart();
            yield return this.DoStartPoint();
        }
        public override IEnumerator DoPointStart()
        {
            this.ResetForBattle();
            yield return base.DoPointStart();
            yield return this.DoStartPoint();
        }
    }
}
