using UnboundLib;
using UnityEngine;
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
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.05f;
        public string[] RoleIDsToOverwrite => new string[] {"GM_TRT_Jester"};
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Swapper>();
        }
    }
    public class Swapper : Jester
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Swapper", 'S', GM_TRT.SwapperColor);

        public override TRT_Role_Appearance Appearance => Swapper.RoleAppearance;

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            // do swapper stuff
        }
        public override bool WinConditionMet(Player[] playersRemaining)
        {
            // the swapper cannot win
            return false;
        }
    }
}
