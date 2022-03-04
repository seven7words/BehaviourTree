// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorInspector
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (Behavior))]
  public class BehaviorInspector : UnityEditor.Editor
  {
    private bool mShowOptions = true;
    private bool mShowVariables;
    private static List<float> variablePosition;
    private static int selectedVariableIndex = -1;
    private static string selectedVariableName;
    private static int selectedVariableTypeIndex;

    private void OnEnable()
    {
      Behavior target = this.target as Behavior;
      if ((Object) target == (Object) null)
        return;
      GizmoManager.UpdateGizmo(target);
      if (!Application.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        BehaviorManager.IsPlaying = true;
      target.CheckForSerialization((Object) BehaviorDesignerWindow.instance == (Object) null && !Application.isPlaying && (Object) target.ExternalBehavior != (Object) null);
    }

    public override void OnInspectorGUI()
    {
      Behavior target = this.target as Behavior;
      if ((Object) target == (Object) null)
        return;
      bool externalModification = false;
      if (!BehaviorInspector.DrawInspectorGUI(target, this.serializedObject, true, ref externalModification, ref this.mShowOptions, ref this.mShowVariables))
        return;
      BehaviorDesignerUtility.SetObjectDirty((Object) target);
      if (!externalModification || !((Object) BehaviorDesignerWindow.instance != (Object) null) || target.GetBehaviorSource().BehaviorID != BehaviorDesignerWindow.instance.ActiveBehaviorID)
        return;
      BehaviorDesignerWindow.instance.LoadBehavior(target.GetBehaviorSource(), false, false);
    }

    public static bool DrawInspectorGUI(
      Behavior behavior,
      SerializedObject serializedObject,
      bool fromInspector,
      ref bool externalModification,
      ref bool showOptions,
      ref bool showVariables)
    {
      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      EditorGUILayout.LabelField("Behavior Name", GUILayout.Width(120f));
      behavior.GetBehaviorSource().behaviorName = EditorGUILayout.TextField(behavior.GetBehaviorSource().behaviorName, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (fromInspector && GUILayout.Button("Open", (GUILayoutOption[]) Array.Empty<GUILayoutOption>()))
      {
        BehaviorDesignerWindow.ShowWindow();
        BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, true);
      }
      GUILayout.EndHorizontal();
      EditorGUILayout.LabelField("Behavior Description", (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      behavior.GetBehaviorSource().behaviorDescription = EditorGUILayout.TextArea(behavior.GetBehaviorSource().behaviorDescription, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, GUILayout.Height(48f));
      serializedObject.Update();
      EditorGUI.BeginChangeCheck();
      GUI.enabled = BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) || PrefabUtility.GetPrefabAssetType((Object) behavior) != PrefabAssetType.Regular && PrefabUtility.GetPrefabAssetType((Object) behavior) != PrefabAssetType.Variant;
      SerializedProperty property = serializedObject.FindProperty("externalBehavior");
      ExternalBehavior objectReferenceValue = property.objectReferenceValue as ExternalBehavior;
      EditorGUILayout.PropertyField(property, true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (EditorGUI.EndChangeCheck())
        serializedObject.ApplyModifiedProperties();
      if (!object.ReferenceEquals((object) behavior.ExternalBehavior, (object) null) && !behavior.ExternalBehavior.Equals((object) objectReferenceValue) || !object.ReferenceEquals((object) objectReferenceValue, (object) null) && !objectReferenceValue.Equals((object) behavior.ExternalBehavior))
      {
        if (!object.ReferenceEquals((object) behavior.ExternalBehavior, (object) null))
        {
          behavior.ExternalBehavior.BehaviorSource.Owner = (IBehavior) behavior.ExternalBehavior;
          behavior.ExternalBehavior.BehaviorSource.CheckForSerialization(true, behavior.GetBehaviorSource());
        }
        else
        {
          behavior.GetBehaviorSource().EntryTask = (Task) null;
          behavior.GetBehaviorSource().RootTask = (Task) null;
          behavior.GetBehaviorSource().DetachedTasks = (List<Task>) null;
          behavior.GetBehaviorSource().Variables = (List<SharedVariable>) null;
          behavior.GetBehaviorSource().CheckForSerialization(true);
          behavior.GetBehaviorSource().Variables = (List<SharedVariable>) null;
          if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
            BinarySerialization.Save(behavior.GetBehaviorSource());
          else
            JSONSerialization.Save(behavior.GetBehaviorSource());
        }
        externalModification = true;
      }
      GUI.enabled = true;
      EditorGUILayout.PropertyField(serializedObject.FindProperty("group"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (fromInspector)
      {
        string key = "BehaviorDesigner.VariablesFoldout." + (object) behavior.GetHashCode();
        if (showVariables = EditorGUILayout.Foldout(EditorPrefs.GetBool(key, true), "Variables"))
        {
          ++EditorGUI.indentLevel;
          bool flag = false;
          BehaviorSource behaviorSource1 = behavior.GetBehaviorSource();
          List<SharedVariable> allVariables = behaviorSource1.GetAllVariables();
          if (allVariables != null && allVariables.Count > 0)
          {
            if (VariableInspector.DrawAllVariables(false, (IVariableSource) behaviorSource1, ref allVariables, false, ref BehaviorInspector.variablePosition, ref BehaviorInspector.selectedVariableIndex, ref BehaviorInspector.selectedVariableName, ref BehaviorInspector.selectedVariableTypeIndex, false, true))
            {
              if (!EditorApplication.isPlayingOrWillChangePlaymode && (Object) behavior.ExternalBehavior != (Object) null)
              {
                BehaviorSource behaviorSource2 = behavior.ExternalBehavior.GetBehaviorSource();
                behaviorSource2.CheckForSerialization(true);
                if (VariableInspector.SyncVariables(behaviorSource2, allVariables))
                {
                  if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                    BinarySerialization.Save(behaviorSource2);
                  else
                    JSONSerialization.Save(behaviorSource2);
                }
              }
              flag = true;
            }
          }
          else
            EditorGUILayout.LabelField("There are no variables to display", (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
          if (flag)
          {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
              BinarySerialization.Save(behaviorSource1);
            else
              JSONSerialization.Save(behaviorSource1);
          }
          --EditorGUI.indentLevel;
        }
        EditorPrefs.SetBool(key, showVariables);
      }
      string key1 = "BehaviorDesigner.OptionsFoldout." + (object) behavior.GetHashCode();
      if (!fromInspector || (showOptions = EditorGUILayout.Foldout(EditorPrefs.GetBool(key1, true), "Options")))
      {
        if (fromInspector)
          ++EditorGUI.indentLevel;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startWhenEnabled"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.PropertyField(serializedObject.FindProperty("asynchronousLoad"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseWhenDisabled"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.PropertyField(serializedObject.FindProperty("restartWhenComplete"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resetValuesOnRestart"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.PropertyField(serializedObject.FindProperty("logTaskChanges"), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        if (fromInspector)
          --EditorGUI.indentLevel;
      }
      if (fromInspector)
        EditorPrefs.SetBool(key1, showOptions);
      if (!EditorGUI.EndChangeCheck())
        return false;
      serializedObject.ApplyModifiedProperties();
      return true;
    }
  }
}
