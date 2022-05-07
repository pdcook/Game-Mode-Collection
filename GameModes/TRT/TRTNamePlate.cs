using UnityEngine;
using TMPro;
using GameModeCollection.Extensions;
using UnboundLib;
using GameModeCollection.Objects;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace GameModeCollection.GameModes.TRT
{
    internal class TRTNamePlate : MonoBehaviour
    {
        // this component controls both the player's nameplate and their role icon. it handles:
        // - setting the nameplate's text
        // - setting the role icon
        // - showing/hiding the nameplate based on the local player's cursor position, player location, and dead/alive state
        // - showing/hiding the role icon based on the local player's role and dead/alive state
        Player LocalPlayer => PlayerManager.instance.GetLocalPlayer(); // the local player

        Player Player { get; set; } // the player whose nameplate this is
        internal bool IsVisible { get; set; } = false; // whether the nameplate is currently visible
        bool wasVisibleLastFrame = false; // whether the nameplate was visible last frame

        string RoleID => RoleManager.GetPlayerRoleID(Player); // the player's role ID
        string roleIDLastFrame = ""; // the player's role ID last frame

        bool updateQueued = false; // whether an update has been queued

        void Start()
        {
            // get this player and the local player
            this.Player = this.transform.root.GetComponent<Player>();

            // add child object for the trigger
            GameObject trigger = new GameObject("TRTNamePlateTrigger", typeof(TRTNamePlateTrigger));
            trigger.transform.SetParent(this.Player.gameObject.transform);

        }
        void Update()
        {
            if (this.Player.data.view.IsMine) { this.IsVisible = true; }
            if (this.updateQueued || this.IsVisible != this.wasVisibleLastFrame || this.RoleID != this.roleIDLastFrame)
            {
                this.updateQueued = true; // queue an update to ensure it updates
                // if the nameplate's visibility changed, update the nameplate's visibility
                TRT_Role_Appearance appearance = this.LocalPlayer is null ? null : (RoleManager.GetPlayerAlignment(this.LocalPlayer) is null ? null : RoleManager.GetPlayerRole(this.Player).AppearToAlignment((Alignment)RoleManager.GetPlayerAlignment(this.LocalPlayer)));
                this.DisplayNamePlate(appearance, !this.IsVisible);
            }
            this.wasVisibleLastFrame = this.IsVisible;
            this.roleIDLastFrame = this.RoleID;
        }            
        public void DisplayNamePlate(TRT_Role_Appearance role_Appearance, bool clear = false, Color? backgroundColor = null)
        {
            Color backgroundColor_ = backgroundColor ?? (clear || role_Appearance is null ? Color.clear : GM_TRT.NameBackgroundColor);

            TextMeshProUGUI nameText = this.Player?.GetComponentInChildren<PlayerName>()?.GetComponent<TextMeshProUGUI>();
            if (nameText is null)
            {
                GameModeCollection.LogWarning($"NAME FOR PLAYER {this.Player?.playerID} IS NULL");
                return;
            }
            string nickName = this.Player.data.view?.Owner?.NickName ?? "";
            if (!clear)
            {
                string reputability = RoleManager.GetReputability(this.Player);
                if (reputability != "") { nickName = reputability + (nickName == "" ? "" : "\n") + nickName; }
            }
            if (clear || role_Appearance is null)
            {
                nameText.text = "";
                nameText.color = new Color(0.6132f, 0.6132f, 0.6132f, 1f);
                nameText.fontStyle = FontStyles.Normal;
            }
            else
            {
                nameText.text = $"[{role_Appearance.Abbr}]{(nickName != "" ? "\n" : "")}{nickName}";
                nameText.color = role_Appearance.Color;
                nameText.fontStyle = FontStyles.Bold;
            }
            this.Player.data.SetNameBackground(backgroundColor_);
            this.updateQueued = false; // clear the update queued flag
        }
    }
    internal class TRTNamePlateTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // this is attached to a child object of the player with a clear circle sprite and circle collider
        // the circle is used to detect when the cursor is within range to display the player's name and reputability
        // the circle collider is used to detect when the local player is within range to display the player's name and reputability

        private const float CursorTriggerSize = 0.05f;
        private const float PlayerTriggerSize = 5f;

        Player LocalPlayer => PlayerManager.instance.GetLocalPlayer(); // the local player
        Player Player { get; set; } // the player this is attached to
        TRTNamePlate NamePlate { get; set; }

        CircleCollider2D PlayerTrigger { get; set; }
        GameObject CursorTrigger { get; set; }

        bool IsCursorInRange { get; set; }
        bool IsPlayerInRange { get; set; }
        bool ShowName => this.LocalPlayer is null || this.LocalPlayer.data.dead || this.IsCursorInRange || this.IsPlayerInRange;
        bool nameWasShownLastFrame = false;

        void Start()
        {
            this.transform.localPosition = Vector3.zero;
            this.transform.localScale = Vector3.one;
            this.gameObject.layer = LayerMask.NameToLayer("PlayerObjectCollider");

            // get both the local player and the player this is attached to
            this.Player = this.transform.root.GetComponentInParent<Player>();
            this.NamePlate = this.Player.GetComponentInChildren<TRTNamePlate>();

            // add a clear circle that is three times as large as the player's body
            Canvas canvas = this.gameObject.GetOrAddComponent<Canvas>();
            canvas.sortingLayerID = SortingLayer.NameToID("MostFront");
            canvas.sortingOrder = 100;
            this.gameObject.GetOrAddComponent<CanvasScaler>();
            this.gameObject.GetOrAddComponent<GraphicRaycaster>();

            this.CursorTrigger = new GameObject("CursorTrigger", typeof(Image));
            this.CursorTrigger.GetOrAddComponent<Image>().sprite = Sprites.Circle;
            this.CursorTrigger.GetComponent<Image>().color = Color.clear;
            this.CursorTrigger.transform.SetParent(this.transform);
            this.CursorTrigger.transform.localPosition = Vector3.zero;
            this.CursorTrigger.transform.localScale = CursorTriggerSize * Vector3.one;

            // add a circle collider
            this.PlayerTrigger = this.gameObject.GetOrAddComponent<CircleCollider2D>();
            Rigidbody2D rig = this.gameObject.GetOrAddComponent<Rigidbody2D>();
            rig.mass = 0f;
            rig.isKinematic = true;
            this.PlayerTrigger.radius = PlayerTriggerSize;
            this.PlayerTrigger.isTrigger = true;
        }
        void Update()
        {
            // the nameplate should be shown if the cursor is within range or the player is within range, OR the local player is dead or null
            if (this.ShowName)
            {
                if (!this.nameWasShownLastFrame)
                {
                    // show the nameplate
                    this.NamePlate.IsVisible = true;
                }

                this.nameWasShownLastFrame = true;
            }
            else
            {
                if (this.nameWasShownLastFrame)
                {
                    // hide the nameplate
                    this.NamePlate.IsVisible = false;
                }

                this.nameWasShownLastFrame = false;
            }
        }            
        void OnTriggerEnter2D(Collider2D other)
        {
            // if the local player is within range, display the player's name and reputability
            if (other?.transform?.root?.GetComponent<Player>()?.data?.view?.IsMine ?? false)
            {
                this.IsPlayerInRange = true;
                GameModeCollection.Log("PLAYER IS IN RANGE");
            }
        }
        void OnTriggerExit2D(Collider2D other)
        {
            // if the local player is out of range, hide the player's name and reputability
            if (other?.transform?.root?.GetComponent<Player>()?.data?.view?.IsMine ?? false)
            {
                this.IsPlayerInRange = false;
                GameModeCollection.Log("PLAYER LEFT RANGE");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // if the cursor is within range, display the player's name and reputability
            this.IsCursorInRange = true;
            GameModeCollection.Log("CURSOR IS IN RANGE");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // if the cursor is out of range, hide the player's name and reputability
            this.IsCursorInRange = false;
            GameModeCollection.Log("CURSOR LEFT RANGE");
        }
    }
}
