using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using UnboundLib.Utils;
using System.Linq;
using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Extensions;
using System.Reflection;
using Photon.Pun;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class VampireRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Vampire.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Vampire.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.1f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Vampire>();
        }
    }
    /// <summary>
    /// possibly the most complicated role because of their two main special abilities
    /// 
    /// 1. can interact with a corpse to eat it and heal 50HP
    ///     -> this requires them to hold down the interact key for a short-ish time, during which they cannot move
    /// 2. when not near a corpse, they can press the interact key to become invisible for a short time
    ///     -> this ability has a cooldown
    /// </summary>
    public class Vampire : Traitor
    {
        public const float InvisibilityCooldown = 10f;
        public const float InvisibilityDuration = 5f;
        public const float InvisSoundIntensity = 1f;
        public const float EatSoundIntensity = 1f;

        public const float InteractRepeatDelay = 0.5f;
        public const float EatCorpseTime = 5f;

        public const float CorpseHealAmount = 50f;

        private TRT_Corpse targetedCorpse = null;
        private float sinceLastPress = InteractRepeatDelay * 2f;
        private float timeHeld = 0f;
        private SoundEvent EatSound;
        private bool EatSoundPlayingForAll = false;
        private bool EatSoundPlaying = false;

        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Vampire", 'V', GM_TRT.VampireColor);

        public override TRT_Role_Appearance Appearance => Vampire.RoleAppearance;

        private Player player => this.GetComponent<Player>();
        private VampireCounterDisplays vampireEffects = null;
        private GameObject VampireEffectsObj = null;

        protected override void Start()
        {
            base.Start();

            // stolen from WWC
            var abyssalCard = CardManager.cards.Values.First(card => card.cardInfo.name.Equals("AbyssalCountdown")).cardInfo;
            var statMods = abyssalCard.gameObject.GetComponentInChildren<CharacterStatModifiers>();
            var abyssalObj = statMods.AddObjectToPlayer;

            this.VampireEffectsObj = Instantiate(abyssalObj, this.player.transform);
            this.VampireEffectsObj.name = "A_TRT_VampireEffects";
            this.VampireEffectsObj.transform.localPosition = Vector3.zero;

            AbyssalCountdown abyssal = this.VampireEffectsObj.GetComponent<AbyssalCountdown>();

            this.EatSound = abyssal.soundAbyssalChargeLoop;

            this.vampireEffects = this.VampireEffectsObj.AddComponent<VampireCounterDisplays>();
            this.vampireEffects.outerRing = abyssal.outerRing;
            this.vampireEffects.fill = abyssal.fill;
            this.vampireEffects.rotator = abyssal.rotator;
            this.vampireEffects.still = abyssal.still;

            UnityEngine.GameObject.Destroy(abyssal);

            foreach (Transform child in this.VampireEffectsObj.transform)
            {
                if (child.name != "Canvas")
                {
                    Destroy(child.gameObject);
                }
            }

            this.vampireEffects.outerRing.color = this.Appearance.Color;
            this.vampireEffects.fill.color = new Color(this.Appearance.Color.r, this.Appearance.Color.g, this.Appearance.Color.b, 0.1f);
            this.vampireEffects.rotator.gameObject.GetComponentInChildren<ProceduralImage>().color = this.vampireEffects.outerRing.color;
            this.vampireEffects.still.gameObject.GetComponentInChildren<ProceduralImage>().color = this.vampireEffects.outerRing.color;
            this.VampireEffectsObj.transform.Find("Canvas/Size/BackRing").GetComponent<ProceduralImage>().color = this.Appearance.Color;
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            if (interact)
            {
                // do vampire stuff
                this.targetedCorpse = corpse;
            }
            else
            {
                corpse.SearchBody(this.GetComponent<Player>(), false);
            }
        }

        void Update()
        {
            if (!this.player.data.view.IsMine) { return; }

            this.sinceLastPress += Time.deltaTime;

            if (this.player.data.playerActions.Interact().WasPressed)
            {
                if (this.sinceLastPress < InteractRepeatDelay && this.vampireEffects.AbilityReady)
                {
                    this.vampireEffects.StartInvisibility();
                }
                this.sinceLastPress = 0f;
                this.timeHeld = 0f;
            }

            if (this.player.data.playerActions.Interact().WasReleased)
            {
                this.timeHeld = 0f;
                this.targetedCorpse = null;
            }
            else if (this.player.data.playerActions.Interact().IsPressed && this.targetedCorpse != null)
            {
                this.timeHeld += Time.deltaTime;
            }
            else if (this.targetedCorpse is null)
            {
                this.timeHeld = 0f;
            }
            this.vampireEffects.EatCorpseProgress(this.timeHeld / EatCorpseTime);
            if (this.timeHeld / EatCorpseTime > 0f && !this.EatSoundPlayingForAll && !this.EatSoundPlaying)
            {
                this.player.data.view.RPC(nameof(RPCA_PlaySound), RpcTarget.All, true);
                this.EatSoundPlaying = true;
            }
            else if (this.timeHeld == 0f && this.EatSoundPlayingForAll && this.EatSoundPlaying)
            {
                this.player.data.view.RPC(nameof(RPCA_PlaySound), RpcTarget.All, false);
                this.EatSoundPlaying = false;
            }

            // eat the corpse
            if (this.timeHeld / EatCorpseTime >= 1f && this.targetedCorpse != null)
            {
                this.player.data.view.RPC(nameof(RPCA_Eat), RpcTarget.All, this.targetedCorpse.Player.playerID);
                this.targetedCorpse = null;
                this.timeHeld = 0f;
            }
        }
        void OnDisable()
        {
            try
            {
                SoundManager.Instance.Stop(this.EatSound, this.transform, true);
            }
            catch { }
        }

        protected override void OnDestroy()
        {
            try
            {
                SoundManager.Instance.Stop(this.EatSound, this.transform, true);
            }
            catch { }
            if (this.VampireEffectsObj != null)
            {
                Destroy(this.VampireEffectsObj);
            }
            base.OnDestroy();
        }
        [PunRPC]
        private void RPCA_PlaySound(bool play)
        {
            if (this.EatSoundPlayingForAll == play) { return; }
            if (play)
            {
                SoundManager.Instance.Play(this.EatSound, this.transform, new SoundParameterBase[] { new SoundParameterIntensity(EatSoundIntensity) });
            }
            else
            {
                SoundManager.Instance.Stop(this.EatSound, this.transform, true);
            }
            this.EatSoundPlayingForAll = play;
        }
        [PunRPC]
        private void RPCA_Eat(int corpseID)
        {
            Player deadPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == corpseID);
            if (deadPlayer is null) { return; }
            if (deadPlayer.GetComponent<TRT_Corpse>() != null)
            {
                UnityEngine.GameObject.Destroy(deadPlayer.GetComponent<TRT_Corpse>());
            }
            deadPlayer.gameObject.SetActive(false);
            this.player.data.healthHandler.Heal(CorpseHealAmount);
        }
    }
	class VampireCounterDisplays : MonoBehaviour
	{
        public const float PlayEatingSoundEvery = 0.1f;
        public float counter;

        public ProceduralImage outerRing;
        public ProceduralImage backRing;

        public ProceduralImage fill;

        public Transform rotator;

        public Transform still;
        public bool Invisible { get; private set; } = false;
        public bool AbilityReady => this.counter >= 1f;
        private float InvisCooldown => Vampire.InvisibilityCooldown;
        private float InvisDuration => Vampire.InvisibilityDuration;
        private Player player { get; set; }
        void Start()
        {
            this.player = this.GetComponentInParent<Player>();

            // spawn with ability ready
            this.transform.localScale = 0.5f * Vector3.one;
            this.counter = 1f;
            this.backRing = this.outerRing.transform.parent.GetChild(0).gameObject.GetComponent<ProceduralImage>();
            this.backRing.type = UnityEngine.UI.Image.Type.Filled;
            this.rotator.gameObject.SetActive(false);
            this.still.gameObject.SetActive(false);
            this.fill.gameObject.SetActive(false);
            this.outerRing.gameObject.SetActive(this.player.data.view.IsMine);
            this.backRing.gameObject.SetActive(this.player.data.view.IsMine);

            this.outerRing.BorderWidth = 20f;
            this.backRing.BorderWidth = 20f;
            this.backRing.fillAmount = 0f;
            this.backRing.color = new Color(1f, 0f, 0f, 0.5f);
            this.backRing.transform.localScale = 0.8f * Vector3.one;
        }

        public void EatCorpseProgress(float progress)
        {
            this.backRing.fillAmount = UnityEngine.Mathf.Clamp01(progress);
        }

        public void StartInvisibility()
        {
            this.counter = 1f;
            this.Invisible = true;
            if (this.player.data.view.IsMine)
            {
                NetworkingManager.RPC(typeof(VampireCounterDisplays), nameof(RPCA_ToggleInvisible), this.player.playerID, true);
            }
        }
        public void StopInvisibility()
        {
            this.counter = 0f;
            this.Invisible = false;
            if (this.player.data.view.IsMine)
            {
                NetworkingManager.RPC(typeof(VampireCounterDisplays), nameof(RPCA_ToggleInvisible), this.player.playerID, false);
            }
        }

        void OnDisable()
        {
            this.StopInvisibility();
        }
        void OnDestroy()
        {
            this.StopInvisibility();
        }

        void Update()
        {
            if (!this.player.data.view.IsMine) { return; }
            if (this.player.data.dead)
            {
                this.counter = 0f;
                if (this.Invisible) { this.StopInvisibility(); }
            }
            if (this.counter < 1f && !this.Invisible)
            {
                this.counter += TimeHandler.deltaTime/this.InvisCooldown; 
                this.counter = Mathf.Clamp01(this.counter);
            }
            else if (this.counter > 0f && this.Invisible)
            {
                this.counter -= TimeHandler.deltaTime/this.InvisDuration; 
                this.counter = Mathf.Clamp01(this.counter);
            }
            else if (this.counter <= 0f && this.Invisible)
            {
                this.StopInvisibility();
            }
            this.outerRing.fillAmount = this.counter;
        }
        [UnboundRPC]
        private static void RPCA_ToggleInvisible(int playerID, bool invisible)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }

            player.GetComponentInChildren<VampireCounterDisplays>(true).Invisible = invisible;

            if (invisible)
            {
                SoundManager.Instance.Play(player.data.block.soundBlockRecharged, player.transform, new SoundParameterBase[] {new SoundParameterIntensity(Vampire.InvisSoundIntensity)});
            }

            player.transform.Find("Art").gameObject.SetActive(!invisible);
            player.transform.Find("WobbleObjects").gameObject.SetActive(!invisible && !player.data.dead);
            player.transform.Find("PlayerSkin").GetChild(0).gameObject.SetActive(!invisible);
            player.transform.Find("Limbs/ArmStuff").gameObject.SetActive(!invisible);
            foreach (Transform child in player.transform.Find("Limbs/LegStuff").gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "Joint(Clone)")
                {
                    child.gameObject.SetActive(!invisible);
                }
            }
            player.GetComponent<WeaponHandler>().gun.transform.Find("Spring/Handle").gameObject.SetActive(!invisible);
            player.GetComponent<WeaponHandler>().gun.transform.Find("Spring/Barrel").gameObject.SetActive(!invisible);
            player.GetComponent<WeaponHandler>().gun.transform.Find("Spring/Ammo/Canvas").gameObject.SetActive(!invisible);

            if (!invisible)
            {
                GameModeCollection.instance.ExecuteAfterFrames(5, () =>
                {
                    GunAmmo gunAmmo = player.GetComponent<WeaponHandler>().gun.GetComponentInChildren<GunAmmo>();

                    // redraw current ammo
                    for (int i = gunAmmo.populate.transform.childCount - 1; i >= 0; i--)
                    {
                        if (gunAmmo.populate.transform.GetChild(i).gameObject.activeSelf)
                        {
                            Object.Destroy(gunAmmo.populate.transform.GetChild(i).gameObject);
                        }
                    }
                    gunAmmo.populate.times = (int)gunAmmo.GetFieldValue("currentAmmo");
                    gunAmmo.populate.DoPopulate();
                    typeof(GunAmmo).InvokeMember("SetActiveBullets",
                                BindingFlags.Instance | BindingFlags.InvokeMethod |
                                BindingFlags.NonPublic, null, gunAmmo, new object[] { false });
                });
            }

        }

    }
}
