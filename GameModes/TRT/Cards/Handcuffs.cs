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

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_HandcuffsPrefab
    {
        private static GameObject _Handcuffs = null;
        public static GameObject Handcuffs
        {
            get
            {
                if (_Handcuffs is null)
                {
                    _Handcuffs = new GameObject("A_Handcuffs", typeof(A_Handcuffs));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Handcuffs);
                }
                return _Handcuffs;
            }
        }
    }
    public class HandcuffsCard : CustomCard
    {
        internal static CardInfo Card = null;
        /*
         * Detective item that allows the detective to handcuff (remove all cards from and disarm for 30 seconds) another player
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Slot_4, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_4 };
            statModifiers.AddObjectToPlayer = A_HandcuffsPrefab.Handcuffs;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }

        protected override string GetTitle()
        {
            return "Handcuffs";
        }
        protected override string GetDescription()
        {
            return "Completely disarm a player for 30 seconds by pressing\n[item 4] near them.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_Handcuffs");
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
        public override void Callback()
        {
            this.gameObject.AddComponent<TRTCardSlotText>();
        }        
        internal static void BuildCardCallback(CardInfo card)
        {
            HandcuffsCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_Handcuffs : MonoBehaviour
    {
        public const float Range = 4f;
        public bool HasCuffed { get; set;} = false;
        private Player Player;
        void Start()
        {
            this.HasCuffed = false;
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine || this.Player.data.dead) { return; }

            if (!this.HasCuffed && this.Player.data.playerActions.ItemWasPressed(4))
            {
                Player cuffPlayer = PlayerManager.instance.GetClosestOtherPlayer(this.Player, true, true);
                if (cuffPlayer is null || Vector2.Distance(this.Player.data.playerVel.position, cuffPlayer.data.playerVel.position) > Range) { return; }
                this.HasCuffed = true;
                NetworkingManager.RPC(typeof(A_Handcuffs), nameof(RPCA_CuffPlayer), this.Player.playerID, cuffPlayer.playerID);
            }
        }
        [UnboundRPC]
        internal static void RPCA_CuffPlayer(int cuffingPlayerID, int cuffedPlayerID)
        {
            Player cuffingPlayer = PlayerManager.instance.GetPlayerWithID(cuffingPlayerID);
            Player cuffedPlayer = PlayerManager.instance.GetPlayerWithID(cuffedPlayerID);
            if (cuffingPlayer is null || cuffedPlayer is null) { return; }

            CardUtils.RemoveCardFromPlayer_ClientsideCardBar(cuffingPlayer, HandcuffsCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);

            if (cuffingPlayer.data.view.IsMine)
            {
                TRTHandler.SendChat(null, $"{RoleManager.GetPlayerColorNameAsColoredString(cuffedPlayer)} was cuffed.", true);
            }
            if (cuffedPlayer.data.view.IsMine)
            {
                TRTHandler.SendChat(null, "You were cuffed.", true);
            }
            cuffedPlayer.gameObject.GetOrAddComponent<Cuffed>();

        }
    }
    class Cuffed : MonoBehaviour
    {
        private const float CuffDuration = 30f;

        private float CuffTimer = CuffDuration;
        private bool IsCuffed = true;
        Player player;
        List<CardInfo> cardsToHold = new List<CardInfo>() { };
        private static readonly List<CardCategory> NonDropCategories = new List<CardCategory>()
        { 
            TRTCardCategories.TRT_DoNotDropOnDeath,
            CardItem.CannotDiscard,
            TRTCardCategories.TRT_Slot_0,
            TRTCardCategories.TRT_Slot_1,
            TRTCardCategories.TRT_Slot_2,
            TRTCardCategories.TRT_Slot_3,
            TRTCardCategories.TRT_Slot_4,
            TRTCardCategories.TRT_Slot_5
        };
        void Start()
        {
            this.player = this.GetComponent<Player>();
            this.player.data.GetData().playerCanCollectCards = false;
            this.player.data.GetData().playerCanAccessShop = false;
            this.CuffTimer = CuffDuration;
            this.IsCuffed = true;
            HideGun(this.player.playerID, true);

            // hold cards that had to have been purchased in the shop, or the player shouldn't be able to drop, and give them back later 
            this.cardsToHold = this.player.data.currentCards.Where(c => c.categories.Intersect(NonDropCategories).Any()).ToList();
            List<CardInfo> cardsToDrop = this.player.data.currentCards.Where(c => !c.categories.Intersect(NonDropCategories).Any()).ToList();
            ModdingUtils.Utils.Cards.instance.RemoveAllCardsFromPlayer(this.player, false);
            if (this.player.data.view.IsMine)
            {
                ModdingUtils.Utils.CardBarUtils.instance.PlayersCardBar(0).ClearBar();
            }
            GM_TRT.instance.StartCoroutine(DropCards(this.player, cardsToDrop.ToArray()));
        }
        void Update()
        {
            this.CuffTimer -= TimeHandler.deltaTime;
            if (this.IsCuffed)
            {
                this.player.data.GetData().playerCanCollectCards = false;
                this.player.data.GetData().playerCanAccessShop = false;
                HideGun(this.player.playerID, true);
            }
            if (this.IsCuffed && this.CuffTimer <= 0f)
            {
                this.player.data.GetData().playerCanCollectCards = true;
                this.player.data.GetData().playerCanAccessShop = true;
                this.IsCuffed = false;
                if (this.player.data.view.IsMine && !this.player.data.dead)
                {
                    CardUtils.Call_AddCardsToPlayer_ClientsideCardBar(this.player, this.cardsToHold.ToArray(), false);
                    TRTHandler.SendChat(null, "You were released.", true);
                }
                Destroy(this);
            }
        }
        static void MoveToHide(Transform transform, bool hide)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, hide ? -10000f : 0f);
        }
        internal static void HideGun(int playerID, bool hide)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            Gun gun = player.data.weaponHandler.gun;
            GameObject springObj = gun.transform.Find("Spring").gameObject;

            MoveToHide(springObj.transform.Find("Ammo/Canvas"), hide);
            MoveToHide(springObj.transform.GetChild(2), hide);
            MoveToHide(springObj.transform.GetChild(3), hide);
            springObj.transform.GetChild(2).GetComponent<RightLeftMirrorSpring>().enabled = !hide;
            springObj.transform.GetChild(3).GetComponent<RightLeftMirrorSpring>().enabled = !hide;

            gun.GetData().disabled = hide;
        }
        void OnDisable()
        {
            Destroy(this);
        }
        void OnDestroy()
        {
            this.player.data.GetData().playerCanCollectCards = true;
            this.player.data.GetData().playerCanAccessShop = true;
            HideGun(this.player.playerID, false);
        }
        private static IEnumerator DropCards(Player player, CardInfo[] cardsToDrop)
        {
            foreach (CardInfo card in cardsToDrop)
            {
                if (!GM_TRT.instance.battleOngoing) { yield break; }
                yield return new WaitForSecondsRealtime(GM_TRT.TimeBetweenCardDrops);
                yield return GM_TRT.instance.PlayerDropCard(player, card);
            }
            yield break;
        }
        internal static IEnumerator RemoveAllCuffsFromPlayers(IGameModeHandler gm)
        {
            PlayerManager.instance.ForEachPlayer(p =>
            {
                try
                {
                    if (p.gameObject.GetComponent<Cuffed>() != null)
                    {
                        GameObject.Destroy(p.gameObject.GetComponent<Cuffed>());
                    }
                }
                catch { }
            });
            yield break;
        }
    }
}

