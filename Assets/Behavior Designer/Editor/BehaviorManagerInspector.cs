// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BehaviorManagerInspector
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    [CustomEditor(typeof (BehaviorManager))]
    public class BehaviorManagerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviorManager target = this.target as BehaviorManager;
            target.UpdateInterval = (UpdateIntervalType) EditorGUILayout.EnumPopup("Update Interval", (Enum) target.UpdateInterval, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
            if (target.UpdateInterval == UpdateIntervalType.SpecifySeconds)
            {
                ++EditorGUI.indentLevel;
                target.UpdateIntervalSeconds = EditorGUILayout.FloatField("Seconds", target.UpdateIntervalSeconds, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
                --EditorGUI.indentLevel;
            }
            target.ExecutionsPerTick = (BehaviorManager.ExecutionsPerTickType) EditorGUILayout.EnumPopup("Task Execution Type", (Enum) target.ExecutionsPerTick, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
            if (target.ExecutionsPerTick != BehaviorManager.ExecutionsPerTickType.Count)
                return;
            ++EditorGUI.indentLevel;
            target.MaxTaskExecutionsPerTick = EditorGUILayout.IntField("Max Execution Count", target.MaxTaskExecutionsPerTick, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
            --EditorGUI.indentLevel;
        }
    }
}