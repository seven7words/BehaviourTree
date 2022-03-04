namespace BehaviorDesigner.Runtime
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public abstract class ExternalBehavior : ScriptableObject, IBehavior
    {
        [SerializeField]
        private BehaviorDesigner.Runtime.BehaviorSource mBehaviorSource;
        private bool mInitialized;

        protected ExternalBehavior()
        {
        }

        int IBehavior.GetInstanceID() => 
            base.GetInstanceID();

        private void CheckForSerialization()
        {
            this.mBehaviorSource.Owner = this;
            this.mBehaviorSource.CheckForSerialization(false, null);
        }

        public T FindTask<T>() where T: Task
        {
            this.CheckForSerialization();
            return this.FindTask<T>(this.mBehaviorSource.RootTask);
        }

        private T FindTask<T>(Task task) where T: Task
        {
            ParentTask task2;
            if (task.GetType().Equals(typeof(T)))
            {
                return (T) task;
            }
            if (((task2 = task as ParentTask) != null) && (task2.Children != null))
            {
                for (int i = 0; i < task2.Children.Count; i++)
                {
                    T local = null;
                    local = this.FindTask<T>(task2.Children[i]);
                    if (local != null)
                    {
                        return local;
                    }
                }
            }
            return null;
        }

        public List<T> FindTasks<T>() where T: Task
        {
            this.CheckForSerialization();
            List<T> taskList = new List<T>();
            this.FindTasks<T>(this.mBehaviorSource.RootTask, ref taskList);
            return taskList;
        }

        private void FindTasks<T>(Task task, ref List<T> taskList) where T: Task
        {
            ParentTask task2;
            if (typeof(T).IsAssignableFrom(task.GetType()))
            {
                taskList.Add((T) task);
            }
            if (((task2 = task as ParentTask) != null) && (task2.Children != null))
            {
                for (int i = 0; i < task2.Children.Count; i++)
                {
                    this.FindTasks<T>(task2.Children[i], ref taskList);
                }
            }
        }

        public List<Task> FindTasksWithName(string taskName)
        {
            this.CheckForSerialization();
            List<Task> taskList = new List<Task>();
            this.FindTasksWithName(taskName, this.mBehaviorSource.RootTask, ref taskList);
            return taskList;
        }

        private void FindTasksWithName(string taskName, Task task, ref List<Task> taskList)
        {
            ParentTask task2;
            if (task.FriendlyName.Equals(taskName))
            {
                taskList.Add(task);
            }
            if (((task2 = task as ParentTask) != null) && (task2.Children != null))
            {
                for (int i = 0; i < task2.Children.Count; i++)
                {
                    this.FindTasksWithName(taskName, task2.Children[i], ref taskList);
                }
            }
        }

        public Task FindTaskWithName(string taskName)
        {
            this.CheckForSerialization();
            return this.FindTaskWithName(taskName, this.mBehaviorSource.RootTask);
        }

        private Task FindTaskWithName(string taskName, Task task)
        {
            ParentTask task2;
            if (task.FriendlyName.Equals(taskName))
            {
                return task;
            }
            if (((task2 = task as ParentTask) != null) && (task2.Children != null))
            {
                for (int i = 0; i < task2.Children.Count; i++)
                {
                    Task task3 = null;
                    task3 = this.FindTaskWithName(taskName, task2.Children[i]);
                    if (task3 != null)
                    {
                        return task3;
                    }
                }
            }
            return null;
        }

        public BehaviorDesigner.Runtime.BehaviorSource GetBehaviorSource() => 
            this.mBehaviorSource;

        public UnityEngine.Object GetObject() => 
            this;

        public string GetOwnerName() => 
            base.name;

        public SharedVariable GetVariable(string name)
        {
            this.CheckForSerialization();
            return this.mBehaviorSource.GetVariable(name);
        }

        public void Init()
        {
            this.CheckForSerialization();
            this.mInitialized = true;
        }

        public void SetBehaviorSource(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            this.mBehaviorSource = behaviorSource;
        }

        public void SetVariable(string name, SharedVariable item)
        {
            this.CheckForSerialization();
            this.mBehaviorSource.SetVariable(name, item);
        }

        public void SetVariableValue(string name, object value)
        {
            SharedVariable variable = this.GetVariable(name);
            if (variable != null)
            {
                variable.SetValue(value);
            }
        }

        public BehaviorDesigner.Runtime.BehaviorSource BehaviorSource
        {
            get => 
                this.mBehaviorSource;
            set => 
                this.mBehaviorSource = value;
        }

        public bool Initialized =>
            this.mInitialized;
    }
}

