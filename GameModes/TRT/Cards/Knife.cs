using UnboundLib.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using System.Collections;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using GameModeCollection.Utils;
using UnboundLib.GameModes;
using GameModeCollection.Objects;
using Sonigon;
using Sonigon.Internal;

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
         * TRT traitor shop card that allows the player to switch to a knife with [item 2]
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_2, CardItem.IgnoreMaxCardsCategory };
            cardInfo.blacklistedCategories = new CardCategory[] { TRTCardCategories.TRT_Slot_2 };
            statModifiers.AddObjectToPlayer = A_KnifePrefab.Knife;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // hide knife just to be sure
            try
            {
                A_Knife.MakeGunKnife(player.playerID, false);
            }
            catch { }
        }

        protected override string GetTitle()
        {
            return "Knife";
        }
        protected override string GetDescription()
        {
            return "A decisive melee weapon you can switch to with [item 2]. Good for one kill.";
        }

        protected override GameObject GetCardArt()
        {
            //return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_KNIFE");
            return GameModeCollection.TRT_Assets.LoadAsset<GameObject>("C_Knife");
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
            KnifeCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_Knife : MonoBehaviour
    {
        internal static IEnumerator RemoveAllKnives(IGameModeHandler gm)
        {
            PlayerManager.instance.ForEachPlayer(p =>
            {
                // hide knife just to be sure
                try
                {
                    A_Knife.MakeGunKnife(p.playerID, false);
                }
                catch { }
            });
            yield break;
        }

        internal const float SwitchDelay = 0.25f;
        public const float Volume = 1f;

        // the knife is consumed on kill
        // it will instakill the stabbed player
        // pressing [item 2] will switch the player's gun into a knife
        private float StabTimer = 0f;
        public bool HasStabbed { get; set;} = false;
        public bool IsOut { get; private set; } = false;
        private Player Player;
        public float SwitchTimer { get; private set; } = 0f;
        SoundEvent KnifePullOutSound;
        void Start()
        {
            this.HasStabbed = false;
            this.IsOut = false;
            this.StabTimer = 0f;
            this.SwitchTimer = 0f;
            this.Player = this.GetComponentInParent<Player>();
            // load sound effect
            AudioClip sound = GameModeCollection.TRT_Assets.LoadAsset<AudioClip>("KnifePullOut.ogg");
            SoundContainer soundContainer = ScriptableObject.CreateInstance<SoundContainer>();
            soundContainer.setting.volumeIntensityEnable = true;
            soundContainer.audioClip[0] = sound;
            this.KnifePullOutSound = ScriptableObject.CreateInstance<SoundEvent>();
            this.KnifePullOutSound.soundContainerArray[0] = soundContainer;
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine || this.Player.data.dead) { return; }

            this.StabTimer -= TimeHandler.deltaTime;
            this.SwitchTimer -= TimeHandler.deltaTime;
            if (!this.HasStabbed && this.SwitchTimer <= 0f && this.Player.data.playerActions.ItemWasPressed(2))
            {
                this.SwitchTimer = this.IsOut ? 0f : SwitchDelay;
                this.IsOut = !this.IsOut;
                if (this.IsOut)
                {
                    // play sound locally
                    SoundManager.Instance.Play(this.KnifePullOutSound, this.Player.transform, new SoundParameterBase[] { new SoundParameterIntensity(Optionshandler.vol_Master * Optionshandler.vol_Sfx * Volume) });
                }
                NetworkingManager.RPC(typeof(A_Knife), nameof(RPCA_Switch_To_Knife), this.Player.playerID, this.IsOut);
            }
            else if (this.HasStabbed && this.IsOut)
            {
                this.IsOut = false;
                NetworkingManager.RPC(typeof(A_Knife), nameof(RPCA_Switch_To_Knife), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        internal static void RPCA_Switch_To_Knife(int stabbingPlayerID, bool knife)
        {
            MakeGunKnife(stabbingPlayerID, knife);
        }
        static void MoveToHide(Transform transform, bool hide)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, hide ? -10000f : 0f);
        }
        internal static void MakeGunKnife(int playerID, bool knife)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            Gun gun = player.GetComponent<Holding>().holdable.GetComponent<Gun>();
            GameObject springObj = gun.transform.Find("Spring").gameObject;
            RightLeftMirrorSpring spring = springObj.transform.GetChild(2).GetComponent<RightLeftMirrorSpring>();

            GameObject Knife = springObj.transform.Find("TRT_Knife(Clone)")?.gameObject;
            if (Knife is null)
            {
                Knife = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_Knife"), springObj.transform);
                KnifeMirror knifeMirror = Knife.GetOrAddComponent<KnifeMirror>();
                KnifeStab knifeStab = Knife.GetOrAddComponent<KnifeStab>();
                KnifeCollider knifeCollider = Knife.transform.GetChild(0).gameObject.GetOrAddComponent<KnifeCollider>();
            }

            Knife.SetActive(knife);

            MoveToHide(springObj.transform.Find("Ammo/Canvas"), knife);
            MoveToHide(springObj.transform.GetChild(2), knife);
            MoveToHide(springObj.transform.GetChild(3), knife);
            springObj.transform.GetChild(2).GetComponent<RightLeftMirrorSpring>().enabled = !knife;
            springObj.transform.GetChild(3).GetComponent<RightLeftMirrorSpring>().enabled = !knife;

            gun.GetData().disabled = knife;
        }

    }
    class KnifeCollider : MonoBehaviour
    {
        KnifeStab KnifeStab;
        void Start()
        {
            // must be on this layer to avoid prop-flying and shadow casting
            this.gameObject.layer = LayerMask.NameToLayer("PlayerObjectCollider");
            this.KnifeStab = this.GetComponentInParent<KnifeStab>();
        }
        void OnTriggerEnter2D(Collider2D collider2D)
        {
            this.KnifeStab?.TryStab(collider2D);
        }
        void OnTriggerStay2D(Collider2D collider2D)
        {
            this.KnifeStab?.TryStab(collider2D);
        }
    }
    class KnifeStab : MonoBehaviour
    {
        private A_Knife Knife;
        private const float StabDuration = 0.25f;
        private float StabTimer = 0f;
        Holdable holdable;
        Player Player;
        GeneralInput Input;
        void OnEnable()
        {
            this.Player = this.transform.root.GetComponent<Holdable>().holder.GetComponent<Player>();
            this.Knife = this.Player.GetComponentInChildren<A_Knife>();
            this.Input = this.Player.data.input;
        }
        void DoStab()
        {
            if (this.Knife.HasStabbed || !this.Knife.IsOut) { return; }
            this.StabTimer = StabDuration;
            this.DoStabAnim();
            if (this.Player.data.view.IsMine)
            {
                NetworkingManager.RPC_Others(typeof(KnifeStab), nameof(RPCO_DoStabAnim), this.Player.playerID);
            }
        }
        [UnboundRPC]
        private static void RPCO_DoStabAnim(int playerID)
        {
            Player stabber = PlayerManager.instance.GetPlayerWithID(playerID);
            if (stabber is null) { return; }
            KnifeStab knife = stabber.data.weaponHandler.gun.transform.GetChild(1).Find("TRT_Knife(Clone)").GetComponent<KnifeStab>();
            knife.DoStabAnim();
        }
        internal void DoStabAnim()
        {
            if (this.holdable is null) { this.holdable = base.transform.root.GetComponent<Holdable>(); }
            bool left = this.transform.root.position.x - 0.1f < this.holdable.holder.transform.position.x;
            this.transform.root.position += (-4f * this.transform.right + (left ? 3f : -3f) * this.transform.up) / 5f;
        }
        void Update()
        {
            this.StabTimer -= TimeHandler.deltaTime;
            if (this.Player.data.view.IsMine && this.Knife.SwitchTimer <= 0f && this.StabTimer <= 0f && this.Input.shootWasPressed && this.Knife.IsOut)
            {
                this.DoStab();
            }
        }
        internal void TryStab(Collider2D collider2D)
        {
            if (this.Knife.HasStabbed || this.StabTimer <= 0f || !this.Player.data.view.IsMine) { return; }
            if (collider2D?.GetComponent<Player>() != null
                && !collider2D.GetComponent<Player>().data.dead
                && collider2D.GetComponent<Player>().playerID != this.Player.playerID)
            {
                this.Knife.HasStabbed = true;
                NetworkingManager.RPC(typeof(KnifeStab), nameof(RPCA_StabPlayer), this.Player.playerID, collider2D.GetComponent<Player>().playerID);
            }
        }
        [UnboundRPC]
        private static void RPCA_StabPlayer(int stabbingPlayerID, int stabbedPlayerID)
        {
            if (stabbedPlayerID == -1) { return; }
            Player stabbingPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == stabbingPlayerID);
            Player stabbedPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == stabbedPlayerID);
            if (stabbedPlayer is null || stabbingPlayer is null) { return; }
            stabbedPlayer.data.healthHandler.DoDamage(10000f * stabbingPlayer.data.weaponHandler.gun.shootPosition.forward, stabbedPlayer.transform.position, Color.white, stabbingPlayer.data?.weaponHandler?.gun?.transform?.GetChild(1)?.Find("TRT_Knife(Clone)")?.gameObject, stabbingPlayer, false, true, true);
            CardUtils.RemoveCardFromPlayer_ClientsideCardBar(stabbingPlayer, KnifeCard.Card, ModdingUtils.Utils.Cards.SelectionType.Oldest);
        }
    }
    class KnifeMirror : MonoBehaviour
    {
        Holdable holdable;
        private static readonly Vector3 LeftPos = new Vector3(-0.1f, 0.4f, 0f);
        private static readonly Vector3 RightPos = new Vector3(0.1f, 0.4f, 0f);
        private static readonly Vector3 LeftScale = new Vector3(-0.15f, 0.15f, 1f);
        private static readonly Vector3 RightScale = new Vector3(-0.15f, -0.15f, 1f);
        private const float LeftRot = 300f;
        private const float RightRot = 235f;
        private bool Spinning = false;
        private float SpinTimer = 0f;
        void OnEnable()
        {
            this.holdable = base.transform.root.GetComponent<Holdable>();

            // do a quick 360 when first pulled out
            this.Spinning = true;
            this.SpinTimer = A_Knife.SwitchDelay;
        }
        void Update()
        {
            if (this.holdable is null || this.holdable.holder is null) { return; }

            bool left = this.transform.root.position.x - 0.1f < this.holdable.holder.transform.position.x;
            this.transform.localScale = left ? LeftScale : RightScale;
            float rot = left ? LeftRot : RightRot;
            if (this.Spinning)
            {
                this.SpinTimer -= TimeHandler.deltaTime;
                rot = Mathf.Lerp(rot, rot + 360f, this.SpinTimer / A_Knife.SwitchDelay);
            }
            this.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, rot));
            this.transform.localPosition = left ? LeftPos : RightPos;
        }
    }
}

