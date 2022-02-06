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
    public static class CardItemPrefab
	{
		private static GameObject _CardItem = null;

		public static GameObject CardItem
		{
			get
			{
				if (CardItemPrefab._CardItem == null)
				{
					GameObject card = GameObject.Instantiate(((CardInfo)typeof(Unbound).GetField("templateCard", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).gameObject);
					GameModeCollection.Log("Card Item Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(card);
					card.name = "CardItemPrefab";
					// must add required components (PhotonView) first
					card.GetOrAddComponent<PhotonView>();
					CardItem cardHandler = card.AddComponent<CardItem>();

					UnityEngine.GameObject.DestroyImmediate(card.GetComponent<GameCrownHandler>());

					PhotonNetwork.PrefabPool.RegisterPrefab(card.name, card);

					CardItemPrefab._CardItem = card;
				}
				return CardItemPrefab._CardItem;
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
                        CardItemPrefab.CardItem.name,
                        position,
                        rotation,
                        0,
						new object[] { card.cardName }
					);
			}

			yield break;
		}

		public string CardName { get; private set; }

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
			object[] data = info.photonView.InstantiationData;

			// data[0] is the name of the card
			this.CardName = (string)data[0];
			this.gameObject.name = this.CardName;

			//this.gameObject.GetComponentInChildren<CardVisuals>().AssignCardInfo(CardManager.GetCardInfoWithName(this.CardName));

			GameModeCollection.Log($"Instantiated {this.CardName} item");

		}

        protected override void Start()
        {
            base.Start();

			this.Trig.radius = 30f;
			this.transform.localScale = 0.25f * Vector3.one;
			//this.gameObject.GetComponentInChildren<CardVisuals>().transform.localScale = 0.003f * Vector3.one;
        }

        protected internal override void OnTriggerEnter2D(Collider2D collider2D)
        {
			if (collider2D?.GetComponent<Player>() != null) { this.GetComponentInChildren<CardVisuals>().ChangeSelected(true); }
            base.OnTriggerEnter2D(collider2D);
        }
        protected internal override void OnTriggerExit2D(Collider2D collider2D)
        {
			if (collider2D?.GetComponent<Player>() != null) { this.GetComponentInChildren<CardVisuals>().ChangeSelected(false); }
            base.OnTriggerExit2D(collider2D);
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
}
