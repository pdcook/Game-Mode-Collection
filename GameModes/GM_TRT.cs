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
    /// - Each client flips over cards ONLY they walk near, and they stay flipped (large circular trigger collider)
    /// - Cards can be collected by walking near them (smaller box trigger collider just barely larger than card's box collider)
    /// - Cards have health (possibly proportional to their card health stat) and can be shot and permanently destroyed
    /// - Each client sees ONLY their own card bar, until they die and enter spectator mode
    /// - Players can have a max of two cards
    /// - Dead players have a separate text chat
    /// - Players can discard cards by clicking on the square in the card bar
    /// - If a non-detective player crouches over a body, it will report it (in the chat?) to the detective [EX: Pykess found the body of Ascyst, they were an innocent!]
    /// - If a detective crouches over a body it will report the approximate color [orang-ish, redd-ish, blue-ish, or green-ish] of the killer (in the chat?) [EX: Pykess inspected the body of Ascyst, the were a traitor killed by a blue-ish player!]
    /// - Add hotkeys for quick chats like: (E -> "[nearest player] is suspicious") (F -> "I'm with [nearest player]") (R -> "Kill [nearest player]!!!")
    /// 
    /// Roles:
    /// - Innocent
    /// - Traitor (red name, sees other traitors' names as red, notified of other traitors and jesters at start of round) [can have three cards instead of two]
    /// - Detective (blue name visible to everyone)
    /// Roles for more than four players:
    /// - Jester (pink name, visible to the traitors only) [deals no damage]
    /// - Glitch (is innocent, but appears as a traitor to the traitors)
    /// - Mercenary (innocent) [can have three cards instead of two]
    /// - Phantom (innocent) [haunts their killer with a smoke trail in their color. when their killer dies, they revive with 50% health]
    /// - Killer (own team, can only ever be one at a time, traitors are notified that there is a killer) [has 150% health, starts with a random card (respecting rarity) and can have up to four cards (one more than traitors)]
    /// 
    /// </summary>
    public class GM_TRT : RWFGameMode
    {
        internal static GM_TRT instance;

        protected override void Awake()
        {
            GM_TRT.instance = this;
            base.Awake();
        }

        protected override void Start()
        {
            // register prefab
            GameObject _ = CardItemPrefab.CardItem;
            base.Start();
        }
        public override IEnumerator DoRoundStart()
        {
            yield return base.DoRoundStart();
            yield return CardItem.MakeCardItem(CardChoice.instance.cards.First(), Vector3.zero, Quaternion.identity);
        }

        public override IEnumerator DoPointStart()
        {
            yield return base.DoPointStart();
            yield return CardItem.MakeCardItem(CardChoice.instance.cards.First(), Vector3.zero, Quaternion.identity);
        }


    }
}
