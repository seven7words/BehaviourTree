namespace BehaviorDesigner.Runtime.Tasks
{
    using BehaviorDesigner.Runtime;
    using System;
    using UnityEngine;

    [TaskDescription("Behavior Reference allows you to run another behavior tree within the current behavior tree."), HelpURL("https://www.opsive.com/support/documentation/behavior-designer/external-behavior-trees/"), TaskIcon("BehaviorTreeReferenceIcon.png")]
    public abstract class BehaviorReference : Action
    {
        [Tooltip("External behavior array that this task should reference")]
        public ExternalBehavior[] externalBehaviors;
        [Tooltip("Any variables that should be set for the specific tree")]
        public SharedNamedVariable[] variables;
        [HideInInspector]
        public bool collapsed;

        protected BehaviorReference()
        {
        }

        public virtual ExternalBehavior[] GetExternalBehaviors() => 
            this.externalBehaviors;

        public override void OnReset()
        {
            this.externalBehaviors = null;
        }
    }
}

