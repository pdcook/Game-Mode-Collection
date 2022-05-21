using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using GameModeCollection.Utils;
using UnboundLib.GameModes;
using GameModeCollection.Objects;
using GameModeCollection.GameModeHandlers;
using Sonigon;
using Sonigon.Internal;
using Photon.Pun;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_DisguiserPrefab
    {
        private static GameObject _Disguiser = null;
        public static GameObject Disguiser
        {
            get
            {
                if (_Disguiser is null)
                {
                    _Disguiser = new GameObject("A_Disguiser", typeof(A_Disguiser));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Disguiser);
                }
                return _Disguiser;
            }
        }
    }
    public class DisguiserCard : CustomCard
    {
        internal static CardInfo Card = null;
        /*
         * Traitor item that allows the traitor to assume the face, color, name, AND reputation appearance of a dead player
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_4, CardItem.IgnoreMaxCardsCategory, TRTCardCategories.TRT_DoNotDropOnDeath };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_4 };
            statModifiers.AddObjectToPlayer = A_DisguiserPrefab.Disguiser;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }

        protected override string GetTitle()
        {
            return "Disguiser";
        }
        protected override string GetDescription()
        {
            return "Assume the identity of a dead player by pressing [item 4] near their corpse.";
        }

        protected override GameObject GetCardArt()
        {
            return null;
            //return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_Disguiser");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.MagicPink;
        }
        public override string GetModName()
        {
            return "TRT";
        }
        public override bool GetEnabled()
        {
            return false;
        }
        public override void Callback()
        {
            this.gameObject.AddComponent<TRTCardSlotText>();
        }        
        internal static void BuildCardCallback(CardInfo card)
        {
            DisguiserCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);

            GameModeManager.AddHook(GameModeHooks.HookPointEnd, HookPointEnd_ResetNames);
        }
        private static IEnumerator HookPointEnd_ResetNames(IGameModeHandler gm)
        {
            PlayerManager.instance.ForEachPlayer(p =>
            {
                p.data.GetData().forcedNickName = null;
                p.data.GetData().forcedReputability = null;
                p.gameObject.GetComponent<TRTNamePlate>()?.ForceUpdate();
            });
            yield break;
        }
    }
    internal class A_Disguiser : MonoBehaviour
    {
        public const float Range = 4f;
        public bool HasCopied { get; set;} = false;
        private Player Player;
        void Start()
        {
            this.HasCopied = false;
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine || this.Player.data.dead) { return; }

            if (!this.HasCopied && this.Player.data.playerActions.ItemWasPressed(4))
            {
                Player corpse = PlayerManager.instance.GetClosestCorpse(this.Player, true);
                if (corpse is null || Vector2.Distance(this.Player.data.playerVel.position, corpse.data.playerVel.position) > Range || corpse.GetComponent<TRT_Corpse>() is null) { return; }
                this.HasCopied = true;
                NetworkingManager.RPC(typeof(A_Disguiser), nameof(RPCA_CopyPlayer), this.Player.playerID, corpse.playerID);
            }
        }
        [UnboundRPC]
        internal static void RPCA_CopyPlayer(int copyingPlayerID, int copiedPlayerID)
        {
            Player copyingPlayer = PlayerManager.instance.GetPlayerWithID(copyingPlayerID);
            Player copiedPlayer = PlayerManager.instance.GetPlayerWithID(copiedPlayerID);
            if (copyingPlayer is null || copiedPlayer is null) { return; }

            CardUtils.RemoveCardFromPlayer_ClientsideCardBar(copyingPlayer, DisguiserCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);

            if (copyingPlayer.data.view.IsMine)
            {
                // send a chat message to the copying player telling them who they copied
                TRTHandler.SendChat(null, $"You have assumed the identity of {TRTHandler.GetPlayerNameAsColoredString(copiedPlayer)}.", true);

                // copy the player's face
                PlayerFace newFace = copiedPlayer.data.GetData().CurrentFace;
                copyingPlayer.data.view.RPC(nameof(Player.RPCA_SetFace), RpcTarget.All, newFace.eyeID, newFace.eyeOffset, newFace.mouthID, newFace.mouthOffset, newFace.detailID, newFace.detailOffset, newFace.detail2ID, newFace.detail2Offset);
            }
            // copy the player's skin
            UnboundLib.Extensions.PlayerExtensions.AssignColorID(copyingPlayer, UnboundLib.Extensions.PlayerExtensions.colorID(copiedPlayer));
            // copy the player's name
            copyingPlayer.data.GetData().forcedNickName = copiedPlayer.data.view.Owner.NickName;
            // copy the player's reputation
            copyingPlayer.data.GetData().forcedReputability = copiedPlayer.data.Reputability();
            // force update the copying player's nameplate
            copyingPlayer.gameObject.GetComponent<TRTNamePlate>()?.ForceUpdate();
            // update the player's gun color with LocalZoom
            LocalZoom.LocalZoom.instance.MakeGunHidden(copyingPlayer);
        }
    }
}

