using UnityEngine;
using UnboundLib;
namespace GMCObjectsEditor.Visualizers
{
    public class TeleporterVisualizer : MonoBehaviour
    {
        LineRenderer lineRenderer;
        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }            
        void Start()
        {
        }
        void OnEnable()
        {
            this.lineRenderer = this.gameObject.GetOrAddComponent<LineRenderer>();
            this.lineRenderer.enabled = true;
            this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            this.lineRenderer.startColor = new Color32(0, 255, 255, 150);
            this.lineRenderer.endColor = new Color32(0, 0, 100, 150);
            this.lineRenderer.startWidth = 0.2f;
            this.lineRenderer.endWidth = 0.2f;                
        }
        void LateUpdate()
        {
            if (!this.lineRenderer.enabled) { return; }
            Vector3? pos1 = this.transform.GetChild(0)?.transform?.position;
            Vector3? pos2 = this.transform.GetChild(1)?.transform?.position;
            if (pos1.HasValue && pos2.HasValue)
            {
                this.lineRenderer.SetPositions(new Vector3[] { pos1.Value, pos2.Value });
            }
        }
    }
}
