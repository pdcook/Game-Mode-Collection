using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MapEmbiggener.Controllers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes;
using GameModeCollection.Extensions;
namespace GameModeCollection.GMCObjects
{
    public static class TraitorInteractablePrefabs
    {
        private static GameObject _InteractableUI = null;
        public static GameObject InteractableUI
        {
            get
            {
                if (_InteractableUI is null)
                {
                    _InteractableUI = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_TraitorInteract"));
                    _InteractableUI.AddComponent<TraitorInteractionIcon>().SetPrefab(true);
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
    /// Interaction base class for traitor map objects
    /// </summary>
    public abstract class TraitorInteractable : MonoBehaviour
    {
        public abstract string HoverText { get; protected set; }
        public abstract Color TextColor { get; protected set; }

        protected void Start()
        {
            // add the interaction UI
            GameObject interactableUI = GameObject.Instantiate(TraitorInteractablePrefabs.InteractableUI, this.transform.parent);
            interactableUI.GetComponent<TraitorInteractionIcon>().SetInteractableObject(this.transform);
            interactableUI.SetActive(true);
            interactableUI.GetComponentInChildren<TextMeshProUGUI>().text = HoverText;
            interactableUI.GetComponentInChildren<TextMeshProUGUI>().color = TextColor;
            interactableUI.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            interactableUI.GetComponentInChildren<TextBackground>().CheckForChanges();
        }
        public void TryInteract(Player player)
        {
            if (!GameModeCollection.DEBUG && (player is null || player.data.dead || RoleManager.GetPlayerAlignment(player) != Alignment.Traitor))
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
    public class TraitorInteractionIcon : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const float DefaultScale = 0.6f; // default scale of the icon
        private const float HoverScale = 0.8f; // scale of the icon when hovered
        private const float ClickScale = 0.5f; // scale of the icon when clicked

        private const float HorizMargin = 0.95f; // horizontal margin of the icon from the screen border
        private const float VertMargin = 0.9f; // vertical margin of the icon from the screen border
        private const float HorizTextOffset = 0.01f; // offset of the text from the icon in viewport units (1f is the entire size of the screen)
        private const float VertTextOffset = 0.05f; // offset of the text from the icon in viewport units (1f is the entire size of the screen)

        private static readonly Color DefaultColor = new Color(1f, 1f, 1f, 0.75f);
        private static readonly Color HoveredColor = new Color(1f, 1f, 1f, 1f);

        private UnityEngine.UI.Image IconImage { get; set; }
        private TextMeshProUGUI Text { get; set; }

        private float Scale => this.IsClicked ? ClickScale : this.IsHovering ? HoverScale : DefaultScale;

        public bool IsHovering { get; private set; } = false;
        public bool IsClicked { get; private set; } = false;
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
        void Start()
        {

            this.IconImage = this.GetComponentInChildren<Image>();
            this.IconImage?.transform.SetGlobalScale(DefaultScale * Vector3.one);
            this.Text = this.GetComponentInChildren<TextMeshProUGUI>();
            this.Text?.transform.SetGlobalScale(DefaultScale * Vector3.one);
            this.Text.gameObject.SetActive(false);
        }
        void Update()
        {
            if (!this.InteractableObject.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
                return;
            }
            if (this.IconImage is null)
            {
                this.IconImage = this.GetComponentInChildren<Image>();
                if (this.IconImage is null)
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
            this.Text.transform.rotation = Quaternion.identity;

            Player player = PlayerManager.instance.GetLocalPlayer();
            if (!GameModeCollection.DEBUG && (this.InteractableObject is null || player is null || player.data.dead || RoleManager.GetPlayerAlignment(player) != Alignment.Traitor))
            {
                this.IconImage.enabled = false;
                this.IsInteractable = false;
                return;
            }

            this.IconImage.enabled = true;
            this.IsInteractable = true;

            this.IconImage.color = this.IsHovering ? HoveredColor : DefaultColor;

            this.IconImage.transform.SetGlobalScale(this.Scale * Vector3.one);
            this.Text.transform.SetGlobalScale(DefaultScale * Vector3.one);

            Vector2 screenPos = MainCam.instance.cam.WorldToViewportPoint(this.InteractableObject.position); // get viewport positions

            if (screenPos.x >= 1-HorizMargin && screenPos.x <= HorizMargin && screenPos.y >= 1-VertMargin && screenPos.y <= VertMargin)
            {
                // point is on screen
                this.IconImage.transform.position = MainCam.instance.cam.WorldToScreenPoint(this.InteractableObject.position);
                this.Text.transform.position = this.IconImage.transform.position + new Vector3(0f, VertTextOffset * Screen.height, 0f);
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
            Vector2 textOffset = offVert && offHoriz ? new Vector2(HorizTextOffset, VertTextOffset) : offVert ? new Vector2(0f, VertTextOffset) : offHoriz ? new Vector2(HorizTextOffset, 0f) : new Vector2(0f, 0f);
            textOffset.Scale(offRight ? new Vector2(-1f, 1f) : Vector2.one);
            textOffset.Scale(offTop ? new Vector2(1f, -1f) : Vector2.one);
            this.Text.transform.rotation = (offTop || offBottom) ? Quaternion.identity : offLeft ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, -90f);
            this.Text.transform.position = MainCam.instance.cam.ViewportToScreenPoint(onScreenPos + textOffset);
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (this.IsHovering)
            {
                this.IsClicked = true;
                this.InteractableObject?.GetComponent<TraitorInteractable>()?.TryInteract(PlayerManager.instance?.GetLocalPlayer());
            }
        }            
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (this.IsClicked)
            {
                this.IsClicked = false;
            }
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
    public class TraitorInteractionTrigger : MonoBehaviour
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
