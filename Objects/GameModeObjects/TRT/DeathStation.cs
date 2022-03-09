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
	public static class DeathStationPrefab
	{
		private static GameObject _DeathStation = null;

		public static GameObject DeathStation
		{
			get
			{
				if (DeathStationPrefab._DeathStation == null)
				{

					GameObject deathStation = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_HealthStation"));
					deathStation.AddComponent<PhotonView>();
					deathStation.AddComponent<DeathStationHandler>();
					deathStation.name = "DeathStationPrefab";

					deathStation.GetComponent<DeathStationHandler>().IsPrefab = true;

					GameModeCollection.Log("DeathStation Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(deathStation);

					PhotonNetwork.PrefabPool.RegisterPrefab(deathStation.name, deathStation);

					DeathStationPrefab._DeathStation = deathStation;
				}
				return DeathStationPrefab._DeathStation;
			}
		}


	}
	public class DeathStationHandler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		private const float TriggerRadius = 2f;
        public override bool RemoveOnPointEnd { get => !this.IsPrefab; protected set => base.RemoveOnPointEnd = value; }
		public bool IsPrefab { get; internal set; } = false;

		internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();

		public bool HasKilledPlayer { get; private set; } = false;

		public Player Placer { get; private set; } = null;
		private float CheckOOBTimer = 0f;
		private const float CheckOOBEvery = 1f;

		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] data = info.photonView.InstantiationData;

			this.Placer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == (int)data[0]);
		}
		internal static IEnumerator MakeDeathStationHandler(int placerID, Vector3 position, Quaternion rotation)
		{
			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        DeathStationPrefab.DeathStation.name,
                        position,
                        rotation,
                        0,
                        new object[] { placerID }
					);
			}

			yield break;
		}
		internal static IEnumerator AskHostToMakeDeathStation(int placerID, Vector3 position, Quaternion rotation)
        {
			NetworkingManager.RPC(typeof(DeathStationHandler), nameof(RPCM_MakeDeathStationHandler), placerID, position, rotation);
			yield break;
        }
		[UnboundRPC]
		private static void RPCM_MakeDeathStationHandler(int placerID, Vector3 position, Quaternion rotation)
        {
			GameModeCollection.instance.StartCoroutine(MakeDeathStationHandler(placerID, position, rotation));	
        }
		protected override void Awake()
		{
			this.PhysicalProperties = new ItemPhysicalProperties(mass: 80000f, bounciness: 0f,
																	playerPushMult: 1000f,
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

			this.Trig.radius = DeathStationHandler.TriggerRadius;
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
			this.Renderer.color = this.HasKilledPlayer ? Color.red : HealthStationHandler.FullColor;
		}

 		protected internal override void OnTriggerStay2D(Collider2D collider2D)
		{
            Player player = collider2D?.GetComponent<Player>();
            if (player != null && player.data.view.IsMine && this.CanSeePlayer(player) && player.data.playerActions.InteractIsPressed())
            {
				player.data.view.RPC("RPCA_Die", RpcTarget.All, Vector2.up);
				this.View.RPC(nameof(RPCA_SetKilledPlayer), RpcTarget.All, true);

			}
			base.OnTriggerStay2D(collider2D);
		}
		[PunRPC]
		private void RPCA_SetKilledPlayer(bool killedPlayer)
        {
			this.HasKilledPlayer = killedPlayer;
        }
        protected override void Update()
        {
			this.Renderer.color = this.HasKilledPlayer ? Color.red : HealthStationHandler.FullColor;
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

        protected override void SetDataToSync()
		{
			this.SetSyncedInt("TRT_DeathStation_HasKilled", this.HasKilledPlayer ? 1 : 0);
		}
		protected override void ReadSyncedData()
		{
			this.HasKilledPlayer = this.GetSyncedInt("TRT_DeathStation_HasKilled") == 1;
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
    }
}
