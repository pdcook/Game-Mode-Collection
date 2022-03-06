using ModdingUtils.Utils;
using UnboundLib;
using UnboundLib.Networking;
using GameModeCollection.Extensions;
namespace GameModeCollection.Utils
{
    static class CardUtils
    {
        internal static void RemoveCardFromPlayer_ClientsideCardBar(Player player, CardInfo card, Cards.SelectionType selectionType)
        {
            RPCA_RemoveCardFromPlayer_ClientsideCardBar(player.playerID, card.cardName, (byte)selectionType);
        }
        internal static void Call_RemoveCardFromPlayer_ClientsideCardBar(Player player, CardInfo card, Cards.SelectionType selectionType)
        {
            NetworkingManager.RPC(typeof(CardUtils), nameof(RPCA_RemoveCardFromPlayer_ClientsideCardBar), player.playerID, card.cardName, (byte)selectionType);
        }
        [UnboundRPC]
        private static void RPCA_RemoveCardFromPlayer_ClientsideCardBar(int playerID, string cardName, byte selectionByte)
        {
            Player player = PlayerManager.instance.GetPlayerWithID(playerID);
            if (player is null) { return; }
            CardInfo card = Cards.instance.GetCardWithName(cardName);
            if (card is null) { return; }
            ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, card, (ModdingUtils.Utils.Cards.SelectionType)selectionByte, false);
            if (player.data.view.IsMine)
            {
                ModdingUtils.Utils.CardBarUtils.instance.PlayersCardBar(0).ClearBar();
            }

        }
    }
}
