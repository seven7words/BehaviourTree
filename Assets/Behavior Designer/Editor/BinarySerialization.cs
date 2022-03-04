﻿// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.BinarySerialization
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner.Editor
{
  public class BinarySerialization
  {
    private static int fieldIndex;
    private static TaskSerializationData taskSerializationData;
    private static FieldSerializationData fieldSerializationData;
    private static HashSet<int> fieldHashes = new HashSet<int>();

    public static void Save(BehaviorSource behaviorSource)
    {
      BinarySerialization.fieldIndex = 0;
      BinarySerialization.taskSerializationData = new TaskSerializationData();
      BinarySerialization.fieldSerializationData = BinarySerialization.taskSerializationData.fieldSerializationData;
      if (behaviorSource.Variables != null)
      {
        for (int index = 0; index < behaviorSource.Variables.Count; ++index)
        {
          BinarySerialization.taskSerializationData.variableStartIndex.Add(BinarySerialization.fieldSerializationData.startIndex.Count);
          BinarySerialization.SaveSharedVariable(behaviorSource.Variables[index], 0);
        }
      }
      if (!object.ReferenceEquals((object) behaviorSource.EntryTask, (object) null))
        BinarySerialization.SaveTask(behaviorSource.EntryTask, -1);
      if (!object.ReferenceEquals((object) behaviorSource.RootTask, (object) null))
        BinarySerialization.SaveTask(behaviorSource.RootTask, 0);
      if (behaviorSource.DetachedTasks != null)
      {
        for (int index = 0; index < behaviorSource.DetachedTasks.Count; ++index)
          BinarySerialization.SaveTask(behaviorSource.DetachedTasks[index], -1);
      }
      BinarySerialization.taskSerializationData.Version = "1.6.8";
      BinarySerialization.taskSerializationData.fieldSerializationData.byteDataArray = BinarySerialization.taskSerializationData.fieldSerializationData.byteData.ToArray();
      BinarySerialization.taskSerializationData.fieldSerializationData.byteData = (List<byte>) null;
      behaviorSource.TaskData = BinarySerialization.taskSerializationData;
      if (behaviorSource.Owner == null || behaviorSource.Owner.Equals((object) null))
        return;
      BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner.GetObject());
    }

    public static void Save(GlobalVariables globalVariables)
    {
      if ((Object) globalVariables == (Object) null)
        return;
      BinarySerialization.fieldIndex = 0;
      globalVariables.VariableData = new VariableSerializationData();
      if (globalVariables.Variables == null || globalVariables.Variables.Count == 0)
        return;
      BinarySerialization.fieldSerializationData = globalVariables.VariableData.fieldSerializationData;
      for (int index = 0; index < globalVariables.Variables.Count; ++index)
      {
        globalVariables.VariableData.variableStartIndex.Add(BinarySerialization.fieldSerializationData.startIndex.Count);
        BinarySerialization.SaveSharedVariable(globalVariables.Variables[index], 0);
      }
      globalVariables.Version = "1.6.8";
      globalVariables.VariableData.fieldSerializationData.byteDataArray = globalVariables.VariableData.fieldSerializationData.byteData.ToArray();
      globalVariables.VariableData.fieldSerializationData.byteData = (List<byte>) null;
      BehaviorDesignerUtility.SetObjectDirty((Object) globalVariables);
    }

    private static void SaveTask(Task task, int parentTaskIndex)
    {
      BinarySerialization.taskSerializationData.types.Add(task.GetType().ToString());
      BinarySerialization.taskSerializationData.parentIndex.Add(parentTaskIndex);
      BinarySerialization.taskSerializationData.startIndex.Add(BinarySerialization.fieldSerializationData.startIndex.Count);
      BinarySerialization.SaveField(typeof (int), "ID", 0, (object) task.ID);
      BinarySerialization.SaveField(typeof (string), "FriendlyName", 0, (object) task.FriendlyName);
      BinarySerialization.SaveField(typeof (bool), "IsInstant", 0, (object) task.IsInstant);
      BinarySerialization.SaveField(typeof (bool), "Disabled", 0, (object) task.Disabled);
      BinarySerialization.SaveNodeData(task.NodeData);
      BinarySerialization.SaveFields((object) task, 0);
      if (!(task is ParentTask))
        return;
      ParentTask parentTask = task as ParentTask;
      if (parentTask.Children == null || parentTask.Children.Count <= 0)
        return;
      for (int index = 0; index < parentTask.Children.Count; ++index)
        BinarySerialization.SaveTask(parentTask.Children[index], parentTask.ID);
    }

    private static void SaveNodeData(NodeData nodeData)
    {
      BinarySerialization.SaveField(typeof (Vector2), "NodeDataOffset", 0, (object) nodeData.Offset);
      BinarySerialization.SaveField(typeof (string), "NodeDataComment", 0, (object) nodeData.Comment);
      BinarySerialization.SaveField(typeof (bool), "NodeDataIsBreakpoint", 0, (object) nodeData.IsBreakpoint);
      BinarySerialization.SaveField(typeof (bool), "NodeDataCollapsed", 0, (object) nodeData.Collapsed);
      BinarySerialization.SaveField(typeof (int), "NodeDataColorIndex", 0, (object) nodeData.ColorIndex);
      BinarySerialization.SaveField(typeof (List<string>), "NodeDataWatchedFields", 0, (object) nodeData.WatchedFieldNames);
    }

    private static void SaveSharedVariable(SharedVariable sharedVariable, int hashPrefix)
    {
      if (sharedVariable == null)
        return;
      BinarySerialization.SaveField(typeof (string), "Type", hashPrefix, (object) sharedVariable.GetType().ToString());
      BinarySerialization.SaveField(typeof (string), "Name", hashPrefix, (object) sharedVariable.Name);
      if (sharedVariable.IsShared)
        BinarySerialization.SaveField(typeof (bool), "IsShared", hashPrefix, (object) sharedVariable.IsShared);
      if (sharedVariable.IsGlobal)
        BinarySerialization.SaveField(typeof (bool), "IsGlobal", hashPrefix, (object) sharedVariable.IsGlobal);
      if (sharedVariable.IsDynamic)
        BinarySerialization.SaveField(typeof (bool), "IsDynamic", hashPrefix, (object) sharedVariable.IsDynamic);
      if (!string.IsNullOrEmpty(sharedVariable.Tooltip))
        BinarySerialization.SaveField(typeof (string), "Tooltip", hashPrefix, (object) sharedVariable.Tooltip);
      if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
      {
        BinarySerialization.SaveField(typeof (string), "PropertyMapping", hashPrefix, (object) sharedVariable.PropertyMapping);
        if (!object.Equals((object) sharedVariable.PropertyMappingOwner, (object) null))
          BinarySerialization.SaveField(typeof (GameObject), "PropertyMappingOwner", hashPrefix, (object) sharedVariable.PropertyMappingOwner);
      }
      BinarySerialization.SaveFields((object) sharedVariable, hashPrefix);
    }

    private static void SaveFields(object obj, int hashPrefix)
    {
      BinarySerialization.fieldHashes.Clear();
      FieldInfo[] allFields = TaskUtility.GetAllFields(obj.GetType());
      for (int index = 0; index < allFields.Length; ++index)
      {
        if (!BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (NonSerializedAttribute)) && (!allFields[index].IsPrivate && !allFields[index].IsFamily || BehaviorDesignerUtility.HasAttribute(allFields[index], typeof (SerializeField))) && (!(obj is ParentTask) || !allFields[index].Name.Equals("children")))
        {
          object objA = allFields[index].GetValue(obj);
          if (!object.ReferenceEquals(objA, (object) null))
            BinarySerialization.SaveField(allFields[index].FieldType, allFields[index].Name, hashPrefix, objA, allFields[index]);
        }
      }
    }

    private static void SaveField(
      Type fieldType,
      string fieldName,
      int hashPrefix,
      object value,
      FieldInfo fieldInfo = null)
    {
      int hashPrefix1 = hashPrefix + fieldType.Name.GetHashCode()+ fieldName.GetHashCode();
      
      if (BinarySerialization.fieldHashes.Contains(hashPrefix1))
        return;
      BinarySerialization.fieldHashes.Add(hashPrefix1);
      BinarySerialization.fieldSerializationData.fieldNameHash.Add(hashPrefix1);
      BinarySerialization.fieldSerializationData.startIndex.Add(BinarySerialization.fieldIndex);
      if (typeof (IList).IsAssignableFrom(fieldType))
      {
        Type fieldType1;
        if (fieldType.IsArray)
        {
          fieldType1 = fieldType.GetElementType();
        }
        else
        {
          Type type = fieldType;
          while (!type.IsGenericType)
            type = type.BaseType;
          fieldType1 = type.GetGenericArguments()[0];
        }
        if (!(value is IList list2))
        {
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(0));
        }
        else
        {
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(list2.Count));
          if (list2.Count <= 0)
            return;
          for (int index = 0; index < list2.Count; ++index)
          {
            if (object.ReferenceEquals(list2[index], (object) null))
              BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(-1));
            else
              BinarySerialization.SaveField(fieldType1, index.ToString(), hashPrefix1 / (index + 1), list2[index], fieldInfo);
          }
        }
      }
      else if (typeof (Task).IsAssignableFrom(fieldType))
      {
        if (fieldInfo != (FieldInfo) null && BehaviorDesignerUtility.HasAttribute(fieldInfo, typeof (InspectTaskAttribute)))
        {
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.StringToBytes(value.GetType().ToString()));
          BinarySerialization.SaveFields(value, hashPrefix1);
        }
        else
          BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes((value as Task).ID));
      }
      else if (typeof (SharedVariable).IsAssignableFrom(fieldType))
        BinarySerialization.SaveSharedVariable(value as SharedVariable, hashPrefix1);
      else if (typeof (Object).IsAssignableFrom(fieldType))
      {
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(BinarySerialization.fieldSerializationData.unityObjects.Count));
        BinarySerialization.fieldSerializationData.unityObjects.Add(value as Object);
      }
      else if (fieldType.Equals(typeof (int)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes((int) value));
      else if (fieldType.Equals(typeof (short)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.Int16ToBytes((short) value));
      else if (fieldType.Equals(typeof (uint)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.UIntToBytes((uint) value));
      else if (fieldType.Equals(typeof (float)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.FloatToBytes((float) value));
      else if (fieldType.Equals(typeof (double)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.DoubleToBytes((double) value));
      else if (fieldType.Equals(typeof (long)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.LongToBytes((long) value));
      else if (fieldType.Equals(typeof (bool)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.BoolToBytes((bool) value));
      else if (fieldType.Equals(typeof (string)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.StringToBytes((string) value));
      else if (fieldType.Equals(typeof (byte)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.ByteToBytes((byte) value));
      else if (fieldType.IsEnum)
        BinarySerialization.SaveField(Enum.GetUnderlyingType(fieldType), fieldName, hashPrefix1, value, fieldInfo);
      else if (fieldType.Equals(typeof (Vector2)))
        BinarySerialization.AddByteData(BinarySerialization.Vector2ToBytes((Vector2) value));
      else if (fieldType.Equals(typeof (Vector2Int)))
        BinarySerialization.AddByteData(BinarySerialization.Vector2IntToBytes((Vector2Int) value));
      else if (fieldType.Equals(typeof (Vector3)))
        BinarySerialization.AddByteData(BinarySerialization.Vector3ToBytes((Vector3) value));
      else if (fieldType.Equals(typeof (Vector3Int)))
        BinarySerialization.AddByteData(BinarySerialization.Vector3IntToBytes((Vector3Int) value));
      else if (fieldType.Equals(typeof (Vector4)))
        BinarySerialization.AddByteData(BinarySerialization.Vector4ToBytes((Vector4) value));
      else if (fieldType.Equals(typeof (Quaternion)))
        BinarySerialization.AddByteData(BinarySerialization.QuaternionToBytes((Quaternion) value));
      else if (fieldType.Equals(typeof (Color)))
        BinarySerialization.AddByteData(BinarySerialization.ColorToBytes((Color) value));
      else if (fieldType.Equals(typeof (Rect)))
        BinarySerialization.AddByteData(BinarySerialization.RectToBytes((Rect) value));
      else if (fieldType.Equals(typeof (Matrix4x4)))
        BinarySerialization.AddByteData(BinarySerialization.Matrix4x4ToBytes((Matrix4x4) value));
      else if (fieldType.Equals(typeof (LayerMask)))
        BinarySerialization.AddByteData((ICollection<byte>) BinarySerialization.IntToBytes(((LayerMask) value).value));
      else if (fieldType.Equals(typeof (AnimationCurve)))
        BinarySerialization.AddByteData(BinarySerialization.AnimationCurveToBytes((AnimationCurve) value));
      else if (fieldType.IsClass || fieldType.IsValueType && !fieldType.IsPrimitive)
      {
        if (object.ReferenceEquals(value, (object) null))
          value = Activator.CreateInstance(fieldType, true);
        BinarySerialization.SaveFields(value, hashPrefix1);
      }
      else
        Debug.LogError((object) ("Missing Serialization for " + (object) fieldType));
    }

    private static byte[] IntToBytes(int value) => BitConverter.GetBytes(value);

    private static byte[] Int16ToBytes(short value) => BitConverter.GetBytes(value);

    private static byte[] UIntToBytes(uint value) => BitConverter.GetBytes(value);

    private static byte[] FloatToBytes(float value) => BitConverter.GetBytes(value);

    private static byte[] DoubleToBytes(double value) => BitConverter.GetBytes(value);

    private static byte[] LongToBytes(long value) => BitConverter.GetBytes(value);

    private static byte[] BoolToBytes(bool value) => BitConverter.GetBytes(value);

    private static byte[] StringToBytes(string str)
    {
      if (str == null)
        str = string.Empty;
      return Encoding.UTF8.GetBytes(str);
    }

    private static byte[] ByteToBytes(byte value) => new byte[1]
    {
      value
    };

    private static ICollection<byte> ColorToBytes(Color color)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(color.r));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(color.g));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(color.b));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(color.a));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector2ToBytes(Vector2 vector2)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector2.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector2.y));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector2IntToBytes(Vector2Int vector2)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector2.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector2.y));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector3ToBytes(Vector3 vector3)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector3.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector3.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector3.z));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector3IntToBytes(Vector3Int vector3)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector3.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector3.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector3.z));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Vector4ToBytes(Vector4 vector4)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector4.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector4.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector4.z));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(vector4.w));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> QuaternionToBytes(Quaternion quaternion)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(quaternion.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(quaternion.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(quaternion.z));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(quaternion.w));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> RectToBytes(Rect rect)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(rect.x));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(rect.y));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(rect.width));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(rect.height));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> Matrix4x4ToBytes(Matrix4x4 matrix4x4)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m00));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m01));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m02));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m03));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m10));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m11));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m12));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m13));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m20));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m21));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m22));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m23));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m30));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m31));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m32));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(matrix4x4.m33));
      return (ICollection<byte>) byteList;
    }

    private static ICollection<byte> AnimationCurveToBytes(AnimationCurve animationCurve)
    {
      List<byte> byteList = new List<byte>();
      Keyframe[] keys = animationCurve.keys;
      if (keys != null)
      {
        byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(keys.Length));
        for (int index = 0; index < keys.Length; ++index)
        {
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(keys[index].time));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(keys[index].value));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(keys[index].inTangent));
          byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(keys[index].outTangent));
        }
      }
      else
        byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes(0));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((int) animationCurve.preWrapMode));
      byteList.AddRange((IEnumerable<byte>) BitConverter.GetBytes((int) animationCurve.postWrapMode));
      return (ICollection<byte>) byteList;
    }

    private static void AddByteData(ICollection<byte> bytes)
    {
      BinarySerialization.fieldSerializationData.dataPosition.Add(BinarySerialization.fieldSerializationData.byteData.Count);
      if (bytes != null)
        BinarySerialization.fieldSerializationData.byteData.AddRange((IEnumerable<byte>) bytes);
      ++BinarySerialization.fieldIndex;
    }
  }
}
