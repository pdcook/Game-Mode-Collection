using System.Collections;
using System.Collections.Generic;
using GameModeCollection.GameModes;
using MapEmbiggener;
using Photon.Pun;
using UnboundLib;
using UnboundLib.GameModes;
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
    
    public class BombObjectHandler : NetworkPhysicsItem<BoxCollider2D,CircleCollider2D>
    {
        public static BombObjectHandler instance;
        private bool hidden = true;

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
            this.SetPos(new Vector3(0,30,0));
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
                
                
                // if the crown has gone OOB off the bottom of the map OR hasn't been touched in a long enough time, respawn it
                if (!OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 normalizedPoint))
                {
                    // if it has gone off the sides, have it bounce back in
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