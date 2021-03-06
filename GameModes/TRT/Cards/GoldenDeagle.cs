using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using GameModeCollection.Objects.GameModeObjects.TRT;
using GameModeCollection.GameModes.TRT.RoundEvents;
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
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_2 };

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
            return "Kills traitors and killers instantly. But if you shoot an innocent, <b>YOU</b> will die instantly. If you shoot a jester or swapper, you both will die.\nPress [item 2] to switch to it.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_GoldenDeagle");
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
        public override void Callback()
        {
            this.gameObject.AddComponent<TRTCardSlotText>();
        }
        internal static void BuildCardCallback(CardInfo card)
        {
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
            this.SetLivesToEffect(1);
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
            this.gun.objectsToSpawn = this.OriginalObjectsToSpawn?.ToArray() ?? new ObjectsToSpawn[]{ };//.Concat(this.gun.objectsToSpawn).ToArray();
            this.gun.dontAllowAutoFire = this.data.currentCards.Any(c => (c.gameObject?.GetComponent<Gun>()?.dontAllowAutoFire ?? false));
            if (this.player.data.currentCards.Contains(SilencerCard.Card))
            {
                this.gun.GetData().silenced = true;
            }
            else
            {
                this.gun.GetData().silenced = false;
            }

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
            // you can't cheese the attack speed by switching back and forth
            this.gun.sinceAttack = 0f;

            // things that can't be changed with ReversibleEffect

            // save originals
            this.OriginalObjectsToSpawn = this.gun.objectsToSpawn.ToList();

            // disable auto-fire (requires demonicpactpatch to reset properly)
            this.gun.dontAllowAutoFire = true; // will be reset by reading all of the cards the player has when this is removed
            this.gun.objectsToSpawn = new ObjectsToSpawn[] { };
            this.gun.GetData().silenced = false;

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
                NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_GoldenGunKillPlayer), damage, ownPlayer.playerID, ownPlayer.playerID, (byte)GoldenDeagleEvent.Result.Suicide, ownPlayer.playerID);
            }
            else
            {
                Alignment? damagedAlignment = RoleManager.GetPlayerAlignment(damagedPlayer);
                switch (damagedAlignment)
                {
                    case null:
                        return;
                    case Alignment.Traitor:
                    case Alignment.Killer:
                        // instakill
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_GoldenGunKillPlayer), damage, damagedPlayer.playerID, ownPlayer.playerID, (byte)GoldenDeagleEvent.Result.Success, damagedPlayer.playerID);
                        break;
                    case Alignment.Innocent:
                        // suicide
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_GoldenGunKillPlayer), damage, ownPlayer.playerID, ownPlayer.playerID, (byte)GoldenDeagleEvent.Result.Fail, damagedPlayer.playerID);
                        break;
                    case Alignment.Chaos:
                        // both players die by suicide
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_GoldenGunKillPlayer), damage, ownPlayer.playerID, ownPlayer.playerID, (byte)GoldenDeagleEvent.Result.Chaos, damagedPlayer.playerID);
                        NetworkingManager.RPC(typeof(GoldenDeagleDealtDamageEffect), nameof(RPCA_GoldenGunKillPlayer), damage, damagedPlayer.playerID, damagedPlayer.playerID, (byte)GoldenDeagleEvent.Result.None, damagedPlayer.playerID);
                        break;
                    default:
                        break;
                }

            }
        }
        [UnboundRPC]
        private static void RPCA_GoldenGunKillPlayer(Vector2 damage, int playerIDToKill, int killingPlayerID, byte goldenGunEventResult, int targetPlayerID)
        {
            // instakill, no revives
            Player playerToKill = PlayerManager.instance.GetPlayerWithID(playerIDToKill);
            Player killingPlayer = PlayerManager.instance.GetPlayerWithID(killingPlayerID);
            if (playerToKill is null) { return; }

            // for logging
            
            Player targetPlayer = PlayerManager.instance.GetPlayerWithID(targetPlayerID);
            GoldenDeagleEvent.Result result = (GoldenDeagleEvent.Result)goldenGunEventResult;
            TRT_Role_Appearance targetAppearance = RoleManager.GetPlayerRole(targetPlayer).Appearance;
            switch (result)
            {
                case GoldenDeagleEvent.Result.Success:
                case GoldenDeagleEvent.Result.Fail:
                case GoldenDeagleEvent.Result.Chaos:
                case GoldenDeagleEvent.Result.Suicide:
                    RoundSummary.LogEvent(GoldenDeagleEvent.ID, killingPlayerID, result, targetAppearance);
                    break;
                case GoldenDeagleEvent.Result.None:
                default:
                    break;
            }
            RoundSummary.LogDamage(killingPlayer, playerToKill, playerToKill.data.health);
            
            playerToKill.data.lastSourceOfDamage = killingPlayer;
            if (playerToKill.data.view.IsMine)
            {
                playerToKill.data.view.RPC("RPCA_Die", RpcTarget.All, damage);
            }
            CardUtils.RemoveCardFromPlayer_ClientsideCardBar(killingPlayer, GoldenDeagleCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
        }
    }
}

