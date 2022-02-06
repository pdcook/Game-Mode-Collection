using GameModeCollection.GameModes;
using MapEmbiggener;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;

namespace GameModeCollection.Objects.GameModeObjects
{
    public static class CrownPrefab
	{
		private static GameObject _Crown = null;

		public static GameObject Crown
		{
			get
			{
				if (CrownPrefab._Crown == null)
				{

					GM_ArmsRace gm = GameModeManager.GetGameMode<GM_ArmsRace>(GameModeManager.ArmsRaceID);

					GameObject crown = GameObject.Instantiate(gm.gameObject.transform.GetChild(0).gameObject);
					GameModeCollection.Log("Crown Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(crown);
					crown.name = "CrownPrefab";
					// must add required components (PhotonView) first
					crown.AddComponent<PhotonView>();
					CrownHandler crownHandler = crown.AddComponent<CrownHandler>();
					crownHandler.transitionCurve = new AnimationCurve((Keyframe[])crown.GetComponent<GameCrownHandler>().transitionCurve.InvokeMethod("GetKeys"));

					UnityEngine.GameObject.DestroyImmediate(crown.GetComponent<GameCrownHandler>());

					PhotonNetwork.PrefabPool.RegisterPrefab(crown.name, crown);

					CrownPrefab._Crown = crown;
				}
				return CrownPrefab._Crown;
			}
		}


	}
	public class CrownHandler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		private static CrownHandler instance;

		private const float TriggerRadius = 1.5f;

		private const float MaxFreeTime = 20f;
		private const float MaxRespawns = 20;

		private const float FadeOutTime = 3f;
		private const float FadeInTime = 1f;

		private const float Bounciness = 0.2f;
		private const float Friction = 0.2f;
		private const float Mass = 500f;
		private const float MinAngularDrag = 0.1f;
		private const float MaxAngularDrag = 1f;
		private const float MinDrag = 0f;
		private const float MaxDrag = 5f;
		private const float MaxSpeed = 200f;
		private const float MaxAngularSpeed = 1000f;
		private const float PhysicsForceMult = 10f;
		private const float PhysicsImpulseMult = 0.001f;

		private bool hidden = true;
		private float crownPos;
		public AnimationCurve transitionCurve;
		private int currentCrownHolder = -1;
		private int previousCrownHolder = -1;
		internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();
		public int CrownHolder => this.currentCrownHolder;

		private int respawns = 0;
		private float fadeInTime = 0f;
		private float freeFor = 0f;
		private float _heldFor = 0f;
		public float HeldFor
		{
			get
			{
				return this._heldFor;
			}
			private set
			{
				this._heldFor = value;
			}
		}
		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] instantiationData = info.photonView.InstantiationData;

			this.gameObject.transform.SetParent(GM_CrownControl.instance.transform);
			GM_CrownControl.instance.SetCrown(this);
			CrownHandler.instance = this;
		}
        internal static void DestroyCrown()
        {
            GM_CrownControl.instance.DestroyCrown();
            if (CrownHandler.instance != null)
            {
                UnityEngine.GameObject.DestroyImmediate(CrownHandler.instance);
            }
        }
		internal static IEnumerator MakeCrownHandler()
		{
			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
					CrownPrefab.Crown.name,
					GM_CrownControl.instance.transform.position,
					GM_CrownControl.instance.transform.rotation,
					0
					);
			}

			yield return new WaitUntil(() => CrownHandler.instance != null);
		}
		protected override void Awake()
        {
			this.PhysicalProperties = new ItemPhysicalProperties(
				bounciness: CrownHandler.Bounciness,
				friction: CrownHandler.Friction,
				mass: CrownHandler.Mass,
				minAngularDrag: CrownHandler.MinAngularDrag,
				maxAngularDrag: CrownHandler.MaxAngularDrag,
				minDrag: CrownHandler.MinDrag,
				maxDrag: CrownHandler.MaxDrag,
				maxAngularSpeed: CrownHandler.MaxAngularSpeed,
				maxSpeed: CrownHandler.MaxSpeed,
				forceMult: CrownHandler.PhysicsForceMult,
				impulseMult: CrownHandler.PhysicsImpulseMult
				);

			base.Awake();
        }
		protected override void Start()
		{
			this.transform.localScale = Vector3.one;
			this.transform.GetChild(0).localScale = new Vector3(0.5f, 0.4f, 1f);

			base.Start();

			this.Trig.radius = CrownHandler.TriggerRadius;
            this.Col.size = new Vector2(1f, 0.5f);
            this.Col.edgeRadius = 0.1f;
		}

		public bool TooManyRespawns
		{
			get
			{
				return this.respawns >= CrownHandler.MaxRespawns;
			}
		}

        protected override bool SyncDataNow()
        {
			return !this.hidden;
        }

        public void Reset()
		{
			this.hidden = true;
			this.currentCrownHolder = -1;
			this.previousCrownHolder = -1;
			this.HeldFor = 0f;
			this.freeFor = 0f;
			this.respawns = 0;
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
			this.fadeInTime = 0f;
			this.SetPos(OutOfBoundsUtils.GetPoint(normalized_position));
			this.SetVel(Vector2.zero);
			this.SetRot(0f);
			this.SetAngularVel(0f);
		}

		public override void SetPos(Vector3 position)
		{
			this.GiveCrownToPlayer(-1);
			base.SetPos(position);
		}
		private Vector3 GetFarthestSpawnFromPlayers()
		{
			Vector3[] spawns = MapManager.instance.GetSpawnPoints().Select(s => s.localStartPos).ToArray();
			float dist = -1f;
			Vector3 best = Vector3.zero;
			foreach (Vector3 spawn in spawns)
			{
				float thisDist = PlayerManager.instance.players.Where(p => !p.data.dead).Select(p => Vector3.Distance(p.transform.position, spawn)).Sum();
				if (thisDist > dist)
				{
					dist = thisDist;
					best = spawn;
				}
			}
			return best;
		}

		protected internal override void OnCollisionEnter2D(Collision2D collision2D)
		{
			int? playerID = collision2D?.collider?.GetComponent<Player>()?.playerID;
			if (playerID != null)
			{
				this.GiveCrownToPlayer((int)playerID);
			}
			base.OnCollisionEnter2D(collision2D);
		}
        protected internal override void OnTriggerEnter2D(Collider2D collider2D)
        {
			int? playerID = collider2D?.GetComponent<Player>()?.playerID;
			if (playerID != null && this.CanSeePlayer(PlayerManager.instance.players.Find(p => p.playerID == playerID)))
			{
				this.GiveCrownToPlayer((int)playerID);
			}
            base.OnTriggerEnter2D(collider2D);
        }
		private bool CanSeePlayer(Player player)
        {
			RaycastHit2D[] array = Physics2D.RaycastAll(this.transform.position, (player.data.playerVel.position - (Vector2)this.transform.position).normalized, Vector2.Distance(this.transform.position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform
					&& !array[i].transform.root.GetComponent<SpawnedAttack>()
					&& !array[i].transform.root.GetComponent<Player>()
					&& !array[i].transform.GetComponentInParent<CrownHandler>()
					)
				{
					return false;
				}
			}
			return true;
        }
        protected override void Update()
		{
			if (this.transform.parent == null)
			{
				this.Rig.isKinematic = true;
				this.transform.position = 100000f * Vector2.up;
				this.currentCrownHolder = -1;
				this.previousCrownHolder = -1;
				return;
			}

			base.Update();

			if (this.currentCrownHolder != -1 || this.hidden)
			{
				this.HeldFor += TimeHandler.deltaTime;

				this.Rig.isKinematic = true;
				this.SetRot(0f);
				this.SetAngularVel(0f);
				this.Col.enabled = false;
				this.Trig.enabled = false;
				if (this.hidden) { this.SetPos(100000f * Vector2.up); }
				if (this.Renderer.color.a != 1f)
				{
					this.Renderer.color = new Color(this.Renderer.color.r, this.Renderer.color.g, this.Renderer.color.b, 1f);
				}
			}
			else
			{
				this.freeFor += TimeHandler.deltaTime;

				this.Rig.isKinematic = false;
				this.Col.enabled = true;
				this.Trig.enabled = true;
				// if the crown has gone OOB off the bottom of the map OR hasn't been touched in a long enough time, respawn it
				if ((!OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 normalizedPoint) && (normalizedPoint.y <= 0f)) || this.freeFor > CrownHandler.MaxFreeTime)
				{
					OutOfBoundsUtils.IsInsideBounds(GetFarthestSpawnFromPlayers(), out Vector3 newSpawn);
					this.Spawn(newSpawn);
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

				// update colors as necessary
				float a;
				if (this.fadeInTime < CrownHandler.FadeInTime)
				{
					a = UnityEngine.Mathf.Lerp(0f, 1f, this.fadeInTime / CrownHandler.FadeInTime);
					this.fadeInTime += TimeHandler.deltaTime;
				}
				else if (CrownHandler.MaxFreeTime >= this.freeFor && CrownHandler.MaxFreeTime - this.freeFor <= CrownHandler.FadeOutTime)
				{
					a = UnityEngine.Mathf.Lerp(0f, 1f, (CrownHandler.MaxFreeTime - this.freeFor) / CrownHandler.FadeOutTime);
				}
				else if (CrownHandler.MaxFreeTime >= this.freeFor)
				{
					a = 1f;
				}
				else
				{
					a = 0f;
				}
				this.Renderer.color = new Color(this.Renderer.color.r, this.Renderer.color.g, this.Renderer.color.b, a);

			}
		}

		void LateUpdate()
		{
			if (this.currentCrownHolder == -1 || this.previousCrownHolder == -1)
			{
				return;
			}
			Vector3 position = Vector3.LerpUnclamped((Vector3)PlayerManager.instance.players[this.previousCrownHolder].data.InvokeMethod("GetCrownPos"), (Vector3)PlayerManager.instance.players[this.currentCrownHolder].data.InvokeMethod("GetCrownPos"), this.crownPos);
			base.transform.position = position;
		}

		public void AddRandomAngularVelocity(float min = -CrownHandler.MaxAngularSpeed, float max = CrownHandler.MaxAngularSpeed)
        {
			if (this.View.IsMine) { this.View.RPC(nameof(RPCA_AddAngularVel), RpcTarget.All, UnityEngine.Random.Range(min, max)); }
        }
		[PunRPC]
		public void RPCA_AddAngularVel(float angVelToAdd)
        {
			this.Rig.angularVelocity += angVelToAdd;
        }

		public void GiveCrownToPlayer(int playerID)
		{
			if (this.View.IsMine && !this.hidden) { this.View.RPC(nameof(RPCA_GiveCrownToPlayer), RpcTarget.All, playerID); }
		}
		[PunRPC]
		private void RPCA_GiveCrownToPlayer(int playerID)
		{
			this.HeldFor = 0f;
			this.freeFor = 0f;
			this.previousCrownHolder = this.currentCrownHolder == -1 ? playerID : this.currentCrownHolder;
			this.currentCrownHolder = playerID;
			if (this.currentCrownHolder != -1 && !this.hidden) { base.StartCoroutine(this.IGiveCrownToPlayer()); }
		}
		private IEnumerator IGiveCrownToPlayer()
		{
			for (float i = 0f; i < this.transitionCurve.keys[this.transitionCurve.keys.Length - 1].time; i += Time.unscaledDeltaTime)
			{
				this.crownPos = Mathf.LerpUnclamped(0f, 1f, this.transitionCurve.Evaluate(i));
				yield return null;
			}
			yield break;
		}

		private const string SyncedRespawnsKey = "Crown_Respawns";
		private const string SyncedHeldForKey = "Crown_Held_For";
		private const string SyncedFreeForKey = "Crown_Free_For";

        protected override void SetDataToSync()
        {
			this.SetSyncedInt(SyncedRespawnsKey, this.respawns);
			this.SetSyncedFloat(SyncedHeldForKey, this.HeldFor);
			this.SetSyncedFloat(SyncedFreeForKey, this.freeFor);
        }
        protected override void ReadSyncedData()
        {
			// syncing
			this.respawns = this.GetSyncedInt(SyncedRespawnsKey, this.respawns);
			this.HeldFor = this.GetSyncedFloat(SyncedHeldForKey, this.HeldFor);
			this.freeFor = this.GetSyncedFloat(SyncedFreeForKey, this.freeFor);
        }
    }
}
