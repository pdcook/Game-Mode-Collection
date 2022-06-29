using System;
using UnityEngine;
using UnboundLib;
using System.Linq;

namespace GameModeCollection.GameModes.TRT.Roles
{
    public class InnocentRoleHelp : IRoleHelp
    {
        public TRT_Role_Appearance RoleAppearance => Innocent.RoleAppearance;
        public Alignment RoleAlignment => Innocent.RoleAlignment;
        public TRT_Role_Appearance[] OpposingRoles => new TRT_Role_Appearance[] { Traitor.RoleAppearance, Hypnotist.RoleAppearance, Vampire.RoleAppearance, Assassin.RoleAppearance, Killer.RoleAppearance };
        public TRT_Role_Appearance[] AlliedRoles => new TRT_Role_Appearance[] { Detective.RoleAppearance, Mercenary.RoleAppearance, Glitch.RoleAppearance, Phantom.RoleAppearance };
        public string WinCondition => $"Kill all members of the {Traitor.RoleAppearance} team and the {Killer.RoleAppearance} if present.";
        public string Description =>
$@"The most common role.";
    }
    public class InnocentRoleHandler : IRoleHandler
    {
        public static string InnocentRoleName => Innocent.RoleAppearance.Name;
        public static string InnocentRoleID = $"GM_TRT_{InnocentRoleName}";
        public const float MinimumPercInnocent = 0.5f;
        public IRoleHelp RoleHelp => new InnocentRoleHelp();
        public Alignment RoleAlignment => Innocent.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => InnocentRoleName;
        public string RoleID => InnocentRoleID;
        public int MinNumberOfPlayersForRole => 0;
        public float Rarity => 1f; // rarity is meaningless for the innocent role
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => null; // this is meaningless for Innocent

        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Innocent>();
        }
    }
    public class Innocent : TRT_Role
    {
        public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Innocent", 'I', GM_TRT.InnocentColor);
        public readonly static Alignment RoleAlignment = Alignment.Innocent;

        public override TRT_Role_Appearance Appearance => Innocent.RoleAppearance;
        public override Alignment Alignment => Innocent.RoleAlignment;
        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => true;

        public override float KarmaChange { get; protected set; } = 0f;
        public override int StartingCredits => 0;

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

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            corpse.SearchBody(this.GetComponent<Player>(), false);
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // punish RDM
            if (killedPlayer?.GetComponent<TRT_Role>()?.Alignment == this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>()?.playerID)
            {
                KarmaChange -= GM_TRT.KarmaPenaltyPerRDM;
            }
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return playersRemaining.Count() > 0 && playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Innocent || a == Alignment.Chaos);
        }
        public override void TryShop()
        {
            // no shop for innocents
            return;
        }
    }
}
