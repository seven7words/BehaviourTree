// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.TaskCopier
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner.Editor
{
  public class TaskCopier : UnityEditor.Editor
  {
    public static TaskSerializer CopySerialized(Task task)
    {
      TaskSerializer taskSerializer = new TaskSerializer()
      {
        offset = (task.NodeData.NodeDesigner as NodeDesigner).GetAbsolutePosition() + new Vector2(10f, 10f),
        unityObjects = new List<Object>()
      };
      taskSerializer.serialization = MiniJSON.Serialize(JSONSerialization.SerializeTask(task, false, ref taskSerializer.unityObjects));
      return taskSerializer;
    }

    public static Task PasteTask(BehaviorSource behaviorSource, TaskSerializer serializer)
    {
      Dictionary<int, Task> IDtoTask = new Dictionary<int, Task>();
      JSONDeserialization.TaskIDs = new Dictionary<JSONDeserialization.TaskField, List<int>>();
      Task task1 = JSONDeserialization.DeserializeTask(behaviorSource, MiniJSON.Deserialize(serializer.serialization) as Dictionary<string, object>, ref IDtoTask, serializer.unityObjects);
      TaskCopier.CheckSharedVariables(behaviorSource, task1);
      if (JSONDeserialization.TaskIDs.Count > 0)
      {
        foreach (JSONDeserialization.TaskField key in JSONDeserialization.TaskIDs.Keys)
        {
          List<int> taskId = JSONDeserialization.TaskIDs[key];
          System.Type fieldType = key.fieldInfo.FieldType;
          if (key.fieldInfo.FieldType.IsArray)
          {
            int length = 0;
            for (int index = 0; index < taskId.Count; ++index)
            {
              Task task2 = TaskCopier.TaskWithID(behaviorSource, taskId[index]);
              if (task2 != null && (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType())))
                ++length;
            }
            Array instance = Array.CreateInstance(fieldType.GetElementType(), length);
            int index1 = 0;
            for (int index2 = 0; index2 < taskId.Count; ++index2)
            {
              Task task2 = TaskCopier.TaskWithID(behaviorSource, taskId[index2]);
              if (task2 != null && (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType())))
              {
                instance.SetValue(task2, index1);
                ++index1;
              }
            }
            key.fieldInfo.SetValue(key.task, instance);
          }
          else
          {
            Task task2 = TaskCopier.TaskWithID(behaviorSource, taskId[0]);
            if (task2 != null && (task2.GetType().Equals(key.fieldInfo.FieldType) || task2.GetType().IsSubclassOf(key.fieldInfo.FieldType)))
              key.fieldInfo.SetValue(key.task, task2);
          }
        }
        JSONDeserialization.TaskIDs = (Dictionary<JSONDeserialization.TaskField, List<int>>) null;
      }
      return task1;
    }

    private static void CheckSharedVariables(BehaviorSource behaviorSource, Task task)
    {
      if (task == null)
        return;
      TaskCopier.CheckSharedVariableFields(behaviorSource, task, task, new HashSet<object>());
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.Children == null)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        TaskCopier.CheckSharedVariables(behaviorSource, parentTask.Children[index]);
    }

    private static void CheckSharedVariableFields(
      BehaviorSource behaviorSource,
      Task task,
      object obj,
      HashSet<object> visitedObjects)
    {
      if (obj == null || visitedObjects.Contains(obj))
        return;
      visitedObjects.Add(obj);
      FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(obj.GetType());
      for (int index = 0; index < serializableFields.Length; ++index)
      {
        if (typeof (SharedVariable).IsAssignableFrom(serializableFields[index].FieldType))
        {
          if (serializableFields[index].GetValue(obj) is SharedVariable sharedVariable)
          {
            if (sharedVariable.IsShared && !sharedVariable.IsGlobal && (!string.IsNullOrEmpty(sharedVariable.Name) && behaviorSource.GetVariable(sharedVariable.Name) == null))
              behaviorSource.SetVariable(sharedVariable.Name, sharedVariable);
            TaskCopier.CheckSharedVariableFields(behaviorSource, task, sharedVariable, visitedObjects);
          }
        }
        else if (serializableFields[index].FieldType.IsClass && !serializableFields[index].FieldType.Equals(typeof (System.Type)) && !typeof (Delegate).IsAssignableFrom(serializableFields[index].FieldType))
          TaskCopier.CheckSharedVariableFields(behaviorSource, task, serializableFields[index].GetValue(obj), visitedObjects);
      }
    }

    private static Task TaskWithID(BehaviorSource behaviorSource, int id)
    {
      Task task = (Task) null;
      if (behaviorSource.RootTask != null)
        task = TaskCopier.TaskWithID(id, behaviorSource.RootTask);
      if (task == null && behaviorSource.DetachedTasks != null)
      {
        int index = 0;
        while (index < behaviorSource.DetachedTasks.Count && (task = TaskCopier.TaskWithID(id, behaviorSource.DetachedTasks[index])) == null)
          ++index;
      }
      return task;
    }

    private static Task TaskWithID(int id, Task task)
    {
      if (task == null)
        return (Task) null;
      if (task.ID == id)
        return task;
      if (task is ParentTask)
      {
        ParentTask parentTask = task as ParentTask;
        if (parentTask.Children != null)
        {
          for (int index = 0; index < parentTask.Children.Count; ++index)
          {
            Task task1 = TaskCopier.TaskWithID(id, parentTask.Children[index]);
            if (task1 != null)
              return task1;
          }
        }
      }
      return (Task) null;
    }
  }
}
