﻿using UnityEngine;
using UnboundLib;
using UnboundLib.Utils;
using System.Linq;
using Photon.Pun;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.Objects;
using GameModeCollection.Extensions;
using UnboundLib.Networking;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class DetectiveRoleHandler : IRoleHandler
    {
        public static string DetectiveRoleName => Detective.RoleAppearance.Name;
        public static string DetectiveRoleID = $"GM_TRT_{DetectiveRoleName}";
        public const float DetectiveRarity = 0.125f;
        public Alignment RoleAlignment => Detective.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => DetectiveRoleName;
        public string RoleID => DetectiveRoleID;
        public int MinNumberOfPlayersForRole => 0;
        public float Rarity => DetectiveRarity; // rarity is meaningless for Detective
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => null; // this is meaningless for Detective
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Detective>();
        }
    }
    public class Detective : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Detective", 'D', GM_TRT.DetectiveColor);
        public override TRT_Role_Appearance Appearance => Detective.RoleAppearance;
        public override int MaxCards => GM_TRT.BaseMaxCards + 1;

        protected override void Start()
        {
            base.Start();

            //CardInfo healingField = CardManager.cards.Values.First(card => card.cardInfo.name.Equals("Healing field")).cardInfo;

            // 80% of the time the detective spawns with healing field
            // 20% of the time they spawn with Golden Gun

            // FOR NOW: the detective has a 80% chance of spawning with a health station
            if ((this.GetComponent<Player>()?.data?.view?.IsMine ?? false) )//&& UnityEngine.Random.Range(0f, 1f) < 0.8f)
            {
                NetworkingManager.RPC(typeof(Detective), nameof(RPCA_AddCardToPlayer), this.GetComponent<Player>().playerID);
            }

            // detective gets a fancy hat
            if (this.GetComponent<Player>()?.data?.view?.IsMine ?? false)
            {
                PlayerFace currentFace = this.GetComponent<CharacterData>().GetCurrentFace();
                this.GetComponent<PhotonView>().RPC("RPCA_SetFace", RpcTarget.All, new object[]
                {
                    currentFace.eyeID,
                    currentFace.eyeOffset,
                    currentFace.mouthID,
                    currentFace.mouthOffset,
                    CharacterCreatorItemLoader.instance.GetItemIDByName("TRT_Detective_Hat", CharacterItemType.Detail),
                    Vector2.zero,
                    0,
                    Vector2.zero
                });
            }

        }
        [UnboundRPC]
        private static void RPCA_AddCardToPlayer(int playerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, HealthStationCard.Card, addToCardBar: false);
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, GoldenDeagleCard.Card, addToCardBar: false);
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, RadarCard.Card, addToCardBar: false);
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, DefuserCard.Card, addToCardBar: false);
            if (player.data.view.IsMine)
            {
                CardItemHandler.ClientsideAddToCardBar(player.playerID, HealthStationCard.Card);
                CardItemHandler.ClientsideAddToCardBar(player.playerID, GoldenDeagleCard.Card);
                CardItemHandler.ClientsideAddToCardBar(player.playerID, RadarCard.Card);
                CardItemHandler.ClientsideAddToCardBar(player.playerID, DefuserCard.Card);
            }
        }

        public override bool AlertAlignment(Alignment alignment)
        {
            return false;
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            return Detective.RoleAppearance;
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            corpse.SearchBody(this.GetComponent<Player>(), true);
        }
    }
}
