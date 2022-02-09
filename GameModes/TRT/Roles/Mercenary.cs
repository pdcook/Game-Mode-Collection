using UnboundLib;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class MercenaryRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Mercenary.RoleAlignment;
        public string RoleName => Mercenary.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.2f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Mercenary>();
        }
    }
    public class Mercenary : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Mercenary", 'M', GM_TRT.MercenaryColor);

        public override TRT_Role_Appearance Appearance => Mercenary.RoleAppearance;

        public override int MaxCards => GM_TRT.BaseMaxCards + 1;
    }
}
