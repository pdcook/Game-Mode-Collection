using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MapEmbiggener.Controllers;
using GameModeCollection.GameModes.TRT;
namespace GameModeCollection.GMCObjects
{
    public static class TraitorInteractablePrefabs
    {
        private static GameObject _InteractableIcon = null;
        public static GameObject InteractableIcon
        {
            get
            {
                if (_InteractableIcon is null)
                {
                    _InteractableIcon = GameObject.Instantiate(GameModeCollection.TRT_Assets.LoadAsset<GameObject>("TRT_TraitorInteractIcon"));
                    _InteractableIcon.AddComponent<TraitorInterationIcon>().SetPrefab(true);
                    UnityEngine.GameObject.DontDestroyOnLoad(_InteractableIcon);
                }
                return _InteractableIcon;
            }
        }
    }
    /// <summary>
    /// Interaction base class for traitor map objects
    /// </summary>
    public abstract class TraitorInteractable : MonoBehaviour
    {
        public abstract string HoverText { get; protected set; }
        public abstract Vector2 InteractionOffset { get; protected set; }

        void Start()
        {
            
        }
        public void TryInteract(Player player)
        {
            if (player is null || player.data.dead || RoleManager.GetPlayerAlignment(player) != Alignment.Traitor)
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
    public class TraitorInterationIcon : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const float DefaultScale = 0.3f; // default scale of the icon
        private const float HoverScale = 0.4f; // scale of the icon when hovered
        private const float ClickScale = 0.2f; // scale of the icon when clicked
        private Vector2 IconPosition; // position of the clickable icon
        private SpriteRenderer IconSprite { get; set; }

        private float Scale => IsClicked ? ClickScale : IsHovering ? HoverScale : DefaultScale;

        public bool IsHovering { get; private set; } = false;
        public bool IsClicked { get; private set; } = false;
        public bool IsInteractable { get; private set; } = true;

        internal void SetPrefab(bool isPrefab)
        {
            this.gameObject.SetActive(!isPrefab);
        }
        void Start()
        {
            this.IconPosition = this.transform.position;

            this.transform.localScale = DefaultScale * MainCam.instance.cam.orthographicSize / ControllerManager.DefaultZoom * Vector3.one;

            this.IconSprite = this.GetComponent<SpriteRenderer>();
            this.IconSprite.sortingLayerID = SortingLayer.NameToID("MostFront");
        }
        void Update()
        {
            Player player = PlayerManager.instance.GetLocalPlayer();
            if (player is null || player.data.dead || RoleManager.GetPlayerAlignment(player) != Alignment.Traitor)
            {
                this.IconSprite.enabled = false;
                this.IsInteractable = false;
                return;
            }

            this.IsInteractable = true;

            this.transform.localScale = this.Scale * MainCam.instance.cam.orthographicSize / ControllerManager.DefaultZoom * Vector3.one;

            Vector2 screenPos = MainCam.instance.cam.WorldToViewportPoint(this.IconPosition); // get viewport positions

            if (screenPos.x >= 0f && screenPos.x <= 1f && screenPos.y >= 0f && screenPos.y <= 1f)
            {
                // point is on screen
                this.transform.position = this.IconPosition;
                this.transform.rotation = Quaternion.identity;
                return;
            }

            Vector2 onScreenPos = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f) * 2f; // 2D version, new mapping
            float max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); // get largest offset
            onScreenPos = 0.95f * (onScreenPos / (max * 2f)) + new Vector2(0.5f, 0.5f); // undo mapping, scale to add margin

            Vector2 newPos = MainCam.instance.cam.ViewportToWorldPoint(onScreenPos);
            this.transform.position = new Vector3(newPos.x, newPos.y, 0f);
            this.transform.up = player.transform.position - (Vector3)this.IconPosition;
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (this.IsHovering)
            {
                this.IsClicked = true;
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
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            this.IsHovering = false;
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
        public const float pad = 50f;
        private bool imageEnabled;
        void Start()
        {
            Transform background = this.transform.parent.Find("Background");
            if (background is null)
            {
                //background = new GameObject("Background", typeof(UnityEngine.UI.Image), typeof(PlayerNameSizeFitter)).transform;
                //background.SetParent(nameText.transform.parent);
                background.SetAsFirstSibling();
                background.localPosition = Vector3.zero;
                background.localScale = Vector3.one;
            }
            //background.GetComponent<UnityEngine.UI.Image>().color = color;
        }
        public void CheckForChanges()
        {
            TextMeshProUGUI text = this.GetComponent<TextMeshProUGUI>();
            if (text is null) { return; }
            text.ForceMeshUpdate();
            this.GetComponent<RectTransform>().sizeDelta = text.textBounds.size + pad * Vector3.one;
            this.transform.localPosition = text.transform.localPosition + text.textBounds.center;
        }
        void Update()
        {
            if (this.GetComponent<UnityEngine.UI.Image>().enabled != this.imageEnabled)
            {
                this.CheckForChanges();
            }
            this.imageEnabled = this.GetComponent<UnityEngine.UI.Image>().enabled;
        }
    }
}
