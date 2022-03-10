using GameModeCollection.GameModes;
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
using Sonigon;
using Sonigon.Internal;

namespace GameModeCollection.Objects.GameModeObjects.TRT
{
	public static class HealthStationPrefab
	{
		private static GameObject _HealthStation = null;

		public static GameObject HealthStation
		{
			get
			{
				if (HealthStationPrefab._HealthStation == null)
				{

					GameObject healthStation = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_HealthStation"));
					healthStation.AddComponent<PhotonView>();
					healthStation.AddComponent<HealthStationHandler>();
					healthStation.name = "HealthStationPrefab";

					healthStation.GetComponent<HealthStationHandler>().IsPrefab = true;

					GameModeCollection.Log("HealthStation Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(healthStation);

					PhotonNetwork.PrefabPool.RegisterPrefab(healthStation.name, healthStation);

					HealthStationPrefab._HealthStation = healthStation;
				}
				return HealthStationPrefab._HealthStation;
			}
		}


	}
	public class HealthStationHandler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		private const float TriggerRadius = 2f;

		private const float Volume = 1f;

        public override bool RemoveOnPointEnd { get => !this.IsPrefab; protected set => base.RemoveOnPointEnd = value; }

        public bool IsPrefab { get; internal set; } = false;

		internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();
		public float Health { get; private set; } = 0f;
		public float MaxHealth { get; private set; } = 1f;

		public static readonly Color FullColor = Color.green;
		public static readonly Color EmptyColor = Color.clear;

		private const float AmountToHeal = 10f;
		private const float Delay = 0.4f;
		private float TimeUntilNextLocalHeal = 0f;

		private float CheckOOBTimer = 0f;
		private const float CheckOOBEvery = 1f;

		private bool isHealing = false;
		private int NumPlayersHealing = 0;
		private bool ContinuousSoundPlaying = false;

		private SoundEvent HealStartSound;
		private SoundEvent HealContinuousSound;

		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] data = info.photonView.InstantiationData;

			this.Health = (float)data[0];
			this.MaxHealth = (float)data[0];
		}
		internal static IEnumerator MakeHealthStationHandler(float health, Vector3 position, Quaternion rotation)
		{
			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        HealthStationPrefab.HealthStation.name,
                        position,
                        rotation,
                        0,
                        new object[] { health }
					);
			}

			yield break;
		}
		internal static IEnumerator AskHostToMakeHealthStation(float health, Vector3 position, Quaternion rotation)
        {
			NetworkingManager.RPC(typeof(HealthStationHandler), nameof(RPCM_MakeHealthStationHandler), health, position, rotation);
			yield break;
        }
		[UnboundRPC]
		private static void RPCM_MakeHealthStationHandler(float health, Vector3 position, Quaternion rotation)
        {
			GameModeCollection.instance.StartCoroutine(MakeHealthStationHandler(health, position, rotation));	
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
			this.isHealing = false;
			this.NumPlayersHealing = 0;
			this.ContinuousSoundPlaying = false;

			// load healstart and healcontinuous sounds
			AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("HealthStationStart.ogg");
			SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
			soundContainer.setting.volumeIntensityEnable = true;
			soundContainer.audioClip[0] = sound;
			this.HealStartSound = ScriptableObject.CreateInstance<SoundEvent>();
			this.HealStartSound.soundContainerArray[0] = soundContainer;
			AudioClip sound2 = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("HealthStationContinuous.ogg");
			SoundContainer soundContainer2 = ScriptableObject.CreateInstance<SoundContainer>();
			soundContainer2.setting.volumeIntensityEnable = true;
			soundContainer2.setting.loopEnabled = true;
			soundContainer2.audioClip[0] = sound2;
			this.HealContinuousSound = ScriptableObject.CreateInstance<SoundEvent>();
			this.HealContinuousSound.soundContainerArray[0] = soundContainer2;

			base.Start();

			this.Trig.radius = HealthStationHandler.TriggerRadius;
			this.Col.size = new Vector2(2.2f, 1.2f);
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
		}

 		protected internal override void OnTriggerStay2D(Collider2D collider2D)
		{
			if (this.Health > 0f)
			{
				Player player = collider2D?.GetComponent<Player>();
				if (player != null && player.data.view.IsMine && this.CanSeePlayer(player) && player.data.playerActions.InteractIsPressed() && this.TimeUntilNextLocalHeal <= 0f && player.data.HealthPercentage < 1f)
				{
					this.View.RPC(nameof(HealPlayer), RpcTarget.All, player.playerID);
					this.TimeUntilNextLocalHeal = Delay;
				}
			}
			base.OnTriggerStay2D(collider2D);
		}
		[PunRPC]
		private void HealPlayer(int playerID)
        {
			Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
			if (player is null) { return; }
			player.data.healthHandler.Heal(AmountToHeal);
			this.Health -= AmountToHeal;
        }
        protected override void Update()
        {
			this.TimeUntilNextLocalHeal -= TimeHandler.deltaTime;

			this.Renderer.color = Color.Lerp(EmptyColor, FullColor, this.Health / this.MaxHealth);

			if (this.TimeUntilNextLocalHeal <= -Delay/10f)
            {
				if (this.isHealing) { this.View.RPC(nameof(RPCA_PlayerStopHealing), RpcTarget.All); }
				this.isHealing = false;
            }
			else
            {
				if (!this.isHealing) { this.View.RPC(nameof(RPCA_PlayerStartHealing), RpcTarget.All); }
				this.isHealing = true;
            }

			base.Update();
			this.CheckOOBTimer -= Time.deltaTime;
			if (this.CheckOOBTimer < 0f)
			{
				this.CheckOOBTimer = CheckOOBEvery;
                Vector3 point = OutOfBoundsUtils.InverseGetPoint(this.Rig.position);
                if (point.x <= 0f || point.x >= 1f || point.y <= 0f)
                {
                    Destroy(this.gameObject);
                }
			}
		}
		[PunRPC]
		private void RPCA_PlayerStartHealing()
        {
			SoundManager.Instance.Play(this.HealStartSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * Volume) });
			if (!this.ContinuousSoundPlaying)
            {
				this.ContinuousSoundPlaying = true;
				SoundManager.Instance.Play(this.HealContinuousSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * Volume) });
            }
			this.NumPlayersHealing++;
        }
		[PunRPC]
		private void RPCA_PlayerStopHealing()
        {
			this.NumPlayersHealing = Mathf.Clamp(this.NumPlayersHealing - 1, 0, int.MaxValue);
			if (this.NumPlayersHealing == 0)
            {
				this.ContinuousSoundPlaying = false;
				SoundManager.Instance.Stop(this.HealContinuousSound, this.transform, true);
            }
        }
        private const string SyncedHealthKey = "HealthStation_Health";
        private const string SyncedHealingKey = "HealthStation_Healing";

		protected override void SetDataToSync()
		{
			this.SetSyncedFloat(SyncedHealthKey, this.Health);
			this.SetSyncedInt(SyncedHealingKey, this.NumPlayersHealing);
		}
		protected override void ReadSyncedData()
		{
			// syncing
			this.Health = this.GetSyncedFloat(SyncedHealthKey, this.Health);
			this.NumPlayersHealing = this.GetSyncedInt(SyncedHealingKey, this.NumPlayersHealing);
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
	}
}
