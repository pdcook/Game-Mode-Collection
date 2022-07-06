using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.GameModes;
using Photon.Pun;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;

namespace GameModeCollection.Utils
{
    public class SuffocationHandler : MonoBehaviour
    {
        public const float SuffocationDamage = 5f;

        public const float SuffocationDelay = 0.5f;
        float suffocationTimer = 0f;
        Player player;
        void Start()
        {
            this.player = this.GetComponent<Player>(); 
        }
        void Update()
        {
            if (!GameModeCollection.SuffocationDamageEnabled)
            {
                return;
            }
            if (GameModeManager.CurrentHandlerID == TRTHandler.GameModeID && RoleManager.GetPlayerAlignment(player) == Alignment.Chaos)
            {
                // jesters and swappers can't take suffocation damage
                return;
            }
            if (this.suffocationTimer > 0f)
            {
                this.suffocationTimer -= Time.deltaTime;
                return;
            }
            if (this.player.data.dead || !this.player.data.isPlaying || !(bool)this.player.data.playerVel.GetFieldValue("simulated"))
            {
                return;
            }
            // use an OverlapPoint to see if any objects on the default layer are overlaping the player
            // if so, and the object is part of the map, then the player is suffocating
            Collider2D[] colliders = Physics2D.OverlapPointAll(this.transform.position, 1 << LayerMask.NameToLayer("Default"));
            if (colliders.Any(c => c.transform.root.GetComponent<Map>() != null))
            {
                this.suffocationTimer = SuffocationDelay;
                if (this.GetComponent<PhotonView>().IsMine)
                {
                    this.player.data.healthHandler.CallTakeDamage(SuffocationDamage * Vector2.up, this.transform.position, null, null, true);
                }
            }
        }
    }

}
