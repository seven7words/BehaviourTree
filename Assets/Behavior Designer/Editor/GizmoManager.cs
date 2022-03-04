namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    public class GizmoManager
    {
        private static string currentScene = SceneManager.GetActiveScene().name;

        static GizmoManager()
        {
            EditorApplication.hierarchyChanged += HierarchyChange;
            if (!Application.isPlaying)
            {
                UpdateAllGizmos();
                EditorApplication.playModeStateChanged += UpdateAllGizmos;
            }
        }

        public static void HierarchyChange()
        {
            BehaviorManager instance = BehaviorManager.instance;
            if (!Application.isPlaying)
            {
                string str = SceneManager.GetActiveScene().name;
                if (currentScene != str)
                {
                    currentScene = str;
                    UpdateAllGizmos();
                }
            }
            else if (instance != null)
            {
                instance.onEnableBehavior = UpdateBehaviorManagerGizmos;
            }
        }

        public static void UpdateAllGizmos()
        {
            Behavior[] behaviorArray = UnityEngine.Object.FindObjectsOfType<Behavior>();
            for (int i = 0; i < behaviorArray.Length; i++)
            {
                UpdateGizmo(behaviorArray[i]);
            }
        }

        public static void UpdateAllGizmos(PlayModeStateChange change)
        {
            UpdateAllGizmos();
        }

        private static void UpdateBehaviorManagerGizmos()
        {
            BehaviorManager instance = BehaviorManager.instance;
            if (instance != null)
            {
                for (int i = 0; i < instance.BehaviorTrees.Count; i++)
                {
                    UpdateGizmo(instance.BehaviorTrees[i].behavior);
                }
            }
        }

        public static void UpdateGizmo(Behavior behavior)
        {
            behavior.gizmoViewMode = (Behavior.GizmoViewMode) BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
            behavior.showBehaviorDesignerGizmo = BehaviorDesignerPreferences.GetBool(BDPreferences.ShowSceneIcon);
        }
    }
}

