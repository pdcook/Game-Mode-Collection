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
using Photon.Pun;
using Sonigon.Internal;
using GameModeCollection.Utils;

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_AWPPrefab
    {
        private static GameObject _AWP = null;
        public static GameObject AWP
        {
            get
            {
                if (_AWP is null)
                {
                    _AWP = new GameObject("A_AWP", typeof(A_AWP));
                    UnityEngine.GameObject.DontDestroyOnLoad(_AWP);
                }
                return _AWP;
            }
        }
        private static GameObject _AWPDealtDamageEffect = null;
        public static GameObject AWPDealtDamageEffect
        {
            get
            {
                if (_AWPDealtDamageEffect is null)
                {
                    _AWPDealtDamageEffect = new GameObject("A_AWPDealtDamageEffect", typeof(AWPDealtDamageEffect));
                    UnityEngine.GameObject.DontDestroyOnLoad(_AWPDealtDamageEffect);
                }
                return _AWPDealtDamageEffect;
            }
        }
    }

    public class AWPCard : CustomCard
    {
        // one shot, very slow rate of fire, sniper rifle available to both traitors and detectives

        internal static CardInfo Card = null;
        internal static string CardName => "AWP";
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_2, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_2 };
            statModifiers.AddObjectToPlayer = A_AWPPrefab.AWP;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.GetOrAddComponent<AWPGun>();
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (player.gameObject.GetComponent<AWPGun>() != null)
            {
                Destroy(player.gameObject.GetComponent<AWPGun>());
            }
        }

        protected override string GetTitle()
        {
            return CardName;
        }
        protected override string GetDescription()
        {
            return "An extremely powerful and loud traitor sniper. Press [item 2] to switch to it.";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_AWP");

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
                    amount = "Instakill",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = true
                },
                new CardInfoStat()
                {
                    stat = "Fire Rate",
                    amount = "Extremely Slow",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = false
                },
                new CardInfoStat()
                {
                    stat = "Movement Speed When Scoped",
                    amount = "Almost No",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    positive = false
                }
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
            AWPCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    public class A_AWP : MonoBehaviour
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
                NetworkingManager.RPC(typeof(A_AWP), nameof(RPCA_SwitchToAWP), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        private static void RPCA_SwitchToAWP(int playerID, bool switchTo)
        {
            if (switchTo) { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<AWPGun>()?.EnableAWP(); }
            else { PlayerManager.instance.GetPlayerWithID(playerID)?.GetComponent<AWPGun>()?.DisableAWP(); }
        }
    }
    public class AWPGun : ReversibleEffect
    {
        private const float BarrelLength = 5f;
        private const float BarrelWidth = 0.5f;
        internal const float Damage = 12.3456f;

        private Vector3? OriginalScale = null;
        private Vector3? OriginalRightPos = null;
        private Vector3? OriginalLeftPos = null;
        private List<ObjectsToSpawn> OriginalObjectsToSpawn;
        private bool OriginalUnblockable = false;
        private int NumCards;

        private SoundShotModifier SoundShotModifier;
        private SoundImpactModifier SoundImpactModifier;
        private SoundParameterBase[] SoundParameters =
            new SoundParameterBase[]
            {
                new SoundParameterSpatializeCutoffDistance(float.MaxValue), // AWP can be heard infinitely far away
                new SoundParameterSpatializeMaxDistance(float.MaxValue), // but the sound continues to fall off
                new SoundParameterSpatializeMinDistance(GMCAudio.MinDistance * 2), // the min distance is doubled
                new SoundParameterSpatializeWalls(UnityEngine.Mathf.Clamp01(GMCAudio.WallPenaltyPercent / 10f), int.MaxValue), // the wall penalty is cut in third and the max number of walls is infinite
            };
        public override void OnAwake()
        {
            this.SetLivesToEffect(1);
            this.applyImmediately = false;

            base.OnAwake();
        }
        public override void OnStart()
        {
            // set sound effects
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("AWPShoot.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            SoundEvent shoot = ScriptableObject.CreateInstance<SoundEvent>();
            shoot.soundContainerArray[0] = soundContainer;

            this.SoundShotModifier = new SoundShotModifier() { single = shoot };
            this.SoundShotModifier.SetSoundParameterArray(this.SoundParameters);

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
            this.SoundImpactModifier.SetSoundParameterArray(this.SoundParameters);

            this.NumCards = this.data.currentCards.Count();

            this.gunAmmoStatModifier.maxAmmo_mult = 0;
            this.gunAmmoStatModifier.maxAmmo_add = 3;
            this.gunStatModifier.bursts_mult = 0;
            this.gunStatModifier.numberOfProjectiles_mult = 0;
            this.gunStatModifier.numberOfProjectiles_add = 1;
            this.gunStatModifier.damage_mult = 0f;
            this.gunStatModifier.damage_add = Damage;
            this.gunStatModifier.projectileSize_mult = 0f;
            this.gunStatModifier.projectileSize_add = -0.99f;
            this.gunStatModifier.attackSpeed_mult = 0f;
            this.gunStatModifier.attackSpeed_add = 5f;
            this.gunAmmoStatModifier.reloadTimeMultiplier_mult = 0f;
            this.gunAmmoStatModifier.reloadTimeAdd_add = 10f;
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

            this.characterStatModifiersModifier.movementSpeed_mult = 0.25f;

            this.characterStatModifiersModifier.objectsToAddToPlayer = new List<GameObject>() { A_AWPPrefab.AWPDealtDamageEffect };

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
                    this.DisableAWP();
                    this.EnableAWP();
                }
            }

            base.OnUpdate();
        }
        public override void OnOnDestroy()
        {
            this.DisableAWP();
            base.OnOnDestroy();
        }
        public void DisableAWP()
        {
            // things that can't be changed with ReversibleEffect

            // restore originals
            this.gun.objectsToSpawn = this.OriginalObjectsToSpawn?.ToArray() ?? new ObjectsToSpawn[]{ };//.Concat(this.gun.objectsToSpawn).ToArray();
            this.gun.dontAllowAutoFire = this.data.currentCards.Any(c => (c.gameObject?.GetComponent<Gun>()?.dontAllowAutoFire ?? false));
            this.gun.unblockable = this.OriginalUnblockable;
            this.gun.soundGun.RemoveSoundShotModifier(this.SoundShotModifier);
            this.gun.soundGun.RemoveSoundImpactModifier(this.SoundImpactModifier);
            this.gun.soundGun.RefreshSoundModifiers();
            if (this.player.data.currentCards.Contains(SilencerCard.Card))
            {
                this.gun.GetData().silenced = true;
            }
            else
            {
                this.gun.GetData().silenced = false;
            }
            this.gun.GetData().pierce = false;

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;
            spring.GetComponentInChildren<SpriteRenderer>().color = this.player.GetTeamColors().color;
            barrel.GetComponentInChildren<SpriteRenderer>().color = this.player.GetTeamColors().color;
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

        public void EnableAWP()
        {
            // AWPs pierce players
            this.gun.GetData().pierce = true;

            // you can't cheese the attack speed by switching back and forth
            this.gun.sinceAttack = 0f;

            // things that can't be changed with ReversibleEffect

            // save originals
            this.OriginalObjectsToSpawn = this.gun.objectsToSpawn.ToList();
            this.OriginalUnblockable = this.gun.unblockable;

            // disable auto-fire (requires demonicpactpatch to reset properly)
            this.gun.dontAllowAutoFire = true; // will be reset by reading all of the cards the player has when this is removed
            this.gun.objectsToSpawn = new ObjectsToSpawn[] { };
            this.gun.unblockable = true; // can't block the awp
            this.gun.soundGun.AddSoundShotModifier(this.SoundShotModifier);
            this.gun.soundGun.AddSoundImpactModifier(this.SoundImpactModifier);
            this.gun.soundGun.RefreshSoundModifiers();
            this.gun.GetData().silenced = false;

            GameObject spring = this.gun.transform.GetChild(1).gameObject;
            GameObject barrel = spring.transform.GetChild(3).gameObject;
            spring.GetComponentInChildren<SpriteRenderer>().color = GM_TRT.DullWhite;
            barrel.GetComponentInChildren<SpriteRenderer>().color = GM_TRT.DullWhite;
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
    public class AWPDealtDamageEffect : DealtDamageEffect
    {
        // awp instakills players. always.

        const float DMG = 1000f;
        const float EPS = 0.001f;

        public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer = null)
        {
            if (damagedPlayer is null) { return; }
            if (UnityEngine.Mathf.Abs(damage.magnitude/55f - AWPGun.Damage) > EPS) { return; } // detect if the player shot a bullet and then very quickly switched to the awp
            if (!this.transform.root.GetComponent<Player>().data.view.IsMine) { return; }

            Player ownPlayer = this.transform.root.GetComponent<Player>();

            NetworkingManager.RPC(typeof(AWPDealtDamageEffect), nameof(RPCA_KillPlayer), damage, damagedPlayer.playerID, ownPlayer.playerID);
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
                playerToKill.data.view.RPC("RPCA_Die", RpcTarget.All, DMG*damage.normalized);
            }
        }
    }
}

