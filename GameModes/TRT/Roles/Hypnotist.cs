using UnboundLib;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class HypnotistRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Hypnotist.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Hypnotist.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.15f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Hypnotist>();
        }
    }

    public class Hypnotist : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Hypnotist", 'H', GM_TRT.HypnotistColor);

        public override TRT_Role_Appearance Appearance => Hypnotist.RoleAppearance;

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            // do hypnotist stuff
        }
    }
}
