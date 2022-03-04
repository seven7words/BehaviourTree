// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.Tasks.ParentTask
// Assembly: BehaviorDesigner.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 84396848-9F85-4A31-BDD9-270D59C9C087
// Assembly location: D:\StudyProject\BehaviourTree\Assets\Behavior Designer\Runtime\BehaviorDesigner.Runtime.dll

using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
  public abstract class ParentTask : Task
  {
    [SerializeField]
    protected List<Task> children;

    public List<Task> Children
    {
      get => this.children;
      private set => this.children = value;
    }

    public virtual int MaxChildren() => int.MaxValue;

    public virtual bool CanRunParallelChildren() => false;

    public virtual int CurrentChildIndex() => 0;

    public virtual bool CanExecute() => true;

    public virtual TaskStatus Decorate(TaskStatus status) => status;

    public virtual bool CanReevaluate() => false;

    public virtual void OnChildExecuted(TaskStatus childStatus)
    {
    }

    public virtual void OnChildExecuted(int childIndex, TaskStatus childStatus)
    {
    }

    public virtual void OnChildStarted()
    {
    }

    public virtual void OnChildStarted(int childIndex)
    {
    }

    public virtual TaskStatus OverrideStatus(TaskStatus status) => status;

    public virtual TaskStatus OverrideStatus() => TaskStatus.Running;

    public virtual void OnConditionalAbort(int childIndex)
    {
    }

    public override float GetUtility()
    {
      float num = 0.0f;
      if (this.children != null)
      {
        for (int index = 0; index < this.children.Count; ++index)
        {
          if (this.children[index] != null && !this.children[index].Disabled)
            num += this.children[index].GetUtility();
        }
      }
      return num;
    }

    public override void OnDrawGizmos()
    {
      if (this.children == null)
        return;
      for (int index = 0; index < this.children.Count; ++index)
      {
        if (this.children[index] != null && !this.children[index].Disabled)
          this.children[index].OnDrawGizmos();
      }
    }

    public void AddChild(Task child, int index)
    {
      if (this.children == null)
        this.children = new List<Task>();
      this.children.Insert(index, child);
    }

    public void ReplaceAddChild(Task child, int index)
    {
      if (this.children != null && index < this.children.Count)
        this.children[index] = child;
      else
        this.AddChild(child, index);
    }
  }
}
