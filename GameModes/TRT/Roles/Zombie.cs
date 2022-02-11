using UnboundLib;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class ZombieRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Zombie.RoleAlignment;
        public string WinMessage => "ZOMBIES WIN";
        public Color WinColor => Zombie.RoleAppearance.Color;
        public string RoleName => Zombie.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
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
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Zombie", 'Z', GM_TRT.ZombieColor);

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
        }
    }
}
