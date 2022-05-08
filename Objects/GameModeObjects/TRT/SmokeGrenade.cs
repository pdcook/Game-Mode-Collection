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
	public static class SmokeGrenadePrefab
	{
		private static GameObject _SmokeGrenade = null;

		public static GameObject SmokeGrenade
		{
			get
			{
				if (SmokeGrenadePrefab._SmokeGrenade == null)
				{

					// placeholder circle sprite
					GameObject smokeGrenade = new GameObject("SmokeGrenade", typeof(SpriteRenderer));
					smokeGrenade.GetComponent<SpriteRenderer>().sprite = Sprites.Circle;
                    smokeGrenade.GetComponent<SpriteRenderer>().color = new Color32(100, 100, 100, 255);
                    smokeGrenade.AddComponent<PhotonView>();
					smokeGrenade.AddComponent<SmokeGrenadeHandler>();
					smokeGrenade.name = "SmokeGrenadePrefab";

					smokeGrenade.GetComponent<SmokeGrenadeHandler>().IsPrefab = true;

					GameModeCollection.Log("SmokeGrenade Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(smokeGrenade);

					PhotonNetwork.PrefabPool.RegisterPrefab(smokeGrenade.name, smokeGrenade);

					SmokeGrenadePrefab._SmokeGrenade = smokeGrenade;
				}
				return SmokeGrenadePrefab._SmokeGrenade;
			}
		}
		private static GameObject _SmokeGrenadeExplosion = null;
		// TODO: replace shockwave effect with custom smokescreen effect
		public static GameObject SmokeGrenadeExplosion
        {
			get
            {
				if (SmokeGrenadePrefab._SmokeGrenadeExplosion is null)
                {
					_SmokeGrenadeExplosion = CardManager.cards.Values.Select(card => card.cardInfo).Where(card => card.cardName.ToLower() == "Shockwave".ToLower()).First().GetComponent<CharacterStatModifiers>().AddObjectToPlayer.GetComponent<SpawnObjects>().objectToSpawn[0];
                }
				return SmokeGrenadePrefab._SmokeGrenadeExplosion;
            }
        }
	}
	public class SmokeGrenadeHandler : NetworkPhysicsItem<CircleCollider2D, CircleCollider2D>
	{
		public const float AngularVelocityMult = 10f;
		public const float TotalFuseTime = 3f;
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
		internal static IEnumerator MakeSmokeGrenadeHandler(int placerID, Vector2 initial_velocity, Vector3 position, Quaternion rotation)
		{
			if ((PlayerManager.instance.GetPlayerWithID(placerID)?.data?.view?.IsMine ?? false) || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        SmokeGrenadePrefab.SmokeGrenade.name,
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
			this.PhysicalProperties = new ItemPhysicalProperties(mass: 10000f, bounciness: 0.55f,
														playerPushMult: 10000f,
														playerDamageMult: 0f,
														collisionDamageThreshold: float.MaxValue,
														friction: 0.7f,
														impulseMult: 1f,
														forceMult: 1f, visibleThroughShader: false);

			base.Awake();
		}
		protected override void Start()
		{
			base.Start();

			this.Exploded = false;
			this.FuseTimer = TotalFuseTime;

			this.Col.radius = 0.25f;

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

        private const string SyncedExplodedKey = "SmokeGrenade_Exploded";
        private const string SyncedTimeKey = "SmokeGrenade_Time";

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
			// spawn the shockwave explosion
			GameObject innerExplosionObj = GameObject.Instantiate(SmokeGrenadePrefab.SmokeGrenadeExplosion, this.transform.position, Quaternion.identity);
			Explosion innerExpl = innerExplosionObj.GetComponent<Explosion>();
			innerExplosionObj.GetOrAddComponent<SpawnedAttack>().spawner = PlayerManager.instance.GetPlayerWithID(this.PlacerID);

			innerExplosionObj.SetActive(true);

			Destroy(this.gameObject);
        }
    }
}
