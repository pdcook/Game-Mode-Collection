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
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Jester>();
        }
    }
    public class Jester : TRT_Role
    {
        public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Chaos, "Jester", 'J', GM_TRT.JesterColor);
        public readonly static Alignment RoleAlignment = Alignment.Chaos;

        private bool hasBeenKilled = false;

        public override TRT_Role_Appearance Appearance => Jester.RoleAppearance;
        public override Alignment Alignment => Jester.RoleAlignment;
        public override int MaxCards => GM_TRT.BaseMaxCards;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => false;
        public override float KarmaChange { get; protected set; } = 0f;

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

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            corpse.SearchBody(this.GetComponent<Player>(), false);
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            // do jester stuff
            if (killingPlayer != null && killingPlayer.playerID != this.GetComponent<Player>().playerID)
            {
                this.hasBeenKilled = true;
            }
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // punish RDM, although this shouldn't be possible since the Jester/Swapper can't deal damage
            if (killedPlayer?.GetComponent<TRT_Role>()?.Alignment == this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>()?.playerID)
            {
                KarmaChange -= GM_TRT.KarmaPenaltyPerRDM;
            }
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return this.hasBeenKilled;
        }
    }
}
