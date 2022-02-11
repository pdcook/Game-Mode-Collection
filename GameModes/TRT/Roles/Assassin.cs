using UnboundLib;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class AssassinRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Assassin.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Assassin.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.1f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Assassin>();
        }
    }
    public class Assassin : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Assassin", 'A', GM_TRT.AssassinColor);

        public override TRT_Role_Appearance Appearance => Assassin.RoleAppearance;

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // do assassin stuff
        }
    }
}
