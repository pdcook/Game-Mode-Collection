using GameModeCollection.Extensions;
using GameModeCollection.Objects;
using LocalZoom;
using MapEmbiggener.Controllers;
using ModdingUtils.MonoBehaviours;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Networking;
using UnityEngine;
using SoundImplementation;
using Sonigon;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_RiflePrefab
    {
        private static GameObject _Rifle = null;
        public static GameObject Rifle
        {
            get
            {
                if (_Rifle is null)
                {
                    _Rifle = new GameObject("A_Rifle", typeof(A_Rifle));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Rifle);
                }
                return _Rifle;
            }
        }
    }

    public class RifleCard : CustomCard
    {    
        // one shot, very slow rate of fire, sniper rifle available to both traitors and detectives

        internal static CardInfo Card = null;
        internal static string CardName => "Rifle";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Detective, TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_2, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_2 };
            statModifiers.AddObjectToPlayer = A_RiflePrefab.Rifle;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.GetOrAddComponent<RifleGun>();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
           if (player.gameObject.GetComponent<RifleGun>() != null)
           {
                Destroy(player.gameObject.GetComponent<RifleGun>());
           }
        }

        protected override string GetTitle()
        {
            return CardName;
        }
        protected override string GetDescription()
        {
            return "A tried and true precision weapon. Press [item 2] to switch to it.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_Rifle");

        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }

        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    stat = "Bullet Velocity",
                    amount = "High",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = true
                },

                new CardInfoStat()
                {
                    stat = "Damage",
                    amount = "Very High",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Fire Rate",
                    amount = "Very Low",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = false
                }
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
            RifleCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_Rifle : MonoBehaviour
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
                NetworkingManager.RPC(typeof(A_Rifle), nameof(RPCA_SwitchToRifle), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        private static void RPCA_SwitchToRifle(int playerID, bool switchTo)
        {
            if (switchTo) { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<RifleGun>()?.EnableRifle(); }
            else { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<RifleGun>()?.DisableRifle(); }
        }
    }
    public class RifleGun : ReversibleEffect
    {
        private const float BarrelLength = 5f;
        private const float BarrelWidth = 0.5f;

        private Vector3 OriginalScale;
        private Vector3 OriginalRightPos;
        private Vector3 OriginalLeftPos;
        private List<ObjectsToSpawn> OriginalObjectsToSpawn;
        private int NumCards;

        private SoundShotModifier SoundShotModifier;
        private SoundImpactModifier SoundImpactModifier;
        public override void OnAwake()
        {
            this.SetLivesToEffect(1);
            this.applyImmediately = false;

            base.OnAwake();
        }
        public override void OnStart()
        {
            // set sound effects
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("SniperShoot.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            SoundEvent shoot = ScriptableObject.CreateInstance<SoundEvent>();
            shoot.soundContainerArray[0] = soundContainer;

            this.SoundShotModifier = new SoundShotModifier() { single = shoot };

            AudioClip soundHitSurface = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("SniperHitSurface.ogg");
            SoundContainer soundContainer2 = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer2.setting.volumeIntensityEnable = true;
            soundContainer2.audioClip[0] = soundHitSurface;
            SoundEvent hitSurface = ScriptableObject.CreateInstance<SoundEvent>();
            hitSurface.soundContainerArray[0] = soundContainer2;
            AudioClip soundHitCharacter = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("SniperHitCharacter.ogg");
            SoundContainer soundContainer3 = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer3.setting.volumeIntensityEnable = true;
            soundContainer3.audioClip[0] = soundHitCharacter;
            SoundEvent hitCharacter = ScriptableObject.CreateInstance<SoundEvent>();
            hitCharacter.soundContainerArray[0] = soundContainer3;

            this.SoundImpactModifier = new SoundImpactModifier() { impactCharacter = hitCharacter, impactEnvironment = hitSurface };

            this.NumCards = this.data.currentCards.Count();

            this.gunAmmoStatModifier.maxAmmo_mult = 0;
            this.gunAmmoStatModifier.maxAmmo_add = 3;
            this.gunStatModifier.bursts_mult = 0;
            this.gunStatModifier.numberOfProjectiles_mult = 0;
            this.gunStatModifier.numberOfProjectiles_add = 1;
            this.gunStatModifier.damage_mult = 0f;
            this.gunStatModifier.damage_add = 2.1f;
            this.gunStatModifier.attackSpeed_mult = 0f;
            this.gunStatModifier.attackSpeed_add = 2f;
            this.gunAmmoStatModifier.reloadTimeMultiplier_mult = 0f;
            this.gunAmmoStatModifier.reloadTimeAdd_add = 5f;
            this.gunStatModifier.gravity_mult = 0f;
            this.gunStatModifier.projectileSpeed_mult = 0f;
            this.gunStatModifier.projectileSpeed_add = 100f;

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
                    this.DisableRifle();
                    this.EnableRifle();
                }
            }

            base.OnUpdate();
        }
        public override void OnOnDestroy()
        {
            this.DisableRifle();
            base.OnOnDestroy();
        }
        public void DisableRifle()
        {
            // things that can't be changed with ReversibleEffect

            // restore originals
            this.gun.objectsToSpawn = this.OriginalObjectsToSpawn.Concat(this.gun.objectsToSpawn).ToArray();
            this.gun.dontAllowAutoFire = this.data.currentCards.Any(c => (c.gameObject?.GetComponent<Gun>()?.dontAllowAutoFire ?? false));
            this.gun.soundGun.RemoveSoundShotModifier(this.SoundShotModifier);
            this.gun.soundGun.RemoveSoundImpactModifier(this.SoundImpactModifier);
            this.gun.soundGun.RefreshSoundModifiers();
            
            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;
            barrel.transform.localScale = this.OriginalScale;
            RightLeftMirrorSpring mirrorSpring = barrel.GetComponent<RightLeftMirrorSpring>();
            mirrorSpring.leftPos = this.OriginalLeftPos;
            mirrorSpring.SetFieldValue("rightPos", this.OriginalRightPos);

            this.ClearModifiers(false);

            // recalculate localzoom
            if (ControllerManager.CurrentCameraControllerID != MyCameraController.ControllerID) { return; }
            ((MyCameraController)ControllerManager.CurrentCameraController).ResetZoomLevel(this.player);

            GameModeCollection.instance.ExecuteAfterFrames(2, () => this.characterStatModifiers.WasUpdated());
        }

        public void EnableRifle()
        {
            // you can't cheese the attack speed by switching back and forth
            this.gun.sinceAttack = 0f;

            // things that can't be changed with ReversibleEffect

            // save originals
            this.OriginalObjectsToSpawn = this.gun.objectsToSpawn.ToList();

            // disable auto-fire (requires demonicpactpatch to reset properly)
            this.gun.dontAllowAutoFire = true; // will be reset by reading all of the cards the player has when this is removed
            this.gun.objectsToSpawn = new ObjectsToSpawn[] { };
            this.gun.soundGun.AddSoundShotModifier(this.SoundShotModifier);
            this.gun.soundGun.AddSoundImpactModifier(this.SoundImpactModifier);
            this.gun.soundGun.RefreshSoundModifiers();

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;
            this.OriginalScale = barrel.transform.localScale;
            barrel.transform.localScale = Vector3.Scale(new Vector3(BarrelLength, BarrelWidth, 1f), this.OriginalScale);
            RightLeftMirrorSpring mirrorSpring = barrel.GetComponent<RightLeftMirrorSpring>();
            this.OriginalLeftPos = mirrorSpring.leftPos;
            this.OriginalRightPos = (Vector3)mirrorSpring.GetFieldValue("rightPos");
            mirrorSpring.leftPos = Vector3.Scale(new Vector3(1f, BarrelLength * 0.9f, 1f), this.OriginalLeftPos);
            mirrorSpring.SetFieldValue("rightPos", Vector3.Scale(new Vector3(1f, BarrelLength * 0.9f, 1f), this.OriginalRightPos));

            this.ApplyModifiers();

            // recalculate localzoom
            if (ControllerManager.CurrentCameraControllerID != MyCameraController.ControllerID) { return; }
            ((MyCameraController)ControllerManager.CurrentCameraController).ResetZoomLevel(this.player);
        }
    }
}

