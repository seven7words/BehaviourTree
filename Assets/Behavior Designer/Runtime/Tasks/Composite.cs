namespace BehaviorDesigner.Runtime.Tasks
{
    using System;
    using UnityEngine;

    public abstract class Composite : ParentTask
    {
        [Tooltip("Specifies the type of conditional abort. More information is located at https://www.opsive.com/support/documentation/behavior-designer/conditional-aborts/."), SerializeField]
        protected BehaviorDesigner.Runtime.Tasks.AbortType abortType;

        protected Composite()
        {
        }

        public virtual void OnReevaluationEnded(TaskStatus status)
        {
        }

        public virtual bool OnReevaluationStarted() => 
            false;

        public BehaviorDesigner.Runtime.Tasks.AbortType AbortType =>
            this.abortType;
    }
}

