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

        public override int MaxCards => GM_TRT.BaseMaxCards + 1;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => true;

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
            // FOR NOW: the traitor has a 40% chance of spawning with a death station
            if ((this.GetComponent<Player>()?.data?.view?.IsMine ?? false) && UnityEngine.Random.Range(0f, 1f) < 0.4f)
            {
                NetworkingManager.RPC(typeof(Traitor), nameof(RPCA_AddCardToPlayer), this.GetComponent<Player>().playerID);
            }

        }
        [UnboundRPC]
        private static void RPCA_AddCardToPlayer(int playerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, DeathStationCard.Card, addToCardBar: false);
            if (player.data.view.IsMine)
            {
                CardItemHandler.ClientsideAddToCardBar(player.playerID, DeathStationCard.Card);
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
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // punish RDM
            if (killedPlayer?.GetComponent<TRT_Role>()?.Alignment == this.Alignment)
            {
                KarmaChange -= GM_TRT.KarmaPenaltyPerRDM;
            }
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Traitor || a == Alignment.Chaos);
        }
    }
}
