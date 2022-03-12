using UnityEngine;
using UnboundLib;
using System.Linq;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class GlitchRoleHandler : IRoleHandler
    {
        public static string GlitchRoleName => Glitch.RoleAppearance.Name;
        public static string GlitchRoleID = $"GM_TRT_{GlitchRoleName}";
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
