using Photon.Pun;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Utils;
using UnityEngine;
using GameModeCollection.Extensions;

namespace GameModeCollection.Objects
{
    public static class CardItemPrefabs
    {
        private static GameObject _CardItemHandler = null;
        public static GameObject CardItemHandler
        {
            get
            {
                if (CardItemPrefabs._CardItemHandler == null)
                {
                    CardItemPrefabs._CardItemHandler = new GameObject("CardItemHandler", typeof(CardItemHandler));
                }
                return CardItemPrefabs._CardItemHandler;
            }

        }

        private static GameObject _CardItem = null;

        public static GameObject CardItem
        {
            get
            {
                if (CardItemPrefabs._CardItem == null)
                {

                    GameObject card = new GameObject("CardItemPrefab", typeof(CardItem));
                    GameModeCollection.Log("Card Item Prefab Instantiated");
                    UnityEngine.GameObject.DontDestroyOnLoad(card);

                    PhotonNetwork.PrefabPool.RegisterPrefab(card.name, card);

                    CardItemPrefabs._CardItem = card;
                }
                return CardItemPrefabs._CardItem;
            }
        }


    }
    class CardItem : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
    {
        internal static IEnumerator MakeCardItem(CardInfo card, Vector3 position, Quaternion rotation)
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                PhotonNetwork.Instantiate(
                        CardItemPrefabs.CardItem.name,
                        position,
                        rotation,
                        0,
                        new object[] { card.cardName }
                    );
            }

            yield break;
        }
        public bool HasBeenTaken { get; private set; } = false;
        public CardInfo Card { get; private set; }
        public string CardName { get; private set; }
        public GameObject CardObj { get; internal set; } = null;
        public const float CollectionDistance = 2f;

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;

            // data[0] is the name of the card
            this.CardName = (string)data[0];
            this.Card = CardManager.GetCardInfoWithName(this.CardName);

            this.gameObject.name = $"{this.CardName} Item";

            this.gameObject.transform.SetParent(CardItemPrefabs.CardItemHandler.transform);

            PhotonNetwork.Instantiate(this.Card.name, CardItemHandler.PositionToCheck, Quaternion.identity, 0);

            GameModeCollection.Log($"Instantiated {this.CardName} item");

            this.gameObject.SetActive(true);

        }
        protected override void Awake()
        {
            this.PhysicalProperties = new ItemPhysicalProperties(   mass: 20000f,
                                                                    playerDamageMult: 0f,
                                                                    collisionDamageThreshold: float.MaxValue,
                                                                    friction: 0.5f,
                                                                    impulseMult: 0.1f,
                                                                    forceMult: 2f);

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            this.Col.size = Vector3.one;
            this.Trig.radius = 40f;
            this.transform.localScale = 0.15f * Vector3.one;
        }

        protected internal override void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (collider2D?.GetComponent<Player>() != null
                && (collider2D?.GetComponent<Player>()?.data?.view?.IsMine ?? false))
            {
                this.CardObj?.GetComponentInChildren<CardVisuals>()?.ChangeSelected(true);
            }
            base.OnTriggerEnter2D(collider2D);
        }
        protected internal override void OnTriggerExit2D(Collider2D collider2D)
        {
            if (collider2D?.GetComponent<Player>() != null
                && (collider2D?.GetComponent<Player>()?.data?.view?.IsMine ?? false))
            {
                this.CardObj?.GetComponentInChildren<CardVisuals>()?.ChangeSelected(false);
            }
            base.OnTriggerExit2D(collider2D);
        }
        protected internal override void OnTriggerStay2D(Collider2D collider2D)
        {
            if (this.HasBeenTaken) { return; }
            if (collider2D?.GetComponent<Player>() != null
                && (collider2D?.GetComponent<Player>()?.data?.view?.IsMine ?? false))
            {
                if (Vector2.Distance(collider2D.GetComponent<Player>().data.playerVel.position, this.transform.position) < CollectionDistance)
                {
                    this.CheckPlayerCollect(collider2D.GetComponent<Player>());
                }
            }
            base.OnTriggerStay2D(collider2D);
        }
        protected override void FixedUpdate()
        {
            if (this.CardObj?.transform?.GetChild(0) != null)
            {
                this.Col.transform.localScale = Vector3.Scale(this.CardObj.GetComponentInChildren<BoxCollider2D>().transform.localScale, this.CardObj.transform.GetChild(0).localScale);
            }

            base.FixedUpdate();
        }

        protected internal void CheckPlayerCollect(Player player)
        {
            if (this.HasBeenTaken) { return; }
            else if (!player.data.CanHaveMoreCards()) { return; }
            else if (!ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, this.Card)) { return; }

            this.HasBeenTaken = true;
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, this.Card, false, "", 0, 0, true);
            Destroy(this.gameObject);
        }

        protected override void ReadSyncedData()
        {
        }

        protected override void SetDataToSync()
        {
        }

        protected override bool SyncDataNow()
        {
            return true;
        }
    }
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    class CardItemHandler : MonoBehaviour
    {
        public static readonly Vector3 PositionToCheck = new Vector3(100000f, 100000f, 0f);
        public static CardItemHandler Instance { get; private set; } = null;
        private CircleCollider2D Trigger => this.gameObject.GetComponent<CircleCollider2D>();
        private Rigidbody2D Rig => this.gameObject.GetComponent<Rigidbody2D>();

        void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            if (Instance != this) { Destroy(this); }
            this.Rig.isKinematic = true;
            this.Trigger.isTrigger = true;
            this.Trigger.radius = 1f;
            this.transform.position = PositionToCheck;
        }
        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider?.transform?.root?.GetComponent<CardInfo>() != null)
            {
                CardItem cardItem = this.FindMatchingCardItem(collider.transform.root.GetComponent<CardInfo>().cardName); 
                if (cardItem != null)
                {
                    // assign the card to the correct cardItem parent, then disable its collider
                    cardItem.CardObj = collider.transform.root.gameObject;
                    cardItem.CardObj.transform.SetParent(cardItem.transform, false);
                    cardItem.CardObj.transform.localPosition = Vector3.zero;
                    collider.enabled = false;
                }
                else
                {
                    DestroyImmediate(collider.gameObject);
                }
            }
        }
        CardItem FindMatchingCardItem(string cardName)
        {
            return this.transform.GetComponentsInChildren<CardItem>().ToList().Find(c => c.CardName == cardName && c.CardObj == null);
        }

    }
}
