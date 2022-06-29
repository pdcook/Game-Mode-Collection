using UnityEngine;
using UnboundLib;
using System.Linq;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class GlitchRoleHelp : IRoleHelp
    {
        public TRT_Role_Appearance RoleAppearance => Glitch.RoleAppearance;
        public Alignment RoleAlignment => Glitch.RoleAlignment;
        public TRT_Role_Appearance[] OpposingRoles => new TRT_Role_Appearance[] { Traitor.RoleAppearance, Hypnotist.RoleAppearance, Vampire.RoleAppearance, Killer.RoleAppearance };
        public TRT_Role_Appearance[] AlliedRoles => new TRT_Role_Appearance[] { Innocent.RoleAppearance, Mercenary.RoleAppearance, Detective.RoleAppearance, Phantom.RoleAppearance };
        public string WinCondition => $"Kill all members of the {Traitor.RoleAppearance} team and the {Killer.RoleAppearance} if present.";
        public string Description =>
$@"A special {Innocent.RoleAppearance}.

Appears as a {Traitor.RoleAppearance} to all members of the {Traitor.RoleAppearance} team.
    Or as a {Zombie.RoleAppearance} to {Zombie.RoleAppearance}s.

Prevents {Traitor.RoleAppearance}s from using {Traitor.RoleAppearance} chat.

All {Traitor.RoleAppearance}s are notified of the presence, <i>but not the identity</i>, of a {Glitch.RoleAppearance} at the start of the round.";
    }
    public class GlitchRoleHandler : IRoleHandler
    {
        public static string GlitchRoleName => Glitch.RoleAppearance.Name;
        public static string GlitchRoleID = $"GM_TRT_{GlitchRoleName}";
        public IRoleHelp RoleHelp => new GlitchRoleHelp();
        public Alignment RoleAlignment => Glitch.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => GlitchRoleName;
        public string RoleID => GlitchRoleID;
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Glitch>();
        }
    }
    public class Glitch : Innocent
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Glitch", 'G', GM_TRT.GlitchColor);
        public override TRT_Role_Appearance Appearance => Glitch.RoleAppearance;

        public override Alignment Alignment => Alignment.Innocent;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => true;

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
                    // appear as a traitor to traitors, or a zombie to zombies
                    // there should never be traitors and zombies at the same time
                    if (PlayerManager.instance.players.Any(p => RoleManager.GetPlayerRoleID(p) ==  ZombieRoleHandler.ZombieRoleID))
                    {
                        return Zombie.RoleAppearance;
                    }
                    else
                    {
                        return Traitor.RoleAppearance;
                    }
                case Alignment.Chaos:
                    return null;
                case Alignment.Killer:
                    return null;
                default:
                    return null;
            }
        }
        void OnDisable()
        {
            // when this player dies, the Traitors will regain the ability to see their chat
            this.ExecuteAfterFrames(5, BetterChat.BetterChat.EvaluateCanSeeGroup);

        }
    }
}
