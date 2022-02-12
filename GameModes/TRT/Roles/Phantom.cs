using UnboundLib;
using UnityEngine;
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
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.1f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Phantom>();
        }
    }
    public class Phantom : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Phantom", 'P', GM_TRT.PhantomColor);

        public override TRT_Role_Appearance Appearance => Phantom.RoleAppearance;

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            // do phantom stuff
        }
    }
}
