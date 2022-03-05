using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class C4Prefab
    {
        private static GameObject _C4 = null;
        public static GameObject C4
        {
            get
            {
                if (_C4 is null)
                {
                    _C4 = new GameObject("A_C4", typeof(A_C4));
                    UnityEngine.GameObject.DontDestroyOnLoad(_C4);
                }
                return _C4;
            }
        }
    }
    public class C4Card : CustomCard
    {
        internal static CardInfo Card = null;
        /*
         * TRT traitor shop card that allows the player to drop c4 when they press [interact]
         * the c4 will detonate after a set period of time, dealing massive damage
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_0 };
            statModifiers.AddObjectToPlayer = C4Prefab.C4;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard()
        {
        }

        protected override string GetTitle()
        {
            return "C4";
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
            return CardThemeColor.CardThemeColorType.DestructiveRed;
        }
        public override string GetModName()
        {
            return "TRT";
        }
        internal static void Callback(CardInfo card)
        {
            card.gameObject.AddComponent<TRTCardSlotText>();
            C4Card.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_C4 : MonoBehaviour
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
            if (!HasPlaced && this.Player.data.playerActions.ItemWasPressed(0))
            {
                GameModeCollection.Log("PLACE C4");
                this.HasPlaced = true;
                this.StartCoroutine(C4Handler.AskHostToMakeC4(this.Player.playerID, 100f, this.Player.transform.position, this.Player.transform.rotation));
            }
        }
    }
}

