using UnityEditor;
using InControl;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

[CustomEditor(typeof(ControlButton))]
public class EditorControlButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        ControlButton script = (ControlButton)target;
        string name = EditorGUILayout.TextField("Button Name", script.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            script.Name = name;
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Keyboard Controls");
        for (int i = 0; i < script.BoundKeyboard.Count; i++)
        {
            script.BoundKeyboard[i] = (Key)EditorGUILayout.EnumPopup(script.BoundKeyboard[i]);
        }
        if (GUILayout.Button("Add Key"))
        {
            script.BoundKeyboard.Add(Key.None);
        }
        if (GUILayout.Button("Remove Key") && script.BoundKeyboard.Count > 0)
        {
            script.BoundKeyboard.RemoveAt(script.BoundKeyboard.Count - 1);
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Mouse Controls");
        for (int i = 0; i < script.BoundMouse.Count; i++)
        {
            script.BoundMouse[i] = (Mouse)EditorGUILayout.EnumPopup(script.BoundMouse[i]);
        }
        if (GUILayout.Button("Add Button"))
        {
            script.BoundMouse.Add(Mouse.None);
        }
        if (GUILayout.Button("Remove Button") && script.BoundMouse.Count > 0)
        {
            script.BoundMouse.RemoveAt(script.BoundMouse.Count - 1);
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Controller Controls");
        for (int i = 0; i < script.BoundController.Count; i++)
        {
            script.BoundController[i] = (InputControlType)EditorGUILayout.EnumPopup(script.BoundController[i]);
        }
        if (GUILayout.Button("Add Control"))
        {
            script.BoundController.Add(InputControlType.None);
        }
        if (GUILayout.Button("Remove Control") && script.BoundController.Count > 0)
        {
            script.BoundController.RemoveAt(script.BoundController.Count - 1);
        }
        Color color = EditorGUILayout.ColorField("Color", script.GetComponent<Image>()?.color ?? Color.clear);
        script.SetColor(color);
    }
}
