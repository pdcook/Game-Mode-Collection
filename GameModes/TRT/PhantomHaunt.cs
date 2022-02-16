using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.GameModes.TRT.Roles;
using System.Linq;
using GameModeCollection.GameModeHandlers;

namespace GameModeCollection.GameModes.TRT
{
    class PhantomHaunt : MonoBehaviour
    {
        public Player PhantomPlayer { get; private set; } = null;
        public void SetPhantomPlayer(Player phantom)
        {
            this.PhantomPlayer = phantom;
        }
        void Start()
        {

        }
        void OnDisable()
        {
            // when the haunted player dies, revive the phantom
            if (this.PhantomPlayer != null)
            {
                this.PhantomPlayer.data.healthHandler.Revive(true, Phantom.ReviveWithHealthFrac);
                // if the local player is the detective, they should be notified that the phantom was revived
                if (RoleManager.GetPlayerRole(PlayerManager.instance.players.FirstOrDefault(p => p.data.view.IsMine))?.Appearance?.Name == Detective.RoleAppearance.Name)
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
    }
}
