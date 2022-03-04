namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GlobalVariables))]
    public class GlobalVariablesInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Global Variabes", Array.Empty<GUILayoutOption>()))
            {
                GlobalVariablesWindow.ShowWindow();
            }
        }
    }
}

