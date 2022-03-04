namespace BehaviorDesigner.Runtime.Tasks
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public abstract class Task
    {
        protected UnityEngine.GameObject gameObject;
        protected UnityEngine.Transform transform;
        [SerializeField] private BehaviorDesigner.Runtime.NodeData nodeData;
        [SerializeField] private Behavior owner;
        [SerializeField] private int id = -1;
        [SerializeField] private string friendlyName = string.Empty;
        [SerializeField] private bool instant = true;
        private int referenceID = -1;
        private bool disabled;

        protected Task()
        {
        }

        protected T GetComponent<T>() where T : Component =>
            this.gameObject.GetComponent<T>();

        protected Component GetComponent(Type type) =>
            this.gameObject.GetComponent(type);

        protected UnityEngine.GameObject GetDefaultGameObject(UnityEngine.GameObject go) =>
            ((go != null) ? go : this.gameObject);

        public virtual float GetPriority() =>
            0f;

        public virtual float GetUtility() =>
            0f;

        public virtual void OnAnimatorIK()
        {
        }

        public virtual void OnAwake()
        {
        }

        public virtual void OnBehaviorComplete()
        {
        }

        public virtual void OnBehaviorRestart()
        {
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
        }

        public virtual void OnCollisionEnter2D(Collision2D collision)
        {
        }

        public virtual void OnCollisionExit(Collision collision)
        {
        }

        public virtual void OnCollisionExit2D(Collision2D collision)
        {
        }

        public virtual void OnConditionalAbort()
        {
        }

        public virtual void OnControllerColliderHit(ControllerColliderHit hit)
        {
        }

        public virtual void OnDrawGizmos()
        {
        }

        public virtual void OnEnd()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnPause(bool paused)
        {
        }

        public virtual void OnReset()
        {
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnTriggerEnter(Collider other)
        {
        }

        public virtual void OnTriggerEnter2D(Collider2D other)
        {
        }

        public virtual void OnTriggerExit(Collider other)
        {
        }

        public virtual void OnTriggerExit2D(Collider2D other)
        {
        }

        public virtual TaskStatus OnUpdate() =>
            TaskStatus.Success;

        protected Coroutine StartCoroutine(IEnumerator routine) =>
            this.Owner.StartCoroutine(routine);

        protected void StartCoroutine(string methodName)
        {
            this.Owner.StartTaskCoroutine(this, methodName);
        }

        protected Coroutine StartCoroutine(string methodName, object value) =>
            this.Owner.StartTaskCoroutine(this, methodName, value);

        protected void StopAllCoroutines()
        {
            this.Owner.StopAllTaskCoroutines();
        }

        protected void StopCoroutine(IEnumerator routine)
        {
            this.Owner.StopCoroutine(routine);
        }

        protected void StopCoroutine(string methodName)
        {
            this.Owner.StopTaskCoroutine(methodName);
        }

        protected void TryGetComponent<T>(out T component) where T : Component
        {
            gameObject.TryGetComponent<T>(out component);
        }

        protected void TryGetComponent(Type type, out Component component)
        {
            this.gameObject.TryGetComponent(type, out component);
        }

        public UnityEngine.GameObject GameObject
        {
            set =>
                this.gameObject = value;
        }

        public UnityEngine.Transform Transform
        {
            set =>
                this.transform = value;
        }

        public BehaviorDesigner.Runtime.NodeData NodeData
        {
            get =>
                this.nodeData;
            set =>
                this.nodeData = value;
        }

        public Behavior Owner
        {
            get =>
                this.owner;
            set =>
                this.owner = value;
        }

        public int ID
        {
            get =>
                this.id;
            set =>
                this.id = value;
        }

        public virtual string FriendlyName
        {
            get =>
                this.friendlyName;
            set =>
                this.friendlyName = value;
        }

        public bool IsInstant
        {
            get =>
                this.instant;
            set =>
                this.instant = value;
        }

        public int ReferenceID
        {
            get =>
                this.referenceID;
            set =>
                this.referenceID = value;
        }

        public bool Disabled
        {
            get =>
                this.disabled;
            set =>
                this.disabled = value;
        }
    }
}