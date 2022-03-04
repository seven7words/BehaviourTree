namespace BehaviorDesigner.Runtime
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public class TaskUtility
    {
        public static char[] TrimCharacters = new char[] { '/' };
        private static Dictionary<string, Type> typeLookup = new Dictionary<string, Type>();
        private static List<Assembly> loadedAssemblies = null;
        private static Dictionary<Type, FieldInfo[]> allFieldsLookup = new Dictionary<Type, FieldInfo[]>();
        private static Dictionary<Type, FieldInfo[]> serializableFieldsLookup = new Dictionary<Type, FieldInfo[]>();
        private static Dictionary<Type, FieldInfo[]> publicFieldsLookup = new Dictionary<Type, FieldInfo[]>();
        private static Dictionary<FieldInfo, Dictionary<Type, bool>> hasFieldLookup = new Dictionary<FieldInfo, Dictionary<Type, bool>>();

        public static bool CompareType(Type t, string typeName)
        {
            Type typeWithinAssembly = GetTypeWithinAssembly(typeName);
            return ((typeWithinAssembly != null) ? t.Equals(typeWithinAssembly) : false);
        }

        public static object CreateInstance(Type t)
        {
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t);
            }
            return Activator.CreateInstance(t, true);
        }

        public static FieldInfo[] GetAllFields(Type t)
        {
            FieldInfo[] infoArray = null;
            if (!allFieldsLookup.TryGetValue(t, out infoArray))
            {
                List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
                fieldList.Clear();
                GetFields(t, ref fieldList, 0x36);
                infoArray = fieldList.ToArray();
                ObjectPool.Return<List<FieldInfo>>(fieldList);
                allFieldsLookup.Add(t, infoArray);
            }
            return infoArray;
        }

        private static void GetFields(Type t, ref List<FieldInfo> fieldList, int flags)
        {
            if ((t != null) && (!t.Equals(typeof(ParentTask)) && (!t.Equals(typeof(Task)) && !t.Equals(typeof(SharedVariable)))))
            {
                FieldInfo[] fields = t.GetFields((BindingFlags) flags);
                for (int i = 0; i < fields.Length; i++)
                {
                    fieldList.Add(fields[i]);
                }
                GetFields(t.BaseType, ref fieldList, flags);
            }
        }

        public static FieldInfo[] GetPublicFields(Type t)
        {
            FieldInfo[] infoArray = null;
            if (!publicFieldsLookup.TryGetValue(t, out infoArray))
            {
                List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
                fieldList.Clear();
                GetFields(t, ref fieldList, 0x16);
                infoArray = fieldList.ToArray();
                ObjectPool.Return<List<FieldInfo>>(fieldList);
                publicFieldsLookup.Add(t, infoArray);
            }
            return infoArray;
        }

        public static FieldInfo[] GetSerializableFields(Type t)
        {
            FieldInfo[] infoArray = null;
            if (!serializableFieldsLookup.TryGetValue(t, out infoArray))
            {
                List<FieldInfo> fieldList = ObjectPool.Get<List<FieldInfo>>();
                fieldList.Clear();
                GetSerializableFields(t, fieldList, 0x36);
                infoArray = fieldList.ToArray();
                ObjectPool.Return<List<FieldInfo>>(fieldList);
                serializableFieldsLookup.Add(t, infoArray);
            }
            return infoArray;
        }

        private static void GetSerializableFields(Type t, IList<FieldInfo> fieldList, int flags)
        {
            if ((t != null) && (!t.Equals(typeof(ParentTask)) && (!t.Equals(typeof(Task)) && !t.Equals(typeof(SharedVariable)))))
            {
                FieldInfo[] fields = t.GetFields((BindingFlags) flags);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].IsPublic || HasAttribute(fields[i], typeof(SerializeField)))
                    {
                        fieldList.Add(fields[i]);
                    }
                }
                GetSerializableFields(t.BaseType, fieldList, flags);
            }
        }

        public static Type GetTypeWithinAssembly(string typeName)
        {
            Type type;
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }
            if (!typeLookup.TryGetValue(typeName, out type))
            {
                type = Type.GetType(typeName);
                if (type == null)
                {
                    if (loadedAssemblies == null)
                    {
                        loadedAssemblies = new List<Assembly>();
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        for (int j = 0; j < assemblies.Length; j++)
                        {
                            loadedAssemblies.Add(assemblies[j]);
                        }
                    }
                    for (int i = 0; i < loadedAssemblies.Count; i++)
                    {
                        type = loadedAssemblies[i].GetType(typeName);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
                if (type != null)
                {
                    typeLookup.Add(typeName, type);
                }
                else if (typeName.Contains("BehaviorDesigner.Runtime.Tasks.Basic"))
                {
                    return GetTypeWithinAssembly(typeName.Replace("BehaviorDesigner.Runtime.Tasks.Basic", "BehaviorDesigner.Runtime.Tasks.Unity"));
                }
            }
            return type;
        }

        public static bool HasAttribute(FieldInfo field, Type attribute)
        {
            bool flag;
            Dictionary<Type, bool> dictionary;
            if (field == null)
            {
                return false;
            }
            if (!hasFieldLookup.TryGetValue(field, out dictionary))
            {
                dictionary = new Dictionary<Type, bool>();
                hasFieldLookup.Add(field, dictionary);
            }
            if (!dictionary.TryGetValue(attribute, out flag))
            {
                flag = field.GetCustomAttributes(attribute, false).Length > 0;
                dictionary.Add(attribute, flag);
            }
            return flag;
        }
    }
}

