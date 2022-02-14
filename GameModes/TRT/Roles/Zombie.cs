using UnboundLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.GameModeHandlers;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class ZombieRoleHandler : IRoleHandler
    {
        public static string ZombieRoleName => Zombie.RoleAppearance.Name;
        public static string ZombieRoleID = $"GM_TRT_{ZombieRoleName}";
        public Alignment RoleAlignment => Zombie.RoleAlignment;
        public string WinMessage => "ZOMBIES WIN";
        public Color WinColor => Zombie.RoleAppearance.Color;
        public string RoleName => ZombieRoleName;
        public string RoleID => ZombieRoleID;
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 2;
        public float Rarity => 0.01f;
        public string[] RoleIDsToOverwrite => new string[] { "GM_TRT_Traitor", "GM_TRT_Vampire", "GM_TRT_Hypnotist", "GM_TRT_Assassin" };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Zombie>();
        }
    }
    public class Zombie : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Zombie", 'Z', GM_TRT.ZombieColor);

        public override int MaxCards => 0;

        public override TRT_Role_Appearance Appearance => Zombie.RoleAppearance;

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Innocent:
                    return null;
                case Alignment.Traitor:
                    return Zombie.RoleAppearance;
                case Alignment.Chaos:
                    return null;
                case Alignment.Killer:
                    return null;
                default:
                    return null;
            }
        }
        public override void OnKilledPlayer(Player killedPlayer)
        {
            // do zombie stuff
            if (this.GetComponent<PhotonView>().IsMine && RoleManager.GetPlayerAlignment(killedPlayer) != this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>().playerID)
            {
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_ZombieInfect), RpcTarget.All, killedPlayer.playerID);
            }

            base.OnKilledPlayer(killedPlayer);
        }
        [PunRPC]
        private void RPCA_ZombieInfect(int playerID)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            if (player is null) { return; }
            player.data.healthHandler.Revive(true);
            foreach (var role in player.gameObject.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            this.ExecuteAfterFrames(2, () =>
            {
                RoleManager.GetHandler(ZombieRoleHandler.ZombieRoleID).AddRoleToPlayer(player);
                RoleManager.DoRoleDisplaySpecific(player);
                if (player.data.view.IsMine)
                {
                    TRTHandler.SendChat(null, $"You've been infected by a {RoleManager.GetRoleColoredName(Zombie.RoleAppearance)}!" , true);
                }
            });
        }
    }
}
