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
using System;
using Sonigon;

namespace GameModeCollection.Objects.GameModeObjects
{
    public static class DeathObjectPrefabs
    {
        public static int Layer => LayerMask.NameToLayer("Player");
        public static int SortingLayerID => SortingLayer.NameToID("Player0");

        private readonly static PlayerSkin DefaultDeathObjectColors = new PlayerSkin()
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
                    ObjectParticleSkin.AddObjectParticleSkin(deathBall.transform, Sprites.Circle, DefaultDeathObjectColors);

                    GameModeCollection.Log("DeathBall Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(deathBall);

                    // must add required components (PhotonView) first
                    deathBall.AddComponent<PhotonView>();
                    deathBall.AddComponent<DeathBall>();

                    PhotonNetwork.PrefabPool.RegisterPrefab(deathBall.name, deathBall);

                    DeathObjectPrefabs._DeathBall = deathBall;
                }
                return DeathObjectPrefabs._DeathBall;
            }
        }
        private static GameObject _DeathTriangle = null;

        public static GameObject DeathTriangle
        {
            get
            {
                if (DeathObjectPrefabs._DeathTriangle == null)
                {
                    GameObject deathTriangle = new GameObject("DeathTrianglePrefab");
                    ObjectParticleSkin.AddObjectParticleSkin(deathTriangle.transform, Sprites.Triangle, DefaultDeathObjectColors);

                    GameModeCollection.Log("DeathTriangle Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(deathTriangle);

                    // must add required components (PhotonView) first
                    deathTriangle.AddComponent<PhotonView>();
                    deathTriangle.AddComponent<DeathTriangle>();

                    PhotonNetwork.PrefabPool.RegisterPrefab(deathTriangle.name, deathTriangle);

                    DeathObjectPrefabs._DeathTriangle = deathTriangle;
                }
                return DeathObjectPrefabs._DeathTriangle;
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
                    ObjectParticleSkin.AddObjectParticleSkin(deathBox.transform, Sprites.Box, DefaultDeathObjectColors);

                    GameModeCollection.Log("DeathBox Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(deathBox);

                    // must add required components (PhotonView) first
                    deathBox.AddComponent<PhotonView>();
                    deathBox.AddComponent<DeathBox>();

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
                    ObjectParticleSkin.AddObjectParticleSkin(deathRod.transform, Sprites.Box, DefaultDeathObjectColors);

                    GameModeCollection.Log("DeathRod Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(deathRod);

                    // must add required components (PhotonView) first
                    deathRod.AddComponent<PhotonView>();
                    deathRod.AddComponent<DeathRod>();

                    PhotonNetwork.PrefabPool.RegisterPrefab(deathRod.name, deathRod);

                    DeathObjectPrefabs._DeathRod = deathRod;
                }
                return DeathObjectPrefabs._DeathRod;
            }
        }
    }

    public static class DeathObjectConstants
    {
        public const float MaxFreeTime = 20f;

        public const float Bounciness = 1f;
        public const float Friction = 0.2f;
        public const float Mass = 20000f;
        public const float MinAngularDrag = 0f;
        public const float MaxAngularDrag = 1f;
        public const float MinDrag = 0f;
        public const float MaxDrag = 5f;
        public const float MaxSpeed = 200f;
        public const float MaxAngularSpeed = 1000f;
        public const float PhysicsForceMult = 5f;
        public const float PhysicsImpulseMult = 2f;
        public const float ThrusterDurationMult = 1f;
        public const float PlayerForceMult = 1f;
        public const float PlayerDamageMult = 5f;

        public const float MaxHealth = 1000f;

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
        internal static void DestroyDeathBall()
        {
            GM_Dodgeball.instance.DestroyDeathBall();
            if (DeathBall.instance != null)
            {
                UnityEngine.GameObject.DestroyImmediate(DeathBall.instance);
            }
        }
        internal static IEnumerator MakeDeathBall()
        {
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
    public class DeathTriangle : DeathObjectHandler<PolygonCollider2D>
    {
        private static DeathTriangle instance;

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;

            this.gameObject.transform.SetParent(GM_Dodgeball.instance.transform);
            GM_Dodgeball.instance.SetDeathObject(this);
            DeathTriangle.instance = this;
        }
        protected override void Start()
        {
            base.Start();

            this.transform.GetChild(0).transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            this.Col.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            Sprite sprite = this.transform.GetChild(0).GetComponentInChildren<SpriteMask>().sprite;
            int shapeCount = sprite.GetPhysicsShapeCount();
            List<Vector2> physicsShape = new List<Vector2>() { };
            for (int i = 0; i < shapeCount; i++)
            {
                sprite.GetPhysicsShape(i, physicsShape);
                this.Col.SetPath(i, physicsShape.ToArray());
            }
        }
        internal static void DestroyDeathTriangle()
        {
            GM_Dodgeball.instance.DestroyDeathTriangle();
            if (DeathTriangle.instance != null)
            {
                UnityEngine.GameObject.DestroyImmediate(DeathTriangle.instance);
            }
        }
        internal static IEnumerator MakeDeathTriangle()
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(
                    DeathObjectPrefabs.DeathTriangle.name,
                    GM_Dodgeball.instance.transform.position,
                    GM_Dodgeball.instance.transform.rotation,
                    0
                    );
            }

            yield return new WaitUntil(() => DeathTriangle.instance != null);
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
        internal static void DestroyDeathBox()
        {
            GM_Dodgeball.instance.DestroyDeathBox();
            if (DeathBox.instance != null)
            {
                UnityEngine.GameObject.DestroyImmediate(DeathBox.instance);
            }
        }
        internal static IEnumerator MakeDeathBox()
        {
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
        protected override void Start()
        {
            base.Start();

            this.Col.size = new Vector2(2f, 2f);
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
        internal static void DestroyDeathRod()
        {
            GM_Dodgeball.instance.DestroyDeathRod();
            if (DeathRod.instance != null)
            {
                UnityEngine.GameObject.DestroyImmediate(DeathRod.instance);
            }
        }
        internal static IEnumerator MakeDeathRod()
        {
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
        protected override void Start()
        {
            base.Start();

            this.Col.size = new Vector2(2f, 2f);

            this.transform.localScale = new Vector3(0.25f, 2f, 1f);
        }
    }

    public class DeathObjectHealth : ObjectHealthHandler
    {
        public override void Revive()
        {
            this.MaxHealth = UnityEngine.Mathf.Clamp(2f * PlayerManager.instance.players.Select(p => p.data.maxHealth).Sum(), 200f, DeathObjectConstants.MaxHealth);
            base.Revive();
        }
        [PunRPC]
        protected override void RPCA_Die(Vector2 deathDirection, int killingPlayerID)
        {
            base.RPCA_Die(deathDirection, killingPlayerID);
            Player killingPlayer = PlayerManager.instance.players.Find(p => p.playerID == killingPlayerID);
            if (killingPlayer is null)
            {
                // get any player to use for the deathEffect and color
                killingPlayer = PlayerManager.instance.players.FirstOrDefault();

            }
            if (killingPlayer is null) { return; }
            // play death effect
            GamefeelManager.GameFeel(deathDirection.normalized * 3f);
            DeathEffect deathEffect = GameObject.Instantiate(killingPlayer.data.healthHandler.deathEffect, this.transform.position, this.transform.rotation).GetComponent<DeathEffect>();
            deathEffect.gameObject.transform.localScale = 2f * Vector3.one;
            deathEffect.PlayDeath(killingPlayer.GetTeamColors().color, killingPlayer.data.playerVel, deathDirection, -1);
        }
    }

    public abstract class DeathObjectHandler<TCollision> : DamagableNetworkPhysicsItem<TCollision, CircleCollider2D> where TCollision : Collider2D
    {
        private const float InvulnerabilityTime = 2f;

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
                    return GM_Dodgeball.objSpawn;
                }
            }
            private set
            {
                this._previousSpawn = value;
            }
        }

        private bool hidden = true;
        internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();

        private Mode _currentMode = Mode.Normal;
        public Mode CurrentMode
        {
            get
            {
                return this._currentMode;
            }
            private set
            {
                this._currentMode = value;
            }
        }
        protected override bool SyncDataNow()
        {
			return !this.hidden;
        }
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
                impulseMult: DeathObjectConstants.PhysicsImpulseMult,
                thrusterDurationMult: DeathObjectConstants.ThrusterDurationMult,
                playerForceMult: DeathObjectConstants.PlayerForceMult,
                playerDamageMult: DeathObjectConstants.PlayerDamageMult
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

            this.gameObject.GetOrAddComponent<ObjectDamagable>();
            this.gameObject.GetOrAddComponent<DeathObjectHealth>();

            this.Trig.enabled = false;
        }

        public void SetMode(Mode mode)
        {
            if (this.View.IsMine || PhotonNetwork.OfflineMode) 
            {
                this.View.RPC(nameof(RPCA_SetMode), RpcTarget.All, (byte)mode);
            }
        }
        [PunRPC]
        private void RPCA_SetMode(byte mode)
        {
            this.CurrentMode = (Mode)mode;
        }

        public void Reset()
        {
            this.hidden = true;
            this._previousSpawn = null;
            this.Health.Revive();
        }

        /// <summary>
        /// takes in a NORMALIZED vector between (0,0,0) and (1,1,0) which represents the percentage across the bounds on each axis
        /// </summary>
        /// <param name="normalized_position"></param>
        public void Spawn(Vector3 normalized_position)
        {
            this.hidden = false;
            this.gameObject.SetActive(true);
            this.SetPos(OutOfBoundsUtils.GetPoint(normalized_position));
            this.SetVel(Vector2.zero);
            this.SetRot(0f);
            this.SetAngularVel(0f);
            this.PreviousSpawn = normalized_position;
            this.AddRandomAngularVelocity();
            this.Health.SetInvulnerableFor(InvulnerabilityTime);
        }
        public void AddRandomAngularVelocity(float min = -DeathObjectConstants.MaxAngularSpeed, float max = DeathObjectConstants.MaxAngularSpeed)
        {
            if (this.View.IsMine) { this.View.RPC(nameof(RPCA_AddAngularVel), RpcTarget.All, UnityEngine.Random.Range(min, max)); }
        }
        [PunRPC]
        public void RPCA_AddAngularVel(float angVelToAdd)
        {
            this.Rig.angularVelocity += angVelToAdd;
        }
        [PunRPC]
        protected override void RPCA_SendForce(Vector2 force, Vector2 point, byte forceMode = (byte)ForceMode2D.Force)
        {
            base.RPCA_SendForce(force, point, forceMode);
        }
        protected internal override void OnCollisionEnter2D(Collision2D collision)
        {
            this.Rig.velocity *= 1.05f; // extra bouncy
            base.OnCollisionEnter2D(collision);
        }
        protected internal override void OnCollisionStay2D(Collision2D collision2D)
        {
            this.Rig.velocity += TimeHandler.deltaTime * Vector2.up;
            base.OnCollisionStay2D(collision2D);
        }
        protected override void Update()
        {
            if (this.Health.Dead) { this.hidden = true; }

            if (this.transform.parent == null || this.hidden)
            {
                this.Rig.isKinematic = true;
                this.transform.position = 100000f * Vector2.up;
                this.gameObject.SetActive(false);
                return;
            }

            base.Update();

            this.Rig.isKinematic = false;
            this.Col.enabled = true;
            this.Trig.enabled = true;
            OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 normalizedPoint);
            // if it has gone off the sides, have it bounce back in
            if (normalizedPoint.x <= 0f)
            {
                this.Rig.velocity = new Vector2(UnityEngine.Mathf.Abs(this.Rig.velocity.x), this.Rig.velocity.y);
            }
            else if (normalizedPoint.x >= 1f)
            {
                this.Rig.velocity = new Vector2(-UnityEngine.Mathf.Abs(this.Rig.velocity.x), this.Rig.velocity.y);
            }
            else if (normalizedPoint.y <= 0f)
            {
                this.Rig.velocity = new Vector2(this.Rig.velocity.x, 1.5f*UnityEngine.Mathf.Abs(this.Rig.velocity.y));
            }
            // if it has gone off the top, just increase it's velocity downwards until it's back in
            else if (normalizedPoint.y >= 1f)
            {
                this.Rig.velocity = new Vector2(this.Rig.velocity.x, this.Rig.velocity.y - DeathObjectConstants.MaxSpeed*Time.deltaTime);
            }
            // if it's getting too close to the walls, gently nudge it back in
            else if (normalizedPoint.x >= 0.9f)
            {
                this.Rig.velocity += (this.Rig.velocity.magnitude / 100f) * Vector2.left;
            }
            else if (normalizedPoint.x <= 0.1f)
            {
                this.Rig.velocity += (this.Rig.velocity.magnitude / 100f) * Vector2.right;
            }
        }

        private const string SyncedModeKey = "DeathObject_Mode";

        protected override void SetDataToSync()
        {
            this.SetSyncedInt(SyncedModeKey, (int)this.CurrentMode);
        }
        protected override void ReadSyncedData()
        {
            // syncing
            this.CurrentMode = (Mode)this.GetSyncedInt(SyncedModeKey, (int)this.CurrentMode);
        }
        public enum Mode
        {
            Normal,
            Flubber,
            Pac,
        }
    }
}
