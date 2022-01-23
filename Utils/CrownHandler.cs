using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using UnboundLib.GameModes;
using MapEmbiggener;
using System.Linq;
using GameModeCollection.GameModes;
using Photon.Pun;
using Sonigon;

namespace GameModeCollection.Utils.UI
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
					crown.AddComponent<PhotonView>();
					UnityEngine.GameObject.DontDestroyOnLoad(crown);
                    crown.name = "CrownPrefab";
                    CrownHandler crownHandler = crown.AddComponent<CrownHandler>();
                    crownHandler.transitionCurve = new AnimationCurve((Keyframe[])crown.GetComponent<GameCrownHandler>().transitionCurve.InvokeMethod("GetKeys"));
                    Rigidbody2D rig = crownHandler.gameObject.AddComponent<Rigidbody2D>();
                    BoxCollider2D bCol = crownHandler.gameObject.AddComponent<BoxCollider2D>();
                    bCol.size = new Vector2(1f, 0.5f);
                    bCol.edgeRadius = 0.1f;

                    UnityEngine.GameObject.DestroyImmediate(crown.GetComponent<GameCrownHandler>());

					PhotonNetwork.PrefabPool.RegisterPrefab(crown.name, crown);

					CrownPrefab._Crown = crown;
                }
				return CrownPrefab._Crown;
            }
        }

    }
	[RequireComponent(typeof(PhotonView))]
	public class CrownHandler : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable
	{

		private readonly int sendFreq = 5;
		private int currentFrame = 5;
		private float lastTime = 0f;
		private List<ObjectSyncPackage> syncPackages = new List<ObjectSyncPackage>();

		private static CrownHandler instance;

		private const float MaxFreeTime = 20f;

		private const float FadeOutTime = 3f;
		private const float FadeInTime = 1f;

		private const float Mass = 0.1f;
		private const float MinAngularDrag = 0.1f;
		private const float MaxAngularDrag = 1f;
		private const float MinDrag = 0.1f;
		private const float MaxDrag = 5f;
		private const float MaxSpeed = 100f;
		private static readonly float MaxSpeedSqr = CrownHandler.MaxSpeed*CrownHandler.MaxSpeed;
		private const float MaxAngularSpeed = 100f;

		private bool hidden = true;
		private float crownPos;
		public AnimationCurve transitionCurve;
		private int currentCrownHolder = -1;
		private int previousCrownHolder = -1;
		internal Rigidbody2D Rig => this.GetComponent<Rigidbody2D>();
		internal BoxCollider2D Col => this.GetComponent<BoxCollider2D>();
		internal PhotonView View => this.GetComponent<PhotonView>();
		internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();
		//internal NetworkPhysicsObject NetPhys => this.GetComponent<NetworkPhysicsObject>();
		public int CrownHolder => this.currentCrownHolder;

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
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
			object[] instantiationData = info.photonView.InstantiationData;

			this.gameObject.transform.SetParent(GM_CrownControl.instance.transform);
			GM_CrownControl.instance.SetCrown(this);
			CrownHandler.instance = this;
        }

		internal static IEnumerator MakeCrownHandler()
		{
			GM_CrownControl.instance.DestroyCrown();
			if (CrownHandler.instance != null)
            {
				UnityEngine.GameObject.DestroyImmediate(CrownHandler.instance);
            }

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
		void Awake()
        {
			this.currentFrame = UnityEngine.Random.Range(0, this.sendFreq);
        }
		void Start()
		{
			this.transform.localScale = Vector3.one;
			this.transform.GetChild(0).localScale = new Vector3(0.5f, 0.4f, 1f);

			this.Rig.drag = CrownHandler.MinDrag;
			this.Rig.angularDrag = CrownHandler.MinAngularDrag;
			this.Rig.mass = CrownHandler.Mass;

			//if (this.View) { this.View.ObservedComponents.Add(this.NetPhys); }
			if (this.View) { this.View.ObservedComponents.Add(this); }

		}

		public void Reset()
		{
			this.hidden = true;
			this.currentCrownHolder = -1;
			this.previousCrownHolder = -1;
			this.HeldFor = 0f;
			this.freeFor = 0f;
		}

		/// <summary>
		/// takes in a NORMALIZED vector between (0,0,0) and (1,1,0) which represents the percentage across the bounds on each axis
		/// </summary>
		/// <param name="normalized_position"></param>
		public void Spawn(Vector3 normalized_position)
		{
			this.hidden = false;
			this.fadeInTime = 0f;
			this.SetPos(OutOfBoundsUtils.GetPoint(normalized_position));
			this.SetVel(Vector2.zero);
			this.SetRot(0f);
			this.SetAngularVel(0f);
		}

		public void SetPos(Vector3 position)
		{
			this.GiveCrownToPlayer(-1);
			this.transform.position = position;
		}
		public void SetVel(Vector2 velocity)
		{
			this.Rig.velocity = velocity;
		}

		public void SetAngularVel(float angularVelocity)
		{
			this.Rig.angularVelocity = angularVelocity;
		}

		public void SetRot(float rot)
		{
			this.Rig.rotation = rot;
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

		void OnCollisionEnter2D(Collision2D collision2D)
		{
			int? playerID = collision2D?.collider?.GetComponent<Player>()?.playerID;
			if (playerID != null)
			{
				this.GiveCrownToPlayer((int)playerID);
			}
		}

		public void TakeForce(Vector2 point, Vector2 force)
        {
			if (this.View.IsMine || PhotonNetwork.OfflineMode) { this.View.RPC(nameof(this.RPCA_TakeForce), RpcTarget.All, point, force); }	
        }
		[PunRPC]
		void RPCA_TakeForce(Vector2 point, Vector2 force)
        {
			if (this.Rig.velocity.sqrMagnitude < CrownHandler.MaxSpeedSqr)
            {
				this.Rig.velocity += force / this.Rig.mass;
            }
			if (this.Rig.angularVelocity < CrownHandler.MaxAngularSpeed)
            {
				Vector2 r = point - this.Rig.position;
				float torque = Vector3.Cross(r, force).z;
				// moment of inertia of a rectangular plate at about a perpendicular axis passing through the point r, relative to the COM
				float I = this.Rig.mass * ((1.25f) / 12f + r.sqrMagnitude);

				this.Rig.angularVelocity += torque / I;
            }
        }

		void FixedUpdate()
        {
			this.Rig.drag = UnityEngine.Mathf.LerpUnclamped(CrownHandler.MinDrag, CrownHandler.MaxDrag, this.Rig.velocity.sqrMagnitude / CrownHandler.MaxSpeedSqr);
			this.Rig.angularDrag = UnityEngine.Mathf.LerpUnclamped(CrownHandler.MinAngularDrag, CrownHandler.MaxAngularDrag, UnityEngine.Mathf.Abs(this.Rig.angularVelocity) / CrownHandler.MaxAngularSpeed);
        }

		void Update()
		{
			if (this.transform.parent == null)
            {
				this.Rig.isKinematic = true;
				this.transform.position = 100000f * Vector2.up;
				this.currentCrownHolder = -1;
				this.previousCrownHolder = -1;
				return;
            }

			if (this.currentCrownHolder != -1 || this.hidden)
			{
				this.HeldFor += TimeHandler.deltaTime;

				this.Rig.isKinematic = true;
				this.SetRot(0f);
				this.SetAngularVel(0f);
				this.Col.enabled = false;
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
				// if the crown has gone OOB off the bottom of the map OR hasn't been touched in a long enough time, respawn it
				if ((!OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 normalizedPoint) && (normalizedPoint.y <= 0f)) || this.freeFor > CrownHandler.MaxFreeTime)
				{
					OutOfBoundsUtils.IsInsideBounds(GetFarthestSpawnFromPlayers(), out Vector3 newSpawn);
					this.Spawn(newSpawn);
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
				float a = 1f;
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

				// syncing
				if (this.syncPackages.Count > 0)
				{
					if (this.syncPackages[0].timeDelta > 0f)
					{
						this.syncPackages[0].timeDelta -= Time.deltaTime * 1.5f * (1f + (float)this.syncPackages.Count * 0.5f);
					}
					else
					{
						if (this.syncPackages.Count > 2)
						{
							this.syncPackages.RemoveAt(0);
						}
						this.transform.position = this.syncPackages[0].pos;
						this.transform.rotation = Quaternion.LookRotation(Vector3.forward, this.syncPackages[0].rot);
						this.Rig.velocity = this.syncPackages[0].vel;
						this.Rig.angularVelocity = this.syncPackages[0].angularVel;
						this.syncPackages.RemoveAt(0);
					}
				}
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
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
			this.currentFrame++;
			if (stream.IsWriting)
			{
				if (this.currentFrame >= this.sendFreq)
				{
					this.currentFrame = 0;
					stream.SendNext((Vector2)this.transform.position);
					stream.SendNext((Vector2)this.transform.up);
					stream.SendNext(this.Rig.velocity);
					stream.SendNext(this.Rig.angularVelocity);
					if (this.lastTime == 0f)
					{
						this.lastTime = Time.time;
					}
					stream.SendNext(Time.time - this.lastTime);
					this.lastTime = Time.time;
					return;
				}
			}
			else
			{
				ObjectSyncPackage objectSyncPackage = new ObjectSyncPackage();
				objectSyncPackage.pos = (Vector2)stream.ReceiveNext();
				objectSyncPackage.rot = (Vector2)stream.ReceiveNext();
				objectSyncPackage.vel = (Vector2)stream.ReceiveNext();
				objectSyncPackage.angularVel = (float)stream.ReceiveNext();
				objectSyncPackage.timeDelta = (float)stream.ReceiveNext();
				this.syncPackages.Add(objectSyncPackage);
			}
		}
    }
}
