using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.Objects;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using GameModeCollection.Utils;
using UnityEngine.UI;
using TMPro;
using System;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_C4Prefab
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
        private static GameObject _C4TimerUI = null;
        public static GameObject C4TimerUI
        {
            get
            {
                if (_C4TimerUI is null)
                {
                    _C4TimerUI = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_C4UI"));
                    _C4TimerUI.AddComponent<C4TimerHandler>().SetPrefab(true);
                    UnityEngine.GameObject.DontDestroyOnLoad(_C4TimerUI);
                }
                return _C4TimerUI;
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
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_0, CardItem.IgnoreMaxCardsCategory};
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_0 };
            statModifiers.AddObjectToPlayer = A_C4Prefab.C4;
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
            return "A powerful timed explosive which you can place with [item 0] and set the timer. Longer timer means the C4 will be harder to find and harder to disarm.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_C4");
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
                this.HasPlaced = true;
                GameObject C4UI = GameObject.Instantiate(A_C4Prefab.C4TimerUI);
                C4TimerHandler handler = C4UI.GetComponent<C4TimerHandler>();
                handler.SetPlayer(this.Player);
                handler.SetPrefab(false);
                CardUtils.Call_RemoveCardFromPlayer_ClientsideCardBar(this.Player, C4Card.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
                Destroy(this.gameObject);
            }
        }
    }

    internal class C4TimerHandler : MonoBehaviour
    {
        private const float RepeatInputDelay = 0.1f;
        private const float StepSize = 15f;
        Slider Slider;
        Player Player;
        bool HasSet = false;
        TextMeshProUGUI Timer;
        float repeatInputTimer = 0f;
        bool IsPrefab = true;
        public void SetPrefab(bool prefab)
        {
            this.IsPrefab = prefab;
            this.transform.GetChild(0).gameObject.SetActive(!prefab);
            this.transform.GetChild(1).gameObject.SetActive(!prefab);
        }
        void Start()
        {
            if (this.IsPrefab) { return; }
            this.GetComponent<Canvas>().worldCamera = Camera.current;
            this.GetComponent<Canvas>().sortingLayerID = SortingLayer.NameToID("MostFront");
            this.GetComponent<GraphicRaycaster>().enabled = true;
            this.Slider = this.GetComponentInChildren<Slider>();
            this.Timer = this.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();
            this.HasSet = false;
            this.Slider.minValue = C4Handler.MinTime;
            this.Slider.maxValue = C4Handler.MaxTime;
        }
        internal void SetPlayer(Player player)
        {
            this.HasSet = false;
            this.Player = player;
            this.Player.data.input.enabled = false;
            this.Player.data.input.jumpIsPressed = false;
            this.Player.data.input.jumpWasPressed = false;
        }
        void Update()
        {
            if (this.IsPrefab) { return; }
            if (this.Slider is null || this.Timer is null) { return; }
            // update text
            this.Timer.text = this.GetClockString(this.Slider.value);

            if (this.Player is null || this.HasSet) { return; }
            // do player input
            this.Player.data.input.enabled = false;
            this.Player.data.input.jumpIsPressed = false;
            this.Player.data.input.jumpWasPressed = false;

            this.repeatInputTimer -= Time.deltaTime;
            if (this.Player.data.playerActions.Left.IsPressed)
            {
                if (this.repeatInputTimer <= 0f)
                {
                    this.repeatInputTimer = RepeatInputDelay;
                    this.Slider.value = Mathf.Clamp(this.Slider.value - StepSize, C4Handler.MinTime, C4Handler.MaxTime);
                }
            }
            else if (this.Player.data.playerActions.Right.IsPressed)
            {
                if (this.repeatInputTimer <= 0f)
                {
                    this.repeatInputTimer = RepeatInputDelay;
                    this.Slider.value = Mathf.Clamp(this.Slider.value + StepSize, C4Handler.MinTime, C4Handler.MaxTime);
                }
            }
            else
            {
                this.repeatInputTimer = 0f;
            }

            if (this.Player.data.playerActions.Jump.WasPressed)
            {
                this.SetC4Timer();
            }
        }
        string GetClockString(float time_in_seconds)
        {
            return TimeSpan.FromSeconds(time_in_seconds).ToString(@"mm\:ss");
        }

        void SetC4Timer()
        {
            this.HasSet = true;
            this.Player.data.input.enabled = true;

            GameModeCollection.instance.StartCoroutine(C4Handler.AskHostToMakeC4(this.Player.playerID, this.Slider.value, this.Player.transform.position, this.Player.transform.rotation));

            Destroy(this.gameObject);
        }
        void OnDestroy()
        {
            if (this.Player != null) { this.Player.data.input.enabled = true; }
        }
    }
}

