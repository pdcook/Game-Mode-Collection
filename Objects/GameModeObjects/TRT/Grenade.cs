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
using GameModeCollection.Objects;

namespace GameModeCollection.Objects.GameModeObjects.TRT
{
	public static class GrenadePrefab
	{
		private static GameObject _Grenade = null;

		public static GameObject Grenade
		{
			get
			{
				if (GrenadePrefab._Grenade == null)
				{

					// placeholder circle sprite
					GameObject grenade = new GameObject("Grenade", typeof(SpriteRenderer));
                    grenade.GetComponent<SpriteRenderer>().sprite = GameModeCollection.TRT_Assets.LoadAsset<Sprite>("TRT_Grenade");
                    grenade.AddComponent<PhotonView>();
					grenade.AddComponent<GrenadeHandler>();
					grenade.name = "GrenadePrefab";

					grenade.GetComponent<GrenadeHandler>().IsPrefab = true;

					GameModeCollection.Log("Grenade Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(grenade);

					PhotonNetwork.PrefabPool.RegisterPrefab(grenade.name, grenade);

					GrenadePrefab._Grenade = grenade;
				}
				return GrenadePrefab._Grenade;
			}
		}
		private static GameObject _GrenadeExplosion = null;
		public static GameObject GrenadeExplosion
        {
			get
            {
				if (GrenadePrefab._GrenadeExplosion is null)
                {
					_GrenadeExplosion = CardManager.cards.Values.Select(card => card.cardInfo).Where(card => card.cardName.ToLower() == "EXPLOSIVE BULLET".ToLower()).First().GetComponent<Gun>().objectsToSpawn[0].effect;
                }
				return GrenadePrefab._GrenadeExplosion;
            }
        }
	}
	public class GrenadeHandler : NetworkPhysicsItem<CircleCollider2D, CircleCollider2D>
	{
        public const float ScaleBy = 0.25f;
		public const float AngularVelocityMult = 10f;
		public const float TotalFuseTime = 3f;
		public const float ExplosionDamage = 100f;
		public const float ExplosionRange = 15f;

        public override bool RemoveOnPointEnd { get => !this.IsPrefab; protected set => base.RemoveOnPointEnd = value; }
        public bool IsPrefab { get; internal set; } = false;
		public float FuseTimer { get; private set; } = TotalFuseTime;
		public bool Exploded { get; private set; } = false;
		public int PlacerID { get; private set; } = -1;

		internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
            // the instantiation data is the player ID and the initial velocity
            object[] data = info.photonView.InstantiationData;

			this.PlacerID = (int)data[0];

            Vector2 intialVelocity = (Vector2)data[1];

            this.SetVel(intialVelocity);
            this.SetAngularVel(-AngularVelocityMult * intialVelocity.x);

        }
		internal static IEnumerator MakeGrenadeHandler(int placerID, Vector2 initial_velocity, Vector3 position, Quaternion rotation)
		{
			if ((PlayerManager.instance.GetPlayerWithID(placerID)?.data?.view?.IsMine ?? false) || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        GrenadePrefab.Grenade.name,
                        position,
                        rotation,
                        0,
                        new object[] { placerID, initial_velocity }
					);
			}

			yield break;
		}
		protected override void Awake()
		{
			this.PhysicalProperties = new ItemPhysicalProperties(mass: 10000f, bounciness: 0.25f,
														playerPushMult: 10000f,
														playerDamageMult: 0f,
														collisionDamageThreshold: float.MaxValue,
														friction: 0.9f,
                                                        minAngularDrag: 1f,
                                                        maxAngularDrag: 100f,
														impulseMult: 1f,
														forceMult: 1f, visibleThroughShader: false);

			base.Awake();
		}
		protected override void Start()
		{
			base.Start();

			this.Exploded = false;
			this.FuseTimer = TotalFuseTime;

            this.transform.localScale = Vector3.one * ScaleBy;

            this.Col.radius = 0.25f/ScaleBy;

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
		protected override void Update()
        {
            this.FuseTimer -= TimeHandler.deltaTime;
            if (!this.Exploded && this.FuseTimer <= 0f && this.View.IsMine)
            {
                this.Exploded = true;
				this.View.RPC(nameof(RPCA_Explode), RpcTarget.All);
            }
            base.Update();
		}

        private const string SyncedExplodedKey = "Grenade_Exploded";
        private const string SyncedTimeKey = "Grenade_Time";

		protected override void SetDataToSync()
		{
			this.SetSyncedFloat(SyncedTimeKey, this.FuseTimer);
            this.SetSyncedInt(SyncedExplodedKey, this.Exploded ? 1 : 0);
        }
		protected override void ReadSyncedData()
		{
			// syncing
			this.FuseTimer = this.GetSyncedFloat(SyncedTimeKey, this.FuseTimer);
            this.Exploded = this.GetSyncedInt(SyncedExplodedKey, this.Exploded ? 1 : 0) == 1;
        }
        protected override bool SyncDataNow()
        {
			return true;
        }
		[PunRPC]
		private void RPCA_Explode()
        {
			this.Exploded = true;
			GameObject explosionObj = GameObject.Instantiate(GrenadePrefab.GrenadeExplosion, this.transform.position, Quaternion.identity);
			Explosion explosion = explosionObj.GetComponent<Explosion>();
			explosionObj.GetOrAddComponent<SpawnedAttack>().spawner = PlayerManager.instance.GetPlayerWithID(this.PlacerID);

			explosion.ignoreTeam = false;
			explosion.ignoreWalls = false;
			explosion.scaleDmg = false;
			explosion.scaleForce = false;
			explosion.scaleRadius = false;
			explosion.scaleSilence = false;
			explosion.scaleSlow = false;
			explosion.scaleStun = false;
			explosion.auto = true;

			explosion.damage = ExplosionDamage;
			explosion.range = ExplosionRange;

			explosionObj.SetActive(true);

			Destroy(this.gameObject);
        }
    }
}
