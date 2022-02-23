using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.GameModes.TRT.Roles;
using System.Linq;
using GameModeCollection.GameModeHandlers;
using Photon.Pun;
using UnboundLib;

namespace GameModeCollection.GameModes.TRT
{
    class PhantomHaunt : MonoBehaviour
    {
        public Player PhantomPlayer { get; private set; } = null;
        public Phantom Phantom { get; private set; } = null;
        private CharacterData HauntedPlayer = null;
        private Vector3 RespawnPos { get; set; } = Vector3.zero;
        public void SetPhantomPlayer(Phantom phantom)
        {
            this.Phantom = phantom;
            this.PhantomPlayer = phantom.GetComponent<Player>();
        }
        void Start()
        {
            this.HauntedPlayer = this.GetComponent<CharacterData>();

            if (this.HauntedPlayer.view.IsMine)
            {
                this.HauntedPlayer.view.RPC(nameof(RPCA_SetRespawnPos), RpcTarget.All, (Vector3)MapManager.instance.currentMap.Map.InvokeMethod("GetRandomSpawnPos"));
            }
        }
        void Update()
        {
            if (!this.HauntedPlayer.dead) { return; }

            // when the haunted player dies, revive the phantom
            if (this.PhantomPlayer != null)
            {
                this.PhantomPlayer.GetComponent<PlayerCollision>().IgnoreWallForFrames(2);
                this.PhantomPlayer.transform.position = this.RespawnPos;
                this.PhantomPlayer.data.healthHandler.Revive(true, Phantom.ReviveWithHealthFrac);
                this.Phantom.IsHaunting = false;
                // if the local player is the detective, they should be notified that the phantom was revived
                if (RoleManager.GetPlayerRoleID(PlayerManager.instance.players.FirstOrDefault(p => p.data.view.IsMine)) == DetectiveRoleHandler.DetectiveRoleID)
                {
                    TRTHandler.SendChat(null, $"The {RoleManager.GetRoleColoredName(Phantom.RoleAppearance)} has been revived!", true);
                }
            }

            Destroy(this);

        }
        public void DestroyNow()
        {
            DestroyImmediate(this);
        }
        [PunRPC]
        private void RPCA_SetRespawnPos(Vector3 location)
        {
            this.RespawnPos = location;            
        }
    }
}
