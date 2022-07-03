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
    static class A_JesterEmulatorPrefab
    {
        private static GameObject _JesterEmulator = null;
        public static GameObject JesterEmulator
        {
            get
            {
                if (_JesterEmulator is null)
                {
                    _JesterEmulator = new GameObject("A_JesterEmulator", typeof(A_JesterEmulator));
                    UnityEngine.GameObject.DontDestroyOnLoad(_JesterEmulator);
                }
                return _JesterEmulator;
            }
        }
    }

    public class JesterEmulatorCard : CustomCard
    {    
        // traitor card that allows the traitor to switch to a weapon that does 0 damage

        internal static CardInfo Card = null;
        internal static string CardName => "Jester Emulator";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_2, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_2 };

            statModifiers.AddObjectToPlayer = A_JesterEmulatorPrefab.JesterEmulator;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.GetOrAddComponent<JesterEmulatorGun>();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
           if (player.gameObject.GetComponent<JesterEmulatorGun>() != null)
           {
                Destroy(player.gameObject.GetComponent<JesterEmulatorGun>());
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
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_JesterEmulator");
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
            return CardThemeColor.CardThemeColorType.MagicPink;
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
            JesterEmulatorCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_JesterEmulator : MonoBehaviour
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
                NetworkingManager.RPC(typeof(A_JesterEmulator), nameof(RPCA_SwitchToJesterEmulator), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        private static void RPCA_SwitchToJesterEmulator(int playerID, bool switchTo)
        {
            if (switchTo) { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<JesterEmulatorGun>()?.EnableJesterEmulator(); }
            else { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<JesterEmulatorGun>()?.DisableJesterEmulator(); }
        }
    }
    public class JesterEmulatorGun : ReversibleEffect
    {
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

            // change color locally
            if (this.player.data.view.IsMine)
            {
                this.gunStatModifier.projectileColor = GM_TRT.JesterColor;
            }

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
                    this.DisableJesterEmulator();
                    this.EnableJesterEmulator();
                }
            }

            base.OnUpdate();
        }
        public override void OnOnDestroy()
        {
            this.DisableJesterEmulator();
            base.OnOnDestroy();
        }
        public void DisableJesterEmulator()
        {
            // things that can't be changed with ReversibleEffect

            // restore originals
            //this.gun.objectsToSpawn = this.OriginalObjectsToSpawn?.ToArray();//.Concat(this.gun.objectsToSpawn).ToArray();
            //this.gun.dontAllowAutoFire = this.data.currentCards.Any(c => (c.gameObject?.GetComponent<Gun>()?.dontAllowAutoFire ?? false));
            /*
            if (this.player.data.currentCards.Contains(SilencerCard.Card))
            {
                this.gun.GetData().silenced = true;
            }
            else
            {
                this.gun.GetData().silenced = false;
            }
            */

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
        }

        public void EnableJesterEmulator()
        {
            // you can't cheese the attack speed by switching back and forth
            this.gun.sinceAttack = 0f;

            // things that can't be changed with ReversibleEffect

            // save originals
            //this.OriginalObjectsToSpawn = this.gun.objectsToSpawn.ToList();

            // disable auto-fire (requires demonicpactpatch to reset properly)
            //this.gun.dontAllowAutoFire = true; // will be reset by reading all of the cards the player has when this is removed
            //this.gun.objectsToSpawn = new ObjectsToSpawn[] { };

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject handle = spring.transform.GetChild(2).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;

            handle.GetComponent<SpriteMask>().enabled = false;
            handle.GetComponent<SpriteRenderer>().enabled = true;
            barrel.GetComponent<SpriteMask>().enabled = false;
            barrel.GetComponent<SpriteRenderer>().enabled = true;
            // change the color locally
            if (this.player.data.view.IsMine)
            {
                handle.GetComponent<SpriteRenderer>().color = GM_TRT.JesterColor;
                barrel.GetComponent<SpriteRenderer>().color = GM_TRT.JesterColor;
            }

            this.ApplyModifiers();
        }
    }
}

