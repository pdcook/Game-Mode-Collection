using UnityEngine;
using UnboundLib;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class DetectiveRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Detective.RoleAlignment;
        public string RoleName => Detective.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 0;
        public int MinNumberOfPlayersWithRole => 1;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 1f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Detective>();
        }
    }
    public class Detective : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Detective", 'D', GM_TRT.DetectiveColor);
        public override TRT_Role_Appearance Appearance => Detective.RoleAppearance;

        public override bool AlertAlignment(Alignment alignment)
        {
            return true;
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            return Detective.RoleAppearance;
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            GM_TRT.instance.IdentifyBody(corpse, true);
        }
    }
}
