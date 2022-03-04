using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Editor
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class HierarchyIcon : ScriptableObject
    {
        private static Texture2D icon = (AssetDatabase.LoadAssetAtPath("Assets/Gizmos/Behavior Designer Hier Icon.png", typeof(Texture2D)) as Texture2D);

        static HierarchyIcon()
        {
            if (icon != null)
            {
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            }
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ShowHierarchyIcon))
                return;
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null) || !((UnityEngine.Object) gameObject.GetComponent<Behavior>() != (UnityEngine.Object) null))
                return;
            Rect position = new Rect(selectionRect);
            position.x = position.width + (selectionRect.x - 16f);
            position.width = 16f;
            position.height = 16f;
            GUI.DrawTexture(position, (Texture) HierarchyIcon.icon);
        }
    }
}

