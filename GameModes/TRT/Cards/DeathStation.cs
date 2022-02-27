using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class DeathStationPrefab
    {
        private static GameObject _DeathStation = null;
        public static GameObject DeathStation
        {
            get
            {
                if (_DeathStation is null)
                {
                    _DeathStation = new GameObject("A_DeathStation", typeof(A_DeathStation));
                    UnityEngine.GameObject.DontDestroyOnLoad(_DeathStation);
                }
                return _DeathStation;
            }
        }
    }
    public class DeathStationCard : CustomCard
    {
        public static CardInfo Card { get; private set; } = null;
        /*
         * TRT detective shop card that allows the player to drop a health station when they press item button 1
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Slot_1, TRTCardCategories.TRT_IgnoreCardLimit };
            statModifiers.AddObjectToPlayer = DeathStationPrefab.DeathStation;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard()
        {
        }

        protected override string GetTitle()
        {
            return "Death Station";
        }
        protected override string GetDescription()
        {
            return "Looks just like a health station, but instantly kills anyone that tries to use it. Press [item 1] to drop.";
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
            return CardThemeColor.CardThemeColorType.EvilPurple;
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
            DeathStationCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_DeathStation : MonoBehaviour
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
                GameModeCollection.Log("PLACE DeathStation");
                this.HasPlaced = true;
                this.StartCoroutine(DeathStationHandler.AskHostToMakeDeathStation(this.Player.playerID, this.Player.transform.position, this.Player.transform.rotation));
                NetworkingManager.RPC(typeof(A_DeathStation), nameof(RPCA_RemoveCardFromPlayer), this.Player.playerID);
                Destroy(this.gameObject);
            }
        }
        [UnboundRPC]
        private static void RPCA_RemoveCardFromPlayer(int playerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, DeathStationCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest, false);
        }
    }
}

