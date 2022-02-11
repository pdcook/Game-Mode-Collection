using UnboundLib;
using System.Linq;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class JesterRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Jester.RoleAlignment;
        public string WinMessage => "THE JESTER WINS";
        public Color WinColor => Jester.RoleAppearance.Color;
        public string RoleName => Jester.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Jester>();
        }
    }
    public class Jester : TRT_Role
    {
        public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Jester", 'J', GM_TRT.JesterColor);
        public readonly static Alignment RoleAlignment = Alignment.Chaos;

        private bool hasBeenKilled = false;

        public override TRT_Role_Appearance Appearance => Jester.RoleAppearance;
        public override Alignment Alignment => Jester.RoleAlignment;
        public override int MaxCards => GM_TRT.BaseMaxCards;

        public override int StartingCards => 0;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamage => false;

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
                    return true;
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
                    return Jester.RoleAppearance;
                case Alignment.Chaos:
                    return null;
                case Alignment.Killer:
                    return Jester.RoleAppearance;
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
            // do jester stuff
            if (killingPlayer != null)
            {
                this.hasBeenKilled = true;
            }
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return this.hasBeenKilled;
        }
    }
}
