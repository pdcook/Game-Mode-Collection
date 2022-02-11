using UnboundLib;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class VampireRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Vampire.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Vampire.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.1f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Vampire>();
        }
    }
    public class Vampire : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Vampire", 'V', GM_TRT.VampireColor);

        public override TRT_Role_Appearance Appearance => Vampire.RoleAppearance;

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            // do vampire stuff
        }
    }
}
