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
using GameModeCollection.Utils;

namespace GameModeCollection.Objects.GameModeObjects.TRT
{
	public static class IncendiaryGrenadePrefabs
	{
		private static GameObject _IncendiaryGrenade = null;

		public static GameObject IncendiaryGrenade
		{
			get
			{
				if (IncendiaryGrenadePrefabs._IncendiaryGrenade == null)
				{

					// placeholder circle sprite
					GameObject incendiaryGrenade = new GameObject("IncendiaryGrenade", typeof(SpriteRenderer));
                    incendiaryGrenade.GetComponent<SpriteRenderer>().sprite = GameModeCollection.TRT_Assets.LoadAsset<Sprite>("TRT_Incendiary");
                    incendiaryGrenade.AddComponent<PhotonView>();
					incendiaryGrenade.AddComponent<IncendiaryGrenadeHandler>();
					incendiaryGrenade.name = "IncendiaryGrenadePrefab";

					incendiaryGrenade.GetComponent<IncendiaryGrenadeHandler>().IsPrefab = true;

					GameModeCollection.Log("IncendiaryGrenade Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(incendiaryGrenade);

					PhotonNetwork.PrefabPool.RegisterPrefab(incendiaryGrenade.name, incendiaryGrenade);

					IncendiaryGrenadePrefabs._IncendiaryGrenade = incendiaryGrenade;
				}
				return IncendiaryGrenadePrefabs._IncendiaryGrenade;
			}
		}
        private static GameObject _IncendiaryGrenadeFragment = null;

		public static GameObject IncendiaryGrenadeFragment
		{
			get
			{
				if (IncendiaryGrenadePrefabs._IncendiaryGrenadeFragment == null)
				{

					// placeholder circle sprite
					GameObject incendiaryGrenadeFragment = new GameObject("IncendiaryGrenadeFragment", typeof(SpriteRenderer));
					incendiaryGrenadeFragment.GetComponent<SpriteRenderer>().sprite = Sprites.Box;
                    incendiaryGrenadeFragment.GetComponent<SpriteRenderer>().color = new Color32(100, 0, 0, 255);
                    incendiaryGrenadeFragment.AddComponent<PhotonView>();
					incendiaryGrenadeFragment.AddComponent<IncendiaryGrenadeFragmentHandler>();
					incendiaryGrenadeFragment.name = "IncendiaryGrenadeFragmentPrefab";

					incendiaryGrenadeFragment.GetComponent<IncendiaryGrenadeFragmentHandler>().IsPrefab = true;

					GameModeCollection.Log("IncendiaryGrenadeFragment Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(incendiaryGrenadeFragment);

					PhotonNetwork.PrefabPool.RegisterPrefab(incendiaryGrenadeFragment.name, incendiaryGrenadeFragment);

					IncendiaryGrenadePrefabs._IncendiaryGrenadeFragment = incendiaryGrenadeFragment;
				}
				return IncendiaryGrenadePrefabs._IncendiaryGrenadeFragment;
			}
		}

        private static GameObject _IncendiaryGrenadeExplosion = null;
        public static GameObject IncendiaryGrenadeExplosion
        {
            get
            {
                if (IncendiaryGrenadePrefabs._IncendiaryGrenadeExplosion is null)
                {
                    _IncendiaryGrenadeExplosion = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("IncendiaryFlame"));
                    _IncendiaryGrenadeExplosion.GetOrAddComponent<IncendiaryAudioHandler>();
                    _IncendiaryGrenadeExplosion.transform.Find("Flames").gameObject.GetOrAddComponent<IncendiaryParticleHandler>();
                    _IncendiaryGrenadeExplosion.transform.Find("Trigger").gameObject.GetOrAddComponent<BurnHandler>();
                    _IncendiaryGrenadeExplosion.transform.Find("Trigger").gameObject.layer = PhysicsItem.TriggerLayer;
                    _IncendiaryGrenadeExplosion.SetActive(false);
                    _IncendiaryGrenadeExplosion.name = "IncendiaryGrenadeExplosion";

                    GameObject.DontDestroyOnLoad(_IncendiaryGrenadeExplosion);
                }
                return IncendiaryGrenadePrefabs._IncendiaryGrenadeExplosion;
            }
        }
    }
	public class IncendiaryGrenadeHandler : NetworkPhysicsItem<CircleCollider2D, CircleCollider2D>
	{
        public const float ScaleBy = 0.15f;
		public const float AngularVelocityMult = 10f;
		public const float TotalFuseTime = 3f;
        public const int Fragments = 5;
        public const float FragmentSpeed = 30f;
        public const float RandomFragDirXRatio = 2f;

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
		internal static IEnumerator MakeIncendiaryGrenadeHandler(int placerID, Vector2 initial_velocity, Vector3 position, Quaternion rotation)
		{
			if ((PlayerManager.instance.GetPlayerWithID(placerID)?.data?.view?.IsMine ?? false) || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        IncendiaryGrenadePrefabs.IncendiaryGrenade.name,
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
														impulseMult: 0.01f,
														forceMult: 0.01f, visibleThroughShader: false);

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

        private const string SyncedExplodedKey = "IncendiaryGrenade_Exploded";
        private const string SyncedTimeKey = "IncendiaryGrenade_Time";

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

            for (int _ = 0; _ < Fragments; _++)
            {
                Vector2 random_direction = new Vector2(RandomFragDirXRatio * UnityEngine.Random.Range(-1f, 1f), 1f);
                random_direction.Normalize();
                Vector2 random_velocity = FragmentSpeed * random_direction * UnityEngine.Random.Range(0.5f, 1f);
                GameModeCollection.instance.StartCoroutine(IncendiaryGrenadeFragmentHandler.MakeIncendiaryGrenadeFragmentHandler(this.PlacerID, random_velocity, this.transform.position, Quaternion.identity));
            }

            Destroy(this.gameObject);
        }
    }
    public class IncendiaryGrenadeFragmentHandler : NetworkPhysicsItem<CircleCollider2D, CircleCollider2D>
    {
        public const float AngularVelocityMult = 10f;
        public const float IgniteVolume = 1f;

        private SoundEvent IgniteSound = null;

        public override bool RemoveOnPointEnd { get => !this.IsPrefab; protected set => base.RemoveOnPointEnd = value; }
        public bool IsPrefab { get; internal set; } = false;
        public bool Exploded { get; private set; } = false;
        public int OwnerID { get; private set; } = -1;

        internal SpriteRenderer Renderer => this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // the instantiation data is the player ID and the initial velocity
            object[] data = info.photonView.InstantiationData;

            this.OwnerID = (int)data[0];

            Vector2 intialVelocity = (Vector2)data[1];

            this.SetVel(intialVelocity);
            this.SetAngularVel(-AngularVelocityMult * intialVelocity.x);

        }
        internal static IEnumerator MakeIncendiaryGrenadeFragmentHandler(int ownerID, Vector2 initial_velocity, Vector3 position, Quaternion rotation)
        {
            if ((PlayerManager.instance.GetPlayerWithID(ownerID)?.data?.view?.IsMine ?? false) || PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(
                        IncendiaryGrenadePrefabs.IncendiaryGrenadeFragment.name,
                        position,
                        rotation,
                        0,
                        new object[] { ownerID, initial_velocity }
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
                                                        friction: 0f,
                                                        impulseMult: 0.01f,
                                                        forceMult: 0.01f, visibleThroughShader: false);

            base.Awake();
        }
        protected override void Start()
        {
            base.Start();

            // load ignite sound
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("IncendiaryIgnite.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            this.IgniteSound = ScriptableObject.CreateInstance<SoundEvent>();
            this.IgniteSound.soundContainerArray[0] = soundContainer;

            // resize sprite
            this.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

            this.Exploded = false;

            this.Col.radius = 0.1f;

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
        private Vector2 GetCollisionSurfaceNormal(Collision2D collision)
        {
            // find collision point and normal. You may want to average over all contacts
            Vector2 point = collision.contacts[0].point;
            Vector2 dir = -collision.contacts[0].normal; // you need vector pointing TOWARDS the collision, not away from it
            // step back a bit
            point -= dir;

            // temporarily disable this object's collider(s) so we don't get them in the raycast
            this.Col.enabled = false;
            this.Trig.enabled = false;
            
            // cast a ray twice as far as your step back. This seems to work in all
            // situations, at least when speeds are not ridiculously big
            RaycastHit2D hitInfo = Physics2D.Raycast(point, dir, 2*dir.magnitude);
            if (hitInfo.collider is null)
            {
                GameModeCollection.LogError("[IncendiaryGrenadeFragmentHandler] Raycast failed!");
                return Vector2.up;
            }
            else
            {
                // this is the collider surface normal
                return hitInfo.normal;
            }
        }
        protected internal override void OnCollisionEnter2D(Collision2D collision2D)
        {
            // check if the collision was with a map object, if so, detonate
            if (!this.Exploded && this.View.IsMine && collision2D?.collider?.transform.root.GetComponent<Map>() != null)
            {
                Vector2 normal = collision2D.contacts[0].normal;
                this.View.RPC(nameof(RPCA_Explode), RpcTarget.All, this.GetCollisionSurfaceNormal(collision2D));
                this.Exploded = true;
            }

            base.OnCollisionEnter2D(collision2D);
        }        

        private const string SyncedExplodedKey = "IncendiaryGrenadeFragment_Exploded";

        protected override void SetDataToSync()
        {
            this.SetSyncedInt(SyncedExplodedKey, this.Exploded ? 1 : 0);
        }
        protected override void ReadSyncedData()
        {
            // syncing
            this.Exploded = this.GetSyncedInt(SyncedExplodedKey, this.Exploded ? 1 : 0) == 1;
        }
        protected override bool SyncDataNow()
        {
            return true;
        }
        [PunRPC]
        private void RPCA_Explode(Vector2 up)
        {
            this.Exploded = true;

            // play ignite sound
            //SoundManager.Instance.Play(this.IgniteSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * IgniteVolume) });

            GameObject expl = GameObject.Instantiate(IncendiaryGrenadePrefabs.IncendiaryGrenadeExplosion, this.transform.position, Quaternion.identity);
            expl.transform.up = up;
            expl.SetActive(true);
            BurnHandler burnHandler = expl.transform.Find("Trigger").GetComponent<BurnHandler>();
            burnHandler.SetOwnerID(this.OwnerID);
            burnHandler.SetDuration(expl.transform.Find("Flames").GetComponent<ParticleSystem>().main.duration);
            expl.transform.Find("Flames").GetComponent<ParticleSystem>().Play();

            Destroy(this.gameObject);
        }
    }
    public class IncendiaryParticleHandler : MonoBehaviour
    {
        ParticleSystem flames;
        void Start()
        {
            // ensure the stop action is set to Callback
            this.flames = this.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = this.flames.main;
            main.stopAction = ParticleSystemStopAction.Callback;

            // ensure the renderer has the correct sorting layer
            //this.flames.GetComponent<ParticleSystemRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
        }
        public void OnParticleSystemStopped()
        {
            GameModeCollection.Log("[IncendiaryParticleHandler] OnParticleSystemStopped");
            // this is called when the particle system stops
            GameObject.Destroy(this.transform.parent.gameObject, 5f);
        }
    }
    public class BurnHandler : MonoBehaviour
    {
        public const float BurnEvery = 0.2f;
        public const float BURNDAMAGE = 5f;
        public const float RepelFragmentStrength = 10f;
        public const float RepelFragmentStrengthMin = 1f;
        public const float RepelFragmentStrengthMax = 100f;

        private int OwnerID = -1;
        private float Duration = -1f;
        private TimeSince Timer = 0f;

        private Dictionary<int, TimeSince> playerBurnTimers = new Dictionary<int, TimeSince>();

        void Start()
        {
            this.Timer = 0f;

        }
        void OnDestroy()
        {
        }

        internal void SetOwnerID(int ownerID)
        {
            this.OwnerID = ownerID;
        }
        internal void SetDuration(float duration)
        {
            this.Duration = duration;
        }

        void Update()
        {

            if (this.Timer > this.Duration && this.Duration > 0f)
            {
                this.transform.parent.gameObject.GetComponent<IncendiaryAudioHandler>()?.FadeOut(1f);
                Destroy(this.gameObject);
                return;
            }

            foreach (Player player in PlayerManager.instance.players.ToList().Where(p => this.playerBurnTimers.ContainsKey(p.playerID)))
            {
                if (this.playerBurnTimers[player.playerID] > BurnEvery)
                {
                    this.DoBurnDamage(player);
                }
            }
        }

        void PlayerEnterFlame(Player player)
        {
            if (!this.playerBurnTimers.ContainsKey(player.playerID))
            {
                this.playerBurnTimers.Add(player.playerID, new TimeSince());
                this.playerBurnTimers[player.playerID] = 0f;
                this.DoBurnDamage(player);
            }
        }
        void PlayerExitFlame(Player player)
        {
            this.playerBurnTimers.Remove(player.playerID);
        }
        void DoBurnDamage(Player player)
        {
            if (this.playerBurnTimers.ContainsKey(player.playerID))
            {
                this.playerBurnTimers[player.playerID] = 0f;
                if (player.data.view.IsMine)
                {
                    player.data.healthHandler.CallTakeDamage(BURNDAMAGE * Vector2.up, player.transform.position, this.transform.parent.gameObject, this.OwnerID == -1 ? null : PlayerManager.instance.GetPlayerWithID(this.OwnerID), true);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collider2D)
        {
            Player player = collider2D?.GetComponent<Player>();
            if (player != null)
            {
                this.PlayerEnterFlame(player);
            }
            else
            {
                IncendiaryGrenadeFragmentHandler fragment = collider2D?.transform?.root.GetComponent<IncendiaryGrenadeFragmentHandler>();
                if (fragment != null)
                {
                    // fragments should bounce off of flames

                    // distance from fragment to bottom of flame
                    Vector2 fragPos = fragment.transform.position;
                    Vector2 flameCenter = this.transform.position;
                    Vector2 flamePlane = this.transform.right;
                    float sqDistance = ((fragPos - flameCenter) - (Vector2.Dot((fragPos - flameCenter), flamePlane)) * flamePlane).sqrMagnitude;

                    fragment.SetVel(fragment.Rig.velocity + (Vector2)this.transform.up * UnityEngine.Mathf.Clamp(RepelFragmentStrength / sqDistance, RepelFragmentStrengthMin, RepelFragmentStrengthMax));

                }
            }
        }
        void OnTriggerStay2D(Collider2D collider2D)
        {
            Player player = collider2D?.GetComponent<Player>();
            if (player != null)
            {
                this.PlayerEnterFlame(player);
            }
            else
            {
                IncendiaryGrenadeFragmentHandler fragment = collider2D?.transform?.root.GetComponent<IncendiaryGrenadeFragmentHandler>();
                if (fragment != null)
                {
                    // fragments should bounce off of flames

                    // distance from fragment to bottom of flame
                    Vector2 fragPos = fragment.transform.position;
                    Vector2 flameCenter = this.transform.position;
                    Vector2 flamePlane = this.transform.right;
                    float sqDistance = ((fragPos - flameCenter) - (Vector2.Dot((fragPos - flameCenter), flamePlane)) * flamePlane).sqrMagnitude;

                    fragment.SetVel(fragment.Rig.velocity + (Vector2)this.transform.up * UnityEngine.Mathf.Clamp(RepelFragmentStrength / sqDistance, RepelFragmentStrengthMin, RepelFragmentStrengthMax));

                }
            }
        }
        void OnTriggerExit2D(Collider2D collider2D)
        {
            Player player = collider2D?.GetComponent<Player>();
            if (player != null)
            {
                this.PlayerExitFlame(player);
            }
        }
    }
    public class IncendiaryAudioHandler : MonoBehaviour
    {
        public const float Volume = 0.5f;

        Player Player = null;
        AudioSource[] audioSources = null;
        bool fade = false;
        TimeSince fadeTimer;
        float fadeTime = 1f;

        internal void FadeOut(float fadeTime)
        {
            this.fade = true;
            this.fadeTimer = 0f;
            this.fadeTime = fadeTime;
        }

        void Start()
        {
            this.Player = PlayerManager.instance.GetLocalPlayer();
            this.audioSources = this.GetComponents<AudioSource>();
            if (this.audioSources is null || this.audioSources.Length == 0)
            {
                GameModeCollection.LogError("IncendiaryAudioHandler: No audio sources found!");
                Destroy(this);
                return;
            }
            foreach (AudioSource audioSource in this.audioSources)
            {
                GMCAudio.ApplyToAudioSource(audioSource, vol: Volume, fadeOut: this.fade, fadeTimer: this.fadeTimer, fadeDuration: this.fadeTime);
                audioSource.Play();
            }
        }
        void Update()
        {
            if (this.audioSources is null) { return; }
            foreach (AudioSource audioSource in this.audioSources)
            {
                GMCAudio.ApplyToAudioSource(audioSource, vol: Volume, fadeOut: this.fade, fadeTimer: this.fadeTimer, fadeDuration: this.fadeTime);
            }
        }

    }
}
