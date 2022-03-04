// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ObjectDrawerUtility
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BehaviorDesigner.Editor
{
  internal static class ObjectDrawerUtility
  {
    private static Dictionary<Type, Type> objectDrawerTypeMap = new Dictionary<Type, Type>();
    private static Dictionary<int, ObjectDrawer> objectDrawerMap = new Dictionary<int, ObjectDrawer>();
    private static bool mapBuilt = false;

    private static void BuildObjectDrawers()
    {
      if (ObjectDrawerUtility.mapBuilt)
        return;
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (!(assembly == (Assembly) null))
        {
          try
          {
            foreach (Type exportedType in assembly.GetExportedTypes())
            {
              if (typeof (ObjectDrawer).IsAssignableFrom(exportedType) && exportedType.IsClass && !exportedType.IsAbstract)
              {
                CustomObjectDrawer[] customAttributes;
                if ((customAttributes = exportedType.GetCustomAttributes(typeof (CustomObjectDrawer), false) as CustomObjectDrawer[]).Length > 0)
                  ObjectDrawerUtility.objectDrawerTypeMap.Add(customAttributes[0].Type, exportedType);
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
      }
      ObjectDrawerUtility.mapBuilt = true;
    }

    private static bool ObjectDrawerForType(
      Type type,
      ref ObjectDrawer objectDrawer,
      ref Type objectDrawerType,
      int hash)
    {
      ObjectDrawerUtility.BuildObjectDrawers();
      if (!ObjectDrawerUtility.objectDrawerTypeMap.ContainsKey(type))
        return false;
      objectDrawerType = ObjectDrawerUtility.objectDrawerTypeMap[type];
      if (ObjectDrawerUtility.objectDrawerMap.ContainsKey(hash))
        objectDrawer = ObjectDrawerUtility.objectDrawerMap[hash];
      return true;
    }

    public static ObjectDrawer GetObjectDrawer(Task task, FieldInfo field)
    {
      ObjectDrawer objectDrawer = (ObjectDrawer) null;
      Type objectDrawerType = (Type) null;
      if (!ObjectDrawerUtility.ObjectDrawerForType(field.FieldType, ref objectDrawer, ref objectDrawerType, (task == null ? 0 : task.GetHashCode()) + field.GetHashCode()))
        return (ObjectDrawer) null;
      if (objectDrawer == null)
      {
        objectDrawer = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
        ObjectDrawerUtility.objectDrawerMap.Add((task == null ? 0 : task.GetHashCode()) + field.GetHashCode(), objectDrawer);
      }
      objectDrawer.FieldInfo = field;
      objectDrawer.Task = task;
      return objectDrawer;
    }

    public static ObjectDrawer GetObjectDrawer(
      Task task,
      FieldInfo field,
      ObjectDrawerAttribute attribute)
    {
      ObjectDrawer objectDrawer = (ObjectDrawer) null;
      Type objectDrawerType = (Type) null;
      if (!ObjectDrawerUtility.ObjectDrawerForType(attribute.GetType(), ref objectDrawer, ref objectDrawerType, (task == null ? 0 : task.GetHashCode()) + field.GetHashCode() + attribute.GetHashCode()))
        return (ObjectDrawer) null;
      if (objectDrawer != null)
      {
        objectDrawer.Task = task;
        return objectDrawer;
      }
      objectDrawer = Activator.CreateInstance(objectDrawerType) as ObjectDrawer;
      objectDrawer.Attribute = attribute;
      objectDrawer.Task = task;
      objectDrawer.FieldInfo = field;
      ObjectDrawerUtility.objectDrawerMap.Add((task == null ? 0 : task.GetHashCode()) + field.GetHashCode() + attribute.GetHashCode(), objectDrawer);
      return objectDrawer;
    }
  }
}
