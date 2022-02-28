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
		private readonly static PlayerSkin DefaultHealthStationSkinColors = new PlayerSkin()
		{
			winText = Color.white,
			color = Color.green,
			backgroundColor = Color.black,
			particleEffect = Color.gray
        };

		private static GameObject _HealthStation = null;

		public static GameObject HealthStation
		{
			get
			{
				if (HealthStationPrefab._HealthStation == null)
				{

					GameObject healthStation = new GameObject("HealthStationPrefab", typeof(PhotonView), typeof(HealthStationHandler));
					ObjectParticleSkin.AddObjectParticleSkin(healthStation.transform, Sprites.Box, DefaultHealthStationSkinColors);

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

		public bool IsPrefab { get; internal set; } = false;

		internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();
		public float Health { get; private set; } = 0f;
		public float MaxHealth { get; private set; } = 1f;

		public static readonly Color FullColor = new Color32(0, 200, 0, 255);
		public static readonly Color EmptyColor = new Color32(100, 100, 100, 255);

		private const float AmountToHeal = 10f;
		private const float Delay = 0.4f;
		private float TimeUntilNextLocalHeal = 10f;

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
			this.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
			this.GetComponentInChildren<PlayerSkinParticle>(true).transform.localPosition = Vector3.zero;
			this.GetComponentInChildren<PlayerSkinParticle>(true).gameObject.SetActive(false);

			base.Start();

			this.Trig.radius = HealthStationHandler.TriggerRadius;
			this.Col.size = new Vector2(1f, 1f);
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
			this.Renderer.color = new Color32 (100, 100, 100, 255);
			//this.Renderer.enabled = false;
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
        private bool CanSeePlayer(Player player)
		{
			RaycastHit2D[] array = Physics2D.RaycastAll(this.transform.position, (player.data.playerVel.position - (Vector2)this.transform.position).normalized, Vector2.Distance(this.transform.position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform
					&& !array[i].transform.root.GetComponent<SpawnedAttack>()
					&& !array[i].transform.root.GetComponent<Player>()
					&& !array[i].transform.root.GetComponent<HealthStationHandler>()
					)
				{
					return false;
				}
			}
			return true;
		}
		protected override void Update()
        {
			this.TimeUntilNextLocalHeal = Mathf.Clamp(this.TimeUntilNextLocalHeal - TimeHandler.deltaTime, 0f, float.MaxValue);

            base.Update();

			this.Renderer.color = Color.Lerp(EmptyColor, FullColor, this.Health / this.MaxHealth);

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
