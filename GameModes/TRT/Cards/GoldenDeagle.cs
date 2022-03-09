using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.Objects;
using UnboundLib.Networking;
using UnboundLib;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using GameModeCollection.Utils;
using ModdingUtils.MonoBehaviours;

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
                    _GoldenDeagle = new GameObject("A_GoldenDeagle", typeof(A_GoldenDeagle));
                    UnityEngine.GameObject.DontDestroyOnLoad(_GoldenDeagle);
                }
                return _GoldenDeagle;
            }
        }
        private static GameObject _GoldenDeagleDealtDamageEffect = null;
        public static GameObject GoldenDeagleDealtDamageEffect
        {
            get
            {
                if (_GoldenDeagleDealtDamageEffect is null)
                {
                    _GoldenDeagleDealtDamageEffect = new GameObject("A_GoldenDeagleDealtDamageEffect", typeof(GoldenDeagleDealtDamageEffect));
                    UnityEngine.GameObject.DontDestroyOnLoad(_GoldenDeagleDealtDamageEffect);
                }
                return _GoldenDeagleDealtDamageEffect;
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
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Slot_2, CardItem.IgnoreMaxCardsCategory };

            statModifiers.AddObjectToPlayer = A_GoldenDeaglePrefab.GoldenDeagle;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.GetOrAddComponent<GoldenDeagleGun>();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
           if (player.gameObject.GetComponent<GoldenDeagleGun>() != null)
           {
                Destroy(player.gameObject.GetComponent<GoldenDeagleGun>());
           }
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
            return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_GOLDENGUN");
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
        public override bool GetEnabled()
        {
            return false;
        }
        internal static void Callback(CardInfo card)
        {
            card.gameObject.AddComponent<TRTCardSlotText>();
            GoldenDeagleCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_GoldenDeagle : MonoBehaviour
    {
        Player Player;
        private const float SwitchDelay = 0.5f;
        private float Timer = 0f;
        private bool IsOut = false;
        void Start()
        {
            this.IsOut = false;
            this.Player = this.GetComponentInParent<Player>();
            this.Timer = 0f;
        }
        void Update()
        {
            this.Timer -= Time.deltaTime;
            if ((this.Player?.data?.view?.IsMine ?? false) && this.Timer <= 0f && !this.Player.data.dead && this.Player.data.playerActions.ItemWasPressed(2))
            {
                this.Timer = SwitchDelay;
                this.IsOut = !this.IsOut;
                NetworkingManager.RPC(typeof(A_GoldenDeagle), nameof(RPCA_SwitchToGoldenDeagle), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        private static void RPCA_SwitchToGoldenDeagle(int playerID, bool switchTo)
        {
            if (switchTo) { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<GoldenDeagleGun>()?.EnableGoldenDeagle(); }
            else { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<GoldenDeagleGun>()?.DisableGoldenDeagle(); }
        }
    }
    public class GoldenDeagleGun : ReversibleEffect
    {
        private List<ObjectsToSpawn> OriginalObjectsToSpawn;
        private int NumCards;
        public override void OnAwake()
        {
            this.SetLivesToEffect(int.MaxValue);
            this.applyImmediately = false;

            base.OnAwake();
        }
        public override void OnStart()
        {
            this.NumCards = this.data.currentCards.Count();

            this.gunAmmoStatModifier.maxAmmo_mult = 0;
            this.gunAmmoStatModifier.maxAmmo_add = 2;
            this.gunStatModifier.bursts_mult = 0;
            this.gunStatModifier.numberOfProjectiles_mult = 0;
            this.gunStatModifier.numberOfProjectiles_add = 1;
            this.gunStatModifier.damage_mult = 0f;
            this.gunStatModifier.damage_add = 0.001f; // damage can't be 0 since HealthHandler::DoDamage would instantly return
            this.gunStatModifier.projectileColor = new Color32(255, 215, 0, 255);

            this.characterStatModifiersModifier.objectsToAddToPlayer = new List<GameObject>() { A_GoldenDeaglePrefab.GoldenDeagleDealtDamageEffect };


            base.OnStart();
        }
        public override void OnUpdate()
        {
            // if the player collects a card, re-apply these stats, if they were currently applied
            if (this.data.currentCards.Count() != this.NumCards)
            {
                this.NumCards = this.data.currentCards.Count();
                if ((bool)this.GetFieldValue("modifiersActive"))
                {
                    this.DisableGoldenDeagle();
                    this.EnableGoldenDeagle();
                }
            }

            base.OnUpdate();
        }
        public override void OnOnDestroy()
        {
            this.DisableGoldenDeagle();
            base.OnOnDestroy();
        }
        public void DisableGoldenDeagle()
        {
            // things that can't be changed with ReversibleEffect

            // restore originals
            this.gun.objectsToSpawn = this.OriginalObjectsToSpawn.Concat(this.gun.objectsToSpawn).ToArray();
            this.gun.dontAllowAutoFire = this.data.currentCards.Any(c => (c.gameObject?.GetComponent<Gun>()?.dontAllowAutoFire ?? false));

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject handle = spring.transform.GetChild(2).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;

            // localzoom's shader needs these to stay the way they are, instead we just restore the color
            handle.GetComponent<SpriteMask>().enabled = false;
            handle.GetComponent<SpriteRenderer>().enabled = true;
            barrel.GetComponent<SpriteMask>().enabled = false;
            barrel.GetComponent<SpriteRenderer>().enabled = true;
            SetTeamColor.TeamColorThis(this.gun.gameObject, this.player.GetTeamColors());

            this.ClearModifiers(false);

            GameModeCollection.instance.ExecuteAfterFrames(2, () => this.characterStatModifiers.WasUpdated());
        }

        public void EnableGoldenDeagle()
        {
            // things that can't be changed with ReversibleEffect

            // save originals
            this.OriginalObjectsToSpawn = this.gun.objectsToSpawn.ToList();

            // disable auto-fire (requires demonicpactpatch to reset properly)
            this.gun.dontAllowAutoFire = true; // will be reset by reading all of the cards the player has when this is removed
            this.gun.objectsToSpawn = new ObjectsToSpawn[] { };

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject handle = spring.transform.GetChild(2).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;

            handle.GetComponent<SpriteMask>().enabled = false;
            handle.GetComponent<SpriteRenderer>().enabled = true;
            handle.GetComponent<SpriteRenderer>().color = new Color32(255, 215, 0, 255);
            barrel.GetComponent<SpriteMask>().enabled = false;
            barrel.GetComponent<SpriteRenderer>().enabled = true;
            barrel.GetComponent<SpriteRenderer>().color = new Color32(255, 215, 0, 255);

            this.ApplyModifiers();
        }
    }
    public class GoldenDeagleDealtDamageEffect : DealtDamageEffect
    {
        private bool Spent = false;
        public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer = null)
        {
            if (damagedPlayer is null) { return; }
            if (this.Spent) { return; }
            if (damage.sqrMagnitude > 0.01f) { return; } // detect if the player shot a bullet and then very quickly switched to the golden deagle
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
            playerToKill.data.lastSourceOfDamage = killingPlayer;
            if (playerToKill.data.view.IsMine)
            {
                playerToKill.data.view.RPC("RPCA_Die", RpcTarget.All, damage);
            }
            CardUtils.RemoveCardFromPlayer_ClientsideCardBar(killingPlayer, GoldenDeagleCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
        }
    }
}

