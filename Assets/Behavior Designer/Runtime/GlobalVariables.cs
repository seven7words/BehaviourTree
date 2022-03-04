namespace BehaviorDesigner.Runtime
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GlobalVariables : ScriptableObject, IVariableSource
    {
        private static GlobalVariables instance;
        [SerializeField]
        private List<SharedVariable> mVariables;
        private Dictionary<string, int> mSharedVariableIndex;
        [SerializeField]
        private VariableSerializationData mVariableData;
        [SerializeField]
        private string mVersion;

        public void CheckForSerialization(bool force)
        {
            if (force || ((this.mVariables == null) || ((this.mVariables.Count > 0) && (this.mVariables[0] == null))))
            {
                if ((this.VariableData != null) && !string.IsNullOrEmpty(this.VariableData.JSONSerialization))
                {
                    JSONDeserialization.Load(this.VariableData.JSONSerialization, this, this.mVersion);
                }
                else
                {
                    //TODO:???
                    // BinaryDeserialization.Load(this, this.mVersion);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod()]
        private static void DomainReset()
        {
            instance = null;
        }

        public List<SharedVariable> GetAllVariables()
        {
            this.CheckForSerialization(false);
            return this.mVariables;
        }

        public SharedVariable GetVariable(string name)
        {
            if (name != null)
            {
                this.CheckForSerialization(false);
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

        public void SetAllVariables(List<SharedVariable> variables)
        {
            this.mVariables = variables;
        }

        public void SetVariable(string name, SharedVariable sharedVariable)
        {
            int num;
            this.CheckForSerialization(false);
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
                else
                {
                    variable.SetValue(sharedVariable.GetValue());
                }
            }
        }

        public void SetVariableValue(string name, object value)
        {
            SharedVariable variable = this.GetVariable(name);
            if (variable != null)
            {
                variable.SetValue(value);
            }
        }

        public void UpdateVariableName(SharedVariable sharedVariable, string name)
        {
            this.CheckForSerialization(false);
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

        public static GlobalVariables Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load("BehaviorDesignerGlobalVariables", typeof(GlobalVariables)) as GlobalVariables;
                    if (instance != null)
                    {
                        instance.CheckForSerialization(false);
                    }
                }
                return instance;
            }
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

        public VariableSerializationData VariableData
        {
            get => 
                this.mVariableData;
            set => 
                this.mVariableData = value;
        }

        public string Version
        {
            get => 
                this.mVersion;
            set => 
                this.mVersion = value;
        }
    }
}

