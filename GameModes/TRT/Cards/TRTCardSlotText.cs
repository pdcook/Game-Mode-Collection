using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace GameModeCollection.GameModes.TRT.Cards
{
    internal class TRTCardSlotText : MonoBehaviour
    {
        private static GameObject _extraTextObj = null;
        internal static GameObject extraTextObj
        {
            get
            {
                if (_extraTextObj != null) { return _extraTextObj; }

                _extraTextObj = new GameObject("ExtraCardText", typeof(TextMeshProUGUI), typeof(DestroyOnUnparent));
                DontDestroyOnLoad(_extraTextObj);
                return _extraTextObj;


            }
            private set { }
        }

        private void Start()
        {
            // add extra text to bottom right
            // create blank object for text, and attach it to the canvas
            // find bottom right edge object
            RectTransform[] allChildrenRecursive = this.gameObject.GetComponentsInChildren<RectTransform>();
            GameObject BottomLeftCorner = allChildrenRecursive.Where(obj => obj.gameObject.name == "EdgePart (1)").FirstOrDefault().gameObject;
            GameObject modNameObj = UnityEngine.GameObject.Instantiate(extraTextObj, BottomLeftCorner.transform.position, BottomLeftCorner.transform.rotation, BottomLeftCorner.transform);
            TextMeshProUGUI modText = modNameObj.gameObject.GetComponent<TextMeshProUGUI>();

            // get the slot number
            CardInfo card = this.gameObject.GetComponentInChildren<CardInfo>();
            string extraText = "";
            foreach (CardCategory cardCategory in card.categories)
            {
                if (cardCategory == TRTCardCategories.TRT_Slot_0)
                {
                    extraText = "Slot 0";
                    break;
                }
                if (cardCategory == TRTCardCategories.TRT_Slot_1)
                {
                    extraText = "Slot 1";
                    break;
                }
                if (cardCategory == TRTCardCategories.TRT_Slot_2)
                {
                    extraText = "Slot 2";
                    break;
                }
                if (cardCategory == TRTCardCategories.TRT_Slot_3)
                {
                    extraText = "Slot 3";
                    break;
                }
                if (cardCategory == TRTCardCategories.TRT_Slot_4)
                {
                    extraText = "Slot 4";
                    break;
                }
                if (cardCategory == TRTCardCategories.TRT_Slot_5)
                {
                    extraText = "Slot 5";
                    break;
                }
            }
            if (string.IsNullOrWhiteSpace(extraText)) { Destroy(this); return; }

            modText.text = extraText;
            modText.enableWordWrapping = false;
            modNameObj.transform.Rotate(0f, 0f, 135f);
            modNameObj.transform.localScale = new Vector3(1f, 1f, 1f);
            modNameObj.transform.localPosition = new Vector3(-50f, -50f, 0f);
            modText.alignment = TextAlignmentOptions.Bottom;
            modText.alpha = 0.1f;
            modText.fontSize = 50;
        }
    }
    // destroy object once its no longer a child
    public class DestroyOnUnparent : MonoBehaviour
    {
        void LateUpdate()
        {
            if (this.gameObject.transform.parent == null) { Destroy(this.gameObject); }
        }
    }
}
