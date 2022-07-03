using GameModeCollection.GameModes;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameModeCollection.Extensions;
using UnboundLib;
using UnboundLib.Utils;
using GameModeCollection.Objects;
using System.Linq;
using Sonigon;
using Sonigon.Internal;

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
                    smokeGrenade.GetComponent<SpriteRenderer>().sprite = GameModeCollection.TRT_Assets.LoadAsset<Sprite>("TRT_SmokeGrenade");
                    //smokeGrenade.GetComponent<SpriteRenderer>().color = new Color32(100, 100, 100, 255);
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
		public static GameObject SmokeGrenadeExplosion
        {
			get
            {
				if (SmokeGrenadePrefab._SmokeGrenadeExplosion is null)
                {
					_SmokeGrenadeExplosion = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("Smoke"));
                    _SmokeGrenadeExplosion.GetComponentInChildren<ParticleSystem>().gameObject.GetOrAddComponent<SmokeParticleHandler>();
                    _SmokeGrenadeExplosion.GetOrAddComponent<SmokeHandler>();
                    _SmokeGrenadeExplosion.SetActive(false);
                    _SmokeGrenadeExplosion.name = "SmokeGrenadeExplosion";

                    GameObject.DontDestroyOnLoad(_SmokeGrenadeExplosion);
                }
				return SmokeGrenadePrefab._SmokeGrenadeExplosion;
            }
        }
	}
    public class SmokeParticleHandler : MonoBehaviour
    {
        ParticleSystem smoke;
        void Start()
        {
            // ensure the stop action is set to Callback
            this.smoke = this.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = this.smoke.main;
            main.stopAction = ParticleSystemStopAction.Callback;

            // ensure the renderer has the correct sorting layer
            this.smoke.GetComponent<ParticleSystemRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
        }
        public void OnParticleSystemStopped()
        {
            // this is called when the particle system stops
            this.GetComponentInParent<SmokeHandler>().OnSmokeStop();
        }
    }
    public class SmokeHandler : MonoBehaviour
    {
        private float duration = -1;
        private bool smokeEnding = false;
        private TimeSince timeSinceStart = 0f;

        void Start()
        {
            this.smokeEnding = false;
            this.timeSinceStart = 0f;
            this.duration = this.GetComponentInChildren<ParticleSystem>().main.duration;
            // ensure the collider is on the correct layer to not interact with players but to block visibility
            this.transform.Find("Collider").gameObject.layer = LayerMask.NameToLayer("Corpse");
        }
        void Update()
        {
            if (!this.smokeEnding)
            {
                if (this.timeSinceStart > this.duration)
                {
                    this.smokeEnding = true;
                    this.OnSmokeEnding();
                }
            }
        }
        public void OnSmokeEnding()
        {
            this.transform.Find("Collider").gameObject.SetActive(false);
        }

        public void OnSmokeStop()
        {
            Destroy(this.gameObject);

        }

    }
	public class SmokeGrenadeHandler : NetworkPhysicsItem<CircleCollider2D, CircleCollider2D>
	{
        public const float ScaleBy = 0.125f;
        public const float SmokeVolume = 1f;
		public const float AngularVelocityMult = 10f;
		public const float TotalFuseTime = 3f;
        private SoundEvent SmokeSound = null;
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

            // load smoke sound
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("SmokeGrenade.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            this.SmokeSound = ScriptableObject.CreateInstance<SoundEvent>();
            this.SmokeSound.soundContainerArray[0] = soundContainer;

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

            // play sound
            SoundManager.Instance.Play(this.SmokeSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * SmokeVolume) });

            // spawn smoke
			GameObject smokeObj = GameObject.Instantiate(SmokeGrenadePrefab.SmokeGrenadeExplosion, this.transform.position, Quaternion.identity);
            

            smokeObj.SetActive(true);
            smokeObj.GetComponentInChildren<ParticleSystem>().Play();

			Destroy(this.gameObject);
        }
    }
}
