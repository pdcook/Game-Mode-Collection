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

        public override bool RemoveOnPointEnd { get => !this.IsPrefab; protected set => base.RemoveOnPointEnd = value; }

        public bool IsPrefab { get; internal set; } = false;

		internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();
		public float Health { get; private set; } = 0f;
		public float MaxHealth { get; private set; } = 1f;

		public static readonly Color FullColor = Color.green;
		public static readonly Color EmptyColor = Color.clear;

		private const float AmountToHeal = 10f;
		private const float Delay = 0.4f;
		private float TimeUntilNextLocalHeal = 10f;

		private float CheckOOBTimer = 0f;
		private const float CheckOOBEvery = 1f;

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
																	playerPushMult: 100000f,
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
			this.TimeUntilNextLocalHeal = Mathf.Clamp(this.TimeUntilNextLocalHeal - TimeHandler.deltaTime, 0f, float.MaxValue);

			this.Renderer.color = Color.Lerp(EmptyColor, FullColor, this.Health / this.MaxHealth);

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
        private const string SyncedHealthKey = "HealthStation_Health";

		protected override void SetDataToSync()
		{
			this.SetSyncedFloat(SyncedHealthKey, this.Health);
		}
		protected override void ReadSyncedData()
		{
			// syncing
			this.Health = this.GetSyncedFloat(SyncedHealthKey, this.Health);
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
	}
}
