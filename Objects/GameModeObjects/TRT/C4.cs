using GameModeCollection.GameModes.TRT.Cards;
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
using Sonigon;
using Sonigon.Internal;

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

					c4.GetComponent<C4Handler>().IsPrefab = true;

					GameModeCollection.Log("C4 Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(c4);

					PhotonNetwork.PrefabPool.RegisterPrefab(c4.name, c4);

					C4Prefab._C4 = c4;
				}
				return C4Prefab._C4;
			}
		}
		private static GameObject _C4Explosion = null;
		public static GameObject C4Explosion
        {
			get
            {
				if (C4Prefab._C4Explosion is null)
                {
					_C4Explosion = CardManager.cards.Values.Select(card => card.cardInfo).Where(card => card.cardName.ToLower() == "EXPLOSIVE BULLET".ToLower()).First().GetComponent<Gun>().objectsToSpawn[0].effect;
				}
				return C4Prefab._C4Explosion;
            }
        }
	}
	public class C4Handler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		public const float ExplosionVolume = 1f;

		public const float InnerExplosionDamage = 1000f;
		public const float OuterExplosionDamage = 70f;
		public const float InnerExplosionRange = 35f;
		public const float OuterExplosionRange = 75f;

		public const float MaxBeepVolume = 0.75f;
		public const float MinBeepVolume = 0.05f;
		public const float MaxBeepPeriod = 10f;
		public const float MinBeepPeriod = 1f;
		public const bool CanHearThroughWalls = false;

		public const float MinTime = 15f;
		public const float MaxTime = 300f;
		public static readonly Color StartDefuseColor = new Color32(230, 0, 0, 255);
		public static readonly Color StartDefuseFillColor = new Color32(230, 0, 0, 26);
		public static readonly Color FinishDefuseColor = new Color32(0, 230, 0, 255);
		public static readonly Color FinishDefuseFillColor = new Color32(0, 230, 0, 26);

		private const float TriggerRadius = 1.5f;
        public override bool RemoveOnPointEnd { get => !this.IsPrefab; protected set => base.RemoveOnPointEnd = value; }
        public bool IsPrefab { get; internal set; } = false;

		// when set for the minimum time, the c4 will beep loudly every 1 second
		// when set for the maximum time, the c4 will beep quietly every 10 seconds
		public float BeepIntensity => Mathf.Lerp(MaxBeepVolume, MinBeepVolume, (this.TotalTime - MinTime) / MaxTime);
		public float BeepEvery => Mathf.Lerp(MinBeepPeriod, MaxBeepPeriod, (this.TotalTime - MinTime) / MaxTime);
		public float BeepTimer = 0f;
		private const float BlinkEvery = 1f;
		private float BlinkTimer = BlinkEvery;
		private int blink = 1; // +1 for on, -1 for off

		public bool IsDefusing { get; private set; } = false;
		public float TimeToDefuse => this.TotalTime / 20f; // takes 1 20th as long to defuse as it was set for
		public float DefuseProgress => this.TimeDefused / this.TimeToDefuse;
		private float TimeDefused = 0f;

		public float TotalTime { get; private set; } = float.MaxValue;
		public float Time { get; private set; } = float.MaxValue;
		public bool Armed { get; private set; } = true;
		public bool Exploded { get; private set; } = false;
		public int PlacerID { get; private set; } = -1;

		private GameObject DefusalTimerObject;
		private DefusalTimerEffect DefusalTimerEffect;
		private SoundEvent BeepSound = null;
		private SoundEvent ExplosionSound = null;

		internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] data = info.photonView.InstantiationData;

			this.PlacerID = (int)data[0];
			this.TotalTime = (float)data[1];
			this.Time = (float)data[1];

			this.BeepTimer = this.BeepEvery;
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
														playerPushMult: 30000f,
														playerDamageMult: 0f,
														collisionDamageThreshold: float.MaxValue,
														friction: 1f,
														impulseMult: 0f,
														forceMult: 1f, visibleThroughShader: false);

			base.Awake();
		}
		protected override void Start()
		{
			base.Start();

			this.Armed = true;
			this.Exploded = false;

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

			// load beep sound
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("BombBeep.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            this.BeepSound = ScriptableObject.CreateInstance<SoundEvent>();
            this.BeepSound.soundContainerArray[0] = soundContainer;

			// load explosion sound
			this.ExplosionSound = CardManager.cards.Values.Select(card => card.cardInfo).Where(card => card.cardName.ToLower() == "EXPLOSIVE BULLET".ToLower()).First().GetComponent<Gun>().soundImpactModifier.impactEnvironment;

			// ring for defusal progress
			var abyssalCard = CardManager.cards.Values.First(card => card.cardInfo.name.Equals("AbyssalCountdown")).cardInfo;
			var statMods = abyssalCard.gameObject.GetComponentInChildren<CharacterStatModifiers>();
			var abyssalObj = statMods.AddObjectToPlayer;

			this.DefusalTimerObject = Instantiate(abyssalObj.transform.Find("Canvas").gameObject, this.transform);
			this.DefusalTimerObject.name = "DefusalTimerEffects";
			this.DefusalTimerObject.transform.localPosition = Vector3.zero;

			this.DefusalTimerEffect = this.DefusalTimerObject.AddComponent<DefusalTimerEffect>();
			this.DefusalTimerEffect.outerRing = this.DefusalTimerObject.transform.Find("Size/Ring").GetComponent<ProceduralImage>();
			this.DefusalTimerEffect.fill = this.DefusalTimerObject.transform.Find("Size/Background").GetComponent<ProceduralImage>();
			this.DefusalTimerEffect.rotator = this.DefusalTimerObject.transform.Find("Size/Rotate").GetComponent<RectTransform>();
			this.DefusalTimerEffect.still = this.DefusalTimerObject.transform.Find("Size/Top").GetComponent<RectTransform>();

			this.DefusalTimerEffect.outerRing.color = StartDefuseColor;
			this.DefusalTimerEffect.fill.color = StartDefuseFillColor;
			this.DefusalTimerEffect.rotator.gameObject.GetComponentInChildren<ProceduralImage>().color = this.DefusalTimerEffect.outerRing.color;
			this.DefusalTimerEffect.still.gameObject.GetComponentInChildren<ProceduralImage>().color = this.DefusalTimerEffect.outerRing.color;
			this.DefusalTimerObject.transform.Find("Size/BackRing").GetComponent<ProceduralImage>().color = Color.clear;

		}
		protected override void Update()
        {
			if (this.Armed && !this.Exploded)
            {
                this.Time -= TimeHandler.deltaTime;

                if (this.IsDefusing) { this.TimeDefused += TimeHandler.deltaTime; }
                else { this.TimeDefused = 0f; }

                this.BeepTimer -= TimeHandler.deltaTime;
                if (this.BeepTimer < 0f)
                {
                    this.BeepTimer = this.BeepEvery;
					// try beep
					this.TryBeep();
                }
				this.BlinkTimer -= TimeHandler.deltaTime;
				if (this.BlinkTimer < 0f)
                {
					this.BlinkTimer = BlinkEvery;
                    this.blink *= -1;
                }


				if (this.DefuseProgress >= 1f)
                {
					this.View.RPC(nameof(this.RPCA_Defuse), RpcTarget.All);
                }

				if (this.Time <= 0f && this.View.IsMine)
                {
					this.View.RPC(nameof(this.RPCA_Explode), RpcTarget.All);
                }

            }
			else
            {
				this.TimeDefused = 0f;
            }

            this.DefusalTimerEffect.DefuseProgress(this.DefuseProgress);

            this.Renderer.color = !this.Exploded ? (this.Armed ? (this.blink > 0 ? Color.red : Color.clear) : Color.green) : Color.clear;

            base.Update();
		}
		void TryBeep()
        {
			Player player = PlayerManager.instance.GetLocalPlayer();
            if (player is null || CanHearThroughWalls || this.CanSeePlayer(player))
            {
				SoundManager.Instance.Play(this.BeepSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * this.BeepIntensity) });
            }
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
        private const string SyncedArmedKey = "C4_Armed";

		protected override void SetDataToSync()
		{
			this.SetSyncedFloat(SyncedTimeKey, this.Time);
			this.SetSyncedInt(SyncedArmedKey, this.Armed ? 1 : 0);
		}
		protected override void ReadSyncedData()
		{
			// syncing
			this.Time = this.GetSyncedFloat(SyncedTimeKey, this.Time);
			this.Armed = this.GetSyncedInt(SyncedArmedKey, this.Armed ? 1 : 0) == 1;
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
		[PunRPC]
		private void RPCA_Defuse()
        {
			this.Armed = false;
        }
		[PunRPC]
		private void RPCA_Explode()
        {
			// play sound
			SoundManager.Instance.Play(this.ExplosionSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * ExplosionVolume) });

			this.Exploded = true;
			// spawn two explosions
			// - one that is smaller, more powerful, and respects walls
			// - one that is larger, less powerful, and goes through walls
			GameObject innerExplosionObj = GameObject.Instantiate(C4Prefab.C4Explosion, this.transform.position, Quaternion.identity);
			GameObject outerExplosionObj = GameObject.Instantiate(C4Prefab.C4Explosion, this.transform.position, Quaternion.identity);
			Explosion innerExpl = innerExplosionObj.GetComponent<Explosion>();
			Explosion outerExpl = outerExplosionObj.GetComponent<Explosion>();
			innerExplosionObj.GetOrAddComponent<SpawnedAttack>().spawner = PlayerManager.instance.GetPlayerWithID(this.PlacerID);
			outerExplosionObj.GetOrAddComponent<SpawnedAttack>().spawner = PlayerManager.instance.GetPlayerWithID(this.PlacerID);

			innerExpl.ignoreTeam = false;
			innerExpl.ignoreWalls = false;
			innerExpl.scaleDmg = false;
			innerExpl.scaleForce = false;
			innerExpl.scaleRadius = false;
			innerExpl.scaleSilence = false;
			innerExpl.scaleSlow = false;
			innerExpl.scaleStun = false;
			innerExpl.auto = true;

			outerExpl.ignoreTeam = false;
			outerExpl.ignoreWalls = true;
			outerExpl.scaleDmg = false;
			outerExpl.scaleForce = false;
			outerExpl.scaleRadius = false;
			outerExpl.scaleSilence = false;
			outerExpl.scaleSlow = false;
			outerExpl.scaleStun = false;
			outerExpl.auto = true;

			innerExpl.damage = InnerExplosionDamage;
			outerExpl.damage = OuterExplosionDamage;
			innerExpl.range = InnerExplosionRange;
			outerExpl.range = OuterExplosionRange;

			innerExplosionObj.SetActive(true);
			outerExplosionObj.SetActive(true);

			Destroy(this.gameObject);
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
            this.transform.localScale = 0.005f * Vector3.one;
            this.counter = 1f;
            this.backRing = this.outerRing.transform.parent.GetChild(0).gameObject.GetComponent<ProceduralImage>();
            this.backRing.type = UnityEngine.UI.Image.Type.Filled;
            this.rotator.gameObject.SetActive(false);
            this.still.gameObject.SetActive(false);
            this.fill.gameObject.SetActive(true);
            this.outerRing.gameObject.SetActive(true);
            this.backRing.gameObject.SetActive(false);
			foreach (SpriteRenderer sprite in this.gameObject.GetComponentsInChildren<SpriteRenderer>(true))
            {
				sprite.sortingLayerID = SortingLayer.NameToID("Player10");
            }

			this.outerRing.fillAmount = 0f;
			this.fill.fillAmount = 0f;
            this.outerRing.BorderWidth = 20f;
            this.backRing.BorderWidth = 20f;
            this.backRing.fillAmount = 0f;
		}
		public void DefuseProgress(float progress)
		{
			this.outerRing.fillAmount = UnityEngine.Mathf.Clamp01(progress);
			this.fill.fillAmount = UnityEngine.Mathf.Clamp01(progress);
			this.outerRing.color = Color.Lerp(C4Handler.StartDefuseColor, C4Handler.FinishDefuseColor, UnityEngine.Mathf.Clamp01(progress));
			this.fill.color = Color.Lerp(C4Handler.StartDefuseFillColor, C4Handler.FinishDefuseFillColor, UnityEngine.Mathf.Clamp01(progress));
		}
	}
}
