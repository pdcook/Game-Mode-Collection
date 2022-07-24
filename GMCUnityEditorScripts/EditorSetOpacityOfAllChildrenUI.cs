using UnityEditor;
using InControl;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using GMCUnityScripts;

namespace GMCUnityEditorScripts
{
    [CustomEditor(typeof(SetOpacityOfAllChildrenUI))]
    public class EditorSetOpacityOfAllChildrenUI : Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            SetOpacityOfAllChildrenUI script = (SetOpacityOfAllChildrenUI)target;
            float a = EditorGUILayout.Slider(script.Opacity, 0f, 1f);
            script.SetOpacity(a);
        }
    }
}
