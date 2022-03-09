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

namespace GameModeCollection.GameModes.TRT.Cards
{
    static class A_ClawPrefab
    {
        private static GameObject _Claw = null;
        public static GameObject Claw
        {
            get
            {
                if (_Claw is null)
                {
                    _Claw = new GameObject("A_Claw", typeof(A_Claw));
                    UnityEngine.GameObject.DontDestroyOnLoad(_Claw);
                }
                return _Claw;
            }
        }
    }
    public class ClawCard : CustomCard
    {
        internal static CardInfo Card = null;
        /*
         * TRT Zombie card - switch to the claws with [item 5] - cannot be discarded
         */
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new CardCategory[] { TRTCardCategories.TRT_Traitor, TRTCardCategories.TRT_Slot_5, CardItem.IgnoreMaxCardsCategory, CardItem.CannotDiscard };
            statModifiers.AddObjectToPlayer = A_ClawPrefab.Claw;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
        }
        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            // hide claw just to be sure
            try
            {
                A_Claw.MakeGunClaw(player.playerID, false);
            }
            catch { }
        }

        protected override string GetTitle()
        {
            return "Claw";
        }
        protected override string GetDescription()
        {
            return "";
        }

        protected override GameObject GetCardArt()
        {
            return GameModeCollection.TRT_Card_Assets.LoadAsset<GameObject>("C_CLAW");
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
            return CardThemeColor.CardThemeColorType.PoisonGreen;
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
            ClawCard.Card = card;
            ModdingUtils.Utils.Cards.instance.AddHiddenCard(card);
        }
    }
    internal class A_Claw : MonoBehaviour
    {
        internal static IEnumerator RemoveAllClaws(IGameModeHandler gm)
        {
            PlayerManager.instance.ForEachPlayer(p =>
            {
                // hide claw just to be sure
                try
                {
                    A_Claw.MakeGunClaw(p.playerID, false);
                }
                catch { }
            });
            yield break;
        }

        internal const float SwitchDelay = 0.10f;

        // pressing [item 5] will switch the player's gun into a claw
        // claws deal 50 damage, if a zombie kills a player using the claw they will become infected
        // unlike the knife, there is significantly less downtime and delay between attacks
        // also unline the knife, it cannot be discarded
        private float SlashTimer = 0f;
        public bool IsOut { get; private set; } = false;
        private Player Player;
        public float SwitchTimer { get; private set; } = 0f;
        void Start()
        {
            this.IsOut = false;
            this.SlashTimer = 0f;
            this.SwitchTimer = 0f;
            this.Player = this.GetComponentInParent<Player>();
        }
        void Update()
        {
            if (this.Player is null || !this.Player.data.view.IsMine || this.Player.data.dead) { return; }

            this.SlashTimer -= TimeHandler.deltaTime;
            this.SwitchTimer -= TimeHandler.deltaTime;
            if (this.SwitchTimer <= 0f && this.Player.data.playerActions.ItemWasPressed(5))
            {
                this.SwitchTimer = this.IsOut ? 0f : SwitchDelay;
                this.IsOut = !this.IsOut;
                NetworkingManager.RPC(typeof(A_Claw), nameof(RPCA_Switch_To_Claw), this.Player.playerID, this.IsOut);
            }
        }
        [UnboundRPC]
        internal static void RPCA_Switch_To_Claw(int slashingPlayerID, bool claw)
        {
            MakeGunClaw(slashingPlayerID, claw);
        }
        internal static void MakeGunClaw(int playerID, bool claw)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            Gun gun = player.GetComponent<Holding>().holdable.GetComponent<Gun>();
            GameObject springObj = gun.transform.GetChild(1).gameObject;
            RightLeftMirrorSpring spring = springObj.transform.GetChild(2).GetComponent<RightLeftMirrorSpring>();

            GameObject Claw = springObj.transform.Find("TRT_Claw(Clone)")?.gameObject;
            if (Claw is null)
            {
                Claw = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_Claw"), springObj.transform);
                ClawMirror clawMirror = Claw.GetOrAddComponent<ClawMirror>();
                ClawSlash clawSlash = Claw.GetOrAddComponent<ClawSlash>();
                ClawCollider clawCollider = Claw.transform.GetChild(0).gameObject.GetOrAddComponent<ClawCollider>();
            }

            Claw.SetActive(claw);

            springObj.transform.Find("Ammo/Canvas").localScale = claw ? Vector3.zero : 0.0018f * Vector3.one;
            springObj.transform.GetChild(2).gameObject.SetActive(!claw);
            springObj.transform.GetChild(3).gameObject.SetActive(!claw);

            gun.GetData().disabled = claw;
        }

    }
    class ClawCollider : MonoBehaviour
    {
        ClawSlash ClawSlash;
        void Start()
        {
            // must be on this layer to avoid prop-flying and shadow casting
            this.gameObject.layer = LayerMask.NameToLayer("PlayerObjectCollider");
            this.ClawSlash = this.GetComponentInParent<ClawSlash>();
        }
        void OnTriggerEnter2D(Collider2D collider2D)
        {
            this.ClawSlash?.TrySlash(collider2D);
        }
        void OnTriggerStay2D(Collider2D collider2D)
        {
            this.ClawSlash?.TrySlash(collider2D);
        }
    }
    class ClawSlash : MonoBehaviour
    {
        private A_Claw Claw;
        private const float SlashDuration = 0.25f;
        private float SlashTimer = 0f;
        Holdable holdable;
        Player Player;
        GeneralInput Input;
        void Start()
        {
            this.Player = this.transform.root.GetComponent<Holdable>().holder.GetComponent<Player>();
            this.Claw = this.Player.GetComponentInChildren<A_Claw>();
            this.Input = this.Player.data.input;
        }
        void DoSlash()
        {
            if (!this.Claw.IsOut) { return; }
            this.SlashTimer = SlashDuration;
            this.DoSlashAnim();
            if (this.Player.data.view.IsMine)
            {
                NetworkingManager.RPC_Others(typeof(ClawSlash), nameof(RPCO_DoSlashAnim), this.Player.playerID);
            }
        }
        [UnboundRPC]
        private static void RPCO_DoSlashAnim(int playerID)
        {
            Player slasher = PlayerManager.instance.GetPlayerWithID(playerID);
            if (slasher is null) { return; }
            ClawSlash claw = slasher.data.weaponHandler.gun.transform.GetChild(1).Find("TRT_Claw(Clone)").GetComponent<ClawSlash>();
            claw.DoSlashAnim();
        }
        internal void DoSlashAnim()
        {
            this.StartCoroutine(this.IDoSlashAnim());
        }
        IEnumerator IDoSlashAnim()
        {
            if (this.holdable is null) { this.holdable = base.transform.root.GetComponent<Holdable>(); }
            bool left = this.transform.root.position.x - 0.1f < this.holdable.holder.transform.position.x;
            this.transform.root.position += 0.707f*(-this.transform.right + (left ? 1f : -1f) * this.transform.up);
            for (int _ = 0; _ < 5; _++)
            {
                yield return new WaitForEndOfFrame();
                this.transform.root.position += (left ? -1f : 1f) * this.transform.up / 10f;
            }
            yield break;
        }
        void Update()
        {
            this.SlashTimer -= TimeHandler.deltaTime;
            if (this.Player.data.view.IsMine && this.Claw.SwitchTimer <= 0f && this.SlashTimer <= 0f && this.Input.shootWasPressed && this.Claw.IsOut)
            {
                this.DoSlash();
            }
        }
        internal void TrySlash(Collider2D collider2D)
        {
            if (this.SlashTimer <= 0f || !this.Player.data.view.IsMine) { return; }
            if (collider2D?.GetComponent<Player>() != null
                && !collider2D.GetComponent<Player>().data.dead
                && collider2D.GetComponent<Player>().playerID != this.Player.playerID)
            {
                NetworkingManager.RPC(typeof(ClawSlash), nameof(RPCA_SlashPlayer), this.Player.playerID, collider2D.GetComponent<Player>().playerID);
            }
        }
        [UnboundRPC]
        private static void RPCA_SlashPlayer(int slashingPlayerID, int slashedPlayerID)
        {
            if (slashedPlayerID == -1) { return; }
            Player slashingPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == slashingPlayerID);
            Player slashedPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == slashedPlayerID);
            if (slashedPlayer is null || slashingPlayer is null) { return; }
            slashedPlayer.data.healthHandler.DoDamage(50f * slashingPlayer.data.weaponHandler.gun.shootPosition.forward, slashedPlayer.transform.position, Color.white, slashingPlayer.data?.weaponHandler?.gun?.transform?.GetChild(1)?.Find("TRT_Claw(Clone)")?.gameObject, slashingPlayer, false, true, true);
        }
    }
    class ClawMirror : MonoBehaviour
    {
        Holdable holdable;
        private static readonly Vector3 LeftPos = new Vector3(-0.1f, 0.4f, 0f);
        private static readonly Vector3 RightPos = new Vector3(0.1f, 0.4f, 0f);
        private static readonly Vector3 LeftScale = new Vector3(-0.1f, 0.1f, 1f);
        private static readonly Vector3 RightScale = new Vector3(-0.1f, -0.1f, 1f);
        private const float LeftRot = 300f;
        private const float RightRot = 235f;
        void Start()
        {
            this.holdable = base.transform.root.GetComponent<Holdable>();
        }
        void Update()
        {
            if (this.holdable is null || this.holdable.holder is null) { return; }

            bool left = this.transform.root.position.x - 0.1f < this.holdable.holder.transform.position.x;
            this.transform.localScale = left ? LeftScale : RightScale;
            float rot = left ? LeftRot : RightRot;
            this.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, rot));
            this.transform.localPosition = left ? LeftPos : RightPos;
        }
    }
}

