using UnboundLib;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.Objects;
using GameModeCollection.GameModes.TRT.Cards;
using UnboundLib.Networking;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class TraitorRoleHandler : IRoleHandler
    {
        public static string TraitorRoleName => Traitor.RoleAppearance.Name;
        public static string TraitorRoleID = $"GM_TRT_{TraitorRoleName}";
        public const float TraitorRarity = 0.25f;
        public Alignment RoleAlignment => Traitor.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => TraitorRoleName;
        public string RoleID => TraitorRoleID;
        public int MinNumberOfPlayersForRole => 0;
        public int MinNumberOfPlayersWithRole => 1;
        public int MaxNumberOfPlayersWithRole => int.MaxValue;
        public float Rarity => TraitorRarity; // rarity is meaningless for Traitor
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => null; // this is meaningless for Traitor
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Traitor>();
        }
    }
    public class Traitor : TRT_Role
    {
        public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Traitor", 'T', GM_TRT.TraitorColor);
        public static readonly Alignment RoleAlignment = Alignment.Traitor;

        public override TRT_Role_Appearance Appearance => Traitor.RoleAppearance;

        public override Alignment Alignment => Traitor.RoleAlignment;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => true;

        public override float KarmaChange { get; protected set; } = 0f;
        public override int StartingCredits => 1;

        private float innocentStepCounter = 1f - GM_TRT.Perc_Inno_For_Reward;
        private int numInnocent = -1;

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

        protected override void Start()
        {
            base.Start();

            this.numInnocent = -1;
            this.innocentStepCounter = 1f - GM_TRT.Perc_Inno_For_Reward;
        }

        public override void OnCorpseInteractedWith(Player player)
        {
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            corpse.SearchBody(this.GetComponent<Player>(), false);
        }
        public override void OnAnyPlayerDied(Player deadPlayer, ITRT_Role[] rolesRemaining)
        {
            if (RoleManager.GetPlayerAlignment(deadPlayer) == Alignment.Innocent)
            {
                if (this.numInnocent == -1)
                {
                    // calculate how many innocents there were
                    this.numInnocent = rolesRemaining.Count(r => r.Alignment == Alignment.Innocent) + 1;
                }
                float percInnoRemain = (float)rolesRemaining.Count(r => r.Alignment == Alignment.Innocent) / (float)this.numInnocent;
                if (percInnoRemain < this.innocentStepCounter)
                {
                    TRTShopHandler.GiveCreditToPlayer(this.GetComponent<Player>());
                    while (percInnoRemain < this.innocentStepCounter)
                    {
                        this.innocentStepCounter -= GM_TRT.Perc_Inno_For_Reward;
                    }
                }

            }
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
            return playersRemaining.Count() == 0 || playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Traitor || a == Alignment.Chaos);
        }

        public override void TryShop()
        {
            TRTShopHandler.ToggleTraitorShop(this.GetComponent<Player>());
        }
    }
}
