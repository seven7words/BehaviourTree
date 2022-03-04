// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ExternalBehaviorInspector
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (ExternalBehavior))]
  public class ExternalBehaviorInspector : UnityEditor.Editor
  {
    private bool mShowVariables;
    private static List<float> variablePosition;
    private static int selectedVariableIndex = -1;
    private static string selectedVariableName;
    private static int selectedVariableTypeIndex;

    public override void OnInspectorGUI()
    {
      ExternalBehavior target = this.target as ExternalBehavior;
      if (target == null)
        return;
      if (target.BehaviorSource.Owner == null)
        target.BehaviorSource.Owner = (IBehavior) target;
      if (!ExternalBehaviorInspector.DrawInspectorGUI(target.BehaviorSource, true, ref this.mShowVariables))
        return;
      BehaviorDesignerUtility.SetObjectDirty(target);
    }

    public void Reset()
    {
      ExternalBehavior target = this.target as ExternalBehavior;
      if (target == null || target.BehaviorSource.Owner != null)
        return;
      target.BehaviorSource.Owner = (IBehavior) target;
    }

    public static bool DrawInspectorGUI(
      BehaviorSource behaviorSource,
      bool fromInspector,
      ref bool showVariables)
    {
      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      EditorGUILayout.LabelField("Behavior Name", GUILayout.Width(120f));
      behaviorSource.behaviorName = EditorGUILayout.TextField(behaviorSource.behaviorName, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (fromInspector && GUILayout.Button("Open", (GUILayoutOption[]) Array.Empty<GUILayoutOption>()))
      {
        BehaviorDesignerWindow.ShowWindow();
        BehaviorDesignerWindow.instance.LoadBehavior(behaviorSource, false, true);
      }
      GUILayout.EndHorizontal();
      EditorGUILayout.LabelField("Behavior Description", (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      behaviorSource.behaviorDescription = EditorGUILayout.TextArea(behaviorSource.behaviorDescription, GUILayout.Height(48f));
      if (fromInspector)
      {
        string key = "BehaviorDesigner.VariablesFoldout." + behaviorSource.GetHashCode();
        if (showVariables = EditorGUILayout.Foldout(EditorPrefs.GetBool(key, true), "Variables"))
        {
          ++EditorGUI.indentLevel;
          List<SharedVariable> allVariables = behaviorSource.GetAllVariables();
          if (allVariables != null && VariableInspector.DrawAllVariables(false, (IVariableSource) behaviorSource, ref allVariables, false, ref ExternalBehaviorInspector.variablePosition, ref ExternalBehaviorInspector.selectedVariableIndex, ref ExternalBehaviorInspector.selectedVariableName, ref ExternalBehaviorInspector.selectedVariableTypeIndex, true, false))
          {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
              BinarySerialization.Save(behaviorSource);
            else
              JSONSerialization.Save(behaviorSource);
          }
          --EditorGUI.indentLevel;
        }
        EditorPrefs.SetBool(key, showVariables);
      }
      return EditorGUI.EndChangeCheck();
    }

    [OnOpenAsset(0)]
    public static bool ClickAction(int instanceID, int line)
    {
      ExternalBehavior externalBehavior = EditorUtility.InstanceIDToObject(instanceID) as ExternalBehavior;
      if (externalBehavior == null)
        return false;
      BehaviorDesignerWindow.ShowWindow();
      BehaviorDesignerWindow.instance.LoadBehavior(externalBehavior.BehaviorSource, false, true);
      return true;
    }
  }
}
