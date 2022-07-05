using UnityEngine;
using UnboundLib;
namespace GMCObjectsEditor.Visualizers
{
    public class TesterVisualizer : MonoBehaviour
    {
        LineRenderer buttonToRoom;
        LineRenderer buttonToDoor;
        LineRenderer buttonToLight;
        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }            
        void Start()
        {

            this.buttonToRoom = new GameObject("ButtonToRoom", typeof(LineRenderer)).GetComponent<LineRenderer>();
            this.buttonToRoom.transform.SetParent(this.transform.parent);
            this.SetupLine(this.buttonToRoom);
            this.buttonToDoor = new GameObject("ButtonToDoor", typeof(LineRenderer)).GetComponent<LineRenderer>();
            this.buttonToDoor.transform.SetParent(this.transform.parent);
            this.SetupLine(this.buttonToDoor);
            this.buttonToLight = new GameObject("ButtonToLight", typeof(LineRenderer)).GetComponent<LineRenderer>();
            this.buttonToLight.transform.SetParent(this.transform.parent);
            this.SetupLine(this.buttonToLight);

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
            if (!this.buttonToRoom.enabled) { return; }
            if (!this.buttonToDoor.enabled) { return; }
            if (!this.buttonToLight.enabled) { return; }
            Vector3? buttonPos = this.transform.GetChild(0)?.transform?.position;
            Vector3? roomPos = this.transform.GetChild(1)?.transform?.position;
            Vector3? lightPos = this.transform.GetChild(2)?.transform?.position;
            Vector3? doorPos = this.transform?.position;
            if (buttonPos.HasValue && roomPos.HasValue && lightPos.HasValue && doorPos.HasValue)
            {
                this.buttonToRoom.SetPositions(new Vector3[] { buttonPos.Value, roomPos.Value });
                this.buttonToLight.SetPositions(new Vector3[] { buttonPos.Value, lightPos.Value });
                this.buttonToDoor.SetPositions(new Vector3[] { buttonPos.Value, doorPos.Value });
            }
        }
    }
}
