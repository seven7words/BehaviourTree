namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [Serializable]
    public class TaskInspector : ScriptableObject
    {
        private BehaviorDesignerWindow behaviorDesignerWindow;
        private Task activeReferenceTask;
        private FieldInfo activeReferenceTaskFieldInfo;
        private Task mActiveMenuSelectionTask;
        private Vector2 mScrollPosition = Vector2.zero;

        private void AddColorMenuItem(ref GenericMenu menu, Task task, string color, int index) => menu.AddItem(
            new GUIContent(color), task.NodeData.ColorIndex == index, new GenericMenu.MenuFunction2(this.SetTaskColor),
            new TaskInspector.TaskColor(task, index));


        private bool CanDrawReflectedField(object task, FieldInfo field)
        {
            if (!field.Name.Contains("parameter") && (!field.Name.Contains("storeResult") &&
                                                      (!field.Name.Contains("fieldValue") &&
                                                       (!field.Name.Contains("propertyValue") &&
                                                        !field.Name.Contains("compareValue")))))
            {
                return true;
            }

            if (this.IsInvokeMethodTask(task.GetType()))
            {
                if (field.Name.Contains("parameter"))
                {
                    return (task.GetType().GetField(field.Name).GetValue(task) != null);
                }

                MethodInfo info2 = null;
                return (((info2 = this.GetInvokeMethodInfo(task)) != null)
                    ? (!field.Name.Equals("storeResult") || !info2.ReturnType.Equals(typeof(void)))
                    : false);
            }

            if (this.IsFieldReflectionTask(task.GetType()))
            {
                SharedVariable variable = task.GetType().GetField("fieldName").GetValue(task) as SharedVariable;
                return ((variable != null) && !string.IsNullOrEmpty((string) variable.GetValue()));
            }

            SharedVariable variable2 = task.GetType().GetField("propertyName").GetValue(task) as SharedVariable;
            return ((variable2 != null) && !string.IsNullOrEmpty((string) variable2.GetValue()));
        }

        public void ClearFocus()
        {
            GUIUtility.keyboardControl = 0;
        }

        private void ClearInvokeVariablesTask()
        {
            for (int i = 0; i < 4; i++)
            {
                this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (i + 1))
                    .SetValue(this.mActiveMenuSelectionTask, null);
            }

            this.mActiveMenuSelectionTask.GetType().GetField("storeResult")
                .SetValue(this.mActiveMenuSelectionTask, null);
        }

        private void ComponentSelectionCallback(object obj)
        {
            if (this.mActiveMenuSelectionTask != null)
            {
                FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("componentName");
                SharedVariable variable =
                    Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString"))
                        as SharedVariable;
                if (obj == null)
                {
                    field.SetValue(this.mActiveMenuSelectionTask, variable);
                    variable = Activator.CreateInstance(
                        TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
                    FieldInfo info2 = null;
                    if (!this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                    {
                        info2 = !this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType())
                            ? this.mActiveMenuSelectionTask.GetType().GetField("propertyName")
                            : this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
                    }
                    else
                    {
                        info2 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
                        this.ClearInvokeVariablesTask();
                    }

                    info2.SetValue(this.mActiveMenuSelectionTask, variable);
                }
                else
                {
                    string str = (string) obj;
                    if (!str.Equals(
                        (string) (field.GetValue(this.mActiveMenuSelectionTask) as SharedVariable).GetValue()))
                    {
                        FieldInfo info3 = null;
                        FieldInfo info4 = null;
                        if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                        {
                            info3 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
                            int num = 0;
                            while (true)
                            {
                                if (num >= 4)
                                {
                                    info4 = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
                                    break;
                                }

                                this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (num + 1))
                                    .SetValue(this.mActiveMenuSelectionTask, null);
                                num++;
                            }
                        }
                        else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
                        {
                            info3 = this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
                            info4 = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
                            if (info4 == null)
                            {
                                info4 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                            }
                        }
                        else
                        {
                            info3 = this.mActiveMenuSelectionTask.GetType().GetField("propertyName");
                            info4 = this.mActiveMenuSelectionTask.GetType().GetField("propertyValue");
                            if (info4 == null)
                            {
                                info4 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                            }
                        }

                        info3.SetValue(this.mActiveMenuSelectionTask, variable);
                        info4.SetValue(this.mActiveMenuSelectionTask, null);
                    }

                    variable = Activator.CreateInstance(
                        TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
                    variable.SetValue(str);
                    field.SetValue(this.mActiveMenuSelectionTask, variable);
                }
            }

            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        private void DrawObjectFields(
            BehaviorSource behaviorSource,
            TaskList taskList,
            Task task,
            object obj,
            bool enabled,
            bool drawWatch)
        {
            if (obj == null)
                return;
            List<System.Type> baseClasses = FieldInspector.GetBaseClasses(obj.GetType());
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                       BindingFlags.NonPublic;
            bool isReflectionTask = this.IsReflectionTask(obj.GetType());
            for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
            {
                FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
                for (int index2 = 0; index2 < fields.Length; ++index2)
                {
                    if (!BehaviorDesignerUtility.HasAttribute(fields[index2], typeof(NonSerializedAttribute)) &&
                        !BehaviorDesignerUtility.HasAttribute(fields[index2], typeof(HideInInspector)) &&
                        (!fields[index2].IsPrivate && !fields[index2].IsFamily ||
                         BehaviorDesignerUtility.HasAttribute(fields[index2], typeof(SerializeField))) &&
                        (!(obj is ParentTask) || !fields[index2].Name.Equals("children")) && (!isReflectionTask ||
                            !fields[index2].FieldType.Equals(typeof(SharedVariable)) &&
                            !fields[index2].FieldType.IsSubclassOf(typeof(SharedVariable)) ||
                            this.CanDrawReflectedField(obj, fields[index2])))
                    {
                        HeaderAttribute[] customAttributes1;
                        if ((customAttributes1 =
                                fields[index2].GetCustomAttributes(typeof(HeaderAttribute), true) as HeaderAttribute[])
                            .Length > 0)
                            EditorGUILayout.LabelField(customAttributes1[0].header,
                                BehaviorDesignerUtility.BoldLabelGUIStyle,
                                (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
                        SpaceAttribute[] customAttributes2;
                        if ((customAttributes2 =
                                fields[index2].GetCustomAttributes(typeof(SpaceAttribute), true) as SpaceAttribute[])
                            .Length > 0)
                            GUILayout.Space(customAttributes2[0].height);
                        string s = fields[index2].Name;
                        if (isReflectionTask && (fields[index2].FieldType.Equals(typeof(SharedVariable)) ||
                                                 fields[index2].FieldType.IsSubclassOf(typeof(SharedVariable))))
                            s = this.InvokeParameterName(obj, fields[index2]);
                        BehaviorDesigner.Runtime.Tasks.TooltipAttribute[] customAttributes3;
                        GUIContent guiContent =
                            (customAttributes3 =
                                fields[index2]
                                        .GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TooltipAttribute),
                                            false)
                                    as BehaviorDesigner.Runtime.Tasks.TooltipAttribute[]).Length <= 0
                                ? new GUIContent(BehaviorDesignerUtility.SplitCamelCase(s))
                                : new GUIContent(BehaviorDesignerUtility.SplitCamelCase(s),
                                    customAttributes3[0].Tooltip);
                        object obj1 = fields[index2].GetValue(obj);
                        System.Type fieldType = fields[index2].FieldType;
                        if (typeof(Task).IsAssignableFrom(fieldType) || typeof(IList).IsAssignableFrom(fieldType) &&
                            (typeof(Task).IsAssignableFrom(fieldType.GetElementType()) || fieldType.IsGenericType &&
                                typeof(Task).IsAssignableFrom(fieldType.GetGenericArguments()[0])))
                        {
                            EditorGUI.BeginChangeCheck();
                            this.DrawTaskValue(behaviorSource, taskList, fields[index2], guiContent, task, obj1 as Task,
                                enabled);
                            if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index2].Name))
                            {
                                GUILayout.Space(-3f);
                                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture,
                                    BehaviorDesignerUtility.PlainTextureGUIStyle, GUILayout.Width(20f));
                            }

                            if (EditorGUI.EndChangeCheck())
                                GUI.changed = true;
                        }
                        else if (fieldType.Equals(typeof(SharedVariable)) ||
                                 fieldType.IsSubclassOf(typeof(SharedVariable)))
                        {
                            GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
                            EditorGUI.BeginChangeCheck();
                            if (drawWatch)
                                this.DrawWatchedButton(task, fields[index2]);
                            SharedVariable sharedVariable = this.DrawSharedVariableValue(behaviorSource, fields[index2],
                                guiContent, task, obj1 as SharedVariable, isReflectionTask, enabled, drawWatch);
                            if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index2].Name))
                            {
                                GUILayout.Space(-3f);
                                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture,
                                    BehaviorDesignerUtility.PlainTextureGUIStyle, GUILayout.Width(20f));
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.Space(4f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                fields[index2].SetValue(obj, sharedVariable);
                                GUI.changed = true;
                            }
                        }
                        else
                        {
                            GUILayout.BeginHorizontal((GUILayoutOption[]) Array.Empty<GUILayoutOption>());
                            EditorGUI.BeginChangeCheck();
                            if (drawWatch)
                                this.DrawWatchedButton(task, fields[index2]);
                            object obj2 = FieldInspector.DrawField(task, guiContent, fields[index2], obj1);
                            if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index2].Name))
                            {
                                GUILayout.Space(-3f);
                                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture,
                                    BehaviorDesignerUtility.PlainTextureGUIStyle, GUILayout.Width(20f));
                            }

                            if (EditorGUI.EndChangeCheck())
                            {
                                fields[index2].SetValue(obj, obj2);
                                GUI.changed = true;
                            }

                            if (TaskUtility.HasAttribute(fields[index2], typeof(RequiredFieldAttribute)) &&
                                !ErrorCheck.IsRequiredFieldValid(fieldType, obj1))
                            {
                                GUILayout.Space(-3f);
                                GUILayout.Box((Texture) BehaviorDesignerUtility.ErrorIconTexture,
                                    BehaviorDesignerUtility.PlainTextureGUIStyle, GUILayout.Width(20f));
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.Space(4f);
                        }
                    }
                }
            }
        }

        private void DrawReflectionField(
            Task task,
            GUIContent guiContent,
            bool drawComponentField,
            FieldInfo field)
        {
            SharedVariable sharedVariable1 =
                task.GetType().GetField("targetGameObject").GetValue(task) as SharedVariable;
            if (drawComponentField)
            {
                GUILayout.Label(guiContent, GUILayout.Width(146f));
                SharedVariable sharedVariable2 = field.GetValue(task) as SharedVariable;
                string empty = string.Empty;
                string text;
                if (sharedVariable2 == null || string.IsNullOrEmpty((string) sharedVariable2.GetValue()))
                {
                    text = "Select";
                }
                else
                {
                    string[] strArray = ((string) sharedVariable2.GetValue()).Split('.');
                    text = strArray[strArray.Length - 1];
                }

                if (GUILayout.Button(text, EditorStyles.toolbarPopup, GUILayout.Width(92f)))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("None"),
                        string.IsNullOrEmpty((string) sharedVariable2.GetValue()),
                        new GenericMenu.MenuFunction2(this.ComponentSelectionCallback), null);
                    GameObject gameObject = (GameObject) null;
                    if (sharedVariable1 == null || sharedVariable1.GetValue() == null)
                    {
                        if (task.Owner != null)
                            gameObject = task.Owner.gameObject;
                    }
                    else
                        gameObject = (GameObject) sharedVariable1.GetValue();

                    if (gameObject != null)
                    {
                        Component[] components = gameObject.GetComponents<Component>();
                        for (int index = 0; index < components.Length; ++index)
                            genericMenu.AddItem(new GUIContent(components[index].GetType().Name),
                                components[index].GetType().FullName.Equals((string) sharedVariable2.GetValue()),
                                new GenericMenu.MenuFunction2(this.ComponentSelectionCallback),
                                components[index].GetType().FullName);
                        genericMenu.ShowAsContext();
                        this.mActiveMenuSelectionTask = task;
                    }
                }
            }
            else
            {
                GUILayout.Label(guiContent, GUILayout.Width(146f));
                SharedVariable sharedVariable2 =
                    task.GetType().GetField("componentName").GetValue(task) as SharedVariable;
                SharedVariable sharedVariable3 = field.GetValue(task) as SharedVariable;
                string empty = string.Empty;
                if (GUILayout.Button(
                        sharedVariable2 == null || string.IsNullOrEmpty((string) sharedVariable2.GetValue())
                            ? "Component Required"
                            : (!string.IsNullOrEmpty((string) sharedVariable3.GetValue())
                                ? (string) sharedVariable3.GetValue()
                                : "Select"), EditorStyles.toolbarPopup, GUILayout.Width(92f)) &&
                    !string.IsNullOrEmpty((string) sharedVariable2.GetValue()))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("None"),
                        string.IsNullOrEmpty((string) sharedVariable3.GetValue()),
                        new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), null);
                    GameObject gameObject = (GameObject) null;
                    if (sharedVariable1 == null || sharedVariable1.GetValue() == null)
                    {
                        if (task.Owner != null)
                            gameObject = task.Owner.gameObject;
                    }
                    else
                        gameObject = (GameObject) sharedVariable1.GetValue();

                    if (gameObject != null)
                    {
                        Component component =
                            gameObject.GetComponent(
                                TaskUtility.GetTypeWithinAssembly((string) sharedVariable2.GetValue()));
                        List<System.Type> sharedVariableTypes = VariableInspector.FindAllSharedVariableTypes(false);
                        if (this.IsInvokeMethodTask(task.GetType()))
                        {
                            MethodInfo[] methods = component.GetType()
                                .GetMethods(BindingFlags.Instance | BindingFlags.Public);
                            for (int index1 = 0; index1 < methods.Length; ++index1)
                            {
                                if (!methods[index1].IsSpecialName && !methods[index1].IsGenericMethod &&
                                    methods[index1].GetParameters().Length <= 4)
                                {
                                    ParameterInfo[] parameters = methods[index1].GetParameters();
                                    bool flag = true;
                                    for (int index2 = 0; index2 < parameters.Length; ++index2)
                                    {
                                        if (!this.SharedVariableTypeExists(sharedVariableTypes,
                                            parameters[index2].ParameterType))
                                        {
                                            flag = false;
                                            break;
                                        }
                                    }

                                    if (flag && (methods[index1].ReturnType.Equals(typeof(void)) ||
                                                 this.SharedVariableTypeExists(sharedVariableTypes,
                                                     methods[index1].ReturnType)))
                                        genericMenu.AddItem(new GUIContent(methods[index1].Name),
                                            methods[index1].Name.Equals((string) sharedVariable3.GetValue()),
                                            new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback),
                                            methods[index1]);
                                }
                            }
                        }
                        else if (this.IsFieldReflectionTask(task.GetType()))
                        {
                            FieldInfo[] fields = component.GetType()
                                .GetFields(BindingFlags.Instance | BindingFlags.Public);
                            for (int index = 0; index < fields.Length; ++index)
                            {
                                if (!fields[index].IsSpecialName &&
                                    this.SharedVariableTypeExists(sharedVariableTypes, fields[index].FieldType))
                                    genericMenu.AddItem(new GUIContent(fields[index].Name),
                                        fields[index].Name.Equals((string) sharedVariable3.GetValue()),
                                        new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback),
                                        fields[index]);
                            }
                        }
                        else
                        {
                            PropertyInfo[] properties = component.GetType()
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public);
                            for (int index = 0; index < properties.Length; ++index)
                            {
                                if (!properties[index].IsSpecialName &&
                                    this.SharedVariableTypeExists(sharedVariableTypes, properties[index].PropertyType))
                                    genericMenu.AddItem(new GUIContent(properties[index].Name),
                                        properties[index].Name.Equals((string) sharedVariable3.GetValue()),
                                        new GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback),
                                        properties[index]);
                            }
                        }

                        genericMenu.ShowAsContext();
                        this.mActiveMenuSelectionTask = task;
                    }
                }
            }

            GUILayout.Space(8f);
        }

        private SharedVariable DrawSharedVariableValue(
            BehaviorSource behaviorSource,
            FieldInfo field,
            GUIContent guiContent,
            Task task,
            SharedVariable sharedVariable,
            bool isReflectionTask,
            bool enabled,
            bool drawWatch)
        {
            if (isReflectionTask)
            {
                if (!field.FieldType.Equals(typeof(SharedVariable)) && sharedVariable == null)
                {
                    sharedVariable = Activator.CreateInstance(field.FieldType) as SharedVariable;
                    if (TaskUtility.HasAttribute(field, typeof(RequiredFieldAttribute)) ||
                        TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
                        sharedVariable.IsShared = true;
                    GUI.changed = true;
                }

                if (sharedVariable == null)
                {
                    this.mActiveMenuSelectionTask = task;
                    this.SecondaryReflectionSelectionCallback(null);
                    this.ClearInvokeVariablesTask();
                    return (SharedVariable) null;
                }

                if (sharedVariable.IsShared)
                {
                    GUILayout.Label(guiContent, GUILayout.Width(126f));
                    string[] names = (string[]) null;
                    int globalStartIndex = -1;
                    int variablesOfType = FieldInspector.GetVariablesOfType(
                        sharedVariable.GetType().GetProperty("Value").PropertyType, sharedVariable.IsGlobal,
                        sharedVariable.Name, behaviorSource, out names, ref globalStartIndex, false, true);
                    Color backgroundColor = GUI.backgroundColor;
                    if (variablesOfType == 0 && !TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
                        GUI.backgroundColor = Color.red;
                    int num = variablesOfType;
                    int index = EditorGUILayout.Popup(variablesOfType, names, EditorStyles.toolbarPopup,
                        (GUILayoutOption[]) Array.Empty<GUILayoutOption>());
                    GUI.backgroundColor = backgroundColor;
                    if (index != num)
                    {
                        if (index == 0)
                        {
                            sharedVariable = !field.FieldType.Equals(typeof(SharedVariable))
                                ? Activator.CreateInstance(field.FieldType) as SharedVariable
                                : Activator.CreateInstance(
                                    FieldInspector.FriendlySharedVariableName(sharedVariable.GetType()
                                        .GetProperty("Value").PropertyType)) as SharedVariable;
                            sharedVariable.IsShared = true;
                        }
                        else
                            sharedVariable = globalStartIndex == -1 || index < globalStartIndex
                                ? behaviorSource.GetVariable(names[index])
                                : GlobalVariables.Instance.GetVariable(names[index]
                                    .Substring(8, names[index].Length - 8));
                    }

                    GUILayout.Space(8f);
                }
                else
                {
                    bool drawComponentField;
                    if ((drawComponentField = field.Name.Equals("componentName")) || field.Name.Equals("methodName") ||
                        (field.Name.Equals("fieldName") || field.Name.Equals("propertyName")))
                        this.DrawReflectionField(task, guiContent, drawComponentField, field);
                    else
                        FieldInspector.DrawFields(task, sharedVariable, guiContent);
                }

                if (!TaskUtility.HasAttribute(field, typeof(RequiredFieldAttribute)) &&
                    !TaskUtility.HasAttribute(field, typeof(SharedRequiredAttribute)))
                    sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
                else if (!sharedVariable.IsShared)
                    sharedVariable.IsShared = true;
            }
            else
                sharedVariable =
                    FieldInspector.DrawSharedVariable(task, guiContent, field, field.FieldType, sharedVariable);

            GUILayout.Space(8f);
            return sharedVariable;
        }

        private bool DrawTaskFields(BehaviorSource behaviorSource, TaskList taskList, Task task, bool enabled)
        {
            if (task == null)
            {
                return false;
            }

            EditorGUI.BeginChangeCheck();
            FieldInspector.behaviorSource = behaviorSource;
            this.DrawObjectFields(behaviorSource, taskList, task, task, enabled, true);
            return EditorGUI.EndChangeCheck();
        }

        public bool DrawTaskInspector(BehaviorSource behaviorSource, TaskList taskList, Task task, bool enabled)
        {
            if ((task == null) || (task.NodeData.NodeDesigner as NodeDesigner).IsEntryDisplay)
            {
                return false;
            }

            this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, Array.Empty<GUILayoutOption>());
            GUI.enabled = enabled;
            if (this.behaviorDesignerWindow == null)
            {
                this.behaviorDesignerWindow = BehaviorDesignerWindow.instance;
            }

            EditorGUIUtility.labelWidth = 150f;
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] {GUILayout.Width(90f)};
            EditorGUILayout.LabelField("Name", optionArray1);
            task.FriendlyName = (EditorGUILayout.TextField(task.FriendlyName, Array.Empty<GUILayoutOption>()));
            if (GUILayout.Button(BehaviorDesignerUtility.DocTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle,
                Array.Empty<GUILayoutOption>()))
            {
                this.OpenHelpURL(task);
            }

            if (GUILayout.Button(BehaviorDesignerUtility.ColorSelectorTexture(task.NodeData.ColorIndex),
                BehaviorDesignerUtility.TransparentButtonOffsetGUIStyle, Array.Empty<GUILayoutOption>()))
            {
                GenericMenu menu = new GenericMenu();
                this.AddColorMenuItem(ref menu, task, "Default", 0);
                this.AddColorMenuItem(ref menu, task, "Red", 1);
                this.AddColorMenuItem(ref menu, task, "Pink", 2);
                this.AddColorMenuItem(ref menu, task, "Brown", 3);
                this.AddColorMenuItem(ref menu, task, "Orange", 4);
                this.AddColorMenuItem(ref menu, task, "Turquoise", 5);
                this.AddColorMenuItem(ref menu, task, "Cyan", 6);
                this.AddColorMenuItem(ref menu, task, "Blue", 7);
                this.AddColorMenuItem(ref menu, task, "Purple", 8);
                menu.ShowAsContext();
            }

            if (GUILayout.Button(BehaviorDesignerUtility.GearTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle,
                Array.Empty<GUILayoutOption>()))
            {
                GenericMenu menu2 = new GenericMenu();
                menu2.AddItem(new GUIContent("Edit Script"), false, OpenInFileEditor, task);
                menu2.AddItem(new GUIContent("Locate Script"), false, SelectInProject, task);
                menu2.AddItem(new GUIContent("Reset"), false, this.ResetTask, task);
                menu2.ShowAsContext();
            }

            GUILayout.EndHorizontal();
            string str = BehaviorDesignerUtility.SplitCamelCase(task.GetType().Name.ToString());
            if (!task.FriendlyName.Equals(str))
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayoutOption[] optionArray2 = new GUILayoutOption[] {GUILayout.Width(90f)};
                EditorGUILayout.LabelField("Type", optionArray2);
                GUILayoutOption[] optionArray3 = new GUILayoutOption[] {GUILayout.MaxWidth(170f)};
                EditorGUILayout.LabelField(str, optionArray3);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray4 = new GUILayoutOption[] {GUILayout.Width(90f)};
            EditorGUILayout.LabelField("Instant", optionArray4);
            task.IsInstant = (EditorGUILayout.Toggle(task.IsInstant, Array.Empty<GUILayoutOption>()));
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Comment", Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray5 = new GUILayoutOption[] {GUILayout.Height(48f)};
            task.NodeData.Comment = (EditorGUILayout.TextArea(task.NodeData.Comment,
                BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, optionArray5));
            if (EditorGUI.EndChangeCheck())
            {
                BehaviorUndo.RegisterUndo("Inspector", behaviorSource.Owner.GetObject());
                GUI.changed = (true);
            }

            BehaviorDesignerUtility.DrawContentSeperator(2);
            GUILayout.Space(6f);
            if (this.DrawTaskFields(behaviorSource, taskList, task, enabled))
            {
                BehaviorUndo.RegisterUndo("Inspector", behaviorSource.Owner.GetObject());
                GUI.changed = (true);
            }

            GUI.enabled = true;
            GUILayout.EndScrollView();
            return GUI.changed;
        }

        private void DrawTaskValue(BehaviorSource behaviorSource, TaskList taskList, FieldInfo field,
            GUIContent guiContent, Task parentTask, Task task, bool enabled)
        {
            if (BehaviorDesignerUtility.HasAttribute(field, typeof(InspectTaskAttribute)))
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayoutOption[] optionArray1 = new GUILayoutOption[] {GUILayout.Width(144f)};
                GUILayout.Label(guiContent, optionArray1);
                GUILayoutOption[] optionArray2 = new GUILayoutOption[] {GUILayout.Width(134f)};
                if (GUILayout.Button(
                    (task == null) ? "Select" : BehaviorDesignerUtility.SplitCamelCase(task.GetType().Name.ToString()),
                    EditorStyles.toolbarPopup, optionArray2))
                {
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("None"), task == null, this.InspectedTaskCallback, null);
                    taskList.AddConditionalTasksToMenu(ref genericMenu, task?.GetType(), string.Empty,
                        this.InspectedTaskCallback);
                    genericMenu.ShowAsContext();
                    this.mActiveMenuSelectionTask = parentTask;
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
                this.DrawObjectFields(behaviorSource, taskList, task, task, enabled, false);
            }
            else
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                this.DrawWatchedButton(parentTask, field);
                GUILayoutOption[] optionArray3 = new GUILayoutOption[] {GUILayout.Width(165f)};
                GUILayout.Label(guiContent, BehaviorDesignerUtility.TaskInspectorGUIStyle, optionArray3);
                bool flag = this.behaviorDesignerWindow.IsReferencingField(field);
                Color color = GUI.backgroundColor;
                if (flag)
                {
                    GUI.backgroundColor = (new Color(0.5f, 1f, 0.5f));
                }

                GUILayoutOption[] optionArray4 = new GUILayoutOption[] {GUILayout.Width(80f)};
                if (GUILayout.Button(!flag ? "Select" : "Done", EditorStyles.miniButtonMid, optionArray4))
                {
                    if (this.behaviorDesignerWindow.IsReferencingTasks() && !flag)
                    {
                        this.behaviorDesignerWindow.ToggleReferenceTasks();
                    }

                    this.behaviorDesignerWindow.ToggleReferenceTasks(parentTask, field);
                }

                GUI.backgroundColor = (color);
                EditorGUILayout.EndHorizontal();
                if (!typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    Task task2 = field.GetValue(parentTask) as Task;
                    GUILayoutOption[] optionArray8 = new GUILayoutOption[] {GUILayout.Width(232f)};
                    GUILayout.Label((task2 == null) ? "No Tasks Referenced" : task2.NodeData.NodeDesigner.ToString(),
                        BehaviorDesignerUtility.TaskInspectorGUIStyle, optionArray8);
                    if (task2 != null)
                    {
                        GUILayoutOption[] optionArray9 = new GUILayoutOption[] {GUILayout.Width(14f)};
                        if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture,
                            BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray9))
                        {
                            this.ReferenceTasks(parentTask, null, field);
                            GUI.changed = true;
                        }

                        GUILayout.Space(3f);
                        GUILayoutOption[] optionArray10 = new GUILayoutOption[] {GUILayout.Width(14f)};
                        if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture,
                            BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray10))
                        {
                            this.behaviorDesignerWindow.IdentifyNode(task2.NodeData.NodeDesigner as NodeDesigner);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    IList list = field.GetValue(parentTask) as IList;
                    if ((list == null) || (list.Count == 0))
                    {
                        GUILayout.Label("No Tasks Referenced", BehaviorDesignerUtility.TaskInspectorGUIStyle,
                            Array.Empty<GUILayoutOption>());
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] is Task)
                            {
                                EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                                GUILayoutOption[] optionArray5 = new GUILayoutOption[] {GUILayout.Width(232f)};
                                GUILayout.Label((list[i] as Task).NodeData.NodeDesigner.ToString(),
                                    BehaviorDesignerUtility.TaskInspectorGUIStyle, optionArray5);
                                GUILayoutOption[] optionArray6 = new GUILayoutOption[] {GUILayout.Width(14f)};
                                if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture,
                                    BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray6))
                                {
                                    this.ReferenceTasks(parentTask,
                                        ((list[i] as Task).NodeData.NodeDesigner as NodeDesigner).Task, field);
                                    GUI.changed = true;
                                }

                                GUILayout.Space(3f);
                                GUILayoutOption[] optionArray7 = new GUILayoutOption[] {GUILayout.Width(14f)};
                                if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture,
                                    BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray7))
                                {
                                    this.behaviorDesignerWindow.IdentifyNode(
                                        (list[i] as Task).NodeData.NodeDesigner as NodeDesigner);
                                }

                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
            }
        }

        private bool DrawWatchedButton(Task task, FieldInfo field)
        {
            GUILayout.Space(3f);
            bool flag = task.NodeData.GetWatchedFieldIndex(field) != -1;
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] {GUILayout.Width(15f)};
            if (!GUILayout.Button(
                !flag
                    ? BehaviorDesignerUtility.VariableWatchButtonTexture
                    : BehaviorDesignerUtility.VariableWatchButtonSelectedTexture,
                BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray1))
            {
                return false;
            }

            if (flag)
            {
                task.NodeData.RemoveWatchedField(field);
            }
            else
            {
                task.NodeData.AddWatchedField(field);
            }

            return true;
        }

        private MethodInfo GetInvokeMethodInfo(object task)
        {
            SharedVariable variable = task.GetType().GetField("targetGameObject").GetValue(task) as SharedVariable;
            GameObject obj2 = null;
            if ((variable != null) && (((GameObject) variable.GetValue()) != null))
            {
                obj2 = (GameObject) variable.GetValue();
            }
            else if ((task as Task).Owner != null)
            {
                obj2 = (task as Task).Owner.gameObject;
            }

            if (obj2 == null)
            {
                return null;
            }

            SharedVariable variable2 = task.GetType().GetField("componentName").GetValue(task) as SharedVariable;
            if ((variable2 == null) || string.IsNullOrEmpty((string) variable2.GetValue()))
            {
                return null;
            }

            SharedVariable variable3 = task.GetType().GetField("methodName").GetValue(task) as SharedVariable;
            if ((variable3 == null) || string.IsNullOrEmpty((string) variable3.GetValue()))
            {
                return null;
            }

            List<Type> list = new List<Type>();
            SharedVariable variable4 = null;
            int num = 0;
            while (true)
            {
                if (num < 4)
                {
                    FieldInfo field = task.GetType().GetField("parameter" + (num + 1));
                    variable4 = field.GetValue(task) as SharedVariable;
                    if (variable4 != null)
                    {
                        list.Add(variable4.GetType().GetProperty("Value").PropertyType);
                        num++;
                        continue;
                    }
                }

                Component component =
                    obj2.GetComponent(TaskUtility.GetTypeWithinAssembly((string) variable2.GetValue()));
                return component?.GetType().GetMethod((string) variable3.GetValue(), list.ToArray());
            }
        }

        public static List<Task> GetReferencedTasks(Task task)
        {
            List<Task> list = new List<Task>();
            FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(task.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if ((!serializableFields[i].IsPrivate && !serializableFields[i].IsFamily) ||
                    BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(SerializeField)))
                {
                    if (!typeof(IList).IsAssignableFrom(serializableFields[i].FieldType) ||
                        (!typeof(Task).IsAssignableFrom(serializableFields[i].FieldType.GetElementType()) &&
                         (!serializableFields[i].FieldType.IsGenericType ||
                          !typeof(Task).IsAssignableFrom(serializableFields[i].FieldType.GetGenericArguments()[0]))))
                    {
                        if (serializableFields[i].FieldType.IsSubclassOf(typeof(Task)) &&
                            (serializableFields[i].GetValue(task) != null))
                        {
                            list.Add(serializableFields[i].GetValue(task) as Task);
                        }
                    }
                    else
                    {
                        Task[] taskArray = serializableFields[i].GetValue(task) as Task[];
                        if (taskArray != null)
                        {
                            for (int j = 0; j < taskArray.Length; j++)
                            {
                                list.Add(taskArray[j]);
                            }
                        }
                    }
                }
            }

            return ((list.Count <= 0) ? null : list);
        }

        public bool HasFocus() =>
            (GUIUtility.keyboardControl != 0);

        private void InspectedTaskCallback(object obj)
        {
            if (this.mActiveMenuSelectionTask != null)
            {
                FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("conditionalTask");
                if (obj == null)
                {
                    field.SetValue(this.mActiveMenuSelectionTask, null);
                }
                else
                {
                    Type type = (Type) obj;
                    Task task = Activator.CreateInstance(type, true) as Task;
                    field.SetValue(this.mActiveMenuSelectionTask, task);
                    FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(type);
                    for (int i = 0; i < serializableFields.Length; i++)
                    {
                        if ((serializableFields[i].FieldType.IsSubclassOf(typeof(SharedVariable)) &&
                             (!BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(HideInInspector)) &&
                              !BehaviorDesignerUtility.HasAttribute(serializableFields[i],
                                  typeof(NonSerializedAttribute)))) &&
                            ((!serializableFields[i].IsPrivate && !serializableFields[i].IsFamily) ||
                             BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(SerializeField))))
                        {
                            SharedVariable variable =
                                Activator.CreateInstance(serializableFields[i].FieldType) as SharedVariable;
                            variable.IsShared = (false);
                            serializableFields[i].SetValue(task, variable);
                        }
                    }
                }
            }

            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        private string InvokeParameterName(object task, FieldInfo field)
        {
            if (!field.Name.Contains("parameter"))
            {
                return field.Name;
            }

            MethodInfo invokeMethodInfo = null;
            invokeMethodInfo = this.GetInvokeMethodInfo(task);
            if (invokeMethodInfo == null)
            {
                return field.Name;
            }

            ParameterInfo[] parameters = invokeMethodInfo.GetParameters();
            int index = int.Parse(field.Name.Substring(9)) - 1;
            return ((index >= parameters.Length) ? field.Name : parameters[index].Name);
        }

        public bool IsActiveTaskArray() =>
            this.activeReferenceTaskFieldInfo.FieldType.IsArray;

        public bool IsActiveTaskNull() =>
            (this.activeReferenceTaskFieldInfo.GetValue(this.activeReferenceTask) == null);

        public static bool IsFieldLinked(FieldInfo field) =>
            BehaviorDesignerUtility.HasAttribute(field, typeof(LinkedTaskAttribute));

        private bool IsFieldReflectionTask(Type type) =>
            ((TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") ||
              TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetFieldValue")) ||
             TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.CompareFieldValue"));

        private bool IsInvokeMethodTask(Type type) =>
            TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.InvokeMethod");

        private bool IsPropertyReflectionTask(Type type) =>
            ((TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue") ||
              TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetPropertyValue")) ||
             TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.ComparePropertyValue"));

        private bool IsReflectionGetterTask(Type type) =>
            (TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") ||
             TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue"));

        private bool IsReflectionTask(Type type) =>
            ((this.IsInvokeMethodTask(type) || this.IsFieldReflectionTask(type)) ||
             this.IsPropertyReflectionTask(type));

        public void OnEnable() => this.hideFlags = HideFlags.HideAndDontSave;

        private void OpenHelpURL(Task task)
        {
            BehaviorDesigner.Runtime.Tasks.HelpURLAttribute[] customAttributes;
            if ((customAttributes =
                task.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.HelpURLAttribute), false) as
                    BehaviorDesigner.Runtime.Tasks.HelpURLAttribute[]).Length <= 0)
                return;
            Application.OpenURL(customAttributes[0].URL);
        }

        public static void OpenInFileEditor(object task)
        {
            MonoScript[] scriptArray = (MonoScript[]) Resources.FindObjectsOfTypeAll(typeof(MonoScript));
            int index = 0;
            while (true)
            {
                if (index < scriptArray.Length)
                {
                    if ((scriptArray[index] == null) || ((scriptArray[index].GetClass() == null) ||
                                                         !scriptArray[index].GetClass().Equals(task.GetType())))
                    {
                        index++;
                        continue;
                    }

                    AssetDatabase.OpenAsset(scriptArray[index]);
                }

                return;
            }
        }

        private void PerformFullSync(Task task)
        {
            List<Task> referencedTasks = GetReferencedTasks(task);
            if (referencedTasks != null)
            {
                FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(task.GetType());
                for (int i = 0; i < serializableFields.Length; i++)
                {
                    if (!IsFieldLinked(serializableFields[i]))
                    {
                        for (int j = 0; j < referencedTasks.Count; j++)
                        {
                            FieldInfo field = referencedTasks[j].GetType().GetField(serializableFields[i].Name);
                            if (field != null)
                            {
                                field.SetValue(referencedTasks[j], serializableFields[i].GetValue(task));
                            }
                        }
                    }
                }
            }
        }

        public bool ReferenceTasks(Task referenceTask) =>
            this.ReferenceTasks(this.activeReferenceTask, referenceTask, this.activeReferenceTaskFieldInfo);

        private bool ReferenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo)
        {
            bool fullSync = false;
            bool doReference = false;
            if (!ReferenceTasks(sourceTask, referenceTask, sourceFieldInfo, ref fullSync, ref doReference, true, false))
            {
                return false;
            }

            if (referenceTask != null)
            {
                (referenceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = doReference;
                if (fullSync)
                {
                    this.PerformFullSync(this.activeReferenceTask);
                }
            }

            return true;
        }

        public static bool ReferenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo,
            ref bool fullSync, ref bool doReference, bool synchronize, bool unreferenceAll)
        {
            if (referenceTask == null)
            {
                Task task = sourceFieldInfo.GetValue(sourceTask) as Task;
                if (task != null)
                {
                    (task.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = false;
                }

                sourceFieldInfo.SetValue(sourceTask, null);
                return true;
            }

            if ((referenceTask.Equals(sourceTask) || ((sourceFieldInfo == null) ||
                                                      (!typeof(IList).IsAssignableFrom(sourceFieldInfo.FieldType) &&
                                                       !sourceFieldInfo.FieldType.IsAssignableFrom(
                                                           referenceTask.GetType())))) ||
                (typeof(IList).IsAssignableFrom(sourceFieldInfo.FieldType) &&
                 ((sourceFieldInfo.FieldType.IsGenericType && !sourceFieldInfo.FieldType.GetGenericArguments()[0]
                     .IsAssignableFrom(referenceTask.GetType())) || (!sourceFieldInfo.FieldType.IsGenericType &&
                                                                     !sourceFieldInfo.FieldType.GetElementType()
                                                                         .IsAssignableFrom(referenceTask.GetType())))))
            {
                return false;
            }

            if (synchronize && !IsFieldLinked(sourceFieldInfo))
            {
                synchronize = false;
            }

            if (unreferenceAll)
            {
                sourceFieldInfo.SetValue(sourceTask, null);
                (sourceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = false;
            }
            else
            {
                doReference = true;
                bool flag = false;
                if (!typeof(IList).IsAssignableFrom(sourceFieldInfo.FieldType))
                {
                    Task task2 = sourceFieldInfo.GetValue(sourceTask) as Task;
                    doReference = !referenceTask.Equals(task2);
                    if (IsFieldLinked(sourceFieldInfo) && (task2 != null))
                    {
                        ReferenceTasks(task2, sourceTask, task2.GetType().GetField(sourceFieldInfo.Name), ref flag,
                            ref doReference, false, true);
                    }

                    if (synchronize)
                    {
                        ReferenceTasks(referenceTask, sourceTask,
                            referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false,
                            !doReference);
                    }

                    sourceFieldInfo.SetValue(sourceTask, !doReference ? null : referenceTask);
                }
                else
                {
                    Type elementType;
                    Task[] taskArray = sourceFieldInfo.GetValue(sourceTask) as Task[];
                    if (sourceFieldInfo.FieldType.IsArray)
                    {
                        elementType = sourceFieldInfo.FieldType.GetElementType();
                    }
                    else
                    {
                        Type fieldType = sourceFieldInfo.FieldType;
                        while (true)
                        {
                            if (fieldType.IsGenericType)
                            {
                                elementType = fieldType.GetGenericArguments()[0];
                                break;
                            }

                            fieldType = fieldType.BaseType;
                        }
                    }

                    Type[] typeArguments = new Type[] {elementType};
                    IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments)) as IList;
                    if (taskArray != null)
                    {
                        for (int i = 0; i < taskArray.Length; i++)
                        {
                            if (referenceTask.Equals(taskArray[i]))
                            {
                                doReference = false;
                            }
                            else
                            {
                                list.Add(taskArray[i]);
                            }
                        }
                    }

                    if (synchronize)
                    {
                        if ((taskArray != null) && (taskArray.Length > 0))
                        {
                            for (int i = 0; i < taskArray.Length; i++)
                            {
                                ReferenceTasks(taskArray[i], referenceTask,
                                    taskArray[i].GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference,
                                    false, false);
                                if (doReference)
                                {
                                    ReferenceTasks(referenceTask, taskArray[i],
                                        referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag,
                                        ref doReference, false, false);
                                }
                            }
                        }
                        else if (doReference)
                        {
                            FieldInfo field = referenceTask.GetType().GetField(sourceFieldInfo.Name);
                            if (field != null)
                            {
                                taskArray = field.GetValue(referenceTask) as Task[];
                                if (taskArray != null)
                                {
                                    int index = 0;
                                    while (true)
                                    {
                                        if (index >= taskArray.Length)
                                        {
                                            doReference = true;
                                            break;
                                        }

                                        list.Add(taskArray[index]);
                                        (taskArray[index].NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon =
                                            true;
                                        ReferenceTasks(taskArray[index], sourceTask,
                                            taskArray[index].GetType().GetField(sourceFieldInfo.Name), ref doReference,
                                            ref flag, false, false);
                                        index++;
                                    }
                                }
                            }
                        }

                        ReferenceTasks(referenceTask, sourceTask,
                            referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false,
                            !doReference);
                    }

                    if (doReference)
                    {
                        list.Add(referenceTask);
                    }

                    if (!sourceFieldInfo.FieldType.IsArray)
                    {
                        sourceFieldInfo.SetValue(sourceTask, list);
                    }
                    else
                    {
                        Array array = Array.CreateInstance(sourceFieldInfo.FieldType.GetElementType(), list.Count);
                        list.CopyTo(array, 0);
                        sourceFieldInfo.SetValue(sourceTask, array);
                    }
                }

                if (synchronize)
                {
                    (referenceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = doReference;
                }

                fullSync = doReference && synchronize;
            }

            return true;
        }

        private void ResetTask(object task)
        {
            (task as Task).OnReset();
            List<System.Type> baseClasses = FieldInspector.GetBaseClasses(task.GetType());
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                       BindingFlags.NonPublic;
            for (int index1 = baseClasses.Count - 1; index1 > -1; --index1)
            {
                FieldInfo[] fields = baseClasses[index1].GetFields(bindingAttr);
                for (int index2 = 0; index2 < fields.Length; ++index2)
                {
                    if (typeof(SharedVariable).IsAssignableFrom(fields[index2].FieldType))
                    {
                        SharedVariable sharedVariable = fields[index2].GetValue(task) as SharedVariable;
                        if (TaskUtility.HasAttribute(fields[index2], typeof(RequiredFieldAttribute)) &&
                            sharedVariable != null && !sharedVariable.IsShared)
                            sharedVariable.IsShared = true;
                    }
                }
            }
        }

        private void SecondaryReflectionSelectionCallback(object obj)
        {
            if (this.mActiveMenuSelectionTask != null)
            {
                SharedVariable instance1 =
                    Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString"))
                        as SharedVariable;
                FieldInfo fieldInfo1;
                if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                {
                    this.ClearInvokeVariablesTask();
                    fieldInfo1 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
                }
                else
                    fieldInfo1 = !this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType())
                        ? this.mActiveMenuSelectionTask.GetType().GetField("propertyName")
                        : this.mActiveMenuSelectionTask.GetType().GetField("fieldName");

                if (obj == null)
                    fieldInfo1.SetValue(this.mActiveMenuSelectionTask, instance1);
                else if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                {
                    MethodInfo methodInfo = (MethodInfo) obj;
                    instance1.SetValue(methodInfo.Name);
                    fieldInfo1.SetValue(this.mActiveMenuSelectionTask, instance1);
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    for (int index = 0; index < 4; ++index)
                    {
                        FieldInfo field = this.mActiveMenuSelectionTask.GetType()
                            .GetField("parameter" + (index + 1));
                        if (index < parameters.Length)
                        {
                            SharedVariable instance2 =
                                Activator.CreateInstance(
                                        FieldInspector.FriendlySharedVariableName(parameters[index].ParameterType)) as
                                    SharedVariable;
                            field.SetValue(this.mActiveMenuSelectionTask, instance2);
                        }
                        else
                            field.SetValue(this.mActiveMenuSelectionTask, null);
                    }

                    if (!methodInfo.ReturnType.Equals(typeof(void)))
                    {
                        FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
                        SharedVariable instance2 =
                            Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(methodInfo.ReturnType))
                                as SharedVariable;
                        instance2.IsShared = true;
                        field.SetValue(this.mActiveMenuSelectionTask, instance2);
                    }
                }
                else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
                {
                    FieldInfo fieldInfo2 = (FieldInfo) obj;
                    instance1.SetValue(fieldInfo2.Name);
                    fieldInfo1.SetValue(this.mActiveMenuSelectionTask, instance1);
                    FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
                    if (field == (FieldInfo) null)
                        field = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                    SharedVariable instance2 =
                        Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(fieldInfo2.FieldType)) as
                            SharedVariable;
                    instance2.IsShared = this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType());
                    field.SetValue(this.mActiveMenuSelectionTask, instance2);
                }
                else
                {
                    PropertyInfo propertyInfo = (PropertyInfo) obj;
                    instance1.SetValue(propertyInfo.Name);
                    fieldInfo1.SetValue(this.mActiveMenuSelectionTask, instance1);
                    FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("propertyValue");
                    if (field == (FieldInfo) null)
                        field = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                    SharedVariable instance2 =
                        Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(propertyInfo.PropertyType))
                            as SharedVariable;
                    instance2.IsShared = this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType());
                    field.SetValue(this.mActiveMenuSelectionTask, instance2);
                }
            }

            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        public static void SelectInProject(object task)
        {
            MonoScript[] objectsOfTypeAll = (MonoScript[]) Resources.FindObjectsOfTypeAll(typeof(MonoScript));
            for (int index = 0; index < objectsOfTypeAll.Length; ++index)
            {
                if (objectsOfTypeAll[index] != null &&
                    objectsOfTypeAll[index].GetClass() != (System.Type) null &&
                    objectsOfTypeAll[index].GetClass().Equals(task.GetType()))
                {
                    Selection.activeObject = objectsOfTypeAll[index];
                    break;
                }
            }
        }

        public void SetActiveReferencedTasks(Task referenceTask, FieldInfo fieldInfo)
        {
            this.activeReferenceTask = referenceTask;
            this.activeReferenceTaskFieldInfo = fieldInfo;
        }

        private void SetTaskColor(object value)
        {
            TaskInspector.TaskColor taskColor = value as TaskInspector.TaskColor;
            if (taskColor.task.NodeData.ColorIndex == taskColor.colorIndex)
                return;
            taskColor.task.NodeData.ColorIndex = taskColor.colorIndex;
            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        private bool SharedVariableTypeExists(List<Type> sharedVariableTypes, Type type)
        {
            Type type2 = FieldInspector.FriendlySharedVariableName(type);
            for (int i = 0; i < sharedVariableTypes.Count; i++)
            {
                if (type2.IsAssignableFrom(sharedVariableTypes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public Task ActiveReferenceTask =>
            this.activeReferenceTask;

        public FieldInfo ActiveReferenceTaskFieldInfo =>
            this.activeReferenceTaskFieldInfo;

        private class TaskColor
        {
            public Task task;
            public int colorIndex;

            public TaskColor(Task task, int colorIndex)
            {
                this.task = task;
                this.colorIndex = colorIndex;
            }
        }
    }
}