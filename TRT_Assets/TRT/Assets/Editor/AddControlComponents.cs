using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GMCUnityScripts;

[CustomEditor(typeof(AddControlComponents))]
public class AddControlComponentsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        AddControlComponents script = (AddControlComponents)target;
        if (GUILayout.Button("Add Control Components (Keyboard)"))
        {
            Transform[] transforms = script.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t.childCount > 0 && t.GetChild(0).name=="Text")
                {
                    ControlButton cb = t.gameObject.GetComponent<ControlButton>();
                    if (cb is null)
                    {
                        cb = t.gameObject.AddComponent<ControlButton>();
                    }
                    cb.Invoke("Reset",0f);
                }
            }
        }
        if (GUILayout.Button("Add Control Components (Other)"))
        {
            Transform[] transforms = script.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t.childCount == 0)
                {
                    ControlButton cb = t.gameObject.GetComponent<ControlButton>();
                    if (cb is null)
                    {
                        cb = t.gameObject.AddComponent<ControlButton>();
                    }
                    cb.Invoke("Reset", 0f);
                }
            }
        }
    }
}
