using UnboundLib;
using System.Linq;
using UnityEngine;

namespace GameModeCollection.GameModes.TRT.Roles
{
    public class KillerRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Killer.RoleAlignment;
        public string WinMessage => "THE KILLER WINS";
        public Color WinColor => Killer.RoleAppearance.Color;
        public string RoleName => Killer.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.05f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Killer>();
        }
    }
    public class Killer : TRT_Role
    {
        public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Killer, "Killer", 'K', GM_TRT.KillerColor);
        public readonly static Alignment RoleAlignment = Alignment.Killer;

        public override TRT_Role_Appearance Appearance => Killer.RoleAppearance;

        public override Alignment Alignment => Killer.RoleAlignment;

        public override int MaxCards => GM_TRT.BaseMaxCards + 3;

        public override int StartingCards => 1;

        public override float BaseHealth => 1.5f*GM_TRT.BaseHealth;

        public override bool CanDealDamage => true;

        public override bool AlertAlignment(Alignment alignment)
        {
            return false;
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            return null;
        }

        public override void OnCorpseInteractedWith(Player player)
        {
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            corpse.SearchBody(this.GetComponent<Player>(), false);
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Killer || a == Alignment.Chaos);
        }
    }
}
