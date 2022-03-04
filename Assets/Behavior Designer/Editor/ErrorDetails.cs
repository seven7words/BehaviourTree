// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ErrorDetails
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    [Serializable]
    public class ErrorDetails
    {
        [SerializeField]
        private ErrorDetails.ErrorType mType;
        [SerializeField]
        private NodeDesigner mNodeDesigner;
        [SerializeField]
        private string mTaskFriendlyName;
        [SerializeField]
        private string mTaskType;
        [SerializeField]
        private string mFieldName;

        public ErrorDetails(ErrorDetails.ErrorType type, Task task, string fieldName)
        {
            this.mType = type;
            if (task != null)
            {
                this.mNodeDesigner = task.NodeData.NodeDesigner as NodeDesigner;
                this.mTaskFriendlyName = task.FriendlyName;
                this.mTaskType = task.GetType().ToString();
            }
            this.mFieldName = fieldName;
        }

        public ErrorDetails.ErrorType Type => this.mType;

        public NodeDesigner NodeDesigner => this.mNodeDesigner;

        public string TaskFriendlyName => this.mTaskFriendlyName;

        public string TaskType => this.mTaskType;

        public string FieldName => this.mFieldName;

        public enum ErrorType
        {
            RequiredField,
            SharedVariable,
            NonUniqueDynamicVariable,
            MissingChildren,
            UnknownTask,
            InvalidTaskReference,
            InvalidVariableReference,
        }
    }
}