using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.Objects;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using GameModeCollection.Utils;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_HealthStationPrefab
    {
        private static GameObject _HealthStation = null;
        public static GameObject HealthStation
        {
            get
            {
                if (_HealthStation is null)
                {
                    _HealthStation = new GameObject("A_HealthStation", typeof(A_HealthStation));
                    UnityEngine.GameObject.DontDestroyOnLoad(_HealthStation);
                }
                return _HealthStation;
            }
        }
    }
    public class HealthStationCard : CustomCard
    {
        public static CardInfo Card { get; private set; } = null;
        /*
         * TRT detective shop card that allows the player to drop a health station when they press item button 1
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_1, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_1 };
            statModifiers.AddObjectToPlayer = A_HealthStationPrefab.HealthStation;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard()
        {
        }

        protected override string GetTitle()
        {
            return "Health Station";
        }
        protected override string GetDescription()
        {
            return "A health station which anyone can [interact] with to heal. Heals up to 200HP. Press [item 1] to drop.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_HEALTHSTATION");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Uncommon;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.NatureBrown;
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
            HealthStationCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_HealthStation : MonoBehaviour
    {
        private bool HasPlaced = false;
        private Player Player;
        void Start()
        {
            this.HasPlaced = false;
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine) { return; }
            if (!HasPlaced && this.Player.data.playerActions.ItemWasPressed(1))
            {
                GameModeCollection.Log("PLACE HealthStation");
                this.HasPlaced = true;
                GameModeCollection.instance.StartCoroutine(HealthStationHandler.AskHostToMakeHealthStation(200f, this.Player.transform.position, this.Player.transform.rotation));
                CardUtils.Call_RemoveCardFromPlayer_ClientsideCardBar(this.Player, HealthStationCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
                Destroy(this.gameObject);
            }
        }
    }
}

