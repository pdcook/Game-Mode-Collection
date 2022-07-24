using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace GMCUnityScripts
{
    public class SetOpacityOfAllChildrenUI : MonoBehaviour
    {
        public float Opacity { get; private set; } = 1f;
        public void SetOpacity(float a)
        {
            this.Opacity = a;
            foreach (Image image in this.GetComponentsInChildren<Image>(true))
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, a);
            }
            foreach (TextMeshProUGUI text in this.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, a);
            }
        }
    }
}
