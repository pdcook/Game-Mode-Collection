using UnboundLib;
using System.Linq;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class TraitorRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Traitor.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Traitor.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 0;
        public int MinNumberOfPlayersWithRole => 1;
        public int MaxNumberOfPlayersWithRole => int.MaxValue;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Traitor>();
        }
    }
    public class Traitor : TRT_Role
    {
        public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Traitor", 'T', GM_TRT.TraitorColor);
        public static readonly Alignment RoleAlignment = Alignment.Traitor;

        public override TRT_Role_Appearance Appearance => Traitor.RoleAppearance;

        public override Alignment Alignment => Traitor.RoleAlignment;

        public override int MaxCards => GM_TRT.BaseMaxCards + 1;

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

        public override void OnCorpseInteractedWith(Player player)
        {
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            GM_TRT.instance.IdentifyBody(corpse, false);
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Traitor || a == Alignment.Chaos);
        }
    }
}
