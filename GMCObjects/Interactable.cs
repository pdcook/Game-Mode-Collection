using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MapEmbiggener.Controllers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes;
using GameModeCollection.Extensions;
using UnboundLib;
using System;
using System.Collections;
namespace GameModeCollection.GMCObjects
{
    public static class InteractablePrefabs
    {
        private static GameObject _InteractableUI = null;
        public static GameObject InteractableUI
        {
            get
            {
                if (_InteractableUI is null)
                {
                    _InteractableUI = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_Interact"));
                    _InteractableUI.AddComponent<InteractionUI>().SetPrefab(true);
                    GameObject interactableText = GameObject.Instantiate(InteractableText, _InteractableUI.transform);
                    interactableText.SetActive(true);
                    UnityEngine.GameObject.DontDestroyOnLoad(_InteractableUI);
                }
                return _InteractableUI;
            }
        }
        private static GameObject _InteractableText = null;
        public static GameObject InteractableText
        {
            get
            {
                if (_InteractableText is null)
                {
                    _InteractableText = new GameObject("InteractableText", typeof(Canvas));
                    _InteractableText.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    Transform text = new GameObject("Text", typeof(TextMeshProUGUI)).transform;
                    text.SetParent(_InteractableText.transform);
                    Transform background = new GameObject("Background", typeof(UnityEngine.UI.Image), typeof(TextBackground)).transform;
                    background.SetParent(_InteractableText.transform);
                    background.SetAsFirstSibling();
                    background.localPosition = Vector3.zero;
                    background.localScale = Vector3.one;
                    background.GetComponent<UnityEngine.UI.Image>().color = GM_TRT.TextBackgroundColor;
                    background.GetComponent<TextBackground>().CheckForChanges();
                    _InteractableText.SetActive(false);
                    GameObject.DontDestroyOnLoad(_InteractableText);
                }
                return _InteractableText;
            }
        }            
    }
    /// <summary>
    /// Interaction base class for interactable map objects
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string HoverText { get; protected set; }
        public abstract Color TextColor { get; protected set; }
        public abstract Color IconColor { get; protected set; }
        public virtual Alignment? RequiredAlignment { get; protected set; } = null; // require player to be this alignment to interact
        public virtual float VisibleDistance { get; protected set; } = float.PositiveInfinity;
        public virtual bool RequireLoS { get; protected set; } = false;
        public virtual bool InteractableInEditor { get; protected set; } = false;
        public bool IsEditorObj => this.GetComponent<DetectMapEditor>()?.IsMapEditor ?? false;
        public string UniqueKey { get; protected set; } // unique key for RPC
        protected InteractionUI InteractionUI = null;

        protected void Start()
        {
            this.UniqueKey = string.Concat(new object[]
            {
            "Interactable ",
            (int)base.GetComponentInParent<Map>().GetFieldValue("levelID"),
            " ",
            base.transform.GetSiblingIndex()
            });
            this.StartCoroutine(this.RegisterRPCWhenReady());

            // add the interaction UI
            GameObject interactableUI = GameObject.Instantiate(InteractablePrefabs.InteractableUI, this.transform.parent);
            this.InteractionUI = interactableUI.GetComponent<InteractionUI>();
            this.InteractionUI.SetInteractableObject(this.transform);
            interactableUI.SetActive(true);
            interactableUI.GetComponentInChildren<TextMeshProUGUI>().text = HoverText;
            interactableUI.GetComponentInChildren<TextMeshProUGUI>().color = TextColor;
            interactableUI.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            interactableUI.GetComponentInChildren<TextBackground>().CheckForChanges();
        }
        private IEnumerator RegisterRPCWhenReady()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitWhile(() => MapManager.instance.GetComponent<ChildRPC>().childRPCsInt.ContainsKey(this.UniqueKey));
            yield return new WaitForEndOfFrame();
            MapManager.instance.GetComponent<ChildRPC>().childRPCsInt.Add(this.UniqueKey, new Action<int>(this.RPCA_TryInteract));
            yield break;
        }
        private void OnDestroy()
        {
            if (MapManager.instance != null)
            {
                try
                {
                    MapManager.instance.GetComponent<ChildRPC>().childRPCsInt.Remove(this.UniqueKey);
                }
                catch
                {
                    GameModeCollection.LogWarning("Failed to remove RPC for " + this.UniqueKey);
                }
            }
            if (this.InteractionUI != null)
            {
                GameObject.Destroy(this.InteractionUI.gameObject);
            }
        }
        public void Call_TryInteract(Player player)
        {
            MapManager.instance.GetComponent<ChildRPC>().CallFunction(this.UniqueKey, player is null ? -1 : player.playerID);
        }
        public void RPCA_TryInteract(int playerID)
        {
            Player player = playerID == -1 ? null : PlayerManager.instance?.GetPlayerWithID(playerID);
            if ((player is null
                || player.data.dead
                || ( this.RequiredAlignment != null
                    && RoleManager.GetPlayerAlignment(player) != this.RequiredAlignment))
               && !GameModeCollection.DEBUG)
                    
            {
                return;
            }
            this.OnInteract(player);
        }

        public abstract void OnInteract(Player player);

    }
    /// <summary>
    /// interaction icon for traitor map objects
    /// </summary>
    public class InteractionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float TextScale = 0.7f;
        private const float DefaultScale = 0.1f; // default scale of the icon
        private const float HoverScale = 0.133f; // scale of the icon when hovered
        private const float ClickScale = 0.083f; // scale of the icon when clicked

        private const float HorizMargin = 0.95f; // horizontal margin of the icon from the screen border
        private const float VertMargin = 0.9f; // vertical margin of the icon from the screen border
        private const float HorizTextOffset = 0.01f; // offset of the text from the icon in viewport units (1f is the entire size of the screen)
        private const float VertTextOffset = 0.05f; // offset of the text from the icon in viewport units (1f is the entire size of the screen)

        private Color DefaultColor
        {
            get
            {
                Color? color = this.InteractableObject?.GetComponent<Interactable>()?.IconColor;
                if (color.HasValue)
                {
                    // return the color with the opacity lowered by 25%
                    return new Color(color.Value.r, color.Value.g, color.Value.b, color.Value.a * 0.75f);
                }
                else { return new Color(1f, 1f, 1f, 0.75f); }
                
            }
        }
        private Color HoveredColor
        {
            get
            {
                Color? color = this.InteractableObject?.GetComponent<Interactable>()?.IconColor;
                return color ?? new Color(1f, 1f, 1f, 1f);
            }
        }

        public UnityEngine.UI.Image IconImage { get; private set; }
        public UnityEngine.UI.Image ShadowImage { get; private set; }
        public TextMeshProUGUI Text { get; private set; }

        private bool IsEditorObj => this.InteractableObject?.GetComponent<Interactable>()?.IsEditorObj ?? false;
        private bool IsVisibleInEditor => this.InteractableObject?.GetComponent<Interactable>()?.InteractableInEditor ?? false;

        private Alignment? RequiredAlignment => this.InteractableObject?.GetComponent<Interactable>()?.RequiredAlignment;
        private bool RequireLoS => this.InteractableObject?.GetComponent<Interactable>()?.RequireLoS ?? false;

        private float Scale => this.IsClicked ? ClickScale : this.IsHovering ? HoverScale : DefaultScale;

        // the local player is alive, is a traitor, is close enough to interact/see the icon, OR debug mode is enabled 
        public bool LocalPlayerIsEligible
        {
            get
            {
                Player player = PlayerManager.instance?.GetLocalPlayer();

                // determine if the player is eligible to see and interact with this
                return (
                        (this.IsEditorObj && this.IsVisibleInEditor) // this is an editor object and is supposed to be visible in the editor
                        || (                                         // OR ALL of the following:
                            this.InteractableObject != null // the interactable object is not null
                            && player != null // and the player is not null
                            && !player.data.dead // and the player is alive
                            && player.data.isPlaying // and the player is playing
                            && (bool)player.data.playerVel.GetFieldValue("simulated") // and the player is simulated
                            && RoleManager.GetPlayerAlignment(player) != null // and the player has an alignment
                            && (
                                !this.RequiredAlignment.HasValue // and (there isn't a required alignment
                                || RoleManager.GetPlayerAlignment(player) == this.RequiredAlignment.Value // OR the player's alignment is the same as the required alignment
                                || GameModeCollection.DEBUG // OR we're in DEBUG)
                               )
                            && Vector2.Distance(player.transform.position, this.InteractableObject.position) <= this.InteractableObject.GetComponent<Interactable>().VisibleDistance // and the player is within range
                            && (
                                !this.RequireLoS // and (there isn't a line of sight requirement
                                || PlayerManager.instance.CanSeePlayer(this.InteractableObject.position, player).canSee // OR the player can see the interactable)
                               )
                           )
                       );
            }
        }

        public bool IsHovering { get; private set; } = false;
        // icons are clicked when the local player is a traitor, is hovering over it (or is a controller player within a certain range), and presses the interact button
        public bool IsClicked
        {
            get
            {
                return this.IsHovering && this.LocalPlayerIsEligible && ((PlayerManager.instance?.GetLocalPlayer()?.data.playerActions.InteractIsPressed() ?? false) || ((GameModeCollection.DEBUG || this.IsEditorObj) && Input.GetKey(KeyCode.F)));
            }                
        }
        public bool WasClicked
        {
            get
            {
                return this.IsHovering && this.LocalPlayerIsEligible && ((PlayerManager.instance?.GetLocalPlayer()?.data.playerActions.InteractWasPressed() ?? false) || ((GameModeCollection.DEBUG || this.IsEditorObj) && Input.GetKeyDown(KeyCode.F)));
            }                
        }
        public bool IsInteractable { get; private set; } = true;
        public Transform InteractableObject { get; private set; } = null;
        internal void SetPrefab(bool isPrefab)
        {
            this.gameObject.SetActive(!isPrefab);
        }
        public void SetInteractableObject(Transform interactableObject)
        {
            this.InteractableObject = interactableObject;
        }
        public void SetText(string text)
        {
            if (this.Text is null) { return; }
            this.Text.text = text;
        }
        public void SetTextColor(Color color)
        {
            if (this.Text is null) { return; }
            this.Text.color = color;
        }
        private void SetImages()
        {
            Image[] images = this.GetComponentsInChildren<Image>();
            foreach (Image image in images)
            {
                if (image.gameObject.name.ToLower().Contains("shadow"))
                {
                    this.ShadowImage = image;
                }
                else if (image.gameObject.name.ToLower().Contains("icon"))
                {
                    this.IconImage = image;
                }
                if (this.IconImage != null && this.ShadowImage != null)
                {
                    break;
                }
            }                
        }
        void Start()
        {
            this.SetImages();
            this.IconImage?.transform.SetGlobalScale(DefaultScale * Vector3.one);
            this.ShadowImage?.transform.SetGlobalScale(DefaultScale * Vector3.one);
            this.Text = this.GetComponentInChildren<TextMeshProUGUI>();
            this.Text?.transform.SetGlobalScale(TextScale * Vector3.one);
            if (this.Text != null)
            {
                this.Text.enableWordWrapping = false;
                this.Text.overflowMode = TextOverflowModes.Overflow;
            }
            this.Text?.gameObject?.SetActive(false);
        }
        void Update()
        {
            if (!(this.InteractableObject?.gameObject?.activeSelf ?? false))
            {
                this.gameObject.SetActive(false);
                return;
            }
            if (this.IconImage is null || this.ShadowImage is null)
            {
                this.SetImages();
                if (this.IconImage is null || this.ShadowImage is null)
                {
                    return;
                }
            }
            if (this.Text is null)
            {
                this.Text = this.GetComponentInChildren<TextMeshProUGUI>();
                if (this.Text is null)
                {
                    return;
                }
            }
            this.IconImage.transform.rotation = Quaternion.identity;
            this.ShadowImage.transform.rotation = Quaternion.identity;
            this.Text.transform.rotation = Quaternion.identity;

            if (!this.LocalPlayerIsEligible)
            {
                this.IconImage.enabled = false;
                this.ShadowImage.enabled = false;
                this.IsInteractable = false;
                return;
            }
            this.IconImage.enabled = true;
            this.ShadowImage.enabled = true;
            this.IsInteractable = true;

            this.IconImage.color = this.IsHovering ? this.HoveredColor : this.DefaultColor;

            this.IconImage.transform.SetGlobalScale(this.Scale * Vector3.one);
            this.ShadowImage.transform.SetGlobalScale(this.Scale * Vector3.one);
            this.Text.transform.SetGlobalScale(TextScale * Vector3.one);

            if (this.WasClicked)
            {
                this.InteractableObject?.GetComponent<Interactable>()?.Call_TryInteract(PlayerManager.instance?.GetLocalPlayer());
            }

            Vector2 screenPos = MainCam.instance.cam.WorldToViewportPoint(this.InteractableObject.position); // get viewport positions

            if (screenPos.x >= 1-HorizMargin && screenPos.x <= HorizMargin && screenPos.y >= 1-VertMargin && screenPos.y <= VertMargin)
            {
                // point is on screen
                this.IconImage.transform.position = MainCam.instance.cam.WorldToScreenPoint(this.InteractableObject.position);
                this.ShadowImage.transform.position = MainCam.instance.cam.WorldToScreenPoint(this.InteractableObject.position);
                this.Text.transform.position = this.IconImage.transform.position - new Vector3(0f, VertTextOffset * Screen.height, 0f);
                return;
            }

            bool offHoriz = screenPos.x < 1 - HorizMargin || screenPos.x > HorizMargin; // is off the horizontal edge
            bool offVert = screenPos.y < 1 - VertMargin || screenPos.y > VertMargin; // is off the vertical edge
            bool offLeft = screenPos.x < 1 - HorizMargin; // is off the left edge
            bool offRight = screenPos.x > HorizMargin; // is off the right edge
            bool offTop = screenPos.y > VertMargin; // is off the top edge
            bool offBottom = screenPos.y < 1 - VertMargin; // is off the bottom edge

            Vector2 onScreenPos = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f) * 2f; // 2D version, new mapping
            float max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); // get largest offset

            float margin = offVert && offHoriz ? Mathf.Max(VertMargin, HorizMargin) : offVert ? VertMargin : offHoriz ? HorizMargin : 1f; // get margin

            onScreenPos = margin * (onScreenPos / (max * 2f)) + new Vector2(0.5f, 0.5f); // undo mapping, scale to add margin

            this.IconImage.transform.position = MainCam.instance.cam.ViewportToScreenPoint(onScreenPos);
            this.ShadowImage.transform.position = MainCam.instance.cam.ViewportToScreenPoint(onScreenPos);
            Vector2 textOffset = offVert && offHoriz ? new Vector2(HorizTextOffset, VertTextOffset) : offVert ? new Vector2(0f, VertTextOffset) : offHoriz ? new Vector2(HorizTextOffset, 0f) : new Vector2(0f, 0f);
            textOffset.Scale(offRight ? new Vector2(-1f, 1f) : Vector2.one);
            textOffset.Scale(offTop ? new Vector2(1f, -1f) : Vector2.one);
            this.Text.transform.rotation = (offTop || offBottom) ? Quaternion.identity : offLeft ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, -90f);
            this.Text.transform.position = MainCam.instance.cam.ViewportToScreenPoint(onScreenPos + textOffset);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            this.IsHovering = true;
            this.Text.gameObject.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            this.IsHovering = false;
            this.Text.gameObject.SetActive(false);
        }

    }
    public class InteractionTrigger : MonoBehaviour
    {
        void Start()
        {

        }
    }
    class TextBackground : MonoBehaviour
    {
        public const float pad = 0f;
        TextMeshProUGUI _text = null;
        TextMeshProUGUI Text
        {
            get
            {
                if (_text is null)
                {
                    _text = this.transform.parent.GetComponentInChildren<TextMeshProUGUI>();
                }
                return _text;
            }
        }
        void Start()
        {
        }
        public void CheckForChanges()
        {
            if (this.Text is null) { return; }
            this.Text.ForceMeshUpdate();
        }
        void Update()
        {
            this.transform.rotation = this.Text.transform.rotation;
            this.GetComponent<RectTransform>().sizeDelta = Text.textBounds.size + pad * Vector3.one;
            this.transform.localPosition = this.Text.transform.localPosition + this.Text.transform.rotation * this.Text.textBounds.center;
            this.GetComponent<UnityEngine.UI.Image>().enabled = this.Text.gameObject.activeSelf;
        }
    }
}
