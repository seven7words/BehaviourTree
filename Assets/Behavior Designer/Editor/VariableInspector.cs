// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.VariableInspector
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace BehaviorDesigner.Editor
{
  public class VariableInspector : ScriptableObject
  {
    private static string[] sharedVariableStrings;
    private static List<System.Type> sharedVariableTypes;
    private static Dictionary<string, int> sharedVariableTypesDict;
    private string mVariableName = string.Empty;
    private int mVariableTypeIndex;
    private Vector2 mScrollPosition = Vector2.zero;
    private bool mFocusNameField;
    [SerializeField]
    private float mVariableStartPosition = -1f;
    [SerializeField]
    private List<float> mVariablePosition;
    [SerializeField]
    private int mSelectedVariableIndex = -1;
    [SerializeField]
    private string mSelectedVariableName;
    [SerializeField]
    private int mSelectedVariableTypeIndex;
    private static SharedVariable mPropertyMappingVariable;
    private static BehaviorSource mPropertyMappingBehaviorSource;
    private static GenericMenu mPropertyMappingMenu;

    public void ResetSelectedVariableIndex()
    {
      this.mSelectedVariableIndex = -1;
      this.mVariableStartPosition = -1f;
      if (this.mVariablePosition == null)
        return;
      this.mVariablePosition.Clear();
    }

    public void OnEnable() => this.hideFlags = HideFlags.HideAndDontSave;

    public static List<System.Type> FindAllSharedVariableTypes(bool removeShared)
    {
      if (VariableInspector.sharedVariableTypes != null)
        return VariableInspector.sharedVariableTypes;
      VariableInspector.sharedVariableTypes = new List<System.Type>();
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        try
        {
          System.Type[] types = assembly.GetTypes();
          for (int index = 0; index < types.Length; ++index)
          {
            if (types[index].IsSubclassOf(typeof (SharedVariable)) && !types[index].IsAbstract)
              VariableInspector.sharedVariableTypes.Add(types[index]);
          }
        }
        catch (Exception ex)
        {
        }
      }
      VariableInspector.sharedVariableTypes.Sort((IComparer<System.Type>) new AlphanumComparator<System.Type>());
      VariableInspector.sharedVariableStrings = new string[VariableInspector.sharedVariableTypes.Count];
      VariableInspector.sharedVariableTypesDict = new Dictionary<string, int>();
      for (int index = 0; index < VariableInspector.sharedVariableTypes.Count; ++index)
      {
        string key = VariableInspector.sharedVariableTypes[index].Name;
        VariableInspector.sharedVariableTypesDict.Add(key, index);
        if (removeShared && key.Length > 6 && key.Substring(0, 6).Equals("Shared"))
          key = key.Substring(6, key.Length - 6);
        VariableInspector.sharedVariableStrings[index] = key;
      }
      return VariableInspector.sharedVariableTypes;
    }

    public bool ClearFocus(bool addVariable, BehaviorSource behaviorSource)
    {
      GUIUtility.keyboardControl = 0;
      GUI.FocusControl(string.Empty);
      bool flag = false;
      if (addVariable && !string.IsNullOrEmpty(this.mVariableName) && VariableInspector.VariableNameValid((IVariableSource) behaviorSource, this.mVariableName))
      {
        flag = VariableInspector.AddVariable((IVariableSource) behaviorSource, this.mVariableName, this.mVariableTypeIndex, false);
        this.mVariableName = string.Empty;
      }
      return flag;
    }

    public bool HasFocus() => GUIUtility.keyboardControl != 0;

    public void FocusNameField() => this.mFocusNameField = true;

    public bool LeftMouseDown(
      IVariableSource variableSource,
      BehaviorSource behaviorSource,
      Vector2 mousePosition)
    {
      return VariableInspector.LeftMouseDown(variableSource, behaviorSource, mousePosition, this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
    }

    public static bool LeftMouseDown(
      IVariableSource variableSource,
      BehaviorSource behaviorSource,
      Vector2 mousePosition,
      List<float> variablePosition,
      float variableStartPosition,
      Vector2 scrollPosition,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex)
    {
      if (variablePosition != null && (double) mousePosition.y > (double) variableStartPosition && variableSource != null)
      {
        List<SharedVariable> allVariables;
        if (!Application.isPlaying && behaviorSource != null && behaviorSource.Owner is Behavior)
        {
          Behavior owner = behaviorSource.Owner as Behavior;
          if ((Object) owner.ExternalBehavior != (Object) null)
          {
            BehaviorSource behaviorSource1 = owner.GetBehaviorSource();
            behaviorSource1.CheckForSerialization(true);
            allVariables = behaviorSource1.GetAllVariables();
            ExternalBehavior externalBehavior = owner.ExternalBehavior;
            externalBehavior.BehaviorSource.Owner = (IBehavior) externalBehavior;
            externalBehavior.BehaviorSource.CheckForSerialization(true, behaviorSource);
          }
          else
            allVariables = variableSource.GetAllVariables();
        }
        else
          allVariables = variableSource.GetAllVariables();
        if (allVariables == null || allVariables.Count != variablePosition.Count)
          return false;
        for (int index = 0; index < variablePosition.Count; ++index)
        {
          if ((double) mousePosition.y < (double) variablePosition[index] - (double) scrollPosition.y)
          {
            if (index == selectedVariableIndex)
              return false;
            selectedVariableIndex = index;
            selectedVariableName = allVariables[index].Name;
            selectedVariableTypeIndex = VariableInspector.sharedVariableTypesDict[allVariables[index].GetType().Name];
            return true;
          }
        }
      }
      if (selectedVariableIndex == -1)
        return false;
      selectedVariableIndex = -1;
      return true;
    }

    public bool DrawVariables(BehaviorSource behaviorSource) => VariableInspector.DrawVariables((IVariableSource) behaviorSource, behaviorSource, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);

    public static bool DrawVariables(
      IVariableSource variableSource,
      BehaviorSource behaviorSource,
      ref string variableName,
      ref bool focusNameField,
      ref int variableTypeIndex,
      ref Vector2 scrollPosition,
      ref List<float> variablePosition,
      ref float variableStartPosition,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex)
    {
      scrollPosition = GUILayout.BeginScrollView(scrollPosition, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      bool flag1 = false;
      bool flag2 = false;
      List<SharedVariable> sharedVariableList = variableSource == null ? (List<SharedVariable>) null : variableSource.GetAllVariables();
      if (VariableInspector.DrawHeader(variableSource, behaviorSource == null, ref variableStartPosition, ref variableName, ref focusNameField, ref variableTypeIndex, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex))
        flag1 = true;
      List<SharedVariable> variables = variableSource == null ? (List<SharedVariable>) null : variableSource.GetAllVariables();
      if (variables != null && variables.Count > 0)
      {
        GUI.enabled = !flag2;
        if (VariableInspector.DrawAllVariables(true, variableSource, ref variables, true, ref variablePosition, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, true, true))
          flag1 = true;
      }
      if (flag1 && variableSource != null)
        variableSource.SetAllVariables(variables);
      GUI.enabled = true;
      GUILayout.EndScrollView();
      if (flag1 && !EditorApplication.isPlayingOrWillChangePlaymode && (behaviorSource != null && behaviorSource.Owner is Behavior))
      {
        Behavior owner = behaviorSource.Owner as Behavior;
        if ((Object) owner.ExternalBehavior != (Object) null)
        {
          if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
            BinarySerialization.Save(behaviorSource);
          else
            JSONSerialization.Save(behaviorSource);
          BehaviorSource behaviorSource1 = owner.ExternalBehavior.GetBehaviorSource();
          behaviorSource1.CheckForSerialization(true);
          VariableInspector.SyncVariables(behaviorSource1, variables);
        }
      }
      return flag1;
    }

    public static bool SyncVariables(
      BehaviorSource localBehaviorSource,
      List<SharedVariable> variables)
    {
      List<SharedVariable> variables1 = localBehaviorSource.GetAllVariables();
      if (variables == null)
      {
        if (variables1 == null || variables1.Count <= 0)
          return false;
        variables1.Clear();
        return true;
      }
      bool flag = false;
      if (variables1 == null)
      {
        variables1 = new List<SharedVariable>();
        localBehaviorSource.SetAllVariables(variables1);
        flag = true;
      }
      for (int index = 0; index < variables.Count; ++index)
      {
        if (variables[index] != null)
        {
          if (variables1.Count - 1 < index)
          {
            SharedVariable instance = Activator.CreateInstance(variables[index].GetType()) as SharedVariable;
            instance.Name = variables[index].Name;
            instance.IsShared = true;
            instance.SetValue(variables[index].GetValue());
            variables1.Add(instance);
            flag = true;
          }
          else if (variables1[index].Name != variables[index].Name || variables1[index].GetType() != variables[index].GetType())
          {
            SharedVariable instance = Activator.CreateInstance(variables[index].GetType()) as SharedVariable;
            instance.Name = variables[index].Name;
            instance.IsShared = true;
            instance.SetValue(variables[index].GetValue());
            variables1[index] = instance;
            flag = true;
          }
        }
      }
      for (int index = variables1.Count - 1; index > variables.Count - 1; --index)
      {
        variables1.RemoveAt(index);
        flag = true;
      }
      return flag;
    }

    private static bool DrawHeader(
      IVariableSource variableSource,
      bool fromGlobalVariablesWindow,
      ref float variableStartPosition,
      ref string variableName,
      ref bool focusNameField,
      ref int variableTypeIndex,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex)
    {
      if (VariableInspector.sharedVariableTypes == null)
        VariableInspector.FindAllSharedVariableTypes(true);
      EditorGUIUtility.labelWidth = 150f;
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUILayout.Space(4f);
      EditorGUILayout.LabelField("Name", GUILayout.Width(70f));
      GUI.SetNextControlName("Name");
      variableName = EditorGUILayout.TextField(variableName, GUILayout.Width(212f));
      if (focusNameField)
      {
        GUI.FocusControl("Name");
        focusNameField = false;
      }
      GUILayout.EndHorizontal();
      GUILayout.Space(2f);
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUILayout.Space(4f);
      GUILayout.Label("Type", GUILayout.Width(70f));
      variableTypeIndex = EditorGUILayout.Popup(variableTypeIndex, VariableInspector.sharedVariableStrings, EditorStyles.popup, GUILayout.Width(163f));
      GUILayout.Space(8f);
      bool flag1 = false;
      bool flag2 = VariableInspector.VariableNameValid(variableSource, variableName);
      bool enabled = GUI.enabled;
      GUI.enabled = flag2 && enabled;
      GUI.SetNextControlName("Add");
      if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(40f)) && flag2)
      {
        if (fromGlobalVariablesWindow && variableSource == null)
        {
          GlobalVariables instance = ScriptableObject.CreateInstance(typeof (GlobalVariables)) as GlobalVariables;
          string str1 = BehaviorDesignerUtility.GetEditorBaseDirectory().Substring(6, BehaviorDesignerUtility.GetEditorBaseDirectory().Length - 13);
          string str2 = str1 + "/Resources/BehaviorDesignerGlobalVariables.asset";
          if (!Directory.Exists(Application.dataPath + str1 + "/Resources"))
            Directory.CreateDirectory(Application.dataPath + str1 + "/Resources");
          if (!File.Exists(Application.dataPath + str2))
          {
            AssetDatabase.CreateAsset((Object) instance, "Assets" + str2);
            EditorUtility.DisplayDialog("Created Global Variables", "Behavior Designer Global Variables asset created:\n\nAssets" + str1 + "/Resources/BehaviorDesignerGlobalVariables.asset\n\nNote: Copy this file to transfer global variables between projects.", "OK");
          }
          variableSource = (IVariableSource) instance;
        }
        flag1 = VariableInspector.AddVariable(variableSource, variableName, variableTypeIndex, fromGlobalVariablesWindow);
        if (flag1)
        {
          selectedVariableIndex = variableSource.GetAllVariables().Count - 1;
          selectedVariableName = variableName;
          selectedVariableTypeIndex = variableTypeIndex;
          variableName = string.Empty;
          GUI.FocusControl(string.Empty);
        }
      }
      GUILayout.Space(6f);
      GUILayout.EndHorizontal();
      if (!fromGlobalVariablesWindow)
      {
        GUI.enabled = true;
        GUILayout.Space(3f);
        GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        GUILayout.Space(5f);
        if (GUILayout.Button("Global Variables", EditorStyles.miniButton, GUILayout.Width(284f)))
          GlobalVariablesWindow.ShowWindow();
        GUILayout.EndHorizontal();
      }
      BehaviorDesignerUtility.DrawContentSeperator(2);
      GUILayout.Space(4f);
      if ((double) variableStartPosition == -1.0 && Event.current.type == UnityEngine.EventType.Repaint)
        variableStartPosition = GUILayoutUtility.GetLastRect().yMax;
      GUI.enabled = enabled;
      return flag1;
    }

    private static bool AddVariable(
      IVariableSource variableSource,
      string variableName,
      int variableTypeIndex,
      bool fromGlobalVariablesWindow)
    {
      SharedVariable variable = VariableInspector.CreateVariable(variableTypeIndex, variableName, fromGlobalVariablesWindow);
      List<SharedVariable> variables = (variableSource == null ? (List<SharedVariable>) null : variableSource.GetAllVariables()) ?? new List<SharedVariable>();
      variables.Add(variable);
      variableSource.SetAllVariables(variables);
      return true;
    }

    public static bool DrawAllVariables(
      bool showFooter,
      IVariableSource variableSource,
      ref List<SharedVariable> variables,
      bool canSelect,
      ref List<float> variablePosition,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex,
      bool drawRemoveButton,
      bool drawLastSeparator)
    {
      if (variables == null)
        return false;
      bool flag = false;
      if (canSelect && variablePosition == null)
        variablePosition = new List<float>();
      for (int index = 0; index < variables.Count; ++index)
      {
        SharedVariable sharedVariable = variables[index];
        if (sharedVariable != null && !sharedVariable.IsDynamic)
        {
          if (canSelect && selectedVariableIndex == index)
          {
            if (index == 0)
              GUILayout.Space(2f);
            bool deleted = false;
            if (VariableInspector.DrawSelectedVariable(variableSource, ref variables, sharedVariable, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, ref deleted))
              flag = true;
            if (deleted)
            {
              if ((Object) BehaviorDesignerWindow.instance != (Object) null)
                BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
              variables.RemoveAt(index);
              if (selectedVariableIndex == index)
                selectedVariableIndex = -1;
              else if (selectedVariableIndex > index)
                --selectedVariableIndex;
              flag = true;
              break;
            }
          }
          else
          {
            GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
            if (VariableInspector.DrawSharedVariable(variableSource, sharedVariable, false))
              flag = true;
            if (drawRemoveButton)
            {
              if (GUILayout.Button((Texture) BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, GUILayout.Width(19f)) && EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
              {
                if ((Object) BehaviorDesignerWindow.instance != (Object) null)
                {
                  if (BehaviorDesignerWindow.instance.ActiveBehaviorSource != null)
                    BehaviorUndo.RegisterUndo("Delete Variable", BehaviorDesignerWindow.instance.ActiveBehaviorSource.Owner.GetObject());
                  BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
                }
                variables.RemoveAt(index);
                if (canSelect)
                {
                  if (selectedVariableIndex == index)
                    selectedVariableIndex = -1;
                  else if (selectedVariableIndex > index)
                    --selectedVariableIndex;
                }
                flag = true;
                break;
              }
            }
            if ((Object) BehaviorDesignerWindow.instance != (Object) null && BehaviorDesignerWindow.instance.ContainsError((Task) null, variables[index].Name))
              GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, GUILayout.Width(20f));
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
            if (index != variables.Count - 1 || drawLastSeparator)
              BehaviorDesignerUtility.DrawContentSeperator(2, 7);
          }
          GUILayout.Space(4f);
          if (canSelect && Event.current.type == UnityEngine.EventType.Repaint)
          {
            if (variablePosition.Count <= index)
              variablePosition.Add(GUILayoutUtility.GetLastRect().yMax);
            else
              variablePosition[index] = GUILayoutUtility.GetLastRect().yMax;
          }
        }
      }
      if (canSelect && variables.Count < variablePosition.Count)
      {
        for (int index = variablePosition.Count - 1; index >= variables.Count; --index)
          variablePosition.RemoveAt(index);
      }
      if (showFooter && variables.Count > 0)
      {
        GUI.enabled = true;
        GUILayout.Label("Select a variable to change its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      }
      return flag;
    }

    private static bool DrawSharedVariable(
      IVariableSource variableSource,
      SharedVariable sharedVariable,
      bool selected)
    {
      if (sharedVariable == null || sharedVariable.GetType().GetProperty("Value") == (PropertyInfo) null)
        return false;
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      bool flag = false;
      if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
      {
        if (selected)
          GUILayout.Label("Property", (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        else
          GUILayout.Label(new GUIContent(sharedVariable.Name, sharedVariable.Tooltip), (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
        string[] strArray = sharedVariable.PropertyMapping.Split('.');
        GUILayout.Label(strArray[strArray.Length - 1].Replace('/', '.'), (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      }
      else
      {
        EditorGUI.BeginChangeCheck();
        FieldInspector.DrawFields((Task) null, (object) sharedVariable, new GUIContent(sharedVariable.Name, sharedVariable.Tooltip));
        flag = EditorGUI.EndChangeCheck();
      }
      if (!sharedVariable.IsGlobal)
      {
        if (GUILayout.Button((Texture) BehaviorDesignerUtility.VariableMapButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
          VariableInspector.ShowPropertyMappingMenu(variableSource as BehaviorSource, sharedVariable);
      }
      GUILayout.EndHorizontal();
      return flag;
    }

    private static bool DrawSelectedVariable(
      IVariableSource variableSource,
      ref List<SharedVariable> variables,
      SharedVariable sharedVariable,
      ref int selectedVariableIndex,
      ref string selectedVariableName,
      ref int selectedVariableTypeIndex,
      ref bool deleted)
    {
      bool flag = false;
      GUILayout.BeginVertical(BehaviorDesignerUtility.SelectedBackgroundGUIStyle, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUILayout.Label("Name", GUILayout.Width(70f));
      EditorGUI.BeginChangeCheck();
      selectedVariableName = EditorGUILayout.TextField(selectedVariableName, GUILayout.Width(140f));
      if (EditorGUI.EndChangeCheck())
      {
        if (VariableInspector.VariableNameValid(variableSource, selectedVariableName))
          variableSource.UpdateVariableName(sharedVariable, selectedVariableName);
        flag = true;
      }
      GUILayout.Space(10f);
      bool enabled = GUI.enabled;
      GUI.enabled = enabled && selectedVariableIndex < variables.Count - 1;
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.DownArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
      {
        SharedVariable sharedVariable1 = variables[selectedVariableIndex + 1];
        variables[selectedVariableIndex + 1] = variables[selectedVariableIndex];
        variables[selectedVariableIndex] = sharedVariable1;
        ++selectedVariableIndex;
        flag = true;
      }
      GUI.enabled = enabled && (selectedVariableIndex < variables.Count - 1 || selectedVariableIndex != 0);
      GUILayout.Box(string.Empty, BehaviorDesignerUtility.ArrowSeparatorGUIStyle, GUILayout.Width(1f), GUILayout.Height(18f));
      GUI.enabled = enabled && selectedVariableIndex != 0;
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.UpArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, GUILayout.Width(20f)))
      {
        SharedVariable sharedVariable1 = variables[selectedVariableIndex - 1];
        variables[selectedVariableIndex - 1] = variables[selectedVariableIndex];
        variables[selectedVariableIndex] = sharedVariable1;
        --selectedVariableIndex;
        flag = true;
      }
      GUI.enabled = enabled;
      if (GUILayout.Button((Texture) BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, GUILayout.Width(19f)) && EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
        deleted = true;
      GUILayout.EndHorizontal();
      GUILayout.Space(2f);
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUILayout.Label("Type", GUILayout.Width(70f));
      EditorGUI.BeginChangeCheck();
      selectedVariableTypeIndex = EditorGUILayout.Popup(selectedVariableTypeIndex, VariableInspector.sharedVariableStrings, EditorStyles.toolbarPopup, GUILayout.Width(200f));
      if (EditorGUI.EndChangeCheck() && VariableInspector.sharedVariableTypesDict[sharedVariable.GetType().Name] != selectedVariableTypeIndex)
      {
        if ((Object) BehaviorDesignerWindow.instance != (Object) null)
          BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
        sharedVariable = VariableInspector.CreateVariable(selectedVariableTypeIndex, sharedVariable.Name, sharedVariable.IsGlobal);
        variables[selectedVariableIndex] = sharedVariable;
        flag = true;
      }
      GUILayout.EndHorizontal();
      GUILayout.Space(2f);
      EditorGUI.BeginChangeCheck();
      sharedVariable.Tooltip = EditorGUILayout.TextField("Tooltip", sharedVariable.Tooltip, (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (EditorGUI.EndChangeCheck())
        flag = true;
      EditorGUI.BeginChangeCheck();
      GUILayout.Space(4f);
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      GUI.enabled = VariableInspector.CanNetworkSync(sharedVariable.GetType().GetProperty("Value").PropertyType);
      EditorGUI.BeginChangeCheck();
      if (EditorGUI.EndChangeCheck())
        flag = true;
      GUILayout.EndHorizontal();
      GUI.enabled = enabled;
      GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
      if (VariableInspector.DrawSharedVariable(variableSource, sharedVariable, true))
        flag = true;
      if ((Object) BehaviorDesignerWindow.instance != (Object) null && BehaviorDesignerWindow.instance.ContainsError((Task) null, variables[selectedVariableIndex].Name))
        GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, GUILayout.Width(20f));
      GUILayout.EndHorizontal();
      BehaviorDesignerUtility.DrawContentSeperator(4, 7);
      GUILayout.EndVertical();
      GUILayout.Space(3f);
      return flag;
    }

    private static bool VariableNameValid(IVariableSource variableSource, string variableName)
    {
      if (variableName.Equals(string.Empty))
        return false;
      return variableSource == null || variableSource.GetVariable(variableName) == null;
    }

    private static SharedVariable CreateVariable(int index, string name, bool global)
    {
      SharedVariable instance = Activator.CreateInstance(VariableInspector.sharedVariableTypes[index]) as SharedVariable;
      instance.Name = name;
      instance.IsShared = true;
      instance.IsGlobal = global;
      return instance;
    }

    private static bool CanNetworkSync(System.Type type) => type == typeof (bool) || type == typeof (Color) || (type == typeof (float) || type == typeof (GameObject)) || (type == typeof (int) || type == typeof (Quaternion) || (type == typeof (Rect) || type == typeof (string))) || (type == typeof (Transform) || type == typeof (Vector2) || (type == typeof (Vector3) || type == typeof (Vector4)));

    private static void ShowPropertyMappingMenu(
      BehaviorSource behaviorSource,
      SharedVariable sharedVariable)
    {
      VariableInspector.mPropertyMappingVariable = sharedVariable;
      VariableInspector.mPropertyMappingBehaviorSource = behaviorSource;
      VariableInspector.mPropertyMappingMenu = new GenericMenu();
      List<string> propertyNames = new List<string>();
      List<GameObject> propertyGameObjects = new List<GameObject>();
      propertyNames.Add("None");
      propertyGameObjects.Add((GameObject) null);
      int num1 = 0;
      if (behaviorSource.Owner.GetObject() is Behavior)
      {
        GameObject gameObject = (behaviorSource.Owner.GetObject() as Behavior).gameObject;
        int num2;
        if ((num2 = VariableInspector.AddPropertyName(sharedVariable, gameObject, ref propertyNames, ref propertyGameObjects, true)) != -1)
          num1 = num2;
        GameObject[] gameObjectArray;
        if (AssetDatabase.GetAssetPath((Object) gameObject).Length == 0)
        {
          gameObjectArray = Object.FindObjectsOfType<GameObject>();
        }
        else
        {
          Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
          gameObjectArray = new GameObject[componentsInChildren.Length];
          for (int index = 0; index < componentsInChildren.Length; ++index)
            gameObjectArray[index] = componentsInChildren[index].gameObject;
        }
        for (int index = 0; index < gameObjectArray.Length; ++index)
        {
          int num3;
          if (!gameObjectArray[index].Equals((object) gameObject) && (num3 = VariableInspector.AddPropertyName(sharedVariable, gameObjectArray[index], ref propertyNames, ref propertyGameObjects, false)) != -1)
            num1 = num3;
        }
      }
      for (int index = 0; index < propertyNames.Count; ++index)
      {
        string[] strArray = propertyNames[index].Split('.');
        if ((Object) propertyGameObjects[index] != (Object) null)
          strArray[strArray.Length - 1] = VariableInspector.GetFullPath(propertyGameObjects[index].transform) + "/" + strArray[strArray.Length - 1];
        GenericMenu propertyMappingMenu = VariableInspector.mPropertyMappingMenu;
        GUIContent content = new GUIContent(strArray[strArray.Length - 1]);
        int num2 = index == num1 ? 1 : 0;
        VariableInspector.SelectedPropertyMapping selectedPropertyMapping = new VariableInspector.SelectedPropertyMapping(propertyNames[index], propertyGameObjects[index]);
        propertyMappingMenu.AddItem(content, num2 != 0, PropertySelected, (object) selectedPropertyMapping);
      }
      VariableInspector.mPropertyMappingMenu.ShowAsContext();
    }

    private static string GetFullPath(Transform transform) => (Object) transform.parent == (Object) null ? transform.name : VariableInspector.GetFullPath(transform.parent) + "/" + transform.name;

    private static int AddPropertyName(
      SharedVariable sharedVariable,
      GameObject gameObject,
      ref List<string> propertyNames,
      ref List<GameObject> propertyGameObjects,
      bool behaviorGameObject)
    {
      int num = -1;
      if ((Object) gameObject != (Object) null)
      {
        Component[] components = gameObject.GetComponents(typeof (Component));
        System.Type propertyType = sharedVariable.GetType().GetProperty("Value").PropertyType;
        for (int index1 = 0; index1 < components.Length; ++index1)
        {
          if (!((Object) components[index1] == (Object) null))
          {
            PropertyInfo[] properties = components[index1].GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int index2 = 0; index2 < properties.Length; ++index2)
            {
              if (properties[index2].PropertyType.Equals(propertyType) && !properties[index2].IsSpecialName)
              {
                string str = components[index1].GetType().FullName + "/" + properties[index2].Name;
                if (str.Equals(sharedVariable.PropertyMapping) && (object.Equals((object) sharedVariable.PropertyMappingOwner, (object) gameObject) || object.Equals((object) sharedVariable.PropertyMappingOwner, (object) null) && behaviorGameObject))
                  num = propertyNames.Count;
                propertyNames.Add(str);
                propertyGameObjects.Add(gameObject);
              }
            }
          }
        }
      }
      return num;
    }

    private static void PropertySelected(object selected)
    {
      VariableInspector.SelectedPropertyMapping selectedPropertyMapping = selected as VariableInspector.SelectedPropertyMapping;
      if (selectedPropertyMapping.Property.Equals("None"))
      {
        VariableInspector.mPropertyMappingVariable.PropertyMapping = string.Empty;
        VariableInspector.mPropertyMappingVariable.PropertyMappingOwner = (GameObject) null;
      }
      else
      {
        VariableInspector.mPropertyMappingVariable.PropertyMapping = selectedPropertyMapping.Property;
        VariableInspector.mPropertyMappingVariable.PropertyMappingOwner = selectedPropertyMapping.GameObject;
      }
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
        BinarySerialization.Save(VariableInspector.mPropertyMappingBehaviorSource);
      else
        JSONSerialization.Save(VariableInspector.mPropertyMappingBehaviorSource);
    }

    private class SelectedPropertyMapping
    {
      private string mProperty;
      private GameObject mGameObject;

      public SelectedPropertyMapping(string property, GameObject gameObject)
      {
        this.mProperty = property;
        this.mGameObject = gameObject;
      }

      public string Property => this.mProperty;

      public GameObject GameObject => this.mGameObject;
    }
  }
}
