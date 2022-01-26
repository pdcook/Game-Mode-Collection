using GameModeCollection.GameModes;
using MapEmbiggener;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using System.Reflection;

namespace GameModeCollection.Objects.GameModeObjects
{
    public static class DeathObjectPrefabs
	{

		public static int Layer => LayerMask.NameToLayer("Player");
		public static int SortingLayerID => SortingLayer.NameToID("Player0");

		private readonly static PlayerSkin DefaultObjectColors = new PlayerSkin()
		{
			winText = Color.white,
			color = Color.white,
			backgroundColor = Color.black,
			particleEffect = Color.white
		};

		private static GameObject _DeathBall = null;

		public static GameObject DeathBall
		{
			get
			{
				if (DeathObjectPrefabs._DeathBall == null)
				{
					GameObject deathBall = new GameObject("DeathBallPrefab");
					GameObject deathBallArt = GameObject.Instantiate(PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject, deathBall.transform);
					deathBallArt.name = "Sprite";
					deathBallArt.gameObject.layer = Layer;
					deathBallArt.transform.localScale = Vector3.one;
					deathBallArt.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
					deathBallArt.GetComponent<SpriteRenderer>().sprite = Sprites.Circle;
					deathBallArt.GetComponent<SpriteMask>().sprite = Sprites.Circle;

					PlayerSkin skin = ((PlayerSkinBank)typeof(PlayerSkinBank).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null)).skins[0].currentPlayerSkin;
					PlayerSkin newSkin = GameObject.Instantiate(skin, deathBall.transform).gameObject.GetComponent<PlayerSkin>();
					newSkin.gameObject.layer = Layer;
					newSkin.gameObject.name = "DeathBallSkin";
					newSkin.transform.localScale = Vector3.one;
					UnityEngine.GameObject.DontDestroyOnLoad(newSkin);
					newSkin.color = DefaultObjectColors.color;
					newSkin.backgroundColor = DefaultObjectColors.backgroundColor;
					newSkin.winText = DefaultObjectColors.winText;
					newSkin.particleEffect = DefaultObjectColors.particleEffect;
					PlayerSkinParticle newSkinPart = newSkin.GetComponentInChildren<PlayerSkinParticle>();
					ParticleSystem part = newSkinPart.GetComponent<ParticleSystem>();
					ParticleSystem.MainModule main = part.main;
					ParticleSystem.MinMaxGradient startColor = main.startColor;
					startColor.colorMin = DefaultObjectColors.backgroundColor;
					startColor.colorMax = DefaultObjectColors.color;
					main.startColor = startColor;

					newSkinPart.SetFieldValue("startColor1", DefaultObjectColors.backgroundColor);
					newSkinPart.SetFieldValue("startColor2", DefaultObjectColors.color);

					GameModeCollection.Log("Deathball Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(deathBall);

					// must add required components (PhotonView) first
					deathBall.AddComponent<PhotonView>();
					deathBall.AddComponent<DeathBall>();
					deathBall.AddComponent<SetSpriteLayer>();

					PhotonNetwork.PrefabPool.RegisterPrefab(deathBall.name, deathBall);

					DeathObjectPrefabs._DeathBall = deathBall;
				}
				return DeathObjectPrefabs._DeathBall;
			}
		}
		private static GameObject _DeathBox = null;

		public static GameObject DeathBox
		{
			get
			{
				if (DeathObjectPrefabs._DeathBox == null)
				{
					GameObject deathBox = new GameObject("DeathBoxPrefab");
					GameObject deathBoxArt = GameObject.Instantiate(PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject, deathBox.transform);
					deathBoxArt.name = "Sprite";
					deathBoxArt.gameObject.layer = Layer;
					deathBoxArt.transform.localScale = Vector3.one;
					deathBoxArt.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
					deathBoxArt.GetComponent<SpriteRenderer>().sprite = Sprites.Box;
					deathBoxArt.GetComponent<SpriteMask>().sprite = Sprites.Box;

					PlayerSkin skin = ((PlayerSkinBank)typeof(PlayerSkinBank).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null)).skins[0].currentPlayerSkin;
					PlayerSkin newSkin = GameObject.Instantiate(skin, deathBox.transform).gameObject.GetComponent<PlayerSkin>();
					newSkin.gameObject.layer = Layer;
					newSkin.gameObject.name = "DeathBoxSkin";
					newSkin.transform.localScale = Vector3.one;
					UnityEngine.GameObject.DontDestroyOnLoad(newSkin);
					newSkin.color = DefaultObjectColors.color;
					newSkin.backgroundColor = DefaultObjectColors.backgroundColor;
					newSkin.winText = DefaultObjectColors.winText;
					newSkin.particleEffect = DefaultObjectColors.particleEffect;
					PlayerSkinParticle newSkinPart = newSkin.GetComponentInChildren<PlayerSkinParticle>();
					ParticleSystem part = newSkinPart.GetComponent<ParticleSystem>();
					ParticleSystem.MainModule main = part.main;
					ParticleSystem.MinMaxGradient startColor = main.startColor;
					startColor.colorMin = DefaultObjectColors.backgroundColor;
					startColor.colorMax = DefaultObjectColors.color;
					main.startColor = startColor;

					newSkinPart.SetFieldValue("startColor1", DefaultObjectColors.backgroundColor);
					newSkinPart.SetFieldValue("startColor2", DefaultObjectColors.color);

					GameModeCollection.Log("DeathBox Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(deathBox);

					// must add required components (PhotonView) first
					deathBox.AddComponent<PhotonView>();
					deathBox.AddComponent<DeathBox>();
					deathBox.AddComponent<SetSpriteLayer>();

					PhotonNetwork.PrefabPool.RegisterPrefab(deathBox.name, deathBox);

					DeathObjectPrefabs._DeathBox = deathBox;
				}
				return DeathObjectPrefabs._DeathBox;
			}
		}
		private static GameObject _DeathRod = null;

		public static GameObject DeathRod
		{
			get
			{
				if (DeathObjectPrefabs._DeathRod == null)
				{
					GameObject deathRod = new GameObject("DeathRodPrefab");
					GameObject deathRodArt = GameObject.Instantiate(PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject, deathRod.transform);
					deathRodArt.name = "Sprite";
					deathRodArt.gameObject.layer = Layer;
					deathRodArt.transform.localScale = Vector3.one;
					deathRodArt.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
					deathRodArt.GetComponent<SpriteRenderer>().sprite = Sprites.Box;
					deathRodArt.GetComponent<SpriteMask>().sprite = Sprites.Box;

					PlayerSkin skin = ((PlayerSkinBank)typeof(PlayerSkinBank).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null)).skins[0].currentPlayerSkin;
					PlayerSkin newSkin = GameObject.Instantiate(skin, deathRod.transform).gameObject.GetComponent<PlayerSkin>();
					newSkin.gameObject.layer = Layer;
					newSkin.gameObject.name = "DeathRodSkin";
					newSkin.transform.localScale = Vector3.one;
					UnityEngine.GameObject.DontDestroyOnLoad(newSkin);
					newSkin.color = DefaultObjectColors.color;
					newSkin.backgroundColor = DefaultObjectColors.backgroundColor;
					newSkin.winText = DefaultObjectColors.winText;
					newSkin.particleEffect = DefaultObjectColors.particleEffect;
					PlayerSkinParticle newSkinPart = newSkin.GetComponentInChildren<PlayerSkinParticle>();
					ParticleSystem part = newSkinPart.GetComponent<ParticleSystem>();
					ParticleSystem.MainModule main = part.main;
					ParticleSystem.MinMaxGradient startColor = main.startColor;
					startColor.colorMin = DefaultObjectColors.backgroundColor;
					startColor.colorMax = DefaultObjectColors.color;
					main.startColor = startColor;

					newSkinPart.SetFieldValue("startColor1", DefaultObjectColors.backgroundColor);
					newSkinPart.SetFieldValue("startColor2", DefaultObjectColors.color);

					GameModeCollection.Log("DeathRod Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(deathRod);

					// must add required components (PhotonView) first
					deathRod.AddComponent<PhotonView>();
					deathRod.AddComponent<DeathRod>();
					deathRod.AddComponent<SetSpriteLayer>();

					PhotonNetwork.PrefabPool.RegisterPrefab(deathRod.name, deathRod);

					DeathObjectPrefabs._DeathRod = deathRod;
				}
				return DeathObjectPrefabs._DeathRod;
			}
		}
	}
	static class Sprites
    {
		public static Sprite Circle => PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite;
		public static Sprite Box => PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite;
	}
	class SetSpriteLayer : MonoBehaviour
    {
		void Start()
        {
			int layerID = SortingLayer.NameToID("Player0");
			this.SetSpriteLayerOfChildren(this.gameObject, layerID);
			this.InitParticles(this.gameObject.GetComponentsInChildren<PlayerSkinParticle>(), layerID);
        }
		private void SetSpriteLayerOfChildren(GameObject obj, int layer)
		{
			SpriteMask[] sprites = obj.GetComponentsInChildren<SpriteMask>();
			for (int i = 0; i < sprites.Length; i++)
			{
                sprites[i].frontSortingLayerID = layer;
                sprites[i].backSortingLayerID = layer;
			}
		}
		private void InitParticles(PlayerSkinParticle[] parts, int layer)
        {
			foreach (PlayerSkinParticle skinpart in parts)
            {
				ParticleSystem part = skinpart.GetComponent<ParticleSystem>();
				skinpart.SetFieldValue("part", part);
				part.GetComponent<ParticleSystemRenderer>().sortingLayerID = layer;
				ParticleSystem.MainModule main = part.main;
				skinpart.SetFieldValue("main", main);
				skinpart.SetFieldValue("startColor1", main.startColor.colorMin);
				skinpart.SetFieldValue("startColor2", main.startColor.colorMax);
				part.Play();
			}
        }
	}

	public static class DeathObjectConstants
    {
		public const float MaxFreeTime = 20f;
		public const float MaxRespawns = 20;

		public const float Bounciness = 1f;
		public const float Friction = 0.2f;
		public const float Mass = 500f;
		public const float MinAngularDrag = 0.1f;
		public const float MaxAngularDrag = 1f;
		public const float MinDrag = 0f;
		public const float MaxDrag = 5f;
		public const float MaxSpeed = 200f;
		public const float MaxAngularSpeed = 1000f;
		public const float PhysicsForceMult = 1f;
		public const float PhysicsImpulseMult = 1f;

    }
	public class DeathBall : DeathObjectHandler<CircleCollider2D>
    {
		private static DeathBall instance;

		private const float Radius = 1f;

		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] instantiationData = info.photonView.InstantiationData;

			this.gameObject.transform.SetParent(GM_Dodgeball.instance.transform);
			GM_Dodgeball.instance.SetDeathObject(this);
			DeathBall.instance = this;
		}
		protected override void Start()
        {
			base.Start();

			this.Col.radius = DeathBall.Radius;
        }
		internal static IEnumerator MakeDeathBall()
		{
			GM_Dodgeball.instance.DestroyDeathBall();
			if (DeathBall.instance != null)
			{
				UnityEngine.GameObject.DestroyImmediate(DeathBall.instance);
			}

			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
					DeathObjectPrefabs.DeathBall.name,
					GM_Dodgeball.instance.transform.position,
					GM_Dodgeball.instance.transform.rotation,
					0
					);
			}

			yield return new WaitUntil(() => DeathBall.instance != null);
		}
    }
	public class DeathBox : DeathObjectHandler<BoxCollider2D>
    {
		private static DeathBox instance;
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] instantiationData = info.photonView.InstantiationData;

			this.gameObject.transform.SetParent(GM_Dodgeball.instance.transform);
			GM_Dodgeball.instance.SetDeathObject(this);
			DeathBox.instance = this;
		}
		internal static IEnumerator MakeDeathBox()
		{
			GM_Dodgeball.instance.DestroyDeathBox();
			if (DeathBox.instance != null)
			{
				UnityEngine.GameObject.DestroyImmediate(DeathBox.instance);
			}

			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
					DeathObjectPrefabs.DeathBox.name,
					GM_Dodgeball.instance.transform.position,
					GM_Dodgeball.instance.transform.rotation,
					0
					);
			}

			yield return new WaitUntil(() => DeathBox.instance != null);
		}
    }
	public class DeathRod : DeathObjectHandler<BoxCollider2D>
    {
		private static DeathRod instance;
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] instantiationData = info.photonView.InstantiationData;

			this.gameObject.transform.SetParent(GM_Dodgeball.instance.transform);
			GM_Dodgeball.instance.SetDeathObject(this);
			DeathRod.instance = this;
		}
		internal static IEnumerator MakeDeathRod()
		{
			GM_Dodgeball.instance.DestroyDeathRod();
			if (DeathRod.instance != null)
			{
				UnityEngine.GameObject.DestroyImmediate(DeathRod.instance);
			}

			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
					DeathObjectPrefabs.DeathRod.name,
					GM_Dodgeball.instance.transform.position,
					GM_Dodgeball.instance.transform.rotation,
					0
					);
			}

			yield return new WaitUntil(() => DeathRod.instance != null);
		}
    }

	public abstract class DeathObjectHandler<TCollision> : NetworkPhysicsItem<TCollision, CircleCollider2D> where TCollision : Collider2D
	{

		private Vector2? _previousSpawn = null;

		public Vector2 PreviousSpawn
        {
			get
            {
				if (this._previousSpawn != null)
                {
					return (Vector2)this._previousSpawn;
                }
				else
                {
					return GM_Dodgeball.ballSpawn;
                }
            }
			private set
            {
				this._previousSpawn = value;
            }
        }

		private bool hidden = true;
		internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();
		private int respawns = 0;
		private float freeFor = 0f;

		protected override void Awake()
        {
			this._PhysicalProperties = new ItemPhysicalProperties(
				bounciness: DeathObjectConstants.Bounciness,
				friction: DeathObjectConstants.Friction,
				mass: DeathObjectConstants.Mass,
				minAngularDrag: DeathObjectConstants.MinAngularDrag,
				maxAngularDrag: DeathObjectConstants.MaxAngularDrag,
				minDrag: DeathObjectConstants.MinDrag,
				maxDrag: DeathObjectConstants.MaxDrag,
				maxAngularSpeed: DeathObjectConstants.MaxAngularSpeed,
				maxSpeed: DeathObjectConstants.MaxSpeed,
				forceMult: DeathObjectConstants.PhysicsForceMult,
				impulseMult: DeathObjectConstants.PhysicsImpulseMult
				);

			base.Awake();
        }
		protected override void Start()
		{
			this.transform.localScale = Vector3.one;
			this.transform.GetChild(0).localScale = 2f*Vector3.one;
			this.transform.GetChild(1).localScale = Vector3.one;
			this.transform.GetChild(1).localPosition = Vector3.zero; 

			base.Start();

			this.Trig.enabled = false;
		}

		public bool TooManyRespawns
		{
			get
			{
				return this.respawns >= DeathObjectConstants.MaxRespawns;
			}
		}

		public void Reset()
		{
			this.hidden = true;
			this.freeFor = 0f;
			this.respawns = 0;
			this._previousSpawn = null;
		}

		/// <summary>
		/// takes in a NORMALIZED vector between (0,0,0) and (1,1,0) which represents the percentage across the bounds on each axis
		/// </summary>
		/// <param name="normalized_position"></param>
		public void Spawn(Vector3 normalized_position)
		{
			if (this.TooManyRespawns)
			{
				this.hidden = true;
				return;
			}
			this.hidden = false;
			this.freeFor = 0f;
			this.SetPos(OutOfBoundsUtils.GetPoint(normalized_position));
			this.SetVel(Vector2.zero);
			this.SetRot(0f);
			this.SetAngularVel(0f);
			this.PreviousSpawn = normalized_position;
		}
        protected override void RPCA_SendForce(Vector2 force, Vector2 point, byte forceMode = (byte)ForceMode2D.Force)
        {
			this.freeFor = 0f;
            base.RPCA_SendForce(force, point, forceMode);
        }
        protected override void Update()
		{
			if (this.transform.parent == null || this.hidden)
			{
				this.Rig.isKinematic = true;
				this.transform.position = 100000f * Vector2.up;
				return;
			}

			base.Update();

            this.freeFor += TimeHandler.deltaTime;

            this.Rig.isKinematic = false;
            this.Col.enabled = true;
            this.Trig.enabled = true;
            // if the object has gone OOB off the bottom of the map OR hasn't been touched in a long enough time, respawn it
            if ((!OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 normalizedPoint) && (normalizedPoint.y <= 0f)) || this.freeFor > DeathObjectConstants.MaxFreeTime)
            {
                this.Spawn(this.PreviousSpawn);
                this.respawns++;
            }
            // if it has gone off the sides, have it bounce back in
            if (normalizedPoint.x <= 0f)
            {
                this.Rig.velocity = new Vector2(UnityEngine.Mathf.Abs(this.Rig.velocity.x), this.Rig.velocity.y);
            }
            else if (normalizedPoint.x >= 1f)
            {
                this.Rig.velocity = new Vector2(-UnityEngine.Mathf.Abs(this.Rig.velocity.x), this.Rig.velocity.y);
            }
		}

		private const string SyncedRespawnsKey = "DeathObject_Respawns";
		private const string SyncedFreeForKey = "DeathObject_Free_For";

        protected override void SetDataToSync()
        {
			this.SetSyncedData(SyncedRespawnsKey, (int)this.respawns);
			this.SetSyncedData(SyncedFreeForKey, (float)this.freeFor);
        }
        protected override void ReadSyncedData()
        {
			// syncing
			this.respawns = this.GetSyncedData<int>(SyncedRespawnsKey, this.respawns);
			this.freeFor = this.GetSyncedData<float>(SyncedFreeForKey, this.freeFor);
        }

		public enum Shapes
        {
			Circle,
			Square,
			Rod,
			Triangle,
			Star,
			Random
        }
    }
}
