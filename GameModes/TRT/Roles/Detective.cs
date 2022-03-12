using UnityEngine;
using UnboundLib;
using UnboundLib.Utils;
using System.Linq;
using Photon.Pun;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.Objects;
using GameModeCollection.Extensions;
using UnboundLib.Networking;
using System.Collections.Generic;
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
        public override int StartingCredits => 2;
        private List<int> playerIDsRewardedFor = new List<int>() { };

        protected override void Start()
        {
            base.Start();

            this.playerIDsRewardedFor = new List<int>() { };

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
        public override void TryShop()
        {
            TRTShopHandler.ToggleDetectiveShop(this.GetComponent<Player>());
        }
        public override void OnAnyPlayerDied(Player deadPlayer, ITRT_Role[] rolesRemaining)
        {
            if (RoleManager.GetPlayerAlignment(deadPlayer) == Alignment.Traitor && !this.playerIDsRewardedFor.Contains(deadPlayer.playerID))
            {
                this.playerIDsRewardedFor.Add(deadPlayer.playerID);
                TRTShopHandler.GiveCreditToPlayer(this.GetComponent<Player>());
            }
        }
    }
}
