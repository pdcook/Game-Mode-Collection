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
        public static Sprite Box => ((GameObject)Resources.Load("4 map objects/Box")).GetComponentInChildren<SpriteRenderer>().sprite;
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

        public const float Bounciness = 1f;
        public const float Friction = 0.2f;
        public const float Mass = 10000f;
        public const float MinAngularDrag = 0f;
        public const float MaxAngularDrag = 1f;
        public const float MinDrag = 0f;
        public const float MaxDrag = 5f;
        public const float MaxSpeed = 200f;
        public const float MaxAngularSpeed = 1000f;
        public const float PhysicsForceMult = 30f;
        public const float PhysicsImpulseMult = 30f;
        public const float ThrusterDurationMult = 1f;

        public const float Damage = 0.5f;
        public const float Force = 1000f;

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

    public class DeathObjectHealth : MonoBehaviour
    {
        private PhotonView View => this.gameObject.GetComponent<PhotonView>();
        private DeathObjectDamagable Damagable => this.gameObject.GetComponent<DeathObjectDamagable>();
        private Player _lastSourceOfDamage = null;
        private bool _dead = false;
        private Action<Player> onPlayerKilledAction = null;

        private float _invulnerableFor = 0f;
        public float InvulnerableFor
        {
            get
            {
                return this._invulnerableFor;
            }
            private set
            {
                this._invulnerableFor = value;
            }
        }
        public void SetInvulnerableFor(float time)
        {
            this.InvulnerableFor = time;
        }

        public void ResetPlayerKilledAction()
        {
            this.onPlayerKilledAction = null;
        }
        public void AddPlayerKilledAction(Action<Player> action)
        {
            if (this.onPlayerKilledAction is null)
            {
                this.onPlayerKilledAction = action;
            }
            else
            {
                this.onPlayerKilledAction += action;
            }
        }
        public bool Dead
        {
            get
            {
                return this._dead;
            }
            private set
            {
                this._dead = value;
            }
        }
        public Player LastSourceOfDamage
        {
            get
            {
                return this._lastSourceOfDamage;
            }
            private set
            {
                this._lastSourceOfDamage = value;
            }
        }
        private float _Health;
        public float Health
        {
            get
            {
                return this._Health;
            }
            private set
            {
                this._Health = value;
            }
        }
        private float lastDamaged = -1f;
        void Update()
        {
            if (this.InvulnerableFor > 0f)
            {
                this.InvulnerableFor -= Time.deltaTime;
            }
        }
        public void Revive()
        {
            this.Health = UnityEngine.Mathf.Clamp(2f * PlayerManager.instance.players.Select(p => p.data.maxHealth).Sum(), 200f, DeathObjectConstants.MaxHealth);
            this.Dead = false;
            this.LastSourceOfDamage = null;
        }
        public void TakeDamage(Vector2 damage, Player damagingPlayer)
        {
            if (this.Dead || this.InvulnerableFor > 0f) { return; }
            this.Health -= damage.magnitude;
            this.LastSourceOfDamage = damagingPlayer;

            if (this.Health <= 0f && this.View.IsMine)
            {
                this.View.RPC(nameof(RPCA_Die), RpcTarget.All, damage, damagingPlayer.playerID);
            }

            if (damagingPlayer != null)
            {
                damagingPlayer.data.stats.DealtDamage(damage, false, null);
            }

            if (this.lastDamaged + 0.15f < Time.time && damagingPlayer != null && damagingPlayer.data.stats.lifeSteal != 0f)
            {
                SoundManager.Instance.Play(damagingPlayer.data.healthHandler.soundDamageLifeSteal, this.transform);
            }

            this.lastDamaged = Time.time;
        }
        [PunRPC]
        private void RPCA_Die(Vector2 deathDirection, int killingPlayerID)
        {
            this.Dead = true;
            this.Damagable.StopAllCoroutines();
            Player killingPlayer = PlayerManager.instance.players.Find(p => p.playerID == killingPlayerID);
            if (killingPlayer is null)
            {
                killingPlayer = PlayerManager.instance.players.FirstOrDefault();
            }
            if (killingPlayer is null) { return; }
            // play death effect
            GamefeelManager.GameFeel(deathDirection.normalized * 3f);
            DeathEffect deathEffect = GameObject.Instantiate(killingPlayer.data.healthHandler.deathEffect, this.transform.position, this.transform.rotation).GetComponent<DeathEffect>();
            deathEffect.gameObject.transform.localScale = 2f * Vector3.one;
            deathEffect.PlayDeath(killingPlayer.GetTeamColors().color, killingPlayer.data.playerVel, deathDirection, -1);
            this.onPlayerKilledAction?.Invoke(killingPlayer);
        }
    }

    public abstract class DeathObjectHandler<TCollision> : NetworkPhysicsItem<TCollision, CircleCollider2D> where TCollision : Collider2D
    {
        private const float InvulnerabilityTime = 2f;
        protected override int sendFreq
        {
            get
            {
                return 5;
            }
        }
        private float Damage => DeathObjectConstants.Damage;
        private float Force => DeathObjectConstants.Force;

        public DeathObjectDamagable Damagable => this.gameObject.GetComponent<DeathObjectDamagable>();
        public DeathObjectHealth Health => this.gameObject.GetComponent<DeathObjectHealth>();

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
                thrusterDurationMult: DeathObjectConstants.ThrusterDurationMult
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

            this.gameObject.GetOrAddComponent<DeathObjectDamagable>();
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
            if (collision?.collider?.GetComponent<PlayerCollision>() != null)
            {
                PlayerCollision playerCol = collision.collider.GetComponent<PlayerCollision>();
                CharacterData data = playerCol.GetComponent<CharacterData>();
                PhotonView view = data.view;
                if (PhotonNetwork.OfflineMode || view.IsMine)
                {
                    view.RPC("RPCADoBounce", RpcTarget.All, (Vector2)(this.transform.position - playerCol.transform.position).normalized, playerCol.transform.position);
                    data.healthHandler.CallTakeDamage(this.Damage * this.Rig.velocity, data.transform.position, null, null, true);
                    data.healthHandler.CallTakeForce(this.Force * this.Rig.velocity, ForceMode2D.Impulse, false, false, (this.Damage * this.Rig.velocity).magnitude / 20f);
                }
            }
            ProjectileCollision projCol = collision?.collider?.GetComponent<ProjectileCollision>();
            if (projCol != null && projCol?.transform?.parent?.GetComponent<ProjectileHit>() != null && (projCol.transform.parent.GetComponentInChildren<PhotonView>().IsMine || PhotonNetwork.OfflineMode))
            {
                Vector2 point = (Vector2)projCol.transform.position;
                Vector2 damage = projCol.gameObject.GetComponentInParent<ProjectileHit>().dealDamageMultiplierr * (projCol.gameObject.GetComponentInParent<ProjectileHit>().bulletCanDealDeamage ? projCol.gameObject.GetComponentInParent<ProjectileHit>().damage : 1f) * (Vector2)projCol.transform.parent.forward;
                Player damagingPlayer = projCol.gameObject.GetComponentInParent<ProjectileHit>().ownPlayer;
                GameObject damagingWeapon = projCol.gameObject.GetComponentInParent<ProjectileHit>().ownWeapon;
                this.Damagable.CallTakeDamage(damage, point, damagingWeapon, damagingPlayer, true);
            }

            base.OnCollisionEnter2D(collision);
        }
        protected override void Update()
        {
            if (this.Health.Dead) { this.hidden = true; }

            if (this.transform.parent == null || this.hidden)
            {
                this.Rig.isKinematic = true;
                this.transform.position = 100000f * Vector2.up;
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
        }

        private const string SyncedModeKey = "DeathObject_Mode";

        protected override void SetDataToSync()
        {
            this.SetSyncedData(SyncedModeKey, (byte)this.CurrentMode);
        }
        protected override void ReadSyncedData()
        {
            // syncing
            this.CurrentMode = (Mode)this.GetSyncedData<byte>(SyncedModeKey, (byte)this.CurrentMode);
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
        public enum Mode
        {
            Normal,
            Flubber,
            Pac,
        }
    }
    [RequireComponent(typeof(PhotonView))]
    public class DeathObjectDamagable : Damagable
    {

        private PhotonView View => this.gameObject.GetComponent<PhotonView>();
        private DeathObjectHealth Health => this.gameObject.GetComponent<DeathObjectHealth>();

        public override void CallTakeDamage(Vector2 damage, Vector2 damagePosition, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            if (damage == Vector2.zero)
            {
                return;
            }
            this.View.RPC(nameof(RPCA_TakeDamage), RpcTarget.All, new object[]
            {
                damage,
                damagePosition,
                lethal,
                (damagingPlayer != null) ? damagingPlayer.playerID : -1
            });
        }

        public override void TakeDamage(Vector2 damage, Vector2 damagePosition, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true, bool ignoreBlock = false)
        {
            if (damage == Vector2.zero) { return; }
            this.TakeDamage(damage, damagePosition, damagingPlayer?.GetTeamColors()?.color ?? Color.red, damagingWeapon, damagingPlayer, lethal, ignoreBlock);
        }

        public override void TakeDamage(Vector2 damage, Vector2 damagePosition, Color dmgColor, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true, bool ignoreBlock = false)
        {
            this.Health.TakeDamage(damage, damagingPlayer);
            foreach (PlayerSkinParticle skin in this.GetComponentsInChildren<PlayerSkinParticle>())
            {
                skin.BlinkColor(dmgColor);
            }
        }
        public void TakeDamageOverTime(Vector2 damage, Vector2 position, float time, float interval, Color color, SoundEvent soundDamageOverTime, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            this.StartCoroutine(this.DoDamageOverTime(damage, position, time, interval, color, soundDamageOverTime, damagingWeapon, damagingPlayer, lethal));
        }
        private IEnumerator DoDamageOverTime(Vector2 damage, Vector2 position, float time, float interval, Color color, SoundEvent soundDamageOverTime, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            float damageDealt = 0f;
            float damageToDeal = damage.magnitude;
            float dpt = damageToDeal / time * interval;
            while (damageDealt < damageToDeal)
            {
                if (soundDamageOverTime != null && !this.Health.Dead)
                {
                    SoundManager.Instance.Play(soundDamageOverTime, this.transform);
                }
                damageDealt += dpt;
                this.TakeDamage(damage.normalized * dpt, position, color, damagingWeapon, damagingPlayer, lethal, false);
                yield return new WaitForSeconds(interval / TimeHandler.timeScale);
            }
            yield break;
        }
        [PunRPC]
        private void RPCA_TakeDamage(Vector2 damage, Vector2 position, bool lethal = true, int playerID = -1)
        {
            if (damage == Vector2.zero)
            {
                return;
            }
            Player playerWithID = PlayerManager.instance.players.Where(p => p.playerID == playerID).FirstOrDefault();
            GameObject damagingWeapon = null;
            if (playerWithID != null)
            {
                damagingWeapon = playerWithID.data.weaponHandler.gun.gameObject;
            }
            this.TakeDamage(damage, position, damagingWeapon, playerWithID, lethal, true);
        }
    }
}
