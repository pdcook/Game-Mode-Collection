using System.Net.Mime;
using UnityEngine;
using MapsExt.Editor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace GMCObjectsEditor.Visualizers
{

	public class TraitorTrapJammerVisualizer : MonoBehaviour
	{
		private Image labelBg;
		private TextMeshProUGUI label;
		private Image positionIndicator;
		private Canvas canvas;

		public void SetEnabled(bool enabled)
		{
			this.enabled = enabled;
		}

		public void OnEnable()
		{
			var collider = this.gameObject.AddComponent<BoxCollider2D>();
			collider.size = new Vector3(3, 3);

			var colliderGo = new GameObject("Label Collider");
			colliderGo.transform.SetParent(this.transform);
			colliderGo.transform.localPosition = new Vector3(0, 2);

			var labelCollider = colliderGo.AddComponent<BoxCollider2D>();
			labelCollider.size = new Vector2(3, 1);

			var canvasGo = new GameObject("Canvas");
			canvasGo.transform.SetParent(this.transform);
			canvasGo.transform.localPosition = Vector3.zero;

			this.canvas = canvasGo.AddComponent<Canvas>();
			this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			this.canvas.sortingOrder = 1;

			var imageGo = new GameObject("Image");
			imageGo.transform.SetParent(canvasGo.transform);
			imageGo.transform.localScale = Vector3.one;

			this.labelBg = imageGo.AddComponent<ProceduralImage>();
			this.labelBg.rectTransform.sizeDelta = new Vector2(110f, 30f);
			this.labelBg.color = new Color(0, 0.57f, 0.45f, 0.2f);

			var modifier = imageGo.AddComponent<UniformModifier>();
			modifier.Radius = 8;

			var textGo = new GameObject("Text");
			textGo.transform.SetParent(canvasGo.transform);

			this.label = textGo.AddComponent<TextMeshProUGUI>();
			this.label.fontSize = 12;
			this.label.fontStyle = FontStyles.Bold;
			this.label.color = new Color(1, 1, 1, 0.8f);
			this.label.alignment = TextAlignmentOptions.Center;

			var pointCanvasGo = new GameObject("Position Indicator");
			pointCanvasGo.transform.SetParent(canvasGo.transform);

			this.positionIndicator = pointCanvasGo.AddComponent<Image>();
			this.positionIndicator.rectTransform.sizeDelta = UIUtils.WorldToScreenRect(new Rect(0, 0, 0.5f, 0.5f)).size;
		}

		public void OnDisable()
		{
			Object.Destroy(this.gameObject.GetComponent<BoxCollider2D>());
			Object.Destroy(this.transform.Find("Canvas").gameObject);
			Object.Destroy(this.transform.Find("Label Collider").gameObject);
		}

		public void LateUpdate()
		{
			this.label.text = "Traitor Trap\n(Jam Comms)";

			var screenPos = MainCam.instance.cam.WorldToScreenPoint(this.transform.position) - MainCam.instance.cam.WorldToScreenPoint(MainCam.instance.cam.transform.position);
			this.positionIndicator.rectTransform.anchoredPosition = screenPos;
			this.label.rectTransform.anchoredPosition = screenPos + new Vector3(0, 50f, 0);
			this.labelBg.rectTransform.anchoredPosition = screenPos + new Vector3(0, 50f, 0);
		}
	}
}
