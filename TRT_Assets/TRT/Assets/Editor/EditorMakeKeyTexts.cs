using UnityEditor;
using InControl;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Assets;

[CustomEditor(typeof(MakeKeyTexts))]
public class EditorMakeKeyTexts : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        MakeKeyTexts script = (MakeKeyTexts)target;
        if (GUILayout.Button("Make Texts"))
        {
            script.MakeTexts();
        }
    }
}
