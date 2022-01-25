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
    /// 
    /// a game mode which must have exactly 2 teams
    /// 
    /// points are awarded WITHOUT switching maps, after a goal is scored
    /// maps switch on round transition
    /// 
    /// </summary>
    public class GM_RoundketLeague : RWFGameMode
    {
        internal static GM_RoundketLeague instance;

        protected override void Awake()
        {
            GM_RoundketLeague.instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }
    }
}
