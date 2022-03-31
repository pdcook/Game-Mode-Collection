using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace GameModeCollection.GameModes.Murder.Cards
{
    internal class ClueCard : CustomCard
    {
        public static CardInfo clueCard = null;
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            throw new System.NotImplementedException();
        }

        protected override GameObject GetCardArt()
        {
            throw new System.NotImplementedException();
        }

        protected override string GetDescription()
        {
            throw new System.NotImplementedException();
        }

        protected override CardInfo.Rarity GetRarity()
        {
            throw new System.NotImplementedException();
        }

        protected override CardInfoStat[] GetStats()
        {
            throw new System.NotImplementedException();
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            throw new System.NotImplementedException();
        }

        protected override string GetTitle()
        {
            throw new System.NotImplementedException();
        }
    }
}
