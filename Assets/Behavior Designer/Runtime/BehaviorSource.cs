namespace BehaviorDesigner.Runtime
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable]
    public class BehaviorSource : IVariableSource
    {
        public string behaviorName;
        public string behaviorDescription;
        private int behaviorID;
        private Task mEntryTask;
        private Task mRootTask;
        private List<Task> mDetachedTasks;
        private List<SharedVariable> mVariables;
        private Dictionary<string, int> mSharedVariableIndex;
        [NonSerialized]
        private bool mHasSerialized;
        [SerializeField]
        private TaskSerializationData mTaskData;
        [SerializeField]
        private IBehavior mOwner;

        public BehaviorSource()
        {
            this.behaviorName = "Behavior";
            this.behaviorDescription = string.Empty;
            this.behaviorID = -1;
        }

        public BehaviorSource(IBehavior owner)
        {
            this.behaviorName = "Behavior";
            this.behaviorDescription = string.Empty;
            this.behaviorID = -1;
            this.Initialize(owner);
        }

        public bool CheckForSerialization(bool force, BehaviorSource behaviorSource = null)
        {
            bool flag = (behaviorSource == null) ? this.HasSerialized : behaviorSource.HasSerialized;
            if ((this.mTaskData == null) || (flag && !force))
            {
                return false;
            }
            if (behaviorSource != null)
            {
                behaviorSource.HasSerialized = true;
            }
            else
            {
                this.HasSerialized = true;
            }
            if (!string.IsNullOrEmpty(this.mTaskData.JSONSerialization))
            {
                JSONDeserialization.Load(this.mTaskData, (behaviorSource != null) ? behaviorSource : this);
            }
            else
            {
                // BinaryDeserialization.Load(behaviorSource);
                //TODO:???
            }
            return true;
        }

        public List<SharedVariable> GetAllVariables()
        {
            this.CheckForSerialization(false, null);
            return this.mVariables;
        }

        public SharedVariable GetVariable(string name)
        {
            if (name != null)
            {
                this.CheckForSerialization(false, null);
                if (this.mVariables != null)
                {
                    int num;
                    if ((this.mSharedVariableIndex == null) || (this.mSharedVariableIndex.Count != this.mVariables.Count))
                    {
                        this.UpdateVariablesIndex();
                    }
                    if (this.mSharedVariableIndex.TryGetValue(name, out num))
                    {
                        return this.mVariables[num];
                    }
                }
            }
            return null;
        }

        public void Initialize(IBehavior owner)
        {
            this.mOwner = owner;
        }

        public void Load(out Task entryTask, out Task rootTask, out List<Task> detachedTasks)
        {
            entryTask = this.mEntryTask;
            rootTask = this.mRootTask;
            detachedTasks = this.mDetachedTasks;
        }

        public void Save(Task entryTask, Task rootTask, List<Task> detachedTasks)
        {
            this.mEntryTask = entryTask;
            this.mRootTask = rootTask;
            this.mDetachedTasks = detachedTasks;
        }

        public void SetAllVariables(List<SharedVariable> variables)
        {
            this.mVariables = variables;
            this.UpdateVariablesIndex();
        }

        public void SetVariable(string name, SharedVariable sharedVariable)
        {
            int num;
            if (this.mVariables == null)
            {
                this.mVariables = new List<SharedVariable>();
            }
            else if (this.mSharedVariableIndex == null)
            {
                this.UpdateVariablesIndex();
            }
            sharedVariable.Name = name;
            if ((this.mSharedVariableIndex == null) || !this.mSharedVariableIndex.TryGetValue(name, out num))
            {
                this.mVariables.Add(sharedVariable);
                this.UpdateVariablesIndex();
            }
            else
            {
                SharedVariable variable = this.mVariables[num];
                if (!variable.GetType().Equals(typeof(SharedVariable)) && !variable.GetType().Equals(sharedVariable.GetType()))
                {
                    Debug.LogError($"Error: Unable to set SharedVariable {name} - the variable type {variable.GetType()} does not match the existing type {sharedVariable.GetType()}");
                }
                else if (string.IsNullOrEmpty(sharedVariable.PropertyMapping))
                {
                    variable.SetValue(sharedVariable.GetValue());
                }
                else
                {
                    variable.PropertyMappingOwner = sharedVariable.PropertyMappingOwner;
                    variable.PropertyMapping = sharedVariable.PropertyMapping;
                    variable.InitializePropertyMapping(this);
                }
            }
        }

        public override string ToString() => 
            (((this.mOwner == null) || (this.mOwner.GetObject() == null)) ? this.behaviorName : (!string.IsNullOrEmpty(this.behaviorName) ? $"{this.Owner.GetOwnerName()} - {this.behaviorName}" : this.Owner.GetOwnerName()));

        public void UpdateVariableName(SharedVariable sharedVariable, string name)
        {
            this.CheckForSerialization(false, null);
            sharedVariable.Name = name;
            this.UpdateVariablesIndex();
        }

        private void UpdateVariablesIndex()
        {
            if (this.mVariables == null)
            {
                if (this.mSharedVariableIndex != null)
                {
                    this.mSharedVariableIndex = null;
                }
            }
            else
            {
                if (this.mSharedVariableIndex == null)
                {
                    this.mSharedVariableIndex = new Dictionary<string, int>(this.mVariables.Count);
                }
                else
                {
                    this.mSharedVariableIndex.Clear();
                }
                for (int i = 0; i < this.mVariables.Count; i++)
                {
                    if (this.mVariables[i] != null)
                    {
                        this.mSharedVariableIndex.Add(this.mVariables[i].Name, i);
                    }
                }
            }
        }

        public int BehaviorID
        {
            get
            {
                return behaviorID;
            }
            set => behaviorID = value;
        }

        public Task EntryTask
        {
            get => 
                this.mEntryTask;
            set => this.mEntryTask = value;
        }

        public Task RootTask
        {
            get => 
                this.mRootTask;
            set => 
                this.mRootTask = value;
        }

        public List<Task> DetachedTasks
        {
            get => 
                this.mDetachedTasks;
            set => 
                this.mDetachedTasks = value;
        }

        public List<SharedVariable> Variables
        {
            get => 
                this.mVariables;
            set
            {
                this.mVariables = value;
                this.UpdateVariablesIndex();
            }
        }

        public bool HasSerialized
        {
            get => 
                this.mHasSerialized;
            set => 
                this.mHasSerialized = value;
        }

        public TaskSerializationData TaskData
        {
            get => 
                this.mTaskData;
            set => this.mTaskData = value;
        }

        public IBehavior Owner
        {
            get => 
                this.mOwner;
            set => 
                this.mOwner = value;
        }
    }
}

