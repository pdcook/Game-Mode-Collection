using UnboundLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.Objects;
using GameModeCollection.GameModes.TRT.Cards;
using UnboundLib.Networking;
using System.Linq;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class MercenaryRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Mercenary.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => Mercenary.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Mercenary>();
        }
    }
    public class Mercenary : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Mercenary", 'M', GM_TRT.MercenaryColor);

        public override TRT_Role_Appearance Appearance => Mercenary.RoleAppearance;

        public override int MaxCards => GM_TRT.BaseMaxCards + 1;

        protected override void Start()
        {
            base.Start();
            // FOR NOW: the merc has a 40% chance of spawning with a death station and a 40% chance of spawning with a healing station
            float rng = UnityEngine.Random.Range(0f, 1f);
            if ((this.GetComponent<Player>()?.data?.view?.IsMine ?? false) && rng < 0.8f)
            {
                NetworkingManager.RPC(typeof(Mercenary), nameof(RPCA_AddCardToPlayer), this.GetComponent<Player>().playerID, rng);
            }

        }
        [UnboundRPC]
        private static void RPCA_AddCardToPlayer(int playerID, float rng)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            CardInfo card = DeathStationCard.Card;
            if (rng > 0.4f)
            {
                card = HealthStationCard.Card;
            }
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, addToCardBar: false);
            if (player.data.view.IsMine)
            {
                CardItemHandler.ClientsideAddToCardBar(player.playerID, card);
            }
        }
    }
}
