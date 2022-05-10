using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MapEmbiggener.Controllers;
using GameModeCollection.GameModes;
namespace GMCObjects
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
                    _InteractableIcon = GameObject.Instantiate(GMCObjects.TRT_Assets.LoadAsset<GameObject>("TRT_TraitorInteractIcon"));
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
            if (player is null || player.data.dead || RoleManager
        }

        public abstract void OnInteract(Player player);

    }
    /// <summary>
    /// interaction icon for traitor map objects
    /// </summary>
    public class TraitorInterationIcon : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const float ConstScale = 0.3f;
        private Vector2 IconPosition; // position of the clickable icon
        private SpriteRenderer IconSprite { get; set; }
        
        internal void SetPrefab(bool isPrefab)
        {
            this.gameObject.SetActive(!isPrefab);
        }
        void Start()
        {
            this.IconPosition = this.transform.position;

            this.transform.localScale = ConstScale * MainCam.instance.cam.orthographicSize / ControllerManager.DefaultZoom * Vector3.one;

            foreach (SpriteRenderer spriteRenderer in this.GetComponentsInChildren<SpriteRenderer>(true))
            {
                spriteRenderer.sortingLayerID = SortingLayer.NameToID("MostFront");
            }

            this.CircleSprite = this.Circle.GetComponent<SpriteRenderer>();
            this.CircleBackgroundSprite = this.Circle.transform.GetChild(0).GetComponent<SpriteRenderer>();
            this.ArrowSprite = this.Arrow.GetComponent<SpriteRenderer>();
            this.ArrowBackgroundSprite = this.Arrow.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
        void Update()
        {
            this.FramesSinceCreation++;

            if (this.Timer != null && this.FramesSinceCreation > 5 && 1f - this.Timer.Perc < FadeOutTimerPerc)
            {
                this.Text.color = Color.Lerp(Color.clear, this.Text.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.CircleSprite.color = Color.Lerp(Color.clear, this.CircleSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.CircleBackgroundSprite.color = Color.Lerp(Color.clear, this.CircleBackgroundSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.ArrowSprite.color = Color.Lerp(Color.clear, this.ArrowSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                this.ArrowBackgroundSprite.color = Color.Lerp(Color.clear, this.ArrowBackgroundSprite.color, (1f - this.Timer.Perc) / (FadeOutTimerPerc - DestroyTimerPerc));
                if (this.Timer.Perc < DestroyTimerPerc)
                {
                    Destroy(this.gameObject);
                }
            }


            this.transform.localScale = ConstScale * MainCam.instance.cam.orthographicSize / ControllerManager.DefaultZoom * Vector3.one;

            this.Text.text = $"{Vector2.Distance(this.Player.transform.position, this.IconPosition):0}";

            Vector2 screenPos = MainCam.instance.cam.WorldToViewportPoint(this.IconPosition); //get viewport positions

            if (screenPos.x >= 0f && screenPos.x <= 1f && screenPos.y >= 0f && screenPos.y <= 1f)
            {
                // point is on screen
                this.transform.position = this.IconPosition;
                this.Circle.SetActive(true);
                this.Arrow.SetActive(false);
                this.transform.rotation = Quaternion.identity;
                return;
            }

            Vector2 onScreenPos = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f) * 2f; // 2D version, new mapping
            float max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); // get largest offset
            onScreenPos = 0.9f * (onScreenPos / (max * 2f)) + new Vector2(0.5f, 0.5f); // undo mapping, scale to add margin

            Vector2 newPos = MainCam.instance.cam.ViewportToWorldPoint(onScreenPos);
            this.transform.position = new Vector3(newPos.x, newPos.y, 0f);
            this.Circle.SetActive(false);
            this.Arrow.SetActive(true);
            this.transform.up = (Vector3)this.IconPosition - this.Player.transform.position;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
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
                background = new GameObject("Background", typeof(UnityEngine.UI.Image), typeof(PlayerNameSizeFitter)).transform;
                background.SetParent(nameText.transform.parent);
                background.SetAsFirstSibling();
                background.localPosition = Vector3.zero;
                background.localScale = Vector3.one;
            }
            background.GetComponent<UnityEngine.UI.Image>().color = color;
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
