using UnityEngine;
namespace GMCObjectsEditor.Visualizers
{
    public class TraitorDoorVisualizer : MonoBehaviour
    {
        LineRenderer openerToDoor;
        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
        void Start()
        {

            this.openerToDoor = new GameObject("OpenerToDoor" + "", typeof(LineRenderer)).GetComponent<LineRenderer>();
            this.openerToDoor.transform.SetParent(this.transform.parent);
            this.SetupLine(this.openerToDoor);

        }
        void SetupLine(LineRenderer line)
        {
            line.enabled = true;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color32(100, 100, 100, 20);
            line.endColor = new Color32(100, 100, 100, 20);
            line.startWidth = 0.2f;
        }
        void LateUpdate()
        {
            if (!this.openerToDoor.enabled) { return; }
            Vector3? openerPos = this.transform.GetChild(0)?.transform?.position;
            Vector3? doorPos = this.transform?.position;
            if (openerPos.HasValue && doorPos.HasValue)
            {
                this.openerToDoor.SetPositions(new Vector3[] { openerPos.Value, doorPos.Value });
            }
        }
    }
}
