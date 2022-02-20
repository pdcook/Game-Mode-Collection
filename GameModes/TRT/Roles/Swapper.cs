using UnboundLib;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using GameModeCollection.GameModeHandlers;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class SwapperRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Swapper.RoleAlignment;
        public string WinMessage => "THE SWAPPER WINS"; // this shouldn't even be possible
        public Color WinColor => Swapper.RoleAppearance.Color;
        public string RoleName => Swapper.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] {};
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Swapper>();
        }
    }
    public class Swapper : Jester
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Chaos, "Swapper", 'S', GM_TRT.SwapperColor);

        public override TRT_Role_Appearance Appearance => Swapper.RoleAppearance;

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            if (killingPlayer?.playerID == this.GetComponent<Player>().playerID)
            {
                return;
            }
            // get role of killing player
            ITRT_Role killingRole = RoleManager.GetPlayerRole(killingPlayer);
            if (killingRole is null) { return; }
            if (this.GetComponent<PhotonView>().IsMine)
            {
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_Swapped), RpcTarget.All, killingPlayer.playerID);
            }
        }
        public override bool WinConditionMet(Player[] playersRemaining)
        {
            // the swapper cannot win
            return false;
        }
        [PunRPC]
        private void RPCA_Swapped(int swappedID)
        {
            Player killingPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == swappedID);
            if (killingPlayer is null) { return; }
            ITRT_Role killingPlayerRole = RoleManager.GetPlayerRole(killingPlayer);
            string roleID = RoleManager.GetRoleID(killingPlayerRole);
            IRoleHandler roleHandler = RoleManager.GetHandler(roleID);

            // the swapper now assumes the role of the killing player
            roleHandler.AddRoleToPlayer(this.GetComponent<Player>());

            // the killing player has all their roles removed and is killed
            foreach (var role in killingPlayer.gameObject.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            if (this.GetComponent<PhotonView>().IsMine)
            {
                killingPlayer.data.view.RPC("RPCA_Die", RpcTarget.All, Vector2.up);
            }

            // the (now killed) killing player will now appear as a swapper
            RoleManager.GetHandler(RoleManager.GetRoleID(this)).AddRoleToPlayer(killingPlayer);

            // finally, the swapper is revived and this role is removed
            this.GetComponent<HealthHandler>().Revive(true);

            Player player = this.GetComponent<Player>();

            GameModeCollection.instance.ExecuteAfterFrames(2, () =>
            {
                RoleManager.DoRoleDisplaySpecific(player);
                if (player.data.view.IsMine)
                {
                    TRTHandler.SendChat(null, $"You've swapped to become a {RoleManager.GetRoleColoredName(killingPlayerRole.Appearance)}!", true);
                }
            });

            Destroy(this);
        }
    }
}
