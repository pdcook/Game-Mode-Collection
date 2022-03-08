﻿using GameModeCollection.GameModes.TRT.Cards;
using MapEmbiggener;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using TMPro;
using System;
using UnboundLib.Networking;
using GameModeCollection.Extensions;
using UnboundLib.Utils;
using UnityEngine.UI.ProceduralImage;

namespace GameModeCollection.Objects.GameModeObjects.TRT
{
	public static class C4Prefab
	{
		private static GameObject _C4 = null;

		public static GameObject C4
		{
			get
			{
				if (C4Prefab._C4 == null)
				{

					GameObject c4 = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_C4"));
					c4.AddComponent<PhotonView>();
					c4.AddComponent<C4Handler>();
					c4.name = "C4Prefab";

					/*
					GameObject Clock = new GameObject("C4 Clock",typeof(TextMeshPro));
					Clock.GetComponent<TextMeshPro>().color = Color.red;
					Clock.transform.SetParent(c4.transform);
					Clock.transform.localPosition = Vector3.zero;
					Clock.transform.localScale = Vector3.one;
					Clock.GetComponent<TextMeshPro>().enabled = false;
					Clock.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
					Clock.GetComponent<TextMeshPro>().fontSize = 10;*/

					c4.GetComponent<C4Handler>().IsPrefab = true;

					GameModeCollection.Log("C4 Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(c4);

					PhotonNetwork.PrefabPool.RegisterPrefab(c4.name, c4);

					C4Prefab._C4 = c4;
				}
				return C4Prefab._C4;
			}
		}


	}
	public class C4Handler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		public static readonly Color StartDefuseColor = new Color32(230, 0, 0, 255);
		public static readonly Color FinishDefuseColor = new Color32(0, 230, 0, 255);

		private const float TriggerRadius = 1.5f;
        public override bool RemoveOnPointEnd { get => true; protected set => base.RemoveOnPointEnd = value; }
        public bool IsPrefab { get; internal set; } = false;

		public bool IsDefusing { get; private set; } = false;
		public float TimeToDefuse { get; private set; } = 10f; // will depend on how long the C4's detonation timer was set for
		public float DefuseProgress => this.TimeDefused / this.TimeToDefuse;
		private float TimeDefused = 0f;

		public float Time { get; private set; } = float.MaxValue;
		public int PlacerID { get; private set; } = -1;

		private GameObject DefusalTimerObject;
		private DefusalTimerEffect DefusalTimerEffect;

		internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] data = info.photonView.InstantiationData;

			this.PlacerID = (int)data[0];
			this.Time = (float)data[1];
		}
		internal static IEnumerator MakeC4Handler(int placerID, float time, Vector3 position, Quaternion rotation)
		{
			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        C4Prefab.C4.name,
                        position,
                        rotation,
                        0,
                        new object[] { placerID, time }
					);
			}

			yield break;
		}
		internal static IEnumerator AskHostToMakeC4(int placerID, float time, Vector3 position, Quaternion rotation)
        {
			NetworkingManager.RPC(typeof(C4Handler), nameof(RPCM_MakeC4Handler), placerID, time, position, rotation);
			yield break;
        }
		[UnboundRPC]
		private static void RPCM_MakeC4Handler(int placerID, float time, Vector3 position, Quaternion rotation)
        {
			GameModeCollection.instance.StartCoroutine(MakeC4Handler(placerID, time, position, rotation));	
        }
		protected override void Awake()
		{
			this.PhysicalProperties = new ItemPhysicalProperties(mass: 80000f, bounciness: 0f,
																	playerPushMult: 0f,
																	playerDamageMult: 0f,
																	collisionDamageThreshold: float.MaxValue,
																	friction: 1f,
																	impulseMult: 0f,
																	forceMult: 0f, visibleThroughShader: false);

			base.Awake();
		}
		protected override void Start()
		{
			base.Start();

			this.Trig.radius = C4Handler.TriggerRadius;
			this.Col.size = new Vector2(2f, 0.65f);
			this.Col.edgeRadius = 0.1f;

			if (this.IsPrefab)
            {
				this.SetPos(1000000f * Vector2.one);
				this.Rig.isKinematic = true;
				this.gameObject.SetActive(false);
            }
			else
            {
				this.Rig.isKinematic = false;
				this.gameObject.SetActive(true);
            }

			// ring for defusal progress

			var abyssalCard = CardManager.cards.Values.First(card => card.cardInfo.name.Equals("AbyssalCountdown")).cardInfo;
			var statMods = abyssalCard.gameObject.GetComponentInChildren<CharacterStatModifiers>();
			var abyssalObj = statMods.AddObjectToPlayer;

			this.DefusalTimerObject = Instantiate(abyssalObj, this.transform);
			this.DefusalTimerObject.name = "A_TRT_VampireEffects";
			this.DefusalTimerObject.transform.localPosition = Vector3.zero;

			AbyssalCountdown abyssal = this.DefusalTimerObject.GetComponent<AbyssalCountdown>();

			this.DefusalTimerEffect = this.DefusalTimerObject.AddComponent<DefusalTimerEffect>();
			this.DefusalTimerEffect.outerRing = abyssal.outerRing;
			this.DefusalTimerEffect.fill = abyssal.fill;
			this.DefusalTimerEffect.rotator = abyssal.rotator;
			this.DefusalTimerEffect.still = abyssal.still;

			UnityEngine.GameObject.Destroy(abyssal);

			foreach (Transform child in this.DefusalTimerObject.transform)
			{
				if (child.name != "Canvas")
				{
					Destroy(child.gameObject);
				}
			}

			this.DefusalTimerEffect.outerRing.color = StartDefuseColor;
			this.DefusalTimerEffect.fill.color = new Color(StartDefuseColor.r, StartDefuseColor.g, StartDefuseColor.b, 0.1f);
			this.DefusalTimerEffect.rotator.gameObject.GetComponentInChildren<ProceduralImage>().color = this.DefusalTimerEffect.outerRing.color;
			this.DefusalTimerEffect.still.gameObject.GetComponentInChildren<ProceduralImage>().color = this.DefusalTimerEffect.outerRing.color;
			this.DefusalTimerObject.transform.Find("Canvas/Size/BackRing").GetComponent<ProceduralImage>().color = Color.clear;

		}
		private const float BlinkEvery = 1f;
		private float BlinkTimer = BlinkEvery;
		private int blink = 1;
		protected override void Update()
        {
			this.Time -= TimeHandler.deltaTime;

			if (this.IsDefusing) { this.TimeDefused += TimeHandler.deltaTime; }
            else { this.TimeDefused = 0f; }

			this.DefusalTimerEffect.DefuseProgress(this.DefuseProgress);

            base.Update();

			this.BlinkTimer -= TimeHandler.deltaTime;
			if (this.BlinkTimer < 0f)
            {
				this.BlinkTimer = BlinkEvery;
				this.blink *= -1;
            }

			this.Renderer.color = this.blink > 0 ? Color.red : Color.clear;
        }
        protected internal override void OnTriggerStay2D(Collider2D collider2D)
        {
			if (collider2D?.GetComponent<Player>() != null
				&& (collider2D.GetComponent<Player>()?.data?.view?.IsMine ?? false)
				&& !collider2D.GetComponent<Player>().data.dead)
			{
				if ( collider2D.GetComponent<Player>().data.currentCards.Select(c => c.cardName).Contains(DefuserCard.CardName)
					&& collider2D.GetComponent<Player>().data.playerActions.ItemIsPressed(3))
				{
					this.IsDefusing = true;
				}
				else
                {
					this.IsDefusing = false;
                }
			}
			base.OnTriggerStay2D(collider2D);
        }
        protected internal override void OnTriggerExit2D(Collider2D collider2D)
        {
			if (collider2D?.GetComponent<Player>() != null
				&& (collider2D.GetComponent<Player>()?.data?.view?.IsMine ?? false)
				&& !collider2D.GetComponent<Player>().data.dead)
			{
                this.IsDefusing = false;
			}
            base.OnTriggerExit2D(collider2D);
        }

        private const string SyncedTimeKey = "C4_Time";

		protected override void SetDataToSync()
		{
			this.SetSyncedFloat(SyncedTimeKey, this.Time);
		}
		protected override void ReadSyncedData()
		{
			// syncing
			this.Time = this.GetSyncedFloat(SyncedTimeKey, this.Time);
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
    }
	class DefusalTimerEffect : MonoBehaviour
    {
		public float counter;

		public ProceduralImage outerRing;
		public ProceduralImage backRing;

		public ProceduralImage fill;

		public Transform rotator;

		public Transform still;

		void Start()
        {
            this.transform.localScale = 0.5f * Vector3.one;
            this.counter = 1f;
            this.backRing = this.outerRing.transform.parent.GetChild(0).gameObject.GetComponent<ProceduralImage>();
            this.backRing.type = UnityEngine.UI.Image.Type.Filled;
            this.rotator.gameObject.SetActive(false);
            this.still.gameObject.SetActive(false);
            this.fill.gameObject.SetActive(false);
            this.outerRing.gameObject.SetActive(true);
            this.backRing.gameObject.SetActive(false);

			this.outerRing.fillAmount = 0f;
            this.outerRing.BorderWidth = 20f;
            this.backRing.BorderWidth = 20f;
            this.backRing.fillAmount = 0f;
		}
		public void DefuseProgress(float progress)
		{
			this.outerRing.fillAmount = UnityEngine.Mathf.Clamp01(progress);
			this.outerRing.color = Color.Lerp(C4Handler.StartDefuseColor, C4Handler.FinishDefuseColor, UnityEngine.Mathf.Clamp01(progress));
		}
	}
}
