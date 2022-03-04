// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ErrorWindow
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner.Editor
{
  public class ErrorWindow : EditorWindow
  {
    private List<BehaviorDesigner.Editor.ErrorDetails> mErrorDetails;
    private Vector2 mScrollPosition;
    public static ErrorWindow instance;

    public List<BehaviorDesigner.Editor.ErrorDetails> ErrorDetails
    {
      set => this.mErrorDetails = value;
    }

    [MenuItem("Tools/Behavior Designer/Error List", false, 2)]
    public static void ShowWindow()
    {
      ErrorWindow window = EditorWindow.GetWindow<ErrorWindow>(false, "Error List");
      window.minSize = new Vector2(400f, 200f);
      window.wantsMouseMove = true;
    }

    public void OnFocus()
    {
      ErrorWindow.instance = this;
      if (!(BehaviorDesignerWindow.instance != null))
        return;
      this.mErrorDetails = BehaviorDesignerWindow.instance.ErrorDetails;
    }

    public void OnGUI()
    {
      this.mScrollPosition = EditorGUILayout.BeginScrollView(this.mScrollPosition, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (this.mErrorDetails != null && this.mErrorDetails.Count > 0)
      {
        for (int index = 0; index < this.mErrorDetails.Count; ++index)
        {
          BehaviorDesigner.Editor.ErrorDetails mErrorDetail = this.mErrorDetails[index];
          if (mErrorDetail != null && (mErrorDetail.Type == BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidVariableReference || !(mErrorDetail.NodeDesigner == null) && mErrorDetail.NodeDesigner.Task != null))
          {
            string label = string.Empty;
            switch (mErrorDetail.Type)
            {
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.RequiredField:
                label = string.Format("The task {0} ({1}, index {2}) requires a value for the field {3}.", mErrorDetail.TaskFriendlyName, mErrorDetail.TaskType, mErrorDetail.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.SharedVariable:
                label = string.Format("The task {0} ({1}, index {2}) has a Shared Variable field ({3}) that is marked as shared but is not referencing a Shared Variable.", mErrorDetail.TaskFriendlyName, mErrorDetail.TaskType, mErrorDetail.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.NonUniqueDynamicVariable:
                label = string.Format("The task {0} ({1}, index {2}) has a dynamic Shared Variable ({3}) but the name matches an existing Shared Varaible.", mErrorDetail.TaskFriendlyName, mErrorDetail.TaskType, mErrorDetail.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.MissingChildren:
                label = string.Format("The {0} task ({1}, index {2}) is a parent task which does not have any children", mErrorDetail.TaskFriendlyName, mErrorDetail.TaskType, mErrorDetail.NodeDesigner.Task.ID);
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.UnknownTask:
                label = string.Format("The task at index {0} is unknown. Has a task been renamed or deleted?", mErrorDetail.NodeDesigner.Task.ID);
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidTaskReference:
                label = string.Format("The task {0} ({1}, index {2}) has a field ({3}) which is referencing an object within the scene. Behavior tree variables at the project level cannot reference objects within a scene.", mErrorDetail.TaskFriendlyName, mErrorDetail.TaskType, mErrorDetail.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(mErrorDetail.FieldName));
                break;
              case BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidVariableReference:
                label = string.Format("The variable {0} is referencing an object within the scene. Behavior tree variables at the project level cannot reference objects within a scene.", mErrorDetail.FieldName);
                break;
            }
            EditorGUILayout.LabelField(label, index % 2 != 0 ? BehaviorDesignerUtility.ErrorListDarkBackground : BehaviorDesignerUtility.ErrorListLightBackground, GUILayout.Height(30f), GUILayout.Width((float) (Screen.width - 7)));
          }
        }
      }
      else if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
        EditorGUILayout.LabelField("Enable realtime error checking from the preferences to view the errors.", BehaviorDesignerUtility.ErrorListLightBackground, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      else
        EditorGUILayout.LabelField("The behavior tree has no errors.", BehaviorDesignerUtility.ErrorListLightBackground, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      EditorGUILayout.EndScrollView();
    }
  }
}
