using UnityEngine;
using Photon.Pun;
using GameModeCollection.GameModeHandlers;
namespace GameModeCollection.GameModes.TRT
{
    public class TRT_Corpse : MonoBehaviour
    {
        public Player Player => this.gameObject.GetComponent<Player>();
        private PhotonView View => this.Player.data.view;
        public float TimeOfDeath { get; private set; }
        public Player Killer { get; private set; }
        public Player LastShot { get; private set; }
        public bool HasBeenIDed { get; private set; } = false;
        public bool HasBeenInvestigated { get; private set; } = false;
        void Start()
        {
            this.TimeOfDeath = Time.realtimeSinceStartup;
            this.Killer = this.Player.data.lastSourceOfDamage;
            this.LastShot = this.Player.data.lastDamagedPlayer;
        }
        public void SearchBody(Player interactingPlayer, bool detective)
        {
            TRTHandler.PlayerIDBody(interactingPlayer, this, this.HasBeenIDed, this.HasBeenInvestigated);
            this.HasBeenIDed = true;
            if (!this.HasBeenInvestigated && detective)
            {
                TRTHandler.PlayerInvestigateBody(interactingPlayer, this, this.HasBeenInvestigated);
                this.HasBeenInvestigated = true;
            }

            this.View.RPC(nameof(this.HasBeen), RpcTarget.All, this.HasBeenIDed, this.HasBeenInvestigated);
        }
        [PunRPC]
        void HasBeen(bool IDed, bool investigated)
        {
            this.HasBeenIDed = IDed;
            this.HasBeenInvestigated = investigated;
        }
    }
}
