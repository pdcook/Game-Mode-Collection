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
        // this component controls both the player's nameplate and their role abbreviation. it handles:
        // - setting the nameplate's text
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

                // if the local player is dead or null, the role appearance should be the true appearance
                TRT_Role_Appearance appearance = null;
                if (this.Player.data.view.IsMine || this.LocalPlayer is null || this.LocalPlayer.data.dead)
                {
                    // if this is the local player, or the local player is dead or null, the role appearance should be the true appearance
                    appearance = RoleManager.GetPlayerRole(this.Player)?.Appearance;
                }
                else
                {
                    // otherwise, the role appearance should be how the player's role appears to the local player's role
                    appearance = RoleManager.GetPlayerAlignment(this.LocalPlayer) is null ? null : RoleManager.GetPlayerRole(this.Player)?.AppearToAlignment((Alignment)RoleManager.GetPlayerAlignment(this.LocalPlayer));
                }
                this.DisplayNamePlate(appearance, this.IsVisible);
            }
            this.wasVisibleLastFrame = this.IsVisible;
            this.roleIDLastFrame = this.RoleID;
        }            
        public void DisplayNamePlate(TRT_Role_Appearance role_Appearance, bool inRange = false, Color? backgroundColor = null)
        {
            /// name plates appear as:
            /// [ROLE ABBR] (displayed at all distances, regardless of hover, only if role_Appearance is not null)
            /// [PLAYER NAME] (displayed only if inRange is true, which is when the player is near enough or the cursor is hovering)
            /// [REPUTABILITY] (same case as above)
            /// the color of the nameplate will be the player's role color, unless role_Appearance is null
            /// the color of the reputability is set separately

            // if there will be no nameplate, set the background color to clear
            Color backgroundColor_ = backgroundColor ?? (!inRange && role_Appearance is null ? Color.clear : GM_TRT.TextBackgroundColor);

            TextMeshProUGUI nameText = this.Player?.GetComponentInChildren<PlayerName>()?.GetComponent<TextMeshProUGUI>();
            if (nameText is null)
            {
                GameModeCollection.LogWarning($"NAME FOR PLAYER {this.Player?.playerID} IS NULL");
                return;
            }
            // always have the text container autosize
            nameText.autoSizeTextContainer = true;
            // do not autosize the font
            nameText.enableAutoSizing = false;
            // do not allow lines to wrap
            nameText.enableWordWrapping = false;
            // move the text up slightly to prevent overlap with the health bar
            nameText.transform.localPosition = new Vector3(0f, 175f, 0f);

            // if in range or hovering, show the health bar
            this.Player?.data?.SetHealthbarVisible(inRange);

            // if the the role appearance is not null, start the content with the role abbreviation
            string namePlateContent = role_Appearance is null ? "" : $"[{role_Appearance.Abbr}]";
            // if the player is in range (or the cursor is hovering), add their name and reputability
            if (inRange)
            {
                if (this.Player?.data?.view?.Owner?.NickName != null)
                {
                    if (role_Appearance != null) { namePlateContent += "\n"; }
                    namePlateContent += this.Player?.data?.view?.Owner?.NickName;
                }
                string reputability = RoleManager.GetReputability(this.Player);
                if (reputability != "")
                { namePlateContent += (namePlateContent == "" ? "" : "\n") + reputability; }
            }
            // if the role appearance is not null, set the nameplate's color to the role's color, otherwise to text white
            if (role_Appearance is null)
            {
                nameText.color = new Color(0.6132f, 0.6132f, 0.6132f, 1f);
                nameText.fontStyle = FontStyles.Bold;
            }
            else
            {
                nameText.color = role_Appearance.Color;
                nameText.fontStyle = FontStyles.Bold;
            }
            nameText.text = namePlateContent;
            this.Player.data.SetNameBackground(backgroundColor_);
            this.updateQueued = false; // clear the update queued flag
        }
    }
    internal class TRTNamePlateTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // this is attached to a child object of the player with a clear circle sprite and circle collider
        // the circle is used to detect when the cursor is within range to display the player's name and reputability
        // the circle collider is used to detect when the local player is within range to display the player's name and reputability

        private const float CursorTriggerSize = 0.025f;
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
