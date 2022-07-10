using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace Assets
{
    public class MakeKeyTexts : MonoBehaviour
    {
        public void MakeTexts()
        {
            foreach (Image im in this.GetComponentsInChildren<Image>(true))
            {
                this.MakeTexts(im.gameObject);
            }

        }

        private void MakeTexts(GameObject go)
        {
            VerticalLayoutGroup vLG = go.GetOrAddComponent<VerticalLayoutGroup>();
            vLG.padding = new RectOffset(2, 2, 2, 2);
            vLG.childAlignment = TextAnchor.MiddleCenter;
            vLG.childControlHeight = true;
            vLG.childControlWidth = true;
            vLG.childForceExpandHeight = true;
            vLG.childForceExpandWidth = true;

            TextMeshProUGUI text = go.transform.GetOrCreateChild("Text").GetOrAddComponent<TextMeshProUGUI>();
            text.enableAutoSizing = true;
            text.fontSizeMin = 0f;
            text.color = Color.black;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.text = go.name.ToUpper();


        }
    }
}
