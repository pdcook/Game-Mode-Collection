using GameModeCollection.Extensions;
using HarmonyLib;
using ModdingUtils.Utils;
using Sonigon;
using System.Collections;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;
using GameModeCollection.Objects;
namespace GameModeCollection.Utils
{
    static class CardUtils
    {
        internal static void AddCardsToPlayer_ClientsideCardBar(Player player, CardInfo[] cards, bool silent = false)
        {
            RPCA_AddCardsToPlayer_ClientsideCardBar(player.playerID, cards.Select(c => c.cardName).ToArray(), silent);
        }
        internal static void Call_AddCardsToPlayer_ClientsideCardBar(Player player, CardInfo[] cards, bool silent = false)
        {
            NetworkingManager.RPC(typeof(CardUtils), nameof(RPCA_AddCardsToPlayer_ClientsideCardBar), player.playerID, cards.Select(c => c.cardName).ToArray(), silent);
        }
        [UnboundRPC]
        private static void RPCA_AddCardsToPlayer_ClientsideCardBar(int playerID, string[] cardNames, bool silent)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            if (player is null) { return; }
            if (cardNames.Any(c => Cards.instance.GetCardWithName(c) is null)) { return; }
            ModdingUtils.Utils.Cards.instance.AddCardsToPlayer(player, cardNames.Select(c => Cards.instance.GetCardWithName(c)).ToArray(), false, null, null, null, false);
            if (player.data.view.IsMine)
            {
                foreach (string cardName in cardNames)
                {
                    ClientsideAddToCardBar(player.playerID, Cards.instance.GetCardWithName(cardName), silent: silent);
                }
            }
        }
        internal static void AddCardToPlayer_ClientsideCardBar(Player player, CardInfo card, bool silent = false)
        {
            RPCA_AddCardToPlayer_ClientsideCardBar(player.playerID, card.cardName, silent);
        }
        internal static void Call_AddCardToPlayer_ClientsideCardBar(Player player, CardInfo card, bool silent = false)
        {
            NetworkingManager.RPC(typeof(CardUtils), nameof(RPCA_AddCardToPlayer_ClientsideCardBar), player.playerID, card.cardName, silent);
        }
        [UnboundRPC]
        private static void RPCA_AddCardToPlayer_ClientsideCardBar(int playerID, string cardName, bool silent)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            if (player is null) { return; }
            CardInfo card = Cards.instance.GetCardWithName(cardName);
            if (card is null) { return; }
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, card, false, "", 0, 0, false);
            if (player.data.view.IsMine)
            {
                ClientsideAddToCardBar(player.playerID, card, silent: silent);
            }
        }
        internal static void RemoveCardFromPlayer_ClientsideCardBar(Player player, CardInfo card, Cards.SelectionType selectionType, bool silent = true)
        {
            RPCA_RemoveCardFromPlayer_ClientsideCardBar(player.playerID, card.cardName, (byte)selectionType, silent);
        }
        internal static void Call_RemoveCardFromPlayer_ClientsideCardBar(Player player, CardInfo card, Cards.SelectionType selectionType, bool silent = true)
        {
            NetworkingManager.RPC(typeof(CardUtils), nameof(RPCA_RemoveCardFromPlayer_ClientsideCardBar), player.playerID, card.cardName, (byte)selectionType, silent);
        }
        [UnboundRPC]
        private static void RPCA_RemoveCardFromPlayer_ClientsideCardBar(int playerID, string cardName, byte selectionByte, bool silent)
        {
            Player player = PlayerManager.instance.GetPlayerWithID(playerID);
            if (player is null) { return; }
            CardInfo card = Cards.instance.GetCardWithName(cardName);
            if (card is null) { return; }
            int numCards = player.data.currentCards.Count();
            ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, card, (ModdingUtils.Utils.Cards.SelectionType)selectionByte, false);
            if (player.data.view.IsMine)
            {
                ModdingUtils.Utils.CardBarUtils.instance.PlayersCardBar(0).ClearBar();
            }
            GameModeCollection.instance.StartCoroutine(RestoreCardBarWhenReady(player, numCards-1, silent));

        }
        static IEnumerator RestoreCardBarWhenReady(Player player, int numCards, bool silent)
        {
            yield return new WaitUntil(() => player.data.currentCards.Count() == numCards);
            if (!player.data.view.IsMine) { yield break; }
            foreach (CardInfo card in player.data.currentCards)
            {
                ClientsideAddToCardBar(player.playerID, card, silent: silent);
            }
        }
        public static void ClientsideAddToCardBar(int playerID, CardInfo card, string twoLetterCode = "", bool silent = true)
        {
            if (!PlayerManager.instance.players.Find(p => p.playerID == playerID).data.view.IsMine) { return; }


            CardBar[] cardBars = (CardBar[])Traverse.Create(CardBarHandler.instance).Field("cardBars").GetValue();
            if (!silent) { SoundManager.Instance.Play(cardBars[0].soundCardPick, cardBars[0].transform); }

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
            gameObject.GetComponent<SelectableCardBarButton>().SetPlayerID(playerID);
            gameObject.gameObject.SetActive(true);
        }
    }
}
