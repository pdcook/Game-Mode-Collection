using UnboundLib;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using GameModeCollection.GameModeHandlers;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class PhantomRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Phantom.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => Phantom.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Phantom>();
        }
    }
    public class Phantom : Innocent
    {
        public const float ReviveWithHealthFrac = 0.5f;

        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Phantom", 'P', GM_TRT.PhantomColor);

        public bool CanHaunt { get; private set; } = true;

        public override TRT_Role_Appearance Appearance => Phantom.RoleAppearance;

        protected override void Start()
        {
            this.CanHaunt = true;
            base.Start();
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            if (killingPlayer is null || killingPlayer.playerID == this.GetComponent<Player>()?.playerID) { return; }
            // do phantom stuff
            if (this.CanHaunt)
            {
                this.CanHaunt = false;
                if (!this.GetComponent<PhotonView>().IsMine) { return; }
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_HauntPlayer), RpcTarget.All, killingPlayer.playerID);
            }
        }
        [PunRPC]
        private void RPCA_HauntPlayer(int hauntedPlayerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == hauntedPlayerID);
            if (player is null) { return; }
            player.gameObject.AddComponent<PhantomHaunt>().SetPhantomPlayer(this.GetComponent<Player>());
            // if the local player is the detective, they should be notified that the phantom was killed
            if (RoleManager.GetPlayerRole(PlayerManager.instance.players.FirstOrDefault(p => p.data.view.IsMine))?.Appearance?.Name == Detective.RoleAppearance.Name)
            {
                TRTHandler.SendChat(null, $"The {RoleManager.GetRoleColoredName(this.Appearance)} has been killed!", true);
            }
        }
    }
}
