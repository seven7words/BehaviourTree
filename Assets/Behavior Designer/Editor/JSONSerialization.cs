﻿namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class JSONSerialization : UnityEngine.Object
    {
        private static TaskSerializationData taskSerializationData;
        private static FieldSerializationData fieldSerializationData;
        private static VariableSerializationData variableSerializationData;

        public static void Save(BehaviorSource behaviorSource)
        {
            behaviorSource.CheckForSerialization(false, null);
            taskSerializationData = new TaskSerializationData();
            fieldSerializationData = taskSerializationData.fieldSerializationData;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (behaviorSource.EntryTask != null)
            {
                dictionary.Add("EntryTask", SerializeTask(behaviorSource.EntryTask, true, ref fieldSerializationData.unityObjects));
            }
            if (behaviorSource.RootTask != null)
            {
                dictionary.Add("RootTask", SerializeTask(behaviorSource.RootTask, true, ref fieldSerializationData.unityObjects));
            }
            if (behaviorSource.DetachedTasks != null && behaviorSource.DetachedTasks.Count > 0)
            {
                Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[behaviorSource.DetachedTasks.Count];
                for (int index = 0; index < behaviorSource.DetachedTasks.Count; ++index)
                    dictionaryArray[index] = JSONSerialization.SerializeTask(behaviorSource.DetachedTasks[index], true, ref JSONSerialization.fieldSerializationData.unityObjects);
                dictionary.Add("DetachedTasks", (object) dictionaryArray);
            }
            if (behaviorSource.Variables != null && behaviorSource.Variables.Count > 0)
                dictionary.Add("Variables", (object) JSONSerialization.SerializeVariables(behaviorSource.Variables, ref JSONSerialization.fieldSerializationData.unityObjects));
            JSONSerialization.taskSerializationData.Version = "1.6.8";
            JSONSerialization.taskSerializationData.JSONSerialization = MiniJSON.Serialize((object) dictionary);
            behaviorSource.TaskData = JSONSerialization.taskSerializationData;
            if (behaviorSource.Owner == null || behaviorSource.Owner.Equals((object) null))
                return;
            BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner.GetObject());
        }

        public static void Save(GlobalVariables variables)
        {
            if ((UnityEngine.Object) variables == (UnityEngine.Object) null)
                return;
            JSONSerialization.variableSerializationData = new VariableSerializationData();
            JSONSerialization.fieldSerializationData = JSONSerialization.variableSerializationData.fieldSerializationData;
            JSONSerialization.variableSerializationData.JSONSerialization = MiniJSON.Serialize((object) new Dictionary<string, object>()
            {
                {
                    "Variables",
                    (object) JSONSerialization.SerializeVariables(variables.Variables, ref JSONSerialization.fieldSerializationData.unityObjects)
                }
            });
            variables.VariableData = JSONSerialization.variableSerializationData;
            variables.Version = "1.6.8";
            BehaviorDesignerUtility.SetObjectDirty((UnityEngine.Object) variables);
        }

         private static void SerializeFields(
      object obj,
      ref Dictionary<string, object> dict,
      ref List<Object> unityObjects)
    {
      FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(obj.GetType());
      for (int index1 = 0; index1 < serializableFields.Length; ++index1)
      {
        if (!BehaviorDesignerUtility.HasAttribute(serializableFields[index1], typeof (NonSerializedAttribute)) && (!serializableFields[index1].IsPrivate && !serializableFields[index1].IsFamily || BehaviorDesignerUtility.HasAttribute(serializableFields[index1], typeof (SerializeField))) && ((!(obj is ParentTask) || !serializableFields[index1].Name.Equals("children")) && serializableFields[index1].GetValue(obj) != null))
        {
          string key = (serializableFields[index1].FieldType.Name + serializableFields[index1].Name).ToString();
          if (typeof (IList).IsAssignableFrom(serializableFields[index1].FieldType))
          {
            if (serializableFields[index1].GetValue(obj) is IList list)
            {
              List<object> objectList = new List<object>();
              for (int index2 = 0; index2 < list.Count; ++index2)
              {
                if (list[index2] == null)
                {
                  objectList.Add((object) null);
                }
                else
                {
                  Type type = list[index2].GetType();
                  if (list[index2] is Task)
                  {
                    Task task = list[index2] as Task;
                    objectList.Add((object) task.ID);
                  }
                  else if (list[index2] is SharedVariable)
                    objectList.Add((object) JSONSerialization.SerializeVariable(list[index2] as SharedVariable, ref unityObjects));
                  else if ((object) (list[index2] as Object) != null)
                  {
                    Object @object = list[index2] as Object;
                    if (!object.ReferenceEquals((object) @object, (object) null) && @object != (Object) null)
                    {
                      objectList.Add((object) unityObjects.Count);
                      unityObjects.Add(@object);
                    }
                  }
                  else if (type.Equals(typeof (LayerMask)))
                    objectList.Add((object) ((LayerMask) list[index2]).value);
                  else if (type.IsPrimitive || type.IsEnum || (type.Equals(typeof (string)) || type.Equals(typeof (Vector2))) || (type.Equals(typeof (Vector2Int)) || type.Equals(typeof (Vector3)) || (type.Equals(typeof (Vector3Int)) || type.Equals(typeof (Vector4)))) || (type.Equals(typeof (Quaternion)) || type.Equals(typeof (Matrix4x4)) || (type.Equals(typeof (Color)) || type.Equals(typeof (Rect)))))
                  {
                    objectList.Add(list[index2]);
                  }
                  else
                  {
                    Dictionary<string, object> dict1 = new Dictionary<string, object>();
                    JSONSerialization.SerializeFields(list[index2], ref dict1, ref unityObjects);
                    objectList.Add((object) dict1);
                  }
                }
              }
              if (objectList != null)
                dict.Add(key, (object) objectList);
            }
          }
          else if (typeof (Task).IsAssignableFrom(serializableFields[index1].FieldType))
          {
            if (serializableFields[index1].GetValue(obj) is Task task6)
            {
              if (BehaviorDesignerUtility.HasAttribute(serializableFields[index1], typeof (InspectTaskAttribute)))
              {
                Dictionary<string, object> dict1 = new Dictionary<string, object>();
                dict1.Add("Type", (object) task6.GetType());
                JSONSerialization.SerializeFields((object) task6, ref dict1, ref unityObjects);
                dict.Add(key, (object) dict1);
              }
              else
                dict.Add(key, (object) task6.ID);
            }
          }
          else if (typeof (SharedVariable).IsAssignableFrom(serializableFields[index1].FieldType))
          {
            if (!dict.ContainsKey(key))
              dict.Add(key, (object) JSONSerialization.SerializeVariable(serializableFields[index1].GetValue(obj) as SharedVariable, ref unityObjects));
          }
          else if (typeof (UnityEngine.Object).IsAssignableFrom(serializableFields[index1].FieldType))
          {
            Object @object = serializableFields[index1].GetValue(obj) as Object;
            if (!object.ReferenceEquals((object) @object, (object) null) && @object != (Object) null)
            {
              dict.Add(key, (object) unityObjects.Count);
              unityObjects.Add(@object);
            }
          }
          else if (serializableFields[index1].FieldType.Equals(typeof (LayerMask)))
            dict.Add(key, (object) ((LayerMask) serializableFields[index1].GetValue(obj)).value);
          else if (serializableFields[index1].FieldType.IsPrimitive || serializableFields[index1].FieldType.IsEnum || (serializableFields[index1].FieldType.Equals(typeof (string)) || serializableFields[index1].FieldType.Equals(typeof (Vector2))) || (serializableFields[index1].FieldType.Equals(typeof (Vector2Int)) || serializableFields[index1].FieldType.Equals(typeof (Vector3)) || (serializableFields[index1].FieldType.Equals(typeof (Vector3Int)) || serializableFields[index1].FieldType.Equals(typeof (Vector4)))) || (serializableFields[index1].FieldType.Equals(typeof (Quaternion)) || serializableFields[index1].FieldType.Equals(typeof (Matrix4x4)) || (serializableFields[index1].FieldType.Equals(typeof (Color)) || serializableFields[index1].FieldType.Equals(typeof (Rect)))))
            dict.Add(key, serializableFields[index1].GetValue(obj));
          else if (serializableFields[index1].FieldType.Equals(typeof (AnimationCurve)))
          {
            AnimationCurve animationCurve = serializableFields[index1].GetValue(obj) as AnimationCurve;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (animationCurve.keys != null)
            {
              Keyframe[] keys = animationCurve.keys;
              List<List<object>> objectListList = new List<List<object>>();
              for (int index2 = 0; index2 < keys.Length; ++index2)
                objectListList.Add(new List<object>()
                {
                  (object) keys[index2].time,
                  (object) keys[index2].value,
                  (object) keys[index2].inTangent,
                  (object) keys[index2].outTangent
                });
              dictionary.Add("Keys", (object) objectListList);
            }
            dictionary.Add("PreWrapMode", (object) animationCurve.preWrapMode);
            dictionary.Add("PostWrapMode", (object) animationCurve.postWrapMode);
            dict.Add(key, (object) dictionary);
          }
          else
          {
            Dictionary<string, object> dict1 = new Dictionary<string, object>();
            JSONSerialization.SerializeFields(serializableFields[index1].GetValue(obj), ref dict1, ref unityObjects);
            dict.Add(key, (object) dict1);
          }
        }
      }
    }
         private static Dictionary<string, object> SerializeNodeData(NodeData nodeData)
         {
             Dictionary<string, object> dictionary = new Dictionary<string, object>();
             dictionary.Add("Offset", (object) nodeData.Offset);
             if (nodeData.Comment.Length > 0)
                 dictionary.Add("Comment", (object) nodeData.Comment);
             if (nodeData.IsBreakpoint)
                 dictionary.Add("IsBreakpoint", (object) nodeData.IsBreakpoint);
             if (nodeData.Collapsed)
                 dictionary.Add("Collapsed", (object) nodeData.Collapsed);
             if (nodeData.ColorIndex != 0)
                 dictionary.Add("ColorIndex", (object) nodeData.ColorIndex);
             if (nodeData.WatchedFieldNames != null && nodeData.WatchedFieldNames.Count > 0)
                 dictionary.Add("WatchedFields", (object) nodeData.WatchedFieldNames);
             return dictionary;
         }

         public static Dictionary<string, object> SerializeTask(
             Task task,
             bool serializeChildren,
             ref List<Object> unityObjects)
         {
             Dictionary<string, object> dict = new Dictionary<string, object>();
             dict.Add("Type", (object) task.GetType());
             dict.Add("NodeData", (object) JSONSerialization.SerializeNodeData(task.NodeData));
             dict.Add("ID", (object) task.ID);
             dict.Add("Name", (object) task.FriendlyName);
             dict.Add("Instant", (object) task.IsInstant);
             if (task.Disabled)
                 dict.Add("Disabled", (object) task.Disabled);
             JSONSerialization.SerializeFields((object) task, ref dict, ref unityObjects);
             if (serializeChildren && task is ParentTask)
             {
                 ParentTask parentTask = task as ParentTask;
                 if (parentTask.Children != null && parentTask.Children.Count > 0)
                 {
                     Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[parentTask.Children.Count];
                     for (int index = 0; index < parentTask.Children.Count; ++index)
                         dictionaryArray[index] = JSONSerialization.SerializeTask(parentTask.Children[index], serializeChildren, ref unityObjects);
                     dict.Add("Children", (object) dictionaryArray);
                 }
             }
             return dict;
         }
         private static Dictionary<string, object> SerializeVariable(
             SharedVariable sharedVariable,
             ref List<Object> unityObjects)
         {
             if (sharedVariable == null)
                 return (Dictionary<string, object>) null;
             Dictionary<string, object> dict = new Dictionary<string, object>();
             dict.Add("Type", (object) sharedVariable.GetType());
             dict.Add("Name", (object) sharedVariable.Name);
             if (sharedVariable.IsShared)
                 dict.Add("IsShared", (object) sharedVariable.IsShared);
             if (sharedVariable.IsGlobal)
                 dict.Add("IsGlobal", (object) sharedVariable.IsGlobal);
             if (sharedVariable.IsDynamic)
                 dict.Add("IsDynamic", (object) sharedVariable.IsDynamic);
             if (!string.IsNullOrEmpty(sharedVariable.Tooltip))
                 dict.Add("Tooltip", (object) sharedVariable.Tooltip);
             if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
             {
                 dict.Add("PropertyMapping", (object) sharedVariable.PropertyMapping);
                 if (!object.Equals((object) sharedVariable.PropertyMappingOwner, (object) null))
                 {
                     dict.Add("PropertyMappingOwner", (object) unityObjects.Count);
                     unityObjects.Add((Object) sharedVariable.PropertyMappingOwner);
                 }
             }
             JSONSerialization.SerializeFields((object) sharedVariable, ref dict, ref unityObjects);
             return dict;
         }
        private static Dictionary<string, object>[] SerializeVariables(List<SharedVariable> variables, ref List<UnityEngine.Object> unityObjects)
        {
            Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[variables.Count];
            for (int i = 0; i < variables.Count; i++)
            {
                dictionaryArray[i] = SerializeVariable(variables[i], ref unityObjects);
            }
            return dictionaryArray;
        }
    }
}

