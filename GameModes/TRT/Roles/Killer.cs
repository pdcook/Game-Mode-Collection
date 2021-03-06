using UnboundLib;
using System.Linq;
using UnityEngine;
using UnboundLib.Networking;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.Objects;
using GameModeCollection.Utils;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class KillerRoleHelp : IRoleHelp
    {
        public TRT_Role_Appearance RoleAppearance => Killer.RoleAppearance;
        public Alignment RoleAlignment => Killer.RoleAlignment;
        public TRT_Role_Appearance[] OpposingRoles => new TRT_Role_Appearance[] { };
        public TRT_Role_Appearance[] AlliedRoles => new TRT_Role_Appearance[] { };
        public string WinCondition => $"Kill all non-{Jester.RoleAppearance} players.";
        public string Description =>
$@"Spawns with 150% HP.

Has access to a shop with <i>all<\i> weapons and equipment
    available to both {Detective.RoleAppearance}s and {Traitor.RoleAppearance}s.
Spawns with 2 credits.
";
    }
    public class KillerRoleHandler : IRoleHandler
    {
        public static string KillerRoleName => Killer.RoleAppearance.Name;
        public static string KillerRoleID = $"GM_TRT_{KillerRoleName}";
        public IRoleHelp RoleHelp => new KillerRoleHelp();
        public Alignment RoleAlignment => Killer.RoleAlignment;
        public string WinMessage => "THE KILLER WINS";
        public Color WinColor => Killer.RoleAppearance.Color;
        public string RoleName => KillerRoleName;
        public string RoleID => KillerRoleID;
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
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

        public override float BaseHealth => 1.5f*GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => true;

        public override float KarmaChange { get; protected set; } = 0f;
        public override int StartingCredits => 2;

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
            // punish RDM, although this shouldn't be possible since there will only ever be one killer
            if (killedPlayer?.GetComponent<TRT_Role>()?.Alignment == this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>()?.playerID)
            {
                KarmaChange -= GM_TRT.KarmaPenaltyPerRDM;
            }
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return playersRemaining.Count() > 0 && playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Killer || a == Alignment.Chaos);
        }
        public override void TryShop()
        {
            TRTShopHandler.ToggleTDShop(this.GetComponent<Player>());
        }
    }
}
