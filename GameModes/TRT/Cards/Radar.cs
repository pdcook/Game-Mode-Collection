using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.GameModes.TRT.Roles;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using MapEmbiggener.Controllers;

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
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_DoNotDropOnDeath };

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
        internal static void Callback(CardInfo card)
        {
            RadarCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    class A_Radar : MonoBehaviour
    {
        private const float ScanEvery = 30f;
        private float ScanTimer = 0f;
        private Player Player;
        private List<GameObject> RadarPoints = new List<GameObject>() { };
        void Start()
        {
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine) { return; }
            this.ScanTimer -= TimeHandler.deltaTime;
            if (this.ScanTimer <= 0f)
            {
                this.ScanTimer = ScanEvery;
                this.DoScan();
            }
        }
        void DoScan()
        {
            foreach (GameObject obj in this.RadarPoints)
            {
                if (obj is null) { continue; }
                Destroy(obj);
            }
            this.RadarPoints.Clear();
            PlayerManager.instance.ForEachAlivePlayer(player =>
            {
                if (player.playerID == this.Player.playerID) { return; }

                GameObject RadarPoint = GameObject.Instantiate(A_RadarPrefabs.RadarPoint, new Vector3(player.transform.position.x, player.transform.position.y, -199f), Quaternion.identity);
                RadarPoint.GetComponent<RadarPoint>().SetCreator(this.Player);
                RadarPoint.GetComponent<RadarPoint>().SetTracked(player);
                RadarPoint.GetComponent<RadarPoint>().SetPrefab(false);
                this.RadarPoints.Add(RadarPoint);

            });
        }
    }
    class RadarPoint : MonoBehaviour
    {
        private const float ConstScale = 0.3f;
        private Player Player;
        private Player TrackedPlayer;
        private TextMeshPro Text;
        private GameObject Circle;
        private GameObject Arrow;
        private Vector2 TrackedPosition;
        private ITRT_Role TargetRole;
        private Alignment? PlayerAlignment;
        internal void SetPrefab(bool isPrefab)
        {
            this.gameObject.SetActive(!isPrefab);
        }
        internal void SetCreator(Player player)
        {
            this.Player = player;
            this.PlayerAlignment = RoleManager.GetPlayerAlignment(player);
        }
        internal void SetTracked(Player player)
        {
            this.TrackedPlayer = player;
            this.TargetRole = RoleManager.GetPlayerRole(this.TrackedPlayer);
        }
        void Start()
        {
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
        }
        void Update()
        {
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

