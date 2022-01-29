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
using System;

namespace GameModeCollection.Objects
{
	public class ItemPhysicalProperties
	{
		private const float DefaultBounciness = 0.2f;
		private const float DefaultFriction = 0.2f;
		private const float DefaultMass = 500f;
		private const float DefaultMinAngularDrag = 0.1f;
		private const float DefaultMaxAngularDrag = 1f;
		private const float DefaultMinDrag = 0f;
		private const float DefaultMaxDrag = 5f;
		private const float DefaultMaxSpeed = 200f;
		private const float DefaultMaxAngularSpeed = 1000f;
		private const float DefaultPhysicsForceMult = 1f;
		private const float DefaultPhysicsImpulseMult = 1f;
		private const float DefaultThrusterDurationMult = 1f;

		public readonly float Bounciness;
		public readonly float Friction;
		public readonly float Mass;
		public readonly float Density;
		public readonly float MinAngularDrag;
		public readonly float MaxAngularDrag;
		public readonly float MinDrag;
		public readonly float MaxDrag;
		public readonly float MaxSpeed;
		public readonly float MaxAngularSpeed;
		public readonly float PhysicsForceMult;
		public readonly float PhysicsImpulseMult;
		public readonly float ThrusterDurationMult;
		public float MaxSpeedSqr => this.MaxSpeed * this.MaxSpeed;

		public ItemPhysicalProperties(
			float bounciness = DefaultBounciness,
			float friction = DefaultFriction,
			float mass = DefaultMass,
			float minAngularDrag = DefaultMinAngularDrag,
			float maxAngularDrag = DefaultMaxAngularDrag,
			float minDrag = DefaultMinDrag,
			float maxDrag = DefaultMaxDrag,
			float maxAngularSpeed = DefaultMaxAngularSpeed,
			float maxSpeed = DefaultMaxSpeed,
			float forceMult = DefaultPhysicsForceMult,
			float impulseMult = DefaultPhysicsImpulseMult,
			float thrusterDurationMult = DefaultThrusterDurationMult)
		{
			this.Bounciness = bounciness;
			this.Friction = friction;
			this.Mass = mass;
			this.MinAngularDrag = minAngularDrag;
			this.MaxAngularDrag = maxAngularDrag;
			this.MinDrag = minDrag;
			this.MaxDrag = maxDrag;
			this.MaxSpeed = maxSpeed;
			this.MaxAngularSpeed = maxAngularSpeed;
			this.PhysicsForceMult = forceMult;
			this.PhysicsImpulseMult = impulseMult;
			this.ThrusterDurationMult = thrusterDurationMult;
		}

	}
	public abstract class PhysicsItem : MonoBehaviour
	{
		public abstract ItemPhysicalProperties PhysicalProperties { get; }
		public static readonly int Layer = LayerMask.NameToLayer("PlayerObjectCollider"); // PlayerObjectCollider layer (layer 19)
		private PhotonView View => this.gameObject.GetComponent<PhotonView>();
		protected internal abstract void OnCollisionEnter2D(Collision2D collision2D);
		protected internal abstract void OnCollisionExit2D(Collision2D collision2D);
		protected internal abstract void OnCollisionStay2D(Collision2D collision2D);
		protected internal abstract void OnTriggerEnter2D(Collider2D collider2D);
		protected internal abstract void OnTriggerExit2D(Collider2D collider2D);
		protected internal abstract void OnTriggerStay2D(Collider2D collider2D);
		protected internal virtual void CallTakeForce(Vector2 force, Vector2 point, ForceMode2D forceMode = ForceMode2D.Force)
		{
			this.View?.RPC(nameof(RPCA_SendForce), RpcTarget.All, force, point, (byte)forceMode);
		}
		[PunRPC]
		protected abstract void RPCA_SendForce(Vector2 force, Vector2 point, byte forceMode);

	}
	[RequireComponent(typeof(PhotonView))]
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class NetworkPhysicsItem<TCollider, TTrigger> : PhysicsItem, IPunInstantiateMagicCallback, IPunObservable where TCollider : Collider2D where TTrigger : Collider2D
	{

		protected const string SyncedPositionKey = "__pos__";
		protected const string SyncedRotationKey = "__rot__";
		protected const string SyncedVelocityKey = "__vel__";
		protected const string SyncedAngularVelocityKey = "__angularVel__";

		protected ItemPhysicalProperties _PhysicalProperties = new ItemPhysicalProperties();
        public override ItemPhysicalProperties PhysicalProperties => this._PhysicalProperties;

        private PhysicsMaterial2D _Material = null;
		public PhysicsMaterial2D Material
		{
			get
			{
				if (this._Material == null)
				{
					this._Material = new PhysicsMaterial2D
					{
						bounciness = this.PhysicalProperties.Bounciness,
						friction = this.PhysicalProperties.Friction
					};
				}

				return this._Material;
			}
		}
		private Dictionary<string, object> _dataToSync = new Dictionary<string, object>() { };
		protected string[] GetSyncedKeys()
		{
			return this._dataToSync.Keys.ToArray();
		}
		protected object GetSyncedData(string key, object default_value = default)
		{
			if (this._dataToSync.TryGetValue(key, out object value))
			{
				return value;
			}
			else
			{
				GameModeCollection.LogWarning($"Key \"{key}\" not found in syncing data of NetworkPhysicsItem component of {this.name}.");
			}
			return default_value;
		}
		protected T GetSyncedData<T>(string key, T default_value = default)
		{
			if (this._dataToSync.TryGetValue(key, out object value))
			{
				try
				{
					return (T)value;
				}
				catch (InvalidCastException)
				{
					GameModeCollection.LogWarning($"InvalidCastException in NetworkPhysicsItem component of {this.name}: {value.ToString()} cannot be cast to type {typeof(T).ToString()}");
				}
			}
			else
			{
				GameModeCollection.LogWarning($"Key \"{key}\" not found in syncing data of NetworkPhysicsItem component of {this.name}.");
			}
			return default_value;
		}
		protected void SetSyncedData<T>(string key, T value)
		{
			this._dataToSync[key] = (object)value;
		}
		protected void SetSyncedData(string key, object value)
		{
			this._dataToSync[key] = value;
		}

		protected virtual int sendFreq
		{
			get
            {
				return 5;
            }
		}
		private int _currentFrame = 5;
		protected int CurrentFrame
		{
			get
			{
				return this._currentFrame;
			}
			private set
			{
				this._currentFrame = value;
			}
		}
		private float _lastTime = 0f;
		protected float LastTime
		{
			get
			{
				return this._lastTime;
			}
			private set
			{
				this._lastTime = value;
			}
		}

		private float _timeDelta = 0f;
		protected float TimeDelta
		{
			get
			{
				return this._timeDelta;
			}
			set
			{
				this._timeDelta = value;
			}
		}

		private List<ItemSyncPackage> syncPackages = new List<ItemSyncPackage>();

		public Rigidbody2D Rig => this.GetComponent<Rigidbody2D>();
		public TCollider Col => this.transform?.Find("Collider")?.GetComponent<TCollider>();
		public TTrigger Trig => this.transform?.Find("Trigger")?.GetComponent<TTrigger>();
		public PhotonView View => this.GetComponent<PhotonView>();

		public abstract void OnPhotonInstantiate(PhotonMessageInfo info);

		protected virtual void Awake()
		{
			this.CurrentFrame = UnityEngine.Random.Range(0, this.sendFreq);
			if (this.Col == null)
			{
				GameObject collider = new GameObject("Collider", typeof(TCollider), typeof(ItemTriggerAndCollision));
				collider.transform.SetParent(this.transform);
				collider.transform.localScale = Vector3.one;
			}
			if (this.Trig == null)
			{
				GameObject trigger = new GameObject("Trigger", typeof(TTrigger), typeof(ItemTriggerAndCollision));
				trigger.transform.SetParent(this.transform);
				trigger.transform.localScale = Vector3.one;
			}
		}
		protected virtual void Start()
		{
			this.Col.GetComponent<ItemTriggerAndCollision>().SetAsTrigger(false);
			this.Trig.GetComponent<ItemTriggerAndCollision>().SetAsTrigger(true);

			this.Rig.drag = this.PhysicalProperties.MinDrag;
			this.Rig.angularDrag = this.PhysicalProperties.MinAngularDrag;
			this.Rig.mass = this.PhysicalProperties.Mass;

			this.Col.sharedMaterial = this.Material;
			this.Trig.sharedMaterial = this.Material;
			this.Rig.sharedMaterial = this.Material;

			this.SetSyncedData<Vector2>(SyncedPositionKey, (Vector2)this.transform.position);
			this.SetSyncedData<Vector2>(SyncedRotationKey, (Vector2)this.transform.up);
			this.SetSyncedData<Vector2>(SyncedVelocityKey, (Vector2)this.Rig.velocity);
			this.SetSyncedData<float>(SyncedAngularVelocityKey, (float)this.Rig.angularVelocity);

			this.gameObject.layer = PhysicsItem.Layer;

			if (this.View != null && this.View.ObservedComponents != null) { this.View.ObservedComponents.Add(this); }

		}
		public virtual void SetPos(Vector3 position)
		{
			this.transform.position = position;
		}
		public virtual void SetVel(Vector2 velocity)
		{
			this.Rig.velocity = velocity;
		}

		public virtual void SetAngularVel(float angularVelocity)
		{
			this.Rig.angularVelocity = angularVelocity;
		}

		public virtual void SetRot(float rot)
		{
			this.Rig.rotation = rot;
		}
		protected internal override void OnCollisionEnter2D(Collision2D collision2D)
		{
			ProjectileCollision projCol = collision2D?.collider?.GetComponent<ProjectileCollision>();
			if (projCol != null && projCol?.transform?.parent?.GetComponent<ProjectileHit>() != null && (projCol.transform.parent.GetComponentInChildren<PhotonView>().IsMine || PhotonNetwork.OfflineMode))
			{
				Vector2 point = (Vector2)projCol.transform.position;
				Vector2 force = projCol.gameObject.GetComponentInParent<ProjectileHit>().force * (Vector2)projCol.transform.parent.forward;
				this.View.RPC(nameof(this.RPCA_DoBulletHit), RpcTarget.All, projCol.transform.parent.GetComponentInChildren<PhotonView>().ViewID, point, force, (Vector2)(-projCol.transform.forward));
			}
		}
		protected internal override void OnCollisionExit2D(Collision2D collision2D)
		{

		}
		protected internal override void OnCollisionStay2D(Collision2D collision2D)
		{

		}
		protected internal override void OnTriggerEnter2D(Collider2D collider2D)
		{

		}
		protected internal override void OnTriggerExit2D(Collider2D collider2D)
		{

		}
		protected internal override void OnTriggerStay2D(Collider2D collider2D)
		{

		}
		[PunRPC]
		protected override void RPCA_SendForce(Vector2 force, Vector2 point, byte forceMode)
		{
			ForceMode2D forceMode2D = (ForceMode2D)forceMode;
			float mult = 1f;
			if (forceMode2D == ForceMode2D.Force) { mult = this.PhysicalProperties.PhysicsForceMult; }
			if (forceMode2D == ForceMode2D.Impulse) { mult = this.PhysicalProperties.PhysicsImpulseMult; }
				this.Rig.AddForceAtPosition(force * mult, point, forceMode2D);
		}
		[PunRPC]
		protected virtual void RPCA_DoBulletHit(int viewID, Vector2 point, Vector2 force, Vector2 normal)
		{
			this.RPCA_SendForce(force, point, (byte)ForceMode2D.Impulse);
			this.StartCoroutine(this.DoBulletHitWhenReady(viewID, point, normal));
		}
		protected virtual IEnumerator DoBulletHitWhenReady(int viewID, Vector2 point, Vector2 normal)
		{
			yield return new WaitUntil(() => PhotonNetwork.GetPhotonView(viewID) != null);
			GameObject bullet = PhotonNetwork.GetPhotonView(viewID)?.gameObject;
			ProjectileHit projHit = bullet?.GetComponent<ProjectileHit>();
			ProjectileCollision projCol = bullet?.GetComponentInChildren<ProjectileCollision>();
			if (bullet == null || projHit == null || projCol == null)
			{
				yield break;
			}
            HitInfo hitInfo = new HitInfo()
            {
                collider = this.Col,
                transform = this.transform,
                rigidbody = this.Rig,
                point = point,
                normal = normal,
            };
			if (projHit.isAllowedToSpawnObjects)
			{
				GamefeelManager.GameFeel(projHit.transform.forward * projHit.shake);
				DynamicParticles.instance.PlayBulletHit(projHit.damage, projHit.transform, hitInfo, projHit.projectileColor);
				for (int i = 0; i < projHit.objectsToSpawn.Length; i++)
				{
					ObjectsToSpawn.SpawnObject(projHit.transform, hitInfo, projHit.objectsToSpawn[i], null, projHit.team, projHit.damage, (SpawnedAttack)projHit.GetFieldValue("spawnedAttack"), false);
				}
				projHit.transform.position = hitInfo.point + hitInfo.normal * 0.01f;
			}
			bool flag = false;
			if (projHit.effects != null && projHit.effects.Count() != 0)
            {
				for (int j = 0; j < projHit.effects.Count; j++)
				{
					HasToReturn hasToReturn = projHit.effects[j].DoHitEffect(hitInfo);
					if (hasToReturn == HasToReturn.hasToReturn)
					{
						flag = true;
					}
					if (hasToReturn == HasToReturn.hasToReturnNow)
					{
						yield break;
					}
				}
			}
			if (flag) { yield break; }
			projCol.Die();
		}

		protected virtual void FixedUpdate()
		{
			this.Rig.drag = UnityEngine.Mathf.LerpUnclamped(this.PhysicalProperties.MinDrag, this.PhysicalProperties.MaxDrag, this.Rig.velocity.sqrMagnitude / this.PhysicalProperties.MaxSpeedSqr);
			this.Rig.angularDrag = UnityEngine.Mathf.LerpUnclamped(this.PhysicalProperties.MinAngularDrag, this.PhysicalProperties.MaxAngularDrag, UnityEngine.Mathf.Abs(this.Rig.angularVelocity) / this.PhysicalProperties.MaxAngularSpeed);
		}

		protected virtual void Update()
		{
			// syncing
			if (this.syncPackages.Count > 0)
			{
				this.TimeDelta = this.syncPackages[0].timeDelta;
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
					// read all synced data
					this._dataToSync = this.syncPackages[0].syncedData;
					this.syncPackages.RemoveAt(0);

					// update physics
					this.ReadSyncedPhysicsData();

					// update all other data
					this.ReadSyncedData();

				}
			}
		}
		private void SetPhysicsDataToSync()
		{
			this.SetSyncedData(SyncedPositionKey, (Vector2)this.transform.position);
			this.SetSyncedData(SyncedRotationKey, (Vector2)this.transform.up);
			this.SetSyncedData(SyncedVelocityKey, (Vector2)this.Rig.velocity);
			this.SetSyncedData(SyncedAngularVelocityKey, (float)this.Rig.angularVelocity);
		}
		private void ReadSyncedPhysicsData()
		{
			// update physics
			if (!this.Rig.isKinematic)
			{
				this.transform.position = this.GetSyncedData<Vector2>(SyncedPositionKey);
				this.transform.rotation = Quaternion.LookRotation(Vector3.forward, this.GetSyncedData<Vector2>(SyncedRotationKey));
				this.Rig.velocity = this.GetSyncedData<Vector2>(SyncedVelocityKey);
				this.Rig.angularVelocity = this.GetSyncedData<float>(SyncedAngularVelocityKey);
			}
		}
		protected abstract void SetDataToSync();
		protected abstract void ReadSyncedData();
		protected abstract bool SyncDataNow();
		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (!this.SyncDataNow()) { return; }
			this.CurrentFrame++;
			if (stream.IsWriting)
			{
				if (this.CurrentFrame >= this.sendFreq)
				{
					this.CurrentFrame = 0;
					this.SetPhysicsDataToSync();
					this.SetDataToSync();
					stream.SendNext(this._dataToSync);
					// timeDelta is special and is sent separately
					if (this.LastTime == 0f)
					{
						this.LastTime = Time.time;
					}
					stream.SendNext(Time.time - this.LastTime);
					this.LastTime = Time.time;
					return;
				}
			}
			else
			{
				ItemSyncPackage objectSyncPackage = new ItemSyncPackage();
				objectSyncPackage.syncedData = (Dictionary<string, object>)stream.ReceiveNext();
				objectSyncPackage.timeDelta = (float)stream.ReceiveNext();
				this.syncPackages.Add(objectSyncPackage);
			}
		}

	}
	public class ItemSyncPackage
	{
		public Dictionary<string, object> syncedData;
		public float timeDelta;
	}
	[RequireComponent(typeof(Collider2D))]
	class ItemTriggerAndCollision : MonoBehaviour
	{
		PhysicsItem Item => this.gameObject?.GetComponentInParent<PhysicsItem>();
		Collider2D Col => this.GetComponent<Collider2D>();

		void Start()
		{
			if (this.Item == null) { Destroy(this); }

			this.gameObject.layer = PhysicsItem.Layer;
		}
		internal void SetAsTrigger(bool isTrigger)
		{
			this.Col.isTrigger = isTrigger;
		}
		void OnCollisionEnter2D(Collision2D collision2D)
		{
			if (!this.Col.isTrigger) { this.Item.OnCollisionEnter2D(collision2D); }
		}
		void OnCollisionExit2D(Collision2D collision2D)
		{
			if (!this.Col.isTrigger) { this.Item.OnCollisionExit2D(collision2D); }
		}
		void OnCollisionStay2D(Collision2D collision2D)
		{
			if (!this.Col.isTrigger) { this.Item.OnCollisionStay2D(collision2D); }
		}
		void OnTriggerEnter2D(Collider2D collider2D)
		{
			if (this.Col.isTrigger) { this.Item.OnTriggerEnter2D(collider2D); }
		}
		void OnTriggerExit2D(Collider2D collider2D)
		{
			if (this.Col.isTrigger) { this.Item.OnTriggerExit2D(collider2D); }
		}
		void OnTriggerStay2D(Collider2D collider2D)
		{
			if (this.Col.isTrigger) { this.Item.OnTriggerStay2D(collider2D); }
		}
	}
}
