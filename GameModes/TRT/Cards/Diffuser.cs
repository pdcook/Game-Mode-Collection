using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using Photon.Pun;
using GameModeCollection.Utils;

namespace GameModeCollection.GameModes.TRT.Cards
{    
    static class A_DiffuserPrefab
    {
        private static GameObject _Diffuser = null;
        public static GameObject Diffuser
        {
            get
            {
                if (_Diffuser is null)
                {
                    _Diffuser = new GameObject("A_Diffuser");
                    UnityEngine.GameObject.DontDestroyOnLoad(_Diffuser);
                }
                return _Diffuser;
            }
        }
    }

    public class DiffuserCard : CustomCard
    {    
        /// One time use, defuses C4

        internal static CardInfo Card = null;
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_IgnoreCardLimit, TRTCardCategories.TRT_Slot_3 };

            statModifiers.AddObjectToPlayer = A_DiffuserPrefab.Diffuser;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }

        protected override string GetTitle()
        {
            return "Diffuser";
        }
        protected override string GetDescription()
        {
            return "";
        }

        protected override GameObject GetCardArt()
        {
            return null;
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
            };
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
            DiffuserCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
}

