namespace BehaviorDesigner.Runtime
{
    using System;
    using System.Collections.Generic;

    public static class ObjectPool
    {
        private static Dictionary<Type, object> poolDictionary = new Dictionary<Type, object>();
        private static object lockObject = new object();

        public static void Clear()
        {
            object lockObject = ObjectPool.lockObject;
            lock (lockObject)
            {
                poolDictionary.Clear();
            }
        }

        public static T Get<T>()
        {
            object lockObject = ObjectPool.lockObject;
            lock (lockObject)
            {
                if (poolDictionary.ContainsKey(typeof(T)))
                {
                    Stack<T> stack = poolDictionary[typeof(T)] as Stack<T>;
                    if (stack.Count > 0)
                    {
                        return stack.Pop();
                    }
                }
                return (T) TaskUtility.CreateInstance(typeof(T));
            }
        }

        public static void Return<T>(T obj)
        {
            if (obj != null)
            {
                object lockObject = ObjectPool.lockObject;
                lock (lockObject)
                {
                    object obj3;
                    if (poolDictionary.TryGetValue(typeof(T), out obj3))
                    {
                        (obj3 as Stack<T>).Push(obj);
                    }
                    else
                    {
                        Stack<T> stack2 = new Stack<T>();
                        stack2.Push(obj);
                        poolDictionary.Add(typeof(T), stack2);
                    }
                }
            }
        }
    }
}

