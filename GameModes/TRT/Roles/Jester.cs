using UnboundLib;
using System.Linq;
using UnityEngine;
using GameModeCollection.GameModes.TRT.RoundEvents;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class JesterRoleHelp : IRoleHelp
    {
        public TRT_Role_Appearance RoleAppearance => Jester.RoleAppearance;
        public Alignment RoleAlignment => Jester.RoleAlignment;
        public TRT_Role_Appearance[] OpposingRoles => new TRT_Role_Appearance[] {};
        public TRT_Role_Appearance[] AlliedRoles => new TRT_Role_Appearance[] {};
        public string WinCondition => $"Be killed by another player.";
        public string Description =>
$@"The {Jester.RoleAppearance} cannot deal damage,
    nor can they take damage from the environment.

{Traitor.RoleAppearance}s and {Killer.RoleAppearance}s are alerted to the <i>identities</i> of all {Jester.RoleAppearance}s.
{Jester.RoleAppearance}s can talk (but not receive responses) in {Traitor.RoleAppearance} chat.";
    }
    public class JesterRoleHandler : IRoleHandler
    {
        public IRoleHelp RoleHelp => new JesterRoleHelp();
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
        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => false;
        public override float KarmaChange { get; protected set; } = 0f;
        public override int StartingCredits => 0;

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
                RoundSummary.LogEvent(KilledChaosEvent.ID, killingPlayer.playerID, Jester.RoleAppearance);
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
        public override void TryShop()
        {
            // no shop for Jester
            return;
        }
    }
}
