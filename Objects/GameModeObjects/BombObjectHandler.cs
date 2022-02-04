using System.Collections;
using System.Collections.Generic;
using GameModeCollection.GameModes;
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
                // if (BombPrefab._Bomb == null)
                // {
                //     var obj = new GameObject("BombPrefab");
                //     var renderer = obj.AddComponent<SpriteRenderer>();
                //     renderer.sprite = Sprites.Box;
                //     renderer.color = Color.white*0.9f;
                //     GameObject.DontDestroyOnLoad(obj);
                //     obj.AddComponent<PhotonView>();
                //     var bombHandler = obj.AddComponent<BombObjectHandler>();
                //     PhotonNetwork.PrefabPool.RegisterPrefab("BombPrefab", obj);
                //     
                //     BombPrefab._Bomb = obj;
                // }
                //
                // return BombPrefab._Bomb;

                if (BombPrefab._Bomb == null)
                {

                    GM_ArmsRace gm = GameModeManager.GetGameMode<GM_ArmsRace>(GameModeManager.ArmsRaceID);

                    GameObject bomb = GameObject.Instantiate(gm.gameObject.transform.GetChild(0).gameObject);
                    GameModeCollection.Log("Bomb Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(bomb);
                    bomb.name = "BombPrefab";
                    // must add required components (PhotonView) first
                    bomb.AddComponent<PhotonView>();
                    BombObjectHandler crownHandler = bomb.AddComponent<BombObjectHandler>();

                    UnityEngine.GameObject.DestroyImmediate(bomb.GetComponent<GameCrownHandler>());

                    PhotonNetwork.PrefabPool.RegisterPrefab(bomb.name, bomb);

                    BombPrefab._Bomb = bomb;
                }
                return BombPrefab._Bomb;
            }
        }
    }
    
    public class BombObjectHandler : NetworkPhysicsItem<BoxCollider2D,CircleCollider2D>
    {
        public static BombObjectHandler instance;
        
        
        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            this.gameObject.transform.SetParent(GM_BombDefusal.instance.transform);
            BombObjectHandler.instance = this;
            GM_BombDefusal.instance.bomb = this;
        }
        
        internal static IEnumerator MakeBombHandler()
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(
                    BombPrefab.Bomb.name,
                    GM_BombDefusal.instance.transform.position,
                    GM_BombDefusal.instance.transform.rotation,
                    0
                );
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