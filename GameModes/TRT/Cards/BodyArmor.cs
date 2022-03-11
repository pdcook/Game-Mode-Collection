using GameModeCollection.Objects;
using UnboundLib.Cards;
using UnityEngine;

namespace GameModeCollection.GameModes.TRT.Cards
{
    public class BodyArmorCard : CustomCard
    {
        internal static CardInfo Card = null;
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_DoNotDropOnDeath, CardItem.IgnoreMaxCardsCategory };

            // double health
            statModifiers.health = 2f;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard()
        {
        }

        protected override string GetTitle()
        {
            return "Body Armor";
        }
        protected override string GetDescription()
        {
            return "Body armor to increase your maximum HP.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_BODYARMOR");
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat(){amount = "+100%", positive = true, simepleAmount=CardInfoStat.SimpleAmount.aHugeAmountOf, stat = "HP"}
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DefensiveBlue;
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
            BodyArmorCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
}

