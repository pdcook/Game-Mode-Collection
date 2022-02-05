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
		private const float DefaultPhysicsPlayerPushMult = 100f;
		private const float DefaultPhysicsPlayerForceMult = 1f;
		private const float DefaultPhysicsPlayerDamageMult = 1f;
		private const float DefaultPhysicsCollisionDamageThreshold = 1f;
		private const float DefaultThrusterDurationMult = 1f;
		private const bool DefaultIgnoreBackgroundObjects = true;

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
		public readonly float PhysicsPlayerPushMult;
		public readonly float PhysicsPlayerForceMult;
		public readonly float PhysicsPlayerDamageMult;
		public readonly float PhysicsCollisionDamageThreshold;
		public readonly float ThrusterDurationMult;
		public readonly bool IgnoreBackgroundObjects;
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
			float playerPushMult = DefaultPhysicsPlayerPushMult,
			float playerForceMult = DefaultPhysicsPlayerForceMult,
			float playerDamageMult = DefaultPhysicsPlayerDamageMult,
			float collisionDamageThreshold = DefaultPhysicsCollisionDamageThreshold,
			float thrusterDurationMult = DefaultThrusterDurationMult,
			bool ignoreBackgroundObjects = DefaultIgnoreBackgroundObjects)
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
			this.PhysicsPlayerPushMult = playerPushMult;
			this.PhysicsPlayerForceMult = playerForceMult;
			this.PhysicsPlayerDamageMult = playerDamageMult;
			this.PhysicsCollisionDamageThreshold = collisionDamageThreshold;
			this.ThrusterDurationMult = thrusterDurationMult;
			this.IgnoreBackgroundObjects = ignoreBackgroundObjects;
		}

	}
	public abstract class PhysicsItem : MonoBehaviour
	{
		public abstract ItemPhysicalProperties PhysicalProperties { get; }
		public static readonly int ColliderLayer = LayerMask.NameToLayer("IgnorePlayer"); // layer that physics objects are on, although this causes things to interact with background objects too
		public static readonly int TriggerLayer = LayerMask.NameToLayer("PlayerObjectCollider");
		private PhotonView View => this.gameObject.GetComponent<PhotonView>();
		protected internal abstract void OnCollisionEnter2D(Collision2D collision2D);
		protected internal abstract void OnCollisionExit2D(Collision2D collision2D);
		protected internal abstract void OnCollisionStay2D(Collision2D collision2D);
		protected internal abstract void OnTriggerEnter2D(Collider2D collider2D);
		protected internal abstract void OnTriggerExit2D(Collider2D collider2D);
		protected internal abstract void OnTriggerStay2D(Collider2D collider2D);
		protected internal abstract Vector3 Push(CharacterData data);
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
		private Dictionary<string, int> _intDataToSync = new Dictionary<string, int>() { };
		private Dictionary<string, string> _stringDataToSync = new Dictionary<string, string>() { };
		private Dictionary<string, float> _floatDataToSync = new Dictionary<string, float>() { };

		protected string[] GetSyncedKeys()
		{
			return this._intDataToSync.Keys.Concat(this._stringDataToSync.Keys).Concat(this._floatDataToSync.Keys).ToArray();
		}
		protected object GetSyncedData(string key, object default_value = default)
		{
			if (this._intDataToSync.TryGetValue(key, out int value))
			{
				return value;
			}
			else if (this._stringDataToSync.TryGetValue(key, out string value1))
			{
				return value1;
			}
			else if (this._floatDataToSync.TryGetValue(key, out float value2))
			{
				return value2;
			}
			else
			{
				GameModeCollection.LogWarning($"Key \"{key}\" not found in syncing data of NetworkPhysicsItem component of {this.name}.");
			}
			return default_value;
		}
		protected int GetSyncedInt(string key, int default_value = default)
		{
			if (this._intDataToSync.TryGetValue(key, out int value))
			{
                return value;
			}
			else
			{
				GameModeCollection.LogWarning($"Key \"{key}\" not found in syncing data of NetworkPhysicsItem component of {this.name}.");
			}
			return default_value;
		}
		protected string GetSyncedString(string key, string default_value = default)
		{
			if (this._stringDataToSync.TryGetValue(key, out string value))
			{
                return value;
			}
			else
			{
				GameModeCollection.LogWarning($"Key \"{key}\" not found in syncing data of NetworkPhysicsItem component of {this.name}.");
			}
			return default_value;
		}
		protected float GetSyncedFloat(string key, float default_value = default)
		{
			if (this._floatDataToSync.TryGetValue(key, out float value))
			{
                return value;
			}
			else
			{
				GameModeCollection.LogWarning($"Key \"{key}\" not found in syncing data of NetworkPhysicsItem component of {this.name}.");
			}
			return default_value;
		}
		protected void SetSyncedInt(string key, int value)
		{
			this._intDataToSync[key] = value;
		}
		protected void SetSyncedString(string key, string value)
		{
			this._stringDataToSync[key] = value;
		}
		protected void SetSyncedFloat(string key, float value)
		{
			this._floatDataToSync[key] = value;
		}

		private int sendFreq
		{
			get
            {
				return 5;
            }
		}

        protected int CurrentFrame { get; private set; } = 5;
        protected float LastTime { get; private set; } = 0f;
        protected float TimeDelta { get; private set; } = 0f;
		protected float SinceDealDamage { get; private set; } = 100f;

        private List<ItemSyncPackage> syncPackages = new List<ItemSyncPackage>();

		public Rigidbody2D Rig => this.GetComponent<Rigidbody2D>();
		public TCollider Col => this.transform?.Find("Collider")?.GetComponent<TCollider>();
		public TTrigger Trig => this.transform?.Find("Trigger")?.GetComponent<TTrigger>();
		internal IgnoreBackgroundObjects IgnoreBackground => this.transform?.Find("IgnoreBackground")?.GetComponent<IgnoreBackgroundObjects>();
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
			if (this.transform?.Find("IgnoreBackground") == null)
            {
				GameObject ignoreBackground = new GameObject("IgnoreBackground", typeof(IgnoreBackgroundObjects));
				ignoreBackground.transform.SetParent(this.transform);
            }
		}
		protected virtual void Start()
		{
			this.Col.GetComponent<ItemTriggerAndCollision>().SetAsTrigger(false);
			this.Trig.GetComponent<ItemTriggerAndCollision>().SetAsTrigger(true);
			if (this.PhysicalProperties.IgnoreBackgroundObjects)
			{
				this.IgnoreBackground.SetColliderToTrack(this.Col);
			}

			this.Rig.drag = this.PhysicalProperties.MinDrag;
			this.Rig.angularDrag = this.PhysicalProperties.MinAngularDrag;
			this.Rig.mass = this.PhysicalProperties.Mass;

			this.Col.sharedMaterial = this.Material;
			this.Trig.sharedMaterial = this.Material;
			this.Rig.sharedMaterial = this.Material;

			this.gameObject.layer = PhysicsItem.ColliderLayer;

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
        protected internal override Vector3 Push(CharacterData data)
        {
			if (!data.view.IsMine) { return Vector3.zero; }
			Vector2 vector = data.input.direction * 8f;
			Vector2 vector2 = this.Col.bounds.ClosestPoint(data.transform.position);
			float num = Vector2.Angle(vector, vector2 - (Vector2)data.transform.position);
			float d = (90f - num) / 90f;
			Vector2 b = TimeHandler.fixedDeltaTime * vector * d * this.PhysicalProperties.PhysicsPlayerPushMult * 1000f;
			this.CallTakeForce(b, vector2, ForceMode2D.Force);

            float d2 = Mathf.Clamp((Vector2.Angle(this.Rig.velocity, base.transform.position - data.transform.position) - 90f) / 90f, 0f, 1f);
            this.OnPlayerCollision(this.Rig.velocity * d2, data);
			return -vector;
		}

		protected virtual void OnPlayerCollision(Vector2 collision, CharacterData player)
        {
			if (player.view.IsMine)
			{
				if (this.SinceDealDamage < 1f)
				{
					return;
				}
				Vector3 a = collision * this.PhysicalProperties.PhysicsPlayerDamageMult;
				if (a.magnitude < this.PhysicalProperties.PhysicsCollisionDamageThreshold)
				{
					return;
				}
				float d = Mathf.Pow(this.Rig.mass / 20000f, 2f);
				float d2 = Mathf.Pow(this.Rig.mass / 20000f, 0.5f);
				player.healthHandler.CallTakeDamage(a * 0.3f * d, player.transform.position, null, null, true);
				player.healthHandler.CallTakeForce(collision * this.PhysicalProperties.PhysicsPlayerForceMult * d2, ForceMode2D.Impulse, false, false, a.magnitude * 0.05f);
				if (player.block.IsBlocking())
				{
					this.Rig.velocity *= -1.1f;
					this.Rig.angularVelocity *= -1.1f;
				}
				else if (this.Rig.mass < 80000f)
				{
					this.Rig.velocity *= -0.5f * (20000f / this.Rig.mass);
					this.Rig.angularVelocity *= -0.5f * (20000f / this.Rig.mass);
				}
				this.SinceDealDamage = 0f;
				this.View.RPC(nameof(RPCA_PlayerCollision), RpcTarget.AllViaServer, collision, this.Rig.velocity, base.transform.position, player.view.ViewID );
			}
		}
		[PunRPC]
		protected virtual void RPCA_PlayerCollision(Vector2 collision, Vector2 velAfter, Vector3 position, int playerID)
		{
			CharacterData data = PhotonNetwork.GetPhotonView(playerID).GetComponent<CharacterData>();
			base.transform.position = position;
			this.Rig.velocity = velAfter;
			this.SinceDealDamage = 0f;
			this.StartCoroutine(data.GetComponent<PlayerCollision>().IDoBounce((Vector2)data.playerVel.GetFieldValue("velocity")));
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
			this.SinceDealDamage += TimeHandler.deltaTime;
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
					if (!this.Rig.isKinematic)
                    {
                        this.transform.position = this.syncPackages[0].pos;
                        this.transform.rotation = Quaternion.LookRotation(Vector3.forward, this.syncPackages[0].rot);
                        this.Rig.velocity = this.syncPackages[0].vel;
                        this.Rig.angularVelocity = this.syncPackages[0].angularVel;
                    }
					this._intDataToSync = this.syncPackages[0].syncedIntData;
					this._stringDataToSync = this.syncPackages[0].syncedStringData;
					this._floatDataToSync = this.syncPackages[0].syncedFloatData;

					this.syncPackages.RemoveAt(0);


					// update all other data
					this.ReadSyncedData();

				}
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
					stream.SendNext((Vector2)this.transform.position);
					stream.SendNext((Vector2)this.transform.up);
					stream.SendNext((Vector2)this.Rig.velocity);
					stream.SendNext(this.Rig.angularVelocity);
					// timeDelta is special and is sent separately
					if (this.LastTime == 0f)
					{
						this.LastTime = Time.time;
					}
					stream.SendNext(Time.time - this.LastTime);
					// send all other data
					this.SetDataToSync();
					stream.SendNext(this._intDataToSync);
					stream.SendNext(this._stringDataToSync);
					stream.SendNext(this._floatDataToSync);
					this.LastTime = Time.time;
					return;
				}
			}
			else
			{
				ItemSyncPackage itemSyncPackage = new ItemSyncPackage();
				itemSyncPackage.pos = (Vector2)stream.ReceiveNext();
				itemSyncPackage.rot = (Vector2)stream.ReceiveNext();
				itemSyncPackage.vel = (Vector2)stream.ReceiveNext();
				itemSyncPackage.angularVel = (float)stream.ReceiveNext();
				itemSyncPackage.timeDelta = UnityEngine.Mathf.Clamp((float)stream.ReceiveNext(), 0f, 0.1f);
				itemSyncPackage.syncedIntData = (Dictionary<string, int>)stream.ReceiveNext();
				itemSyncPackage.syncedStringData = (Dictionary<string, string>)stream.ReceiveNext();
				itemSyncPackage.syncedFloatData = (Dictionary<string, float>)stream.ReceiveNext();
				this.syncPackages.Add(itemSyncPackage);
			}
		}

	}
	public class ItemSyncPackage
	{
		public float timeDelta;
		public Vector2 pos;
		public Vector2 rot;
		public Vector2 vel;
		public float angularVel;
		public Dictionary<string, int> syncedIntData;
		public Dictionary<string, string> syncedStringData;
		public Dictionary<string, float> syncedFloatData;
	}
	[RequireComponent(typeof(Collider2D))]
	class ItemTriggerAndCollision : MonoBehaviour
	{
		PhysicsItem Item => this.gameObject?.GetComponentInParent<PhysicsItem>();
		Collider2D Col => this.GetComponent<Collider2D>();

		void Start()
		{
			if (this.Item == null) { Destroy(this); }

			this.gameObject.layer = this.Col.isTrigger ? PhysicsItem.TriggerLayer : PhysicsItem.ColliderLayer;
		}
		internal void SetAsTrigger(bool isTrigger)
		{
			this.Col.isTrigger = isTrigger;
			this.gameObject.layer = this.Col.isTrigger ? PhysicsItem.TriggerLayer : PhysicsItem.ColliderLayer;
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
