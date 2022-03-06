using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using Photon.Pun;
using GameModeCollection.Utils;

namespace GameModeCollection.GameModes.TRT.Cards
{    
    static class A_GoldenDeaglePrefab
    {
        private static GameObject _GoldenDeagle = null;
        public static GameObject GoldenDeagle
        {
            get
            {
                if (_GoldenDeagle is null)
                {
                    _GoldenDeagle = new GameObject("A_GoldenDeagle", typeof(GoldenDeagleDealtDamageEffect));
                    UnityEngine.GameObject.DontDestroyOnLoad(_GoldenDeagle);
                }
                return _GoldenDeagle;
            }
        }
    }

    public class GoldenDeagleCard : CustomCard
    {    
        /// --> One time use, once it hits any player, it is destroyed
        /// --> If it shoots a traitor/killer, they will die instantly, no phoenix revives either
        /// --> If it shoots an innocent, the shooter will die instantly, no phoenix revives
        /// --> If it shoots a jester/swapper, BOTH players will be killed instantly, no phoenix revives AND the jester/swapper will NOT win

        internal static CardInfo Card = null;
        internal static string CardName => "Golden Deagle";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective };

            statModifiers.AddObjectToPlayer = A_GoldenDeaglePrefab.GoldenDeagle;
            // disable auto-fire (requires demonicpactpatch to reset properly)
            gun.dontAllowAutoFire = true;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            gun.bursts = 0;
            gun.numberOfProjectiles = 1;
            gun.objectsToSpawn = new ObjectsToSpawn[] { };

            GameObject spring = gun.transform.GetChild(1).gameObject;
            GameObject handle = spring.transform.GetChild(2).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;

            handle.GetComponent<SpriteMask>().enabled = false;
            handle.GetComponent<SpriteRenderer>().enabled = true;
            handle.GetComponent<SpriteRenderer>().color = new Color32(255, 215, 0, 255);
            barrel.GetComponent<SpriteMask>().enabled = false;
            barrel.GetComponent<SpriteRenderer>().enabled = true;
            barrel.GetComponent<SpriteRenderer>().color = new Color32(255, 215, 0, 255);
            handle.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
            barrel.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            
            GameObject spring = gun.transform.GetChild(1).gameObject;
            GameObject handle = spring.transform.GetChild(2).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;

            handle.GetComponent<SpriteMask>().enabled = true;
            handle.GetComponent<SpriteRenderer>().enabled = false;
            handle.GetComponent<SpriteRenderer>().color = Color.grey;
            barrel.GetComponent<SpriteMask>().enabled = true;
            barrel.GetComponent<SpriteRenderer>().enabled = false;
            barrel.GetComponent<SpriteRenderer>().color = Color.grey;

            handle.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Default");
            barrel.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Default");
        }

        protected override string GetTitle()
        {
            return CardName;
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
            return CardThemeColor.CardThemeColorType.FirepowerYellow;
        }
        public override string GetModName()
        {
            return "TRT";
        }
        internal static void Callback(CardInfo card)
        {
            GoldenDeagleCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class GoldenDeagleDealtDamageEffect : DealtDamageEffect
    {
        private const float ForceStatsEvery = 0.2f;
        private float Timer = 0f;
        private bool Spent = false;
        void Update()
        {
            this.Timer -= TimeHandler.deltaTime;
            if (this.Timer <= 0f && !this.Spent && this.GetComponentInParent<Holding>() != null)
            {
                this.Timer = ForceStatsEvery;

                Gun gun = this.GetComponentInParent<Holding>().holdable.GetComponent<Gun>();

                gun.bursts = 0;
                gun.numberOfProjectiles = 1;
                gun.objectsToSpawn = new ObjectsToSpawn[] { };

                GameObject spring = gun.transform.GetChild(1).gameObject;
                GameObject handle = spring.transform.GetChild(2).gameObject;
                GameObject barrel = spring.transform.GetChild(3).gameObject;

                handle.GetComponent<SpriteMask>().enabled = false;
                handle.GetComponent<SpriteRenderer>().enabled = true;
                handle.GetComponent<SpriteRenderer>().color = new Color32(255, 215, 0, 255);
                barrel.GetComponent<SpriteMask>().enabled = false;
                barrel.GetComponent<SpriteRenderer>().enabled = true;
                barrel.GetComponent<SpriteRenderer>().color = new Color32(255, 215, 0, 255);
                handle.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
                barrel.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("MostFront");
            }
        }
        public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer = null)
        {
            if (damagedPlayer is null) { return; }
            if (this.Spent) { return; }
            this.Spent = true;
            if (!this.gameObject.GetComponentInParent<Player>().data.view.IsMine) { return; }

            Player ownPlayer = this.gameObject.GetComponentInParent<Player>();

            if (selfDamage || (damagedPlayer.playerID == ownPlayer.playerID))
            {
                NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_KillPlayer), damage, ownPlayer.playerID, ownPlayer.playerID);
            }
            else
            {
                Alignment? damagedAlignment = RoleManager.GetPlayerAlignment(damagedPlayer);
                switch (damagedAlignment)
                {
                    case null:
                        return;
                    case Alignment.Traitor:
                        // instakill
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_KillPlayer), damage, damagedPlayer.playerID, ownPlayer.playerID);
                        break;
                    case Alignment.Killer:
                        // instakill
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_KillPlayer), damage, damagedPlayer.playerID, ownPlayer.playerID);
                        break;
                    case Alignment.Innocent:
                        // suicide
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_KillPlayer), damage, ownPlayer.playerID, ownPlayer.playerID);
                        break;
                    case Alignment.Chaos:
                        // both players die by suicide
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_KillPlayer), damage, ownPlayer.playerID, ownPlayer.playerID);
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_KillPlayer), damage, damagedPlayer.playerID, damagedPlayer.playerID);
                        break;
                    default:
                        break;
                }

            }
        }
        [UnboundRPC]
        private static void RPCA_KillPlayer(Vector2 damage, int playerIDToKill, int killingPlayerID)
        {
            // instakill, no revives
            Player playerToKill = PlayerManager.instance.GetPlayerWithID(playerIDToKill);
            Player killingPlayer = PlayerManager.instance.GetPlayerWithID(killingPlayerID);
            if (playerToKill is null) { return; }
            try
            {
                RoleManager.GetPlayerRole(playerToKill)?.OnKilledByPlayer(killingPlayer);
            }
            catch { }
            try
            {
                RoleManager.GetPlayerRole(killingPlayer)?.OnKilledPlayer(playerToKill);
            }
            catch { }
            playerToKill.data.lastSourceOfDamage = killingPlayer;
            if (playerToKill.data.view.IsMine)
            {
                playerToKill.data.view.RPC("RPCA_Die", RpcTarget.All, damage);
            }
            CardUtils.RemoveCardFromPlayer_ClientsideCardBar(killingPlayer, GoldenDeagleCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
        }
    }
}

