using System.Collections;
using GameModeCollection.GameModes;
using MapEmbiggener;
using Photon.Pun;
using UnityEngine;

namespace GameModeCollection.Objects.GameModeObjects
{
    public static class BombPrefab
    {
        private static GameObject _Bomb;

        public static GameObject Bomb
        {
            get
            {
                if (BombPrefab._Bomb == null)
                {
                    var obj = new GameObject("BombPrefab");
                    var spriteObj = new GameObject("Sprite");
                    spriteObj.transform.parent = obj.transform;
                    spriteObj.transform.localPosition = Vector3.zero;
                    spriteObj.transform.localScale = Vector3.one;
                    var renderer = spriteObj.AddComponent<SpriteRenderer>();
                    renderer.sprite = Sprites.Box;
                    renderer.color = Color.white*0.9f;
                    Object.DontDestroyOnLoad(obj);
                    obj.AddComponent<PhotonView>();
                    var bombHandler = obj.AddComponent<BombObjectHandler>();
                    PhotonNetwork.PrefabPool.RegisterPrefab("BombPrefab", obj);
                    
                    BombPrefab._Bomb = obj;
                }
                
                return BombPrefab._Bomb;
            }
        }
    }
    
    public class BombObjectHandler : NetworkPhysicsItem<BoxCollider2D,BoxCollider2D>
    {
        public static BombObjectHandler instance;
        private bool hidden = true;
        
        
        private const float Bounciness = 0.2f;
        private const float Friction = 0.8f;
        private const float Mass = 10000f;
        private const float MinAngularDrag = 0.1f;
        private const float MaxAngularDrag = 1f;
        private const float MinDrag = 0f;
        private const float MaxDrag = 5f;
        private const float MaxSpeed = 200f;
        private const float MaxAngularSpeed = 1000f;
        private const float PhysicsForceMult = 10f;
        private const float PlayerPushMult = 200f;
        private const float PhysicsImpulseMult = 0.0005f;


        protected override void Awake()
        {
            this.PhysicalProperties = new ItemPhysicalProperties(
                bounciness: BombObjectHandler.Bounciness,
                friction: BombObjectHandler.Friction,
                mass: BombObjectHandler.Mass,
                minAngularDrag: BombObjectHandler.MinAngularDrag,
                maxAngularDrag: BombObjectHandler.MaxAngularDrag,
                minDrag: BombObjectHandler.MinDrag,
                maxDrag: BombObjectHandler.MaxDrag,
                maxAngularSpeed: BombObjectHandler.MaxAngularSpeed,
                maxSpeed: BombObjectHandler.MaxSpeed,
                forceMult: BombObjectHandler.PhysicsForceMult,
                impulseMult: BombObjectHandler.PhysicsImpulseMult,
                playerPushMult: BombObjectHandler.PlayerPushMult
            );
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            this.gameObject.transform.localScale = new Vector3(2,3.5f,1);
        }

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            GM_BombDefusal.instance.SetBomb(this);
            this.gameObject.transform.SetParent(GM_BombDefusal.instance.transform);
            BombObjectHandler.instance = this;
        }

        public void Reset()
        {
            this.hidden = true;
        }

        public void Spawn()
        {
            this.hidden = false;
            
            this.SetPos(MapManager.instance.GetSpawnPoints()[UnityEngine.Random.Range(0, MapManager.instance.GetSpawnPoints().Length)].localStartPos);
            this.SetVel(Vector2.zero);
            this.SetRot(0f);
            this.SetAngularVel(0f);
        }


        protected override void Update()
        {
            if (this.transform.parent == null)
            {
                this.Rig.isKinematic = true;
                this.transform.position = 100000f * Vector2.up;
                return;
            }
            
            base.Update();
            
            if (this.hidden)
            {
                this.Rig.isKinematic = true;
                this.SetRot(0f);
                this.SetAngularVel(0f);
                this.Col.enabled = false;
                this.Trig.enabled = false;
                if (this.hidden) { this.SetPos(100000f * Vector2.up); }
            }
            else
            {
                this.Rig.isKinematic = false;
                this.Col.enabled = true;
                this.Trig.enabled = true;
                
                
                // if the bomb has gone OOB bounce it back in
                if (!OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 normalizedPoint))
                {
                    if (normalizedPoint.x <= 0f)
                    {
                        this.Rig.velocity = new Vector2(UnityEngine.Mathf.Abs(this.Rig.velocity.x), this.Rig.velocity.y)*1.5f;
                    }
                    else if (normalizedPoint.x >= 1f)
                    {
                        this.Rig.velocity = new Vector2(-UnityEngine.Mathf.Abs(this.Rig.velocity.x), this.Rig.velocity.y)*1.5f;
                    } 
                    else if (normalizedPoint.y <= 0f)
                    {
                        this.Rig.velocity = new Vector2(this.Rig.velocity.x, UnityEngine.Mathf.Abs(this.Rig.velocity.y))*1.5f;
                    }
                    else if (normalizedPoint.y >= 1f)
                    {
                        this.Rig.velocity = new Vector2(this.Rig.velocity.x, -UnityEngine.Mathf.Abs(this.Rig.velocity.y))*1.5f;
                    }
                }
                
                
                // if a player is near the bomb, make them defuse it
                foreach (var player in PlayerManager.instance.players)
                {
                    if (player.data.dead) { continue; }
                    if (Vector2.Distance(player.transform.position, this.transform.position) < 1.5f)
                    {
                        UnityEngine.Debug.Log("Defusing bomb");
                    }
                }
            }
            
        }

        internal static IEnumerator MakeBombHandler()
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(BombPrefab.Bomb.name, GM_BombDefusal.instance.transform.position, GM_BombDefusal.instance.transform.rotation);
            }

            yield return new WaitUntil(() => BombObjectHandler.instance != null);
        }

        protected override void SetDataToSync()
        {
            
        }

        protected override void ReadSyncedData()
        {
            
        }

        protected override bool SyncDataNow()
        {
            return true;
        }
    }
}