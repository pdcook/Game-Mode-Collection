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
    static class A_GrenadePrefab
    {
        private static GameObject _Grenade = null;
        public static GameObject Grenade
        {
            get
            {
                if (_Grenade is null)
                {
                    _Grenade = new GameObject("A_Grenade", typeof(A_Grenade));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Grenade);
                }
                return _Grenade;
            }
        }
    }

    public class GrenadeCard : CustomCard
    {    
        internal static CardInfo Card = null;
        internal static string CardName => "Grenade";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_CanSpawnNaturally, TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_3, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_3 };
            statModifiers.AddObjectToPlayer = A_GrenadePrefab.Grenade;
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
            return "A standard fragmentation grenade. Press [item 3] to throw it.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_Grenade");

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
            return CardThemeColor.CardThemeColorType.FirepowerYellow;
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
            GrenadeCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_Grenade : MonoBehaviour
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
            if ((this.Player?.data?.view?.IsMine ?? false) && !this.Player.data.dead && (bool)this.Player.data.playerVel.GetFieldValue("simulated") && this.Player.data.playerActions.ItemWasPressed(3) && !used)
            {
                used = true;
                GameModeCollection.instance.StartCoroutine(GrenadeHandler.MakeGrenadeHandler(this.Player.playerID, ThrowSpeed * this.Player.data.weaponHandler.gun.shootPosition.forward, this.Player.data.weaponHandler.transform.position + this.Player.data.weaponHandler.gun.shootPosition.forward, Quaternion.identity));
                CardUtils.Call_RemoveCardFromPlayer_ClientsideCardBar(this.Player, GrenadeCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
                Destroy(this);
            }
        }
    }
}

