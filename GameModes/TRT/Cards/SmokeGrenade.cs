using GameModeCollection.Extensions;
using GameModeCollection.Objects;
using LocalZoom;
using MapEmbiggener.Controllers;
using ModdingUtils.MonoBehaviours;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Networking;
using UnityEngine;
using SoundImplementation;
using Sonigon;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.Utils;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_SmokeGrenadePrefab
    {
        private static GameObject _SmokeGrenade = null;
        public static GameObject SmokeGrenade
        {
            get
            {
                if (_SmokeGrenade is null)
                {
                    _SmokeGrenade = new GameObject("A_SmokeGrenade", typeof(A_SmokeGrenade));
                    UnityEngine.GameObject.DontDestroyOnLoad(_SmokeGrenade);
                }
                return _SmokeGrenade;
            }
        }
    }

    public class SmokeGrenadeCard : CustomCard
    {    
        internal static CardInfo Card = null;
        internal static string CardName => "Smoke Grenade";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_CanSpawnNaturally, TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_5, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_5 };
            statModifiers.AddObjectToPlayer = A_SmokeGrenadePrefab.SmokeGrenade;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }

        protected override string GetTitle()
        {
            return CardName;
        }
        protected override string GetDescription()
        {
            return "A standard non-lethal smoke grenade. Press [item 5] to throw it.";
        }

        protected override GameObject GetCardArt()
        {
            return null;
            //return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_SmokeGrenade");

        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[] { };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.TechWhite;
        }
        public override string GetModName()
        {
            return "TRT";
        }
        public override bool GetEnabled()
        {
            return false;
        }
        internal static void Callback(CardInfo card)
        {
            card.gameObject.AddComponent<TRTCardSlotText>();
            SmokeGrenadeCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_SmokeGrenade : MonoBehaviour
    {
        public const float ThrowSpeed = 50f;
        Player Player;
        bool used = false;
        void Start()
        {
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if ((this.Player?.data?.view?.IsMine ?? false) && !this.Player.data.dead && this.Player.data.playerActions.ItemWasPressed(5) && !used)
            {
                used = true;
                GameModeCollection.instance.StartCoroutine(SmokeGrenadeHandler.MakeSmokeGrenadeHandler(this.Player.playerID, ThrowSpeed * this.Player.data.weaponHandler.gun.shootPosition.forward, this.Player.data.weaponHandler.transform.position, Quaternion.identity));
                CardUtils.Call_RemoveCardFromPlayer_ClientsideCardBar(this.Player, SmokeGrenadeCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
                Destroy(this);
            }
        }
    }
}

