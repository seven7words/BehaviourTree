namespace BehaviorDesigner.Runtime
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    [Serializable]
    public class NodeData
    {
        [SerializeField]
        private object nodeDesigner;
        [SerializeField]
        private Vector2 offset;
        [SerializeField]
        private string friendlyName = string.Empty;
        [SerializeField]
        private string comment = string.Empty;
        [SerializeField]
        private bool isBreakpoint;
        [SerializeField]
        private Texture icon;
        [SerializeField]
        private bool collapsed;
        [SerializeField]
        private int colorIndex;
        [SerializeField]
        private List<string> watchedFieldNames;
        private List<FieldInfo> watchedFields;
        private float pushTime = -1f;
        private float popTime = -1f;
        private float interruptTime = -1f;
        private bool isReevaluating;
        private TaskStatus executionStatus;

        public void AddWatchedField(FieldInfo field)
        {
            if (this.watchedFields == null)
            {
                this.watchedFields = new List<FieldInfo>();
                this.watchedFieldNames = new List<string>();
            }
            if (this.GetWatchedFieldIndex(field) == -1)
            {
                this.watchedFields.Add(field);
                this.watchedFieldNames.Add(field.Name);
            }
        }

        public void CopyFrom(NodeData nodeData, Task task)
        {
            this.nodeDesigner = nodeData.NodeDesigner;
            this.offset = nodeData.Offset;
            this.comment = nodeData.Comment;
            this.isBreakpoint = nodeData.IsBreakpoint;
            this.collapsed = nodeData.Collapsed;
            if ((nodeData.WatchedFields != null) && (nodeData.WatchedFields.Count > 0))
            {
                this.watchedFields = new List<FieldInfo>();
                this.watchedFieldNames = new List<string>();
                for (int i = 0; i < nodeData.watchedFields.Count; i++)
                {
                    FieldInfo field = task.GetType().GetField(nodeData.WatchedFields[i].Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null)
                    {
                        this.watchedFields.Add(field);
                        this.watchedFieldNames.Add(field.Name);
                    }
                }
            }
        }

        public int GetWatchedFieldIndex(FieldInfo field)
        {
            if (this.watchedFields != null)
            {
                for (int i = 0; i < this.watchedFields.Count; i++)
                {
                    if ((this.watchedFields[i] != null) && ((this.watchedFields[i].FieldType == field.FieldType) && (this.watchedFields[i].Name == field.Name)))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void InitWatchedFields(Task task)
        {
            if ((this.watchedFieldNames != null) && (this.watchedFieldNames.Count > 0))
            {
                this.watchedFields = new List<FieldInfo>();
                for (int i = 0; i < this.watchedFieldNames.Count; i++)
                {
                    FieldInfo field = task.GetType().GetField(this.watchedFieldNames[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (field != null)
                    {
                        this.watchedFields.Add(field);
                    }
                }
            }
        }

        public void RemoveWatchedField(FieldInfo field)
        {
            int watchedFieldIndex = this.GetWatchedFieldIndex(field);
            if (watchedFieldIndex != -1)
            {
                this.watchedFields.RemoveAt(watchedFieldIndex);
                this.watchedFieldNames.RemoveAt(watchedFieldIndex);
            }
        }

        private static Vector2 StringToVector2(string vector2String)
        {
            char[] separator = new char[] { ',' };
            string[] strArray = vector2String.Substring(1, vector2String.Length - 2).Split(separator);
            return new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]));
        }

        public object NodeDesigner
        {
            get => 
                this.nodeDesigner;
            set => 
                this.nodeDesigner = value;
        }

        public Vector2 Offset
        {
            get => 
                this.offset;
            set => 
                this.offset = value;
        }

        public string FriendlyName
        {
            get => 
                this.friendlyName;
            set => 
                this.friendlyName = value;
        }

        public string Comment
        {
            get => 
                this.comment;
            set => 
                this.comment = value;
        }

        public bool IsBreakpoint
        {
            get => 
                this.isBreakpoint;
            set => 
                this.isBreakpoint = value;
        }

        public Texture Icon
        {
            get => 
                this.icon;
            set => 
                this.icon = value;
        }

        public bool Collapsed
        {
            get => 
                this.collapsed;
            set => 
                this.collapsed = value;
        }

        public int ColorIndex
        {
            get => 
                this.colorIndex;
            set => 
                this.colorIndex = value;
        }

        public List<string> WatchedFieldNames
        {
            get => 
                this.watchedFieldNames;
            set => 
                this.watchedFieldNames = value;
        }

        public List<FieldInfo> WatchedFields
        {
            get => 
                this.watchedFields;
            set => 
                this.watchedFields = value;
        }

        public float PushTime
        {
            get => 
                this.pushTime;
            set =>
                this.pushTime = value;
        }

        public float PopTime
        {
            get => 
                this.popTime;
            set => 
                this.popTime = value;
        }

        public float InterruptTime
        {
            get => 
                this.interruptTime;
            set => 
                this.interruptTime = value;
        }

        public bool IsReevaluating
        {
            get => 
                this.isReevaluating;
            set => 
                this.isReevaluating = value;
        }

        public TaskStatus ExecutionStatus
        {
            get => 
                this.executionStatus;
            set => 
                this.executionStatus = value;
        }
    }
}

