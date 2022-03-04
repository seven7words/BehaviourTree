// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.VariableSynchronizerInspector
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using  Object = UnityEngine.Object;
namespace BehaviorDesigner.Editor
{
  [CustomEditor(typeof (VariableSynchronizer))]
  public class VariableSynchronizerInspector : UnityEditor.Editor
  {
    [SerializeField]
    private VariableSynchronizerInspector.Synchronizer sharedVariableSynchronizer = new VariableSynchronizerInspector.Synchronizer();
    [SerializeField]
    private string sharedVariableValueTypeName;
    private System.Type sharedVariableValueType;
    [SerializeField]
    private VariableSynchronizer.SynchronizationType synchronizationType;
    [SerializeField]
    private bool setVariable;
    [SerializeField]
    private VariableSynchronizerInspector.Synchronizer targetSynchronizer;
    private Action<VariableSynchronizerInspector.Synchronizer, System.Type> thirdPartySynchronizer;
    private System.Type playMakerSynchronizationType;
    private System.Type uFrameSynchronizationType;

    public override void OnInspectorGUI()
    {
      VariableSynchronizer target = this.target as VariableSynchronizer;
      if ((Object) target == (Object) null)
        return;
      GUILayout.Space(5f);
      target.UpdateInterval = (UpdateIntervalType) EditorGUILayout.EnumPopup("Update Interval", (Enum) target.UpdateInterval, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (target.UpdateInterval == UpdateIntervalType.SpecifySeconds)
        target.UpdateIntervalSeconds = EditorGUILayout.FloatField("Seconds", target.UpdateIntervalSeconds, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUILayout.Space(5f);
      GUI.enabled = !Application.isPlaying;
      this.DrawSharedVariableSynchronizer(this.sharedVariableSynchronizer, (System.Type) null);
      if (string.IsNullOrEmpty(this.sharedVariableSynchronizer.targetName))
      {
        this.DrawSynchronizedVariables(target);
      }
      else
      {
        EditorGUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.LabelField("Direction", GUILayout.MaxWidth(146f));
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.LoadTexture(!this.setVariable ? "RightArrowButton.png" : "LeftArrowButton.png", obj: ((Object) this)), BehaviorDesignerUtility.ButtonGUIStyle, GUILayout.Width(22f)))
          this.setVariable = !this.setVariable;
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        this.synchronizationType = (VariableSynchronizer.SynchronizationType) EditorGUILayout.EnumPopup("Type", (Enum) this.synchronizationType, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        if (EditorGUI.EndChangeCheck())
          this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
        if (this.targetSynchronizer == null)
          this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
        if (this.sharedVariableValueType == (System.Type) null && !string.IsNullOrEmpty(this.sharedVariableValueTypeName))
          this.sharedVariableValueType = TaskUtility.GetTypeWithinAssembly(this.sharedVariableValueTypeName);
        switch (this.synchronizationType)
        {
          case VariableSynchronizer.SynchronizationType.BehaviorDesigner:
            this.DrawSharedVariableSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
          case VariableSynchronizer.SynchronizationType.Property:
            this.DrawPropertySynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
          case VariableSynchronizer.SynchronizationType.Animator:
            this.DrawAnimatorSynchronizer(this.targetSynchronizer);
            break;
          case VariableSynchronizer.SynchronizationType.PlayMaker:
            this.DrawPlayMakerSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
          case VariableSynchronizer.SynchronizationType.uFrame:
            this.DrawuFrameSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
            break;
        }
        if (string.IsNullOrEmpty(this.targetSynchronizer.targetName))
          GUI.enabled = false;
        if (GUILayout.Button("Add", (GUILayoutOption[]) Array.Empty<GUILayoutOption>()))
        {
          VariableSynchronizer.SynchronizedVariable synchronizedVariable = new VariableSynchronizer.SynchronizedVariable(this.synchronizationType, this.setVariable, this.sharedVariableSynchronizer.component as Behavior, this.sharedVariableSynchronizer.targetName, this.sharedVariableSynchronizer.global, this.targetSynchronizer.component, this.targetSynchronizer.targetName, this.targetSynchronizer.global);
          target.SynchronizedVariables.Add(synchronizedVariable);
          BehaviorDesignerUtility.SetObjectDirty((Object) target);
          this.sharedVariableSynchronizer = new VariableSynchronizerInspector.Synchronizer();
          this.targetSynchronizer = new VariableSynchronizerInspector.Synchronizer();
        }
        GUI.enabled = true;
        this.DrawSynchronizedVariables(target);
      }
    }

    public static void DrawComponentSelector(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      System.Type componentType,
      VariableSynchronizerInspector.ComponentListType listType)
    {
      bool flag = false;
      EditorGUI.BeginChangeCheck();
      synchronizer.gameObject = EditorGUILayout.ObjectField("GameObject", (Object) synchronizer.gameObject, typeof (GameObject), true, (GUILayoutOption[]) Array.Empty<GUILayoutOption>()) as GameObject;
      if (EditorGUI.EndChangeCheck())
        flag = true;
      if ((Object) synchronizer.gameObject == (Object) null)
        GUI.enabled = false;
      switch (listType)
      {
        case VariableSynchronizerInspector.ComponentListType.Instant:
          if (!flag)
            break;
          if ((Object) synchronizer.gameObject != (Object) null)
          {
            synchronizer.component = synchronizer.gameObject.GetComponent(componentType);
            break;
          }
          synchronizer.component = (Component) null;
          break;
        case VariableSynchronizerInspector.ComponentListType.Popup:
          int selectedIndex = 0;
          List<string> stringList = new List<string>();
          Component[] componentArray = (Component[]) null;
          stringList.Add("None");
          if ((Object) synchronizer.gameObject != (Object) null)
          {
            componentArray = synchronizer.gameObject.GetComponents(componentType);
            for (int index1 = 0; index1 < componentArray.Length; ++index1)
            {
              if (componentArray[index1].Equals((object) synchronizer.component))
                selectedIndex = stringList.Count;
              string str = BehaviorDesignerUtility.SplitCamelCase(componentArray[index1].GetType().Name);
              int num = 0;
              for (int index2 = 0; index2 < stringList.Count; ++index2)
              {
                if (stringList[index1].Equals(str))
                  ++num;
              }
              if (num > 0)
                str = str + " " + (object) num;
              stringList.Add(str);
            }
          }
          EditorGUI.BeginChangeCheck();
          int num1 = EditorGUILayout.Popup("Component", selectedIndex, stringList.ToArray(), (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
          if (!EditorGUI.EndChangeCheck())
            break;
          if (num1 != 0)
          {
            synchronizer.component = componentArray[num1 - 1];
            break;
          }
          synchronizer.component = (Component) null;
          break;
        case VariableSynchronizerInspector.ComponentListType.BehaviorDesignerGroup:
          if (!((Object) synchronizer.gameObject != (Object) null))
            break;
          Behavior[] components = synchronizer.gameObject.GetComponents<Behavior>();
          if (components != null && components.Length > 1)
            synchronizer.componentGroup = EditorGUILayout.IntField("Behavior Tree Group", synchronizer.componentGroup, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
          synchronizer.component = (Component) VariableSynchronizerInspector.GetBehaviorWithGroup(components, synchronizer.componentGroup);
          break;
      }
    }

    private bool DrawSharedVariableSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      System.Type valueType)
    {
      VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof (Behavior), VariableSynchronizerInspector.ComponentListType.BehaviorDesignerGroup);
      int selectedIndex = 0;
      int globalStartIndex = -1;
      string[] names = (string[]) null;
      if ((Object) synchronizer.component != (Object) null)
      {
        Behavior component = synchronizer.component as Behavior;
        selectedIndex = FieldInspector.GetVariablesOfType(valueType, synchronizer.global, synchronizer.targetName, component.GetBehaviorSource(), out names, ref globalStartIndex, valueType == (System.Type) null, false);
      }
      else
        names = new string[1]{ "None" };
      EditorGUI.BeginChangeCheck();
      int index = EditorGUILayout.Popup("Shared Variable", selectedIndex, names, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (EditorGUI.EndChangeCheck())
      {
        if (index != 0)
        {
          if (globalStartIndex != -1 && index >= globalStartIndex)
          {
            synchronizer.targetName = names[index].Substring(8, names[index].Length - 8);
            synchronizer.global = true;
          }
          else
          {
            synchronizer.targetName = names[index];
            synchronizer.global = false;
          }
          if (valueType == (System.Type) null)
          {
            this.sharedVariableValueTypeName = (!synchronizer.global ? (object) (synchronizer.component as Behavior).GetVariable(names[index]) : (object) GlobalVariables.Instance.GetVariable(synchronizer.targetName)).GetType().GetProperty("Value").PropertyType.FullName;
            this.sharedVariableValueType = (System.Type) null;
          }
        }
        else
          synchronizer.targetName = (string) null;
      }
      if (string.IsNullOrEmpty(synchronizer.targetName))
        GUI.enabled = false;
      return GUI.enabled;
    }

    private static Behavior GetBehaviorWithGroup(Behavior[] behaviors, int group)
    {
      if (behaviors == null || behaviors.Length == 0)
        return (Behavior) null;
      if (behaviors.Length == 1)
        return behaviors[0];
      for (int index = 0; index < behaviors.Length; ++index)
      {
        if (behaviors[index].Group == group)
          return behaviors[index];
      }
      return behaviors[0];
    }

    private void DrawPropertySynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      System.Type valueType)
    {
      VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof (Component), VariableSynchronizerInspector.ComponentListType.Popup);
      int selectedIndex = 0;
      List<string> stringList = new List<string>();
      stringList.Add("None");
      if ((Object) synchronizer.component != (Object) null)
      {
        PropertyInfo[] properties = synchronizer.component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        for (int index = 0; index < properties.Length; ++index)
        {
          if (properties[index].PropertyType.Equals(valueType) && !properties[index].IsSpecialName)
          {
            if (properties[index].Name.Equals(synchronizer.targetName))
              selectedIndex = stringList.Count;
            stringList.Add(properties[index].Name);
          }
        }
      }
      EditorGUI.BeginChangeCheck();
      int index1 = EditorGUILayout.Popup("Property", selectedIndex, stringList.ToArray(), (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (!EditorGUI.EndChangeCheck())
        return;
      if (index1 != 0)
        synchronizer.targetName = stringList[index1];
      else
        synchronizer.targetName = string.Empty;
    }

    private void DrawAnimatorSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer)
    {
      VariableSynchronizerInspector.DrawComponentSelector(synchronizer, typeof (Animator), VariableSynchronizerInspector.ComponentListType.Instant);
      synchronizer.targetName = EditorGUILayout.TextField("Parameter Name", synchronizer.targetName, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
    }

    private void DrawPlayMakerSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      System.Type valueType)
    {
      if (this.playMakerSynchronizationType == (System.Type) null)
      {
        this.playMakerSynchronizationType = System.Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_PlayMaker, Assembly-CSharp-Editor");
        if (this.playMakerSynchronizationType == (System.Type) null)
        {
          EditorGUILayout.LabelField("Unable to find PlayMaker inspector task.", (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
          return;
        }
      }
      if (this.thirdPartySynchronizer == null)
      {
        MethodInfo method = this.playMakerSynchronizationType.GetMethod(nameof (DrawPlayMakerSynchronizer));
        if (method != (MethodInfo) null)
          this.thirdPartySynchronizer = (Action<VariableSynchronizerInspector.Synchronizer, System.Type>) Delegate.CreateDelegate(typeof (Action<VariableSynchronizerInspector.Synchronizer, System.Type>), method);
      }
      this.thirdPartySynchronizer(synchronizer, valueType);
    }

    private void DrawuFrameSynchronizer(
      VariableSynchronizerInspector.Synchronizer synchronizer,
      System.Type valueType)
    {
      if (this.uFrameSynchronizationType == (System.Type) null)
      {
        this.uFrameSynchronizationType = System.Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_uFrame, Assembly-CSharp-Editor");
        if (this.uFrameSynchronizationType == (System.Type) null)
        {
          EditorGUILayout.LabelField("Unable to find uFrame inspector task.", (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
          return;
        }
      }
      if (this.thirdPartySynchronizer == null)
      {
        MethodInfo method = this.uFrameSynchronizationType.GetMethod("DrawSynchronizer");
        if (method != (MethodInfo) null)
          this.thirdPartySynchronizer = (Action<VariableSynchronizerInspector.Synchronizer, System.Type>) Delegate.CreateDelegate(typeof (Action<VariableSynchronizerInspector.Synchronizer, System.Type>), method);
      }
      this.thirdPartySynchronizer(synchronizer, valueType);
    }

    private void DrawSynchronizedVariables(VariableSynchronizer variableSynchronizer)
    {
      GUI.enabled = true;
      if (variableSynchronizer.SynchronizedVariables == null || variableSynchronizer.SynchronizedVariables.Count == 0)
        return;
      Rect lastRect = GUILayoutUtility.GetLastRect();
      lastRect.x = -5f;
      lastRect.y += lastRect.height + 1f;
      lastRect.height = 2f;
      lastRect.width += 20f;
      GUI.DrawTexture(lastRect, (Texture) BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", obj: ((Object) this)));
      GUILayout.Space(6f);
      for (int index = 0; index < variableSynchronizer.SynchronizedVariables.Count; ++index)
      {
        VariableSynchronizer.SynchronizedVariable synchronizedVariable = variableSynchronizer.SynchronizedVariables[index];
        if (synchronizedVariable.global)
        {
          if (GlobalVariables.Instance.GetVariable(synchronizedVariable.variableName) == null)
          {
            variableSynchronizer.SynchronizedVariables.RemoveAt(index);
            break;
          }
        }
        else if (synchronizedVariable.behavior.GetVariable(synchronizedVariable.variableName) == null)
        {
          variableSynchronizer.SynchronizedVariables.RemoveAt(index);
          break;
        }
        EditorGUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        EditorGUILayout.LabelField(synchronizedVariable.variableName, GUILayout.MaxWidth(120f));
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.LoadTexture(!synchronizedVariable.setVariable ? "RightArrowButton.png" : "LeftArrowButton.png", obj: ((Object) this)), BehaviorDesignerUtility.ButtonGUIStyle, GUILayout.Width(22f)) && !Application.isPlaying)
          synchronizedVariable.setVariable = !synchronizedVariable.setVariable;
        EditorGUILayout.LabelField(string.Format("{0} ({1})", (object) synchronizedVariable.targetName, (object) synchronizedVariable.synchronizationType.ToString()), GUILayout.MinWidth(120f));
        GUILayout.FlexibleSpace();
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.LoadTexture("DeleteButton.png", obj: ((Object) this)), BehaviorDesignerUtility.ButtonGUIStyle, GUILayout.Width(22f)))
        {
          variableSynchronizer.SynchronizedVariables.RemoveAt(index);
          EditorGUILayout.EndHorizontal();
          break;
        }
        GUILayout.Space(2f);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(2f);
      }
      GUILayout.Space(4f);
    }

    public enum ComponentListType
    {
      Instant,
      Popup,
      BehaviorDesignerGroup,
      None,
    }

    [Serializable]
    public class Synchronizer
    {
      public GameObject gameObject;
      public Component component;
      public string targetName;
      public bool global;
      public int componentGroup;
      public string componentName;
    }
  }
}
