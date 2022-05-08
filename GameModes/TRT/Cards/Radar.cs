using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.Objects;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using MapEmbiggener.Controllers;
using UnboundLib.GameModes;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_RadarPrefabs
    {
        private static GameObject _Radar = null;
        public static GameObject Radar
        {
            get
            {
                if (_Radar is null)
                {
                    _Radar = new GameObject("A_Radar", typeof(A_Radar));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Radar);
                }
                return _Radar;
            }
        }
        private static GameObject _RadarPoint = null;
        public static GameObject RadarPoint
        {
            get
            {
                if (_RadarPoint is null)
                {
					_RadarPoint = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_Radar_Point"));
                    _RadarPoint.AddComponent<RadarPoint>().SetPrefab(true);
                    UnityEngine.GameObject.DontDestroyOnLoad(_RadarPoint);
                }
                return _RadarPoint;
            }
        }
    }
    public class RadarCard : CustomCard
    {
        internal static CardInfo Card = null;
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            // load prefabs
            GameObject _ = A_RadarPrefabs.RadarPoint;

            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_DoNotDropOnDeath, CardItem.IgnoreMaxCardsCategory };

            statModifiers.AddObjectToPlayer = A_RadarPrefabs.Radar;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }

        protected override string GetTitle()
        {
            return "Radar";
        }
        protected override string GetDescription()
        {
            return "Shows the positions of all other players every 30 seconds.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_RADAR");
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
            RadarCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    class RadarTimer : MonoBehaviour
    {
        public const float ScanEvery = 30f;
        private float ScanTimer = 0f;
        public bool IsReady => this.ScanTimer <= 0f;
        public float Perc => 1f - this.ScanTimer / ScanEvery;
        void Start()
        {
            this.ScanTimer = 0f;
        }
        void Update()
        {
            if (this.ScanTimer <= 0f) { return; }
            this.ScanTimer -= TimeHandler.deltaTime;
        }
        public void ResetTimer()
        {
            this.ScanTimer = ScanEvery;
        }
    }
    class A_Radar : MonoBehaviour
    {
        private Player Player;
        private RadarTimer Timer;
        private List<GameObject> RadarPoints = new List<GameObject>() { };
        internal static List<GameObject> AllRadarPoints = new List<GameObject>(){};
        void Start()
        {
            this.Player = this.GetComponentInParent<Player>();
            if (this.Player != null) { this.Timer = this.Player.gameObject.GetOrAddComponent<RadarTimer>(); }
        }
        void Update()
        {
            if (this.Player is null || this.Timer is null || !this.Player.data.view.IsMine) { return; }
            if (this.Timer.IsReady)
            {
                this.Timer.ResetTimer();
                this.DoScan();
            }
        }
        void DoScan()
        {
            this.DestroyAllPoints();
            PlayerManager.instance.ForEachAlivePlayer(player =>
            {
                if (player.playerID == this.Player.playerID) { return; }

                GameObject RadarPoint = GameObject.Instantiate(A_RadarPrefabs.RadarPoint, new Vector3(player.transform.position.x, player.transform.position.y, -199f), Quaternion.identity);
                RadarPoint.GetComponent<RadarPoint>().SetCreator(this.Player);
                RadarPoint.GetComponent<RadarPoint>().SetTracked(player);
                RadarPoint.GetComponent<RadarPoint>().SetPrefab(false);
                RadarPoint.GetComponent<RadarPoint>().CreatePing();
                this.RadarPoints.Add(RadarPoint);
                AllRadarPoints.Add(RadarPoint);

            });
        }
        void OnDestroy()
        {
            //this.DestroyAllPoints();
        }
        void DestroyAllPoints()
        {
            foreach (GameObject obj in this.RadarPoints)
            {
                if (obj is null) { continue; }
                Destroy(obj);
            }
            this.RadarPoints.Clear();
        }
        internal static IEnumerator DestroyAllPointsOnPointEnd(IGameModeHandler gm)
        {
            foreach (GameObject point in AllRadarPoints)
            {
                if (point is null) { continue; }
                GameObject.Destroy(point);
            }
            AllRadarPoints.Clear();

            yield break;
        }

    }
    class RadarPoint : MonoBehaviour
    {
        private const float DestroyTimerPerc = 0.01f;
        private const float FadeOutTimerPerc = 0.1f;
        private const float ConstScale = 0.3f;
        private Player Player;
        private RadarTimer Timer;
        private Player TrackedPlayer;
        private TextMeshPro Text;
        private GameObject Circle;
        private GameObject Arrow;
        private SpriteRenderer CircleSprite;
        private SpriteRenderer ArrowSprite;
        private SpriteRenderer CircleBackgroundSprite;
        private SpriteRenderer ArrowBackgroundSprite;
        private Vector2 TrackedPosition;
        private ITRT_Role TargetRole;
        private Alignment? PlayerAlignment;
        private int FramesSinceCreation = 0;
        internal void SetPrefab(bool isPrefab)
        {
            this.gameObject.SetActive(!isPrefab);
        }
        internal void SetCreator(Player player)
        {
            this.Player = player;
            this.PlayerAlignment = RoleManager.GetPlayerAlignment(player);
            this.Timer = player.GetComponent<RadarTimer>();
        }
        internal void SetTracked(Player player)
        {
            this.TrackedPlayer = player;
            this.TargetRole = RoleManager.GetPlayerRole(this.TrackedPlayer);
        }
        internal void CreatePing()
        {
            this.FramesSinceCreation = 0;

            this.TrackedPosition = this.transform.position;

            this.Text = this.GetComponentInChildren<TextMeshPro>(true);
            this.Circle = this.transform.Find("TRT_Radar_Circle")?.gameObject;
            this.Arrow = this.transform.Find("TRT_Radar_Arrow")?.gameObject;

            if (this.Text is null || this.Circle is null || this.Arrow is null || this.Player is null || this.TrackedPlayer is null)
            {
                return;
            }
            this.Text.transform.localPosition = new Vector3(0f, 0f, 100f);
            this.Text.gameObject.AddComponent<RadarTextOrientator>();
            this.transform.localScale = ConstScale * MainCam.instance.cam.orthographicSize / ControllerManager.DefaultZoom * Vector3.one;

            foreach (SpriteRenderer spriteRenderer in this.GetComponentsInChildren<SpriteRenderer>(true))
            {
                spriteRenderer.sortingLayerID = SortingLayer.NameToID("MostFront");
            }
            this.Text.sortingLayerID = SortingLayer.NameToID("MostFront");

            if (this.PlayerAlignment != null && this.TargetRole != null)
            {
                Color color = (TargetRole.AppearToAlignment((Alignment)this.PlayerAlignment) ?? Innocent.RoleAppearance).Color;

                this.Text.color = color;
                this.Circle.GetComponent<SpriteRenderer>().color = color;
                this.Arrow.GetComponent<SpriteRenderer>().color = color;
            }
            this.CircleSprite = this.Circle.GetComponent<SpriteRenderer>();
            this.CircleBackgroundSprite = this.Circle.transform.GetChild(0).GetComponent<SpriteRenderer>();
            this.ArrowSprite = this.Arrow.GetComponent<SpriteRenderer>();
            this.ArrowBackgroundSprite = this.Arrow.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            this.FramesSinceCreation++;

            if (this.Timer != null && this.FramesSinceCreation > 5 && 1f - this.Timer.Perc < FadeOutTimerPerc)
            {
                this.Text.color = Color.Lerp(Color.clear, this.Text.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.CircleSprite.color = Color.Lerp(Color.clear, this.CircleSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.CircleBackgroundSprite.color = Color.Lerp(Color.clear, this.CircleBackgroundSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.ArrowSprite.color = Color.Lerp(Color.clear, this.ArrowSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.ArrowBackgroundSprite.color = Color.Lerp(Color.clear, this.ArrowBackgroundSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                if (this.Timer.Perc < DestroyTimerPerc)
                {
                    Destroy(this.gameObject);
                }
            }


            this.transform.localScale = ConstScale * MainCam.instance.cam.orthographicSize / ControllerManager.DefaultZoom * Vector3.one;

            this.Text.text = $"{Vector2.Distance(this.Player.transform.position, this.TrackedPosition):0}";

            Vector2 screenPos = MainCam.instance.cam.WorldToViewportPoint(this.TrackedPosition); //get viewport positions

            if (screenPos.x >= 0f && screenPos.x <= 1f && screenPos.y >= 0f && screenPos.y <= 1f)
            {
                // point is on screen
                this.transform.position = this.TrackedPosition;
                this.Circle.SetActive(true);
                this.Arrow.SetActive(false);
                this.transform.rotation = Quaternion.identity;
                return;
            }

            Vector2 onScreenPos = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f) * 2f; // 2D version, new mapping
            float max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); // get largest offset
            onScreenPos = 0.9f * (onScreenPos / (max * 2f)) + new Vector2(0.5f, 0.5f); // undo mapping, scale to add margin

            Vector2 newPos = MainCam.instance.cam.ViewportToWorldPoint(onScreenPos);
            this.transform.position = new Vector3(newPos.x, newPos.y, 0f);
            this.Circle.SetActive(false);
            this.Arrow.SetActive(true);
            this.transform.up = (Vector3)this.TrackedPosition - this.Player.transform.position;
        }
    }
    public class RadarTextOrientator : MonoBehaviour
    {
        Transform Parent;
        void Start()
        {
            this.Parent = this.transform.parent;
        }

        void Update()
        {
            float angle = Vector2.SignedAngle(Vector2.up, this.Parent.up);
            if (angle >= -45f && angle <= 45f)
            {
                this.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (angle >= 45f && angle <= 135f)
            {
                this.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            }
            else if (angle >= 135f || angle <= -135f)
            {
                this.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            }
            else if (angle >= -135f && angle <= -45f)
            {
                this.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }
    }

}

