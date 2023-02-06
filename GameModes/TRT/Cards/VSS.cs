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
using Sonigon;
using Sonigon.Internal;
using SoundImplementation;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_VSSPrefab
    {
        private static GameObject _VSS = null;
        public static GameObject VSS
        {
            get
            {
                if (_VSS is null)
                {
                    _VSS = new GameObject("A_VSS", typeof(A_VSS));
                    UnityEngine.GameObject.DontDestroyOnLoad(_VSS);
                }
                return _VSS;
            }
        }
    }

    public class VSSCard : CustomCard
    {    
        // high (not one-shot) damage, slow rate of fire, SILENCED sniper rifle available to traitors

        internal static CardInfo Card = null;
        internal static string CardName => "VSS Vintorez";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_2, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_2 };
            statModifiers.AddObjectToPlayer = A_VSSPrefab.VSS;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.GetOrAddComponent<VSSGun>();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
           if (player.gameObject.GetComponent<VSSGun>() != null)
           {
                Destroy(player.gameObject.GetComponent<VSSGun>());
           }
        }

        protected override string GetTitle()
        {
            return CardName;
        }
        protected override string GetDescription()
        {
            return "A Traitor's <b>silenced</b> sniper rifle. Press [item 2] to switch to it.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_VSS");
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
                    amount = "High",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Fire Rate",
                    amount = "Low",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = false
                },
                new CardInfoStat()
                {
                    stat = "Movement Speed When Scoped",
                    amount = "Low",
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
        public override void Callback()
        {
            this.gameObject.AddComponent<TRTCardSlotText>();
        }
        internal static void BuildCardCallback(CardInfo card)
        {
            VSSCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_VSS : MonoBehaviour
    {
        Player Player;
        private const float SwitchDelay = 0.2f;
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
                NetworkingManager.RPC(typeof(A_VSS), nameof(RPCA_SwitchToVSS), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        private static void RPCA_SwitchToVSS(int playerID, bool switchTo)
        {
            if (switchTo) { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<VSSGun>()?.EnableVSS(); }
            else { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<VSSGun>()?.DisableVSS(); }
        }
    }
    public class VSSGun : ReversibleEffect
    {
        private const float BarrelLength = 5f;
        private const float BarrelWidth = 1f;

        private const float VolumeMultiplier = 0.5f; // multiplier for the impact sound volume

        private Vector3? OriginalScale = null;
        private Vector3? OriginalRightPos = null;
        private Vector3? OriginalLeftPos = null;
        private List<ObjectsToSpawn> OriginalObjectsToSpawn;
        private bool OriginalSilence;
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
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("VSSShoot.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            SoundEvent shoot = ScriptableObject.CreateInstance<SoundEvent>();
            shoot.soundContainerArray[0] = soundContainer;

            this.SoundShotModifier = ScriptableObject.CreateInstance<SoundShotModifier>();
            this.SoundShotModifier.single = shoot;

            AudioClip soundHitSurface = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("SniperHitSurface.ogg");
            SoundContainer soundContainer2 = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer2.setting.volumeIntensityEnable = true;
            soundContainer2.setting.intensityMultiplier = VolumeMultiplier;
            soundContainer2.audioClip[0] = soundHitSurface;
            SoundEvent hitSurface = ScriptableObject.CreateInstance<SoundEvent>();
            hitSurface.soundContainerArray[0] = soundContainer2;
            AudioClip soundHitCharacter = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("SniperHitCharacter.ogg");
            SoundContainer soundContainer3 = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer3.setting.volumeIntensityEnable = true;
            soundContainer3.setting.intensityMultiplier = VolumeMultiplier;
            soundContainer3.audioClip[0] = soundHitCharacter;
            SoundEvent hitCharacter = ScriptableObject.CreateInstance<SoundEvent>();
            hitCharacter.soundContainerArray[0] = soundContainer3;

            this.SoundImpactModifier = ScriptableObject.CreateInstance<SoundImpactModifier>();
            this.SoundImpactModifier.impactCharacter = hitCharacter;
            this.SoundImpactModifier.impactEnvironment = hitSurface;

            this.NumCards = this.data.currentCards.Count();

            this.gunAmmoStatModifier.maxAmmo_mult = 0;
            this.gunAmmoStatModifier.maxAmmo_add = 5;
            this.gunStatModifier.bursts_mult = 0;
            this.gunStatModifier.numberOfProjectiles_mult = 0;
            this.gunStatModifier.numberOfProjectiles_add = 1;
            this.gunStatModifier.damage_mult = 0f;
            this.gunStatModifier.damage_add = 1.2f;
            this.gunStatModifier.attackSpeed_mult = 0f;
            this.gunStatModifier.attackSpeed_add = 1f;
            this.gunAmmoStatModifier.reloadTimeMultiplier_mult = 0f;
            this.gunAmmoStatModifier.reloadTimeAdd_add = 3.5f;
            this.gunStatModifier.gravity_mult = 0f;
            this.gunStatModifier.projectileSpeed_mult = 0f;
            this.gunStatModifier.projectileSpeed_add = 100f;
            this.gunStatModifier.reflects_add = 0;
            this.gunStatModifier.reflects_mult = 0;
            /*
            this.gunStatModifier.randomBounces_add = 0;
            this.gunStatModifier.randomBounces_mult = 0;
            this.gunStatModifier.smartBounce_add = 0;
            this.gunStatModifier.smartBounce_mult = 0;
            */

            this.characterStatModifiersModifier.movementSpeed_mult = 0.85f;

            base.OnStart();
        }
        public override void OnUpdate()
        {
            // if the player collects a card, re-apply these stats, if they were currently applied
            if (this.data.currentCards.Count() != this.NumCards)
            {
                this.NumCards = this.data.currentCards.Count();
                if (this.modifiersActive)
                {
                    this.DisableVSS();
                    this.EnableVSS();
                }
            }

            base.OnUpdate();
        }
        public override void OnOnDestroy()
        {
            this.DisableVSS();
            base.OnOnDestroy();
        }
        public void DisableVSS()
        {
            // things that can't be changed with ReversibleEffect

            // restore originals
            this.gun.objectsToSpawn = this.OriginalObjectsToSpawn?.ToArray() ?? new ObjectsToSpawn[]{ };//.Concat(this.gun.objectsToSpawn).ToArray();
            this.gun.GetData().silenced = this.OriginalSilence;
            this.gun.dontAllowAutoFire = this.data.currentCards.Any(c => (c.gameObject?.GetComponent<Gun>()?.dontAllowAutoFire ?? false));
            //this.gun.soundDisableRayHitBulletSound = false; // the vanilla game never modifies this
            this.gun.soundGun.RemoveSoundShotModifier(this.SoundShotModifier);
            this.gun.soundGun.RemoveSoundImpactModifier(this.SoundImpactModifier);
            this.gun.soundGun.RefreshSoundModifiers();

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;
            if (this.OriginalScale.HasValue) { barrel.transform.localScale = this.OriginalScale.Value; }
            RightLeftMirrorSpring mirrorSpring = barrel.GetComponent<RightLeftMirrorSpring>();
            if (this.OriginalLeftPos.HasValue) { mirrorSpring.leftPos = this.OriginalLeftPos.Value; }
            if (this.OriginalRightPos.HasValue) { mirrorSpring.SetFieldValue("rightPos", this.OriginalRightPos.Value); }

            this.ClearModifiers(false);

            // recalculate localzoom
            if (ControllerManager.CurrentCameraControllerID != MyCameraController.ControllerID) { return; }
            ((MyCameraController)ControllerManager.CurrentCameraController).ResetZoomLevel(this.player);

            GameModeCollection.instance.ExecuteAfterFrames(2, () => this.stats.WasUpdated());
        }

        public void EnableVSS()
        {

            // you can't cheese the attack speed by switching back and forth
            this.gun.sinceAttack = 0f;

            // things that can't be changed with ReversibleEffect

            // save originals
            this.OriginalObjectsToSpawn = this.gun.objectsToSpawn.ToList();
            this.OriginalSilence = this.gun.GetData().silenced;

            // disable auto-fire (requires demonicpactpatch to reset properly)
            this.gun.dontAllowAutoFire = true; // will be reset by reading all of the cards the player has when this is removed
            this.gun.objectsToSpawn = new ObjectsToSpawn[] { };
            //this.gun.soundDisableRayHitBulletSound = true;
            this.gun.GetData().silenced = true;

            this.gun.soundGun.AddSoundShotModifier(this.SoundShotModifier);
            
            // impact sound always plays, but is quieter for the VSS
            this.gun.soundGun.AddSoundImpactModifier(this.SoundImpactModifier);
            this.gun.soundGun.RefreshSoundModifiers();

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;
            this.OriginalScale = barrel.transform.localScale;
            barrel.transform.localScale = Vector3.Scale(new Vector3(BarrelLength, BarrelWidth, 1f), this.OriginalScale.Value);
            RightLeftMirrorSpring mirrorSpring = barrel.GetComponent<RightLeftMirrorSpring>();
            this.OriginalLeftPos = mirrorSpring.leftPos;
            this.OriginalRightPos = (Vector3)mirrorSpring.GetFieldValue("rightPos");
            mirrorSpring.leftPos = Vector3.Scale(new Vector3(1f, BarrelLength * 0.9f, 1f), this.OriginalLeftPos.Value);
            mirrorSpring.SetFieldValue("rightPos", Vector3.Scale(new Vector3(1f, BarrelLength * 0.9f, 1f), this.OriginalRightPos.Value));

            this.ApplyModifiers();

            // recalculate localzoom
            if (ControllerManager.CurrentCameraControllerID != MyCameraController.ControllerID) { return; }
            ((MyCameraController)ControllerManager.CurrentCameraController).ResetZoomLevel(this.player);
        }
    }
}

