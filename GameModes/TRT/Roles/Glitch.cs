using UnityEngine;
using UnboundLib;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class GlitchRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Glitch.RoleAlignment;
        public string RoleName => Glitch.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.2f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Glitch>();
        }
    }
    public class Glitch : Innocent
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Glitch", 'G', GM_TRT.GlitchColor);
        public override TRT_Role_Appearance Appearance => Glitch.RoleAppearance;

        public override Alignment Alignment => Alignment.Innocent;

        public override int MaxCards => GM_TRT.BaseMaxCards;

        public override int StartingCards => 0;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamage => true;

        public override bool AlertAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Innocent:
                    return false;
                case Alignment.Traitor:
                    return true;
                case Alignment.Chaos:
                    return false;
                case Alignment.Killer:
                    return false;
                default:
                    return false;
            }
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Innocent:
                    return null;
                case Alignment.Traitor:
                    return Traitor.RoleAppearance;
                case Alignment.Chaos:
                    return null;
                case Alignment.Killer:
                    return null;
                default:
                    return null;
            }
        }
    }
}
