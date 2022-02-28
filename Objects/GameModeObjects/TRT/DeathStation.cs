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
		private readonly static PlayerSkin DefaultDeathStationSkinColors = new PlayerSkin()
		{
			winText = Color.white,
			color = Color.green,
			backgroundColor = Color.black,
			particleEffect = Color.gray
        };

		private static GameObject _DeathStation = null;

		public static GameObject DeathStation
		{
			get
			{
				if (DeathStationPrefab._DeathStation == null)
				{

					GameObject healthStation = new GameObject("DeathStationPrefab", typeof(PhotonView), typeof(DeathStationHandler));
					ObjectParticleSkin.AddObjectParticleSkin(healthStation.transform, Sprites.Box, DefaultDeathStationSkinColors);

					healthStation.GetComponent<DeathStationHandler>().IsPrefab = true;

					GameModeCollection.Log("DeathStation Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(healthStation);

					PhotonNetwork.PrefabPool.RegisterPrefab(healthStation.name, healthStation);

					DeathStationPrefab._DeathStation = healthStation;
				}
				return DeathStationPrefab._DeathStation;
			}
		}


	}
	public class DeathStationHandler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		private const float TriggerRadius = 2f;

		public bool IsPrefab { get; internal set; } = false;

		internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();

		public Player Placer { get; private set; } = null;

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

			this.Trig.radius = DeathStationHandler.TriggerRadius;
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
			this.Renderer.color = HealthStationHandler.FullColor;
			//this.Renderer.enabled = false;
		}

 		protected internal override void OnTriggerStay2D(Collider2D collider2D)
		{
            Player player = collider2D?.GetComponent<Player>();
            if (player != null && player.data.view.IsMine && this.CanSeePlayer(player) && player.data.playerActions.InteractIsPressed())
            {
				player.data.view.RPC("RPCA_Die", RpcTarget.All, Vector2.up);

			}
			base.OnTriggerStay2D(collider2D);
		}
        private bool CanSeePlayer(Player player)
		{
			RaycastHit2D[] array = Physics2D.RaycastAll(this.transform.position, (player.data.playerVel.position - (Vector2)this.transform.position).normalized, Vector2.Distance(this.transform.position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform
					&& !array[i].transform.root.GetComponent<SpawnedAttack>()
					&& !array[i].transform.root.GetComponent<Player>()
					&& !array[i].transform.root.GetComponent<DeathStationHandler>()
					)
				{
					return false;
				}
			}
			return true;
		}

		protected override void SetDataToSync()
		{
		}
		protected override void ReadSyncedData()
		{
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
    }
}
