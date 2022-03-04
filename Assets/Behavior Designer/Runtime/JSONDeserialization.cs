namespace BehaviorDesigner.Runtime
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class JSONDeserialization : UnityEngine.Object
    {
        private static Dictionary<TaskField, List<int>> taskIDs = null;
        private static GlobalVariables globalVariables = null;
        public static bool updatedSerialization = true;
        private static Dictionary<int, Dictionary<string, object>> serializationCache = new Dictionary<int, Dictionary<string, object>>();

        private static NodeData DeserializeNodeData(Dictionary<string, object> dict, Task task)
        {
            object obj2;
            NodeData data = new NodeData();
            if (dict.TryGetValue("Offset", out obj2))
            {
                data.Offset = StringToVector2((string) obj2);
            }
            if (dict.TryGetValue("FriendlyName", out obj2))
            {
                task.FriendlyName = (string) obj2;
            }
            if (dict.TryGetValue("Comment", out obj2))
            {
                data.Comment = (string) obj2;
            }
            if (dict.TryGetValue("IsBreakpoint", out obj2))
            {
                data.IsBreakpoint = Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
            }
            if (dict.TryGetValue("Collapsed", out obj2))
            {
                data.Collapsed = Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
            }
            if (dict.TryGetValue("ColorIndex", out obj2))
            {
                data.ColorIndex = Convert.ToInt32(obj2, CultureInfo.InvariantCulture);
            }
            if (dict.TryGetValue("WatchedFields", out obj2))
            {
                data.WatchedFieldNames = new List<string>();
                data.WatchedFields = new List<FieldInfo>();
                IList list = obj2 as IList;
                for (int i = 0; i < list.Count; i++)
                {
                    FieldInfo field = task.GetType().GetField((string) list[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null)
                    {
                        data.WatchedFieldNames.Add(field.Name);
                        data.WatchedFields.Add(field);
                    }
                }
            }
            return data;
        }

        private static void DeserializeObject(Task task, object obj, Dictionary<string, object> dict, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
        {
            if (dict != null)
            {
                FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(obj.GetType());
                for (int i = 0; i < serializableFields.Length; i++)
                {
                    object obj2;
                    string key = !updatedSerialization ? (serializableFields[i].FieldType.Name.GetHashCode() + serializableFields[i].Name.GetHashCode()).ToString() : (serializableFields[i].FieldType.Name + serializableFields[i].Name);
                    if (!dict.TryGetValue(key, out obj2))
                    {
                        if (typeof(SharedVariable).IsAssignableFrom(serializableFields[i].FieldType) && !serializableFields[i].FieldType.IsAbstract)
                        {
                            if (dict.TryGetValue((serializableFields[i].FieldType.Name.GetHashCode() + serializableFields[i].Name.GetHashCode()).ToString(), out obj2))
                            {
                                SharedVariable variable = TaskUtility.CreateInstance(serializableFields[i].FieldType) as SharedVariable;
                                variable.SetValue(ValueToObject(task, serializableFields[i].FieldType, obj2, variableSource, unityObjects));
                                serializableFields[i].SetValue(obj, variable);
                            }
                            else
                            {
                                SharedVariable variable2 = TaskUtility.CreateInstance(serializableFields[i].FieldType) as SharedVariable;
                                SharedVariable variable3 = serializableFields[i].GetValue(obj) as SharedVariable;
                                if (variable3 != null)
                                {
                                    variable2.SetValue(variable3.GetValue());
                                }
                                serializableFields[i].SetValue(obj, variable2);
                            }
                        }
                    }
                    else if (!typeof(IList).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        Type fieldType = serializableFields[i].FieldType;
                        if (!fieldType.Equals(typeof(Task)) && !fieldType.IsSubclassOf(typeof(Task)))
                        {
                            object obj5 = ValueToObject(task, fieldType, obj2, variableSource, unityObjects);
                            if ((obj5 != null) && (!obj5.Equals(null) && fieldType.IsAssignableFrom(obj5.GetType())))
                            {
                                serializableFields[i].SetValue(obj, obj5);
                            }
                        }
                        else if (!TaskUtility.HasAttribute(serializableFields[i], typeof(InspectTaskAttribute)))
                        {
                            if (taskIDs != null)
                            {
                                List<int> list4 = new List<int> {
                                    Convert.ToInt32(obj2, CultureInfo.InvariantCulture)
                                };
                                taskIDs.Add(new TaskField(task, serializableFields[i]), list4);
                            }
                        }
                        else
                        {
                            Dictionary<string, object> dictionary = obj2 as Dictionary<string, object>;
                            Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dictionary["Type"] as string);
                            if (typeWithinAssembly != null)
                            {
                                Task task2 = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
                                DeserializeObject(task2, task2, dictionary, variableSource, unityObjects);
                                serializableFields[i].SetValue(task, task2);
                            }
                        }
                    }
                    else
                    {
                        IList list = obj2 as IList;
                        if (list != null)
                        {
                            Type elementType;
                            if (serializableFields[i].FieldType.IsArray)
                            {
                                elementType = serializableFields[i].FieldType.GetElementType();
                            }
                            else
                            {
                                Type fieldType = serializableFields[i].FieldType;
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
                            if (elementType.Equals(typeof(Task)) || elementType.IsSubclassOf(typeof(Task)))
                            {
                                if (taskIDs != null)
                                {
                                    List<int> list2 = new List<int>();
                                    int num3 = 0;
                                    while (true)
                                    {
                                        if (num3 >= list.Count)
                                        {
                                            taskIDs.Add(new TaskField(task, serializableFields[i]), list2);
                                            break;
                                        }
                                        list2.Add(Convert.ToInt32(list[num3], CultureInfo.InvariantCulture));
                                        num3++;
                                    }
                                }
                            }
                            else if (serializableFields[i].FieldType.IsArray)
                            {
                                Array array = Array.CreateInstance(elementType, list.Count);
                                int index = 0;
                                while (true)
                                {
                                    if (index >= list.Count)
                                    {
                                        serializableFields[i].SetValue(obj, array);
                                        break;
                                    }
                                    if (list[index] == null)
                                    {
                                        array.SetValue(null, index);
                                    }
                                    else
                                    {
                                        object o = ValueToObject(task, elementType, list[index], variableSource, unityObjects);
                                        if (!elementType.IsInstanceOfType(o))
                                        {
                                            array.SetValue(null, index);
                                        }
                                        else
                                        {
                                            array.SetValue(o, index);
                                        }
                                    }
                                    index++;
                                }
                            }
                            else
                            {
                                IList list3;
                                if (!serializableFields[i].FieldType.IsGenericType)
                                {
                                    list3 = TaskUtility.CreateInstance(serializableFields[i].FieldType) as IList;
                                }
                                else
                                {
                                    Type[] typeArguments = new Type[] { elementType };
                                    list3 = TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(typeArguments)) as IList;
                                }
                                int num5 = 0;
                                while (true)
                                {
                                    if (num5 >= list.Count)
                                    {
                                        serializableFields[i].SetValue(obj, list3);
                                        break;
                                    }
                                    if (list[num5] == null)
                                    {
                                        list3.Add(null);
                                    }
                                    else
                                    {
                                        object obj4 = ValueToObject(task, elementType, list[num5], variableSource, unityObjects);
                                        if ((obj4 != null) && !obj4.Equals(null))
                                        {
                                            list3.Add(ValueToObject(task, elementType, list[num5], variableSource, unityObjects));
                                        }
                                        else
                                        {
                                            list3.Add(null);
                                        }
                                    }
                                    num5++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, IVariableSource variableSource, bool fromSource, List<UnityEngine.Object> unityObjects)
        {
            object obj2;
            if (dict == null)
            {
                return null;
            }
            SharedVariable sharedVariable = null;
            if (!fromSource && ((variableSource != null) && (dict.TryGetValue("Name", out obj2) && (BehaviorManager.IsPlaying || !dict.ContainsKey("IsDynamic")))))
            {
                object obj3;
                dict.TryGetValue("IsGlobal", out obj3);
                if (!dict.TryGetValue("IsGlobal", out obj3) || !Convert.ToBoolean(obj3, CultureInfo.InvariantCulture))
                {
                    sharedVariable = variableSource.GetVariable(obj2 as string);
                }
                else
                {
                    if (globalVariables == null)
                    {
                        globalVariables = GlobalVariables.Instance;
                    }
                    if (globalVariables != null)
                    {
                        sharedVariable = globalVariables.GetVariable(obj2 as string);
                    }
                }
            }
            Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
            if (typeWithinAssembly == null)
            {
                return null;
            }
            bool flag = true;
            if ((sharedVariable == null) || !(flag = sharedVariable.GetType().Equals(typeWithinAssembly)))
            {
                object obj4;
                sharedVariable = TaskUtility.CreateInstance(typeWithinAssembly) as SharedVariable;
                sharedVariable.Name = dict["Name"] as string;
                if (dict.TryGetValue("IsShared", out obj4))
                {
                    sharedVariable.IsShared = Convert.ToBoolean(obj4, CultureInfo.InvariantCulture);
                }
                if (dict.TryGetValue("IsGlobal", out obj4))
                {
                    sharedVariable.IsGlobal = Convert.ToBoolean(obj4, CultureInfo.InvariantCulture);
                }
                if (dict.TryGetValue("IsDynamic", out obj4))
                {
                    sharedVariable.IsDynamic = Convert.ToBoolean(obj4, CultureInfo.InvariantCulture);
                    if (BehaviorManager.IsPlaying)
                    {
                        variableSource.SetVariable(sharedVariable.Name, sharedVariable);
                    }
                }
                if (dict.TryGetValue("Tooltip", out obj4))
                {
                    sharedVariable.Tooltip = obj4 as string;
                }
                if (!sharedVariable.IsGlobal && dict.TryGetValue("PropertyMapping", out obj4))
                {
                    sharedVariable.PropertyMapping = obj4 as string;
                    if (dict.TryGetValue("PropertyMappingOwner", out obj4))
                    {
                        sharedVariable.PropertyMappingOwner = IndexToUnityObject(Convert.ToInt32(obj4, CultureInfo.InvariantCulture), unityObjects) as GameObject;
                    }
                    sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
                }
                if (!flag)
                {
                    sharedVariable.IsShared = true;
                }
                DeserializeObject(null, sharedVariable, dict, variableSource, unityObjects);
            }
            return sharedVariable;
        }

        public static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, List<UnityEngine.Object> unityObjects)
        {
            Task task = null;
            object obj2;
            try
            {
                Type typeWithinAssembly = TaskUtility.GetTypeWithinAssembly(dict["Type"] as string);
                if (typeWithinAssembly == null)
                {
                    typeWithinAssembly = !dict.ContainsKey("Children") ? typeof(UnknownTask) : typeof(UnknownParentTask);
                }
                task = TaskUtility.CreateInstance(typeWithinAssembly) as Task;
                if (task is UnknownTask)
                {
                    (task as UnknownTask).JSONSerialization = MiniJSON.Serialize(dict);
                }
            }
            catch (Exception)
            {
            }
            if (task == null)
            {
                return null;
            }
            task.Owner = behaviorSource.Owner.GetObject() as Behavior;
            task.ID = Convert.ToInt32(dict["ID"], CultureInfo.InvariantCulture);
            if (dict.TryGetValue("Name", out obj2))
            {
                task.FriendlyName = (string) obj2;
            }
            if (dict.TryGetValue("Instant", out obj2))
            {
                task.IsInstant = Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
            }
            if (dict.TryGetValue("Disabled", out obj2))
            {
                task.Disabled = Convert.ToBoolean(obj2, CultureInfo.InvariantCulture);
            }
            IDtoTask.Add(task.ID, task);
            task.NodeData = DeserializeNodeData(dict["NodeData"] as Dictionary<string, object>, task);
            if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
            {
                if (!task.FriendlyName.Contains("Unknown "))
                {
                    task.FriendlyName = $"Unknown {task.FriendlyName}";
                }
                task.NodeData.Comment = "Unknown Task. Right click and Replace to locate new task.";
            }
            DeserializeObject(task, task, dict, behaviorSource, unityObjects);
            if ((task is ParentTask) && dict.TryGetValue("Children", out obj2))
            {
                ParentTask task3 = task as ParentTask;
                if (task3 != null)
                {
                    IEnumerator enumerator = (obj2 as IEnumerable).GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Dictionary<string, object> current = (Dictionary<string, object>) enumerator.Current;
                            Task child = DeserializeTask(behaviorSource, current, ref IDtoTask, unityObjects);
                            task3.AddChild(child, (task3.Children != null) ? task3.Children.Count : 0);
                        }
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
            return task;
        }

        private static void DeserializeVariables(IVariableSource variableSource, Dictionary<string, object> dict, List<UnityEngine.Object> unityObjects)
        {
            object obj2;
            if (dict.TryGetValue("Variables", out obj2))
            {
                List<SharedVariable> variables = new List<SharedVariable>();
                IList list2 = obj2 as IList;
                int num = 0;
                while (true)
                {
                    if (num >= list2.Count)
                    {
                        variableSource.SetAllVariables(variables);
                        break;
                    }
                    SharedVariable item = DeserializeSharedVariable(list2[num] as Dictionary<string, object>, variableSource, true, unityObjects);
                    variables.Add(item);
                    num++;
                }
            }
        }

        private static UnityEngine.Object IndexToUnityObject(int index, List<UnityEngine.Object> unityObjects) => 
            (((index < 0) || (index >= unityObjects.Count)) ? null : unityObjects[index]);

        public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
        {
            Dictionary<string, object> dictionary;
            behaviorSource.EntryTask = null;
            behaviorSource.RootTask = null;
            behaviorSource.DetachedTasks = null;
            behaviorSource.Variables = null;
            if (!serializationCache.TryGetValue(taskData.JSONSerialization.GetHashCode(), out dictionary))
            {
                dictionary = MiniJSON.Deserialize(taskData.JSONSerialization) as Dictionary<string, object>;
                serializationCache.Add(taskData.JSONSerialization.GetHashCode(), dictionary);
            }
            if (dictionary == null)
            {
                Debug.Log("Failed to deserialize");
            }
            else
            {
                taskIDs = new Dictionary<TaskField, List<int>>();
                updatedSerialization = new Version(taskData.Version).CompareTo(new Version("1.5.7")) >= 0;
                Dictionary<int, Task> iDtoTask = new Dictionary<int, Task>();
                DeserializeVariables(behaviorSource, dictionary, taskData.fieldSerializationData.unityObjects);
                if (dictionary.ContainsKey("EntryTask"))
                {
                    behaviorSource.EntryTask = DeserializeTask(behaviorSource, dictionary["EntryTask"] as Dictionary<string, object>, ref iDtoTask, taskData.fieldSerializationData.unityObjects);
                }
                if (dictionary.ContainsKey("RootTask"))
                {
                    behaviorSource.RootTask = DeserializeTask(behaviorSource, dictionary["RootTask"] as Dictionary<string, object>, ref iDtoTask, taskData.fieldSerializationData.unityObjects);
                }
                if (dictionary.ContainsKey("DetachedTasks"))
                {
                    List<Task> list = new List<Task>();
                    IEnumerator enumerator = (dictionary["DetachedTasks"] as IEnumerable).GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            Dictionary<string, object> current = (Dictionary<string, object>) enumerator.Current;
                            list.Add(DeserializeTask(behaviorSource, current, ref iDtoTask, taskData.fieldSerializationData.unityObjects));
                        }
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    behaviorSource.DetachedTasks = list;
                }
                if ((taskIDs != null) && (taskIDs.Count > 0))
                {
                    foreach (TaskField field in taskIDs.Keys)
                    {
                        List<int> list2 = taskIDs[field];
                        Type fieldType = field.fieldInfo.FieldType;
                        if (!field.fieldInfo.FieldType.IsArray)
                        {
                            Task task3 = iDtoTask[list2[0]];
                            if (!task3.GetType().Equals(field.fieldInfo.FieldType) && !task3.GetType().IsSubclassOf(field.fieldInfo.FieldType))
                            {
                                continue;
                            }
                            field.fieldInfo.SetValue(field.task, task3);
                            continue;
                        }
                        int length = 0;
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 >= list2.Count)
                            {
                                Array array = Array.CreateInstance(fieldType.GetElementType(), length);
                                int index = 0;
                                int num4 = 0;
                                while (true)
                                {
                                    if (num4 >= list2.Count)
                                    {
                                        field.fieldInfo.SetValue(field.task, array);
                                        break;
                                    }
                                    Task task2 = iDtoTask[list2[num4]];
                                    if (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType()))
                                    {
                                        array.SetValue(task2, index);
                                        index++;
                                    }
                                    num4++;
                                }
                                break;
                            }
                            Task task = iDtoTask[list2[num2]];
                            if (task.GetType().Equals(fieldType.GetElementType()) || task.GetType().IsSubclassOf(fieldType.GetElementType()))
                            {
                                length++;
                            }
                            num2++;
                        }
                    }
                    taskIDs = null;
                }
            }
        }

        public static void Load(string serialization, GlobalVariables globalVariables, string version)
        {
            if (globalVariables != null)
            {
                Dictionary<string, object> dict = MiniJSON.Deserialize(serialization) as Dictionary<string, object>;
                if (dict == null)
                {
                    Debug.Log("Failed to deserialize");
                }
                else
                {
                    if (globalVariables.VariableData == null)
                    {
                        globalVariables.VariableData = new VariableSerializationData();
                    }
                    updatedSerialization = new Version(globalVariables.Version).CompareTo(new Version("1.5.7")) >= 0;
                    DeserializeVariables(globalVariables, dict, globalVariables.VariableData.fieldSerializationData.unityObjects);
                }
            }
        }

        private static Color StringToColor(string colorString)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = colorString.Substring(5, colorString.Length - 6).Split(separator);
            return new Color(float.Parse(strArray[0], CultureInfo.InvariantCulture), float.Parse(strArray[1], CultureInfo.InvariantCulture), float.Parse(strArray[2], CultureInfo.InvariantCulture), float.Parse(strArray[3], CultureInfo.InvariantCulture));
        }

        private static Matrix4x4 StringToMatrix4x4(string matrixString)
        {
            string[] strArray = matrixString.Split(null);
            return new Matrix4x4 { 
                m00 = float.Parse(strArray[0], CultureInfo.InvariantCulture),
                m01 = float.Parse(strArray[1], CultureInfo.InvariantCulture),
                m02 = float.Parse(strArray[2], CultureInfo.InvariantCulture),
                m03 = float.Parse(strArray[3], CultureInfo.InvariantCulture),
                m10 = float.Parse(strArray[4], CultureInfo.InvariantCulture),
                m11 = float.Parse(strArray[5], CultureInfo.InvariantCulture),
                m12 = float.Parse(strArray[6], CultureInfo.InvariantCulture),
                m13 = float.Parse(strArray[7], CultureInfo.InvariantCulture),
                m20 = float.Parse(strArray[8], CultureInfo.InvariantCulture),
                m21 = float.Parse(strArray[9], CultureInfo.InvariantCulture),
                m22 = float.Parse(strArray[10], CultureInfo.InvariantCulture),
                m23 = float.Parse(strArray[11], CultureInfo.InvariantCulture),
                m30 = float.Parse(strArray[12], CultureInfo.InvariantCulture),
                m31 = float.Parse(strArray[13], CultureInfo.InvariantCulture),
                m32 = float.Parse(strArray[14], CultureInfo.InvariantCulture),
                m33 = float.Parse(strArray[15], CultureInfo.InvariantCulture)
            };
        }

        private static Quaternion StringToQuaternion(string quaternionString)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = quaternionString.Substring(1, quaternionString.Length - 2).Split(separator);
            return new Quaternion(float.Parse(strArray[0]), float.Parse(strArray[1], CultureInfo.InvariantCulture), float.Parse(strArray[2], CultureInfo.InvariantCulture), float.Parse(strArray[3], CultureInfo.InvariantCulture));
        }

        private static Rect StringToRect(string rectString)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = rectString.Substring(1, rectString.Length - 2).Split(separator);
            return new Rect(float.Parse(strArray[0].Substring(2, strArray[0].Length - 2), CultureInfo.InvariantCulture), float.Parse(strArray[1].Substring(3, strArray[1].Length - 3), CultureInfo.InvariantCulture), float.Parse(strArray[2].Substring(7, strArray[2].Length - 7), CultureInfo.InvariantCulture), float.Parse(strArray[3].Substring(8, strArray[3].Length - 8), CultureInfo.InvariantCulture));
        }

        private static Vector2 StringToVector2(string vector2String)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = vector2String.Substring(1, vector2String.Length - 2).Split(separator);
            return new Vector2(float.Parse(strArray[0], CultureInfo.InvariantCulture), float.Parse(strArray[1], CultureInfo.InvariantCulture));
        }

        private static Vector2Int StringToVector2Int(string vector2String)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = vector2String.Substring(1, vector2String.Length - 2).Split(separator);
            return new Vector2Int(int.Parse(strArray[0], CultureInfo.InvariantCulture), int.Parse(strArray[1], CultureInfo.InvariantCulture));
        }

        private static Vector3 StringToVector3(string vector3String)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = vector3String.Substring(1, vector3String.Length - 2).Split(separator);
            return new Vector3(float.Parse(strArray[0], CultureInfo.InvariantCulture), float.Parse(strArray[1], CultureInfo.InvariantCulture), float.Parse(strArray[2], CultureInfo.InvariantCulture));
        }

        private static Vector3Int StringToVector3Int(string vector3String)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = vector3String.Substring(1, vector3String.Length - 2).Split(separator);
            return new Vector3Int(int.Parse(strArray[0], CultureInfo.InvariantCulture), int.Parse(strArray[1], CultureInfo.InvariantCulture), int.Parse(strArray[2], CultureInfo.InvariantCulture));
        }

        private static Vector4 StringToVector4(string vector4String)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = vector4String.Substring(1, vector4String.Length - 2).Split(separator);
            return new Vector4(float.Parse(strArray[0], CultureInfo.InvariantCulture), float.Parse(strArray[1], CultureInfo.InvariantCulture), float.Parse(strArray[2], CultureInfo.InvariantCulture), float.Parse(strArray[3], CultureInfo.InvariantCulture));
        }

        private static AnimationCurve ValueToAnimationCurve(Dictionary<string, object> value)
        {
            object obj2;
            AnimationCurve curve = new AnimationCurve();
            if (value.TryGetValue("Keys", out obj2))
            {
                List<object> list = obj2 as List<object>;
                for (int i = 0; i < list.Count; i++)
                {
                    List<object> list2 = list[i] as List<object>;
                    Keyframe keyframe = new Keyframe((float) Convert.ChangeType(list2[0], typeof(float), CultureInfo.InvariantCulture), (float) Convert.ChangeType(list2[1], typeof(float), CultureInfo.InvariantCulture), (float) Convert.ChangeType(list2[2], typeof(float), CultureInfo.InvariantCulture), (float) Convert.ChangeType(list2[3], typeof(float), CultureInfo.InvariantCulture));
                    curve.AddKey(keyframe);
                }
            }
            if (value.TryGetValue("PreWrapMode", out obj2))
            {
                curve.preWrapMode = (WrapMode) Enum.Parse(typeof(WrapMode), (string) obj2);
                // curve.preWrapMode((WrapMode) Enum.Parse(typeof(WrapMode), (string) obj2));
            }
            if (value.TryGetValue("PostWrapMode", out obj2))
            {
                curve.postWrapMode = (WrapMode) Enum.Parse(typeof(WrapMode), (string) obj2);
            }
            return curve;
        }

        private static LayerMask ValueToLayerMask(int value)
        {
            LayerMask mask = new LayerMask();
            mask.value = value;
            return mask;
        }

        private static object ValueToObject(Task task, Type type, object obj, IVariableSource variableSource, List<UnityEngine.Object> unityObjects)
        {
            object obj2;
            if (typeof(SharedVariable).IsAssignableFrom(type))
            {
                SharedVariable variable = DeserializeSharedVariable(obj as Dictionary<string, object>, variableSource, false, unityObjects);
                if (variable == null)
                {
                    variable = TaskUtility.CreateInstance(type) as SharedVariable;
                }
                return variable;
            }
            if (type.Equals(typeof(UnityEngine.Object)) || type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return IndexToUnityObject(Convert.ToInt32(obj, CultureInfo.InvariantCulture), unityObjects);
            }
            if (type.IsPrimitive || type.Equals(typeof(string)))
            {
                try
                {
                    obj2 = Convert.ChangeType(obj, type);
                }
                catch (Exception)
                {
                    obj2 = null;
                }
            }
            else
            {
                if (!type.IsSubclassOf(typeof(Enum)))
                {
                    if (type.Equals(typeof(Vector2)))
                    {
                        return StringToVector2((string) obj);
                    }
                    if (type.Equals(typeof(Vector2Int)))
                    {
                        return StringToVector2Int((string) obj);
                    }
                    if (type.Equals(typeof(Vector3)))
                    {
                        return StringToVector3((string) obj);
                    }
                    if (type.Equals(typeof(Vector3Int)))
                    {
                        return StringToVector3Int((string) obj);
                    }
                    if (type.Equals(typeof(Vector4)))
                    {
                        return StringToVector4((string) obj);
                    }
                    if (type.Equals(typeof(Quaternion)))
                    {
                        return StringToQuaternion((string) obj);
                    }
                    if (type.Equals(typeof(Matrix4x4)))
                    {
                        return StringToMatrix4x4((string) obj);
                    }
                    if (type.Equals(typeof(Color)))
                    {
                        return StringToColor((string) obj);
                    }
                    if (type.Equals(typeof(Rect)))
                    {
                        return StringToRect((string) obj);
                    }
                    if (type.Equals(typeof(LayerMask)))
                    {
                        return ValueToLayerMask(Convert.ToInt32(obj, CultureInfo.InvariantCulture));
                    }
                    if (type.Equals(typeof(AnimationCurve)))
                    {
                        return ValueToAnimationCurve((Dictionary<string, object>) obj);
                    }
                    object obj3 = TaskUtility.CreateInstance(type);
                    DeserializeObject(task, obj3, obj as Dictionary<string, object>, variableSource, unityObjects);
                    return obj3;
                }
                try
                {
                    obj2 = Enum.Parse(type, (string) obj);
                }
                catch (Exception)
                {
                    obj2 = null;
                }
            }
            return obj2;
        }

        public static Dictionary<TaskField, List<int>> TaskIDs
        {
            get => 
                taskIDs;
            set => 
                taskIDs = value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TaskField
        {
            public Task task;
            public FieldInfo fieldInfo;
            public TaskField(Task t, FieldInfo f)
            {
                this.task = t;
                this.fieldInfo = f;
            }
        }
    }
}

