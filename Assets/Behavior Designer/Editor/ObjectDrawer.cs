// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.ObjectDrawer
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime.Tasks;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    public class ObjectDrawer
    {
        protected FieldInfo fieldInfo;
        protected ObjectDrawerAttribute attribute;
        protected object value;
        protected Task task;

        public FieldInfo FieldInfo
        {
            get => this.fieldInfo;
            set => this.fieldInfo = value;
        }

        public ObjectDrawerAttribute Attribute
        {
            get => this.attribute;
            set => this.attribute = value;
        }

        public object Value
        {
            get => this.value;
            set => this.value = value;
        }

        public Task Task
        {
            get => this.task;
            set => this.task = value;
        }

        public virtual void OnGUI(GUIContent label)
        {
        }
    }
}