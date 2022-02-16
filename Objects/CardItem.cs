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
using HarmonyLib;
using TMPro;
using Sonigon;

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
    [RequireComponent(typeof(ObjectDamagable))]
    [RequireComponent(typeof(CardItemHealth))]
    class CardItem : DamagableNetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
    {
        internal static IEnumerator MakeCardItem(CardInfo card, Vector3 position, Quaternion rotation, Vector2 velocity = default, float angularVelocity = 0f, float maxHealth = -1f, bool requireInteract = true)
        {
            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                // create item
                PhotonNetwork.Instantiate(
                        CardItemPrefabs.CardItem.name,
                        position,
                        rotation,
                        0,
                        new object[] { card.cardName, velocity, angularVelocity, maxHealth, requireInteract }
                    );

                // create card
                PhotonNetwork.Instantiate(card.name, CardItemHandler.PositionToCheck, Quaternion.identity, 0);

            }

            yield break;
        }
        public bool HasBeenTaken { get; private set; } = false;
        public bool RequiresInteraction { get; private set; } = false;
        public CardInfo Card { get; private set; }
        public string CardName { get; private set; }
        public GameObject CardObj { get; internal set; } = null;
        public const float CollectionDistance = 2f;

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] data = info.photonView.InstantiationData;

            // data[0] is the name of the card
            this.CardName = (string)data[0];
            // data[1] is the starting velocity of the card
            Vector2 velocty = (Vector2)data[1];
            // data[2] is the starting angular velocity of the card
            float angularVelocity = (float)data[2];
            // data[3] is the maximum health of the card - if it is -1, then it is unkillable
            float health = (float)data[3];
            // data[4] is the bool to determine if the card must be interacted with (F or ~ by default) to be picked up
            this.RequiresInteraction = (bool)data[4];
            this.Health.MaxHealth = health > 0f ? health : float.MaxValue;
            this.Health.Revive();

            this.Card = CardManager.GetCardInfoWithName(this.CardName);

            this.gameObject.name = $"{this.CardName} Item";

            this.gameObject.transform.SetParent(CardItemPrefabs.CardItemHandler.transform);

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

            this.gameObject.GetOrAddComponent<ObjectDamagable>();
            this.gameObject.GetOrAddComponent<CardItemHealth>();

            this.Col.size = Vector3.one;
            this.Trig.radius = 40f;
            this.transform.localScale = 0.15f * Vector3.one;
        }

        protected internal override void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (collider2D?.GetComponent<Player>() != null
                && (collider2D?.GetComponent<Player>()?.data?.view?.IsMine ?? false)
                && !collider2D.GetComponent<Player>().data.dead
                && this.CanSeePlayer(collider2D.GetComponent<Player>()))
            {
                this.CardObj?.GetComponentInChildren<CardVisuals>()?.ChangeSelected(true);
                this.CardObj?.PlayAllAnimators();
            }
            base.OnTriggerEnter2D(collider2D);
        }
        protected internal override void OnTriggerExit2D(Collider2D collider2D)
        {
            if (collider2D?.GetComponent<Player>() != null
                && (collider2D?.GetComponent<Player>()?.data?.view?.IsMine ?? false)
                && !collider2D.GetComponent<Player>().data.dead)
            {
                this.CardObj?.GetComponentInChildren<CardVisuals>()?.ChangeSelected(false);
                this.CardObj?.PauseAllAnimators();
            }
            base.OnTriggerExit2D(collider2D);
        }
        protected internal override void OnTriggerStay2D(Collider2D collider2D)
        {
            if (this.HasBeenTaken) { return; }
            if (collider2D?.GetComponent<Player>() != null
                && (collider2D?.GetComponent<Player>()?.data?.view?.IsMine ?? false)
                && !collider2D.GetComponent<Player>().data.dead)
            {
                if (this.CanSeePlayer(collider2D.GetComponent<Player>()))
                {
                    this.CardObj?.GetComponentInChildren<CardVisuals>()?.ChangeSelected(true);
                    if(this.RequiresInteraction && !collider2D.GetComponent<Player>().data.playerActions.Interact().WasPressed)
                    {
                        return;
                    }
                    if (Vector2.Distance(collider2D.GetComponent<Player>().data.playerVel.position, this.transform.position) < CollectionDistance)
                    {
                        this.CheckPlayerCollect(collider2D.GetComponent<Player>());
                    }
                }
                else
                {
                    this.CardObj?.GetComponentInChildren<CardVisuals>()?.ChangeSelected(false);
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
            else if (player.data.dead) { return; }
            else if (!player.data.CanHaveMoreCards()) { return; }
            else if (!ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(player, this.Card)) { return; }

            this.View.RPC(nameof(RPCA_AddCardToPlayer), RpcTarget.All, player.playerID);
            CardItemHandler.ClientsideAddToCardBar(player.playerID, this.Card);
        }
        [PunRPC]
        private void RPCA_AddCardToPlayer(int playerID)
        {
            this.HasBeenTaken = true;
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(PlayerManager.instance.players.Find(p => p.playerID == playerID), this.Card, false, "", 0, 0, false);
            this.RPCA_DestroyCardItem();
        }

		private bool CanSeePlayer(Player player)
        {
			RaycastHit2D[] array = Physics2D.RaycastAll(this.transform.position, (player.data.playerVel.position - (Vector2)this.transform.position).normalized, Vector2.Distance(this.transform.position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform
					&& !array[i].transform.root.GetComponent<SpawnedAttack>()
					&& !array[i].transform.root.GetComponent<Player>()
					&& !array[i].transform.root.GetComponent<CardItemHandler>()
					)
				{
					return false;
				}
			}
			return true;
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
        public void CallDestroy()
        {
            this.View.RPC(nameof(RPCA_DestroyCardItem), RpcTarget.All);
        }
        [PunRPC]
        private void RPCA_DestroyCardItem()
        {
            if (this.View.IsMine)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
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

        public void DestroyAllCardItems()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Destroy(this.transform.GetChild(i).gameObject);
            }
        }

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
            this.Rig.velocity = Vector2.zero;
        }
        void Update()
        {
            this.Rig.isKinematic = true;
            this.Trigger.isTrigger = true;
            this.Trigger.radius = 1f;
            this.Rig.position = PositionToCheck;
            this.Rig.velocity = Vector2.zero;
        }
        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider?.transform?.root?.GetComponent<CardInfo>() != null)
            {
                CardItem cardItem = this.FindMatchingCardItem(collider.transform.root.GetComponent<CardInfo>().cardName); 
                if (cardItem != null)
                {
                    GameModeCollection.Log($"Assigning {collider.transform.root.GetComponent<CardInfo>().cardName} to item.");
                    // assign the card to the correct cardItem parent, then disable its collider
                    cardItem.CardObj = collider.transform.root.gameObject;
                    cardItem.CardObj.transform.SetParent(cardItem.transform, false);
                    cardItem.CardObj.transform.localPosition = Vector3.zero;
                    cardItem.CardObj.PauseAllAnimators();
                    collider.enabled = false;
                }
                else
                {
                    GameModeCollection.LogWarning($"Destroying {collider.transform.root.GetComponent<CardInfo>().cardName}, as there is no card item for it to be assigned to.");
                    Destroy(collider.gameObject.transform.root.gameObject);
                }
            }
        }
        CardItem FindMatchingCardItem(string cardName)
        {
            return this.transform.GetComponentsInChildren<CardItem>().ToList().Find(c => c.CardName == cardName && c.CardObj == null);
        }
        public static void ClientsideAddToCardBar(int playerID, CardInfo card, string twoLetterCode = "")
        {
            if (!PlayerManager.instance.players.Find(p => p.playerID == playerID).data.view.IsMine) { return; }


            CardBar[] cardBars = (CardBar[])Traverse.Create(CardBarHandler.instance).Field("cardBars").GetValue();
            SoundManager.Instance.Play(cardBars[0].soundCardPick, cardBars[0].transform);

            Traverse.Create(cardBars[0]).Field("ci").SetValue(card);
            GameObject source = (GameObject)Traverse.Create(cardBars[playerID]).Field("source").GetValue();
            GameObject topBarSource = (GameObject)Traverse.Create(cardBars[0]).Field("source").GetValue();
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(source, topBarSource.transform.position, topBarSource.transform.rotation, topBarSource.transform.parent);
            gameObject.transform.localScale = Vector3.one;
            string text = card.cardName;
            if (twoLetterCode != "") { text = twoLetterCode; }
            text = text.Substring(0, 2);
            string text2 = text[0].ToString().ToUpper();
            if (text.Length > 1)
            {
                string str = text[1].ToString().ToLower();
                text = text2 + str;
            }
            else
            {
                text = text2;
            }
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
            Traverse.Create(gameObject.GetComponent<CardBarButton>()).Field("card").SetValue(card);
            gameObject.gameObject.SetActive(true);
        }
    }
    class CardItemHealth : ObjectHealthHandler
    {
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
            GamefeelManager.GameFeel(deathDirection.normalized * 1f);
            DeathEffect deathEffect = GameObject.Instantiate(killingPlayer.data.healthHandler.deathEffect, this.transform.position, this.transform.rotation).GetComponent<DeathEffect>();
            deathEffect.gameObject.transform.localScale = Vector3.one;
            deathEffect.PlayDeath(killingPlayer.GetTeamColors().color, killingPlayer.data.playerVel, deathDirection, -1);
            SoundManager.Instance.Play(killingPlayer.data.healthHandler.soundDie, this.transform);
            this.GetComponent<CardItem>().CallDestroy();
        }
    }
}
