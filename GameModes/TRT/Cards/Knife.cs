using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using System.Collections;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_KnifePrefab
    {
        private static GameObject _Knife = null;
        public static GameObject Knife
        {
            get
            {
                if (_Knife is null)
                {
                    _Knife = new GameObject("A_Knife", typeof(A_Knife));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Knife);
                }
                return _Knife;
            }
        }
    }
    public class KnifeCard : CustomCard
    {
        internal static CardInfo Card = null;
        /*
         * TRT traitor shop card that allows the player to instakill a (very) nearby target by pressing [item 2]
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_2 };
            statModifiers.AddObjectToPlayer = A_KnifePrefab.Knife;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard()
        {
        }

        protected override string GetTitle()
        {
            return "Knife";
        }
        protected override string GetDescription()
        {
            return "";
        }

        protected override GameObject GetCardArt()
        {
            return null;
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DestructiveRed;
        }
        public override string GetModName()
        {
            return "TRT";
        }
        internal static void Callback(CardInfo card)
        {
            card.gameObject.AddComponent<TRTCardSlotText>();
            KnifeCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_Knife : MonoBehaviour
    {
        // TODO: add knife asset that replaces the player's gun when stabbing

        // the knife is consumed on kill
        // it will instakill the stabbed player
        // after stabbing, the player's gun will be a red knife for a short time, and be unable to shoot
        private const float Radius = 2.5f;
        private const float StabDelay = 1f;
        private float StabTimer = 0f;
        private bool HasStabbed = false;
        private Player Player;
        void Start()
        {
            this.HasStabbed = false;
            this.StabTimer = 0f;
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine) { return; }

            this.StabTimer -= TimeHandler.deltaTime;
            if (!this.HasStabbed && this.StabTimer <= 0f && this.Player.data.playerActions.ItemWasPressed(2))
            {
                this.StabTimer = StabDelay;
                int playerID = -1;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(this.Player.transform.position, Radius);
                foreach (Collider2D collider in colliders)
                {
                    if (collider.transform.root.GetComponent<Player>() != null)
                    {
                        playerID = collider.transform.root.GetComponent<Player>().playerID;
                        if (playerID == this.Player.playerID) { playerID = -1; continue; }
                        break;
                    }
                }
                NetworkingManager.RPC(typeof(A_Knife), nameof(RPCA_TRT_Knife), this.Player.playerID, playerID);
                if (playerID != -1)
                {
                    this.HasStabbed = true;
                    Destroy(this.gameObject);
                }
            }
        }
        [UnboundRPC]
        private static void RPCA_TRT_Knife(int stabbingPlayerID, int stabbedPlayerID)
        {
            GameModeCollection.instance.StartCoroutine(IMakeGunKnife(stabbingPlayerID));
            if (stabbedPlayerID == -1) { return; }
            Player stabbingPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == stabbingPlayerID);
            Player stabbedPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == stabbedPlayerID);
            if (stabbedPlayer is null || stabbingPlayer is null) { return; }
            stabbedPlayer.data.healthHandler.DoDamage(10000f * Vector2.up, stabbedPlayer.transform.position, Color.white, null, stabbingPlayer, false, true, true);
            ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(stabbingPlayer, KnifeCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest, false);
        }
        static IEnumerator IMakeGunKnife(int playerID)
        {
            MakeGunKnife(playerID, true);
            yield return new WaitForSecondsRealtime(StabDelay);
            MakeGunKnife(playerID, false);
            yield break;
        }
        static void MakeGunKnife(int playerID, bool knife)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            Gun gun = player.GetComponent<Holding>().holdable.GetComponent<Gun>();
            GameObject spring = gun.transform.GetChild(1).gameObject;
            GameObject handle = spring.transform.GetChild(2).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;

            gun.enabled = !knife;

            handle.GetComponent<SpriteMask>().enabled = !knife;
            //handle.GetComponent<SpriteRenderer>().enabled = knife;
            //handle.GetComponent<SpriteRenderer>().color = Color.black;
            barrel.GetComponent<SpriteMask>().enabled = !knife;
            barrel.GetComponent<SpriteRenderer>().enabled = knife;
            barrel.GetComponent<SpriteRenderer>().color = Color.red;
        }

    }
}

