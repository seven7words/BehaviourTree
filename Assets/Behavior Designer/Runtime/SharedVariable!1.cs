namespace BehaviorDesigner.Runtime
{
    using System;
    using System.Reflection;
    using UnityEngine;

    public abstract class SharedVariable<T> : SharedVariable
    {
        private Func<T> mGetter;
        private Action<T> mSetter;
        [SerializeField]
        protected T mValue;

        protected SharedVariable()
        {
        }

        public override object GetValue() => 
            this.Value;

        public override void InitializePropertyMapping(BehaviorSource behaviorSource)
        {
            if ((BehaviorManager.IsPlaying && (behaviorSource.Owner.GetObject() is Behavior)) && !string.IsNullOrEmpty(base.PropertyMapping))
            {
                char[] separator = new char[] { '/' };
                string[] strArray = base.PropertyMapping.Split(separator);
                GameObject obj2 = null;
                try
                {
                    obj2 = Equals(base.PropertyMappingOwner, null) ? (behaviorSource.Owner.GetObject() as Behavior).gameObject : base.PropertyMappingOwner;
                }
                catch (Exception)
                {
                    Behavior behavior = behaviorSource.Owner.GetObject() as Behavior;
                    if ((behavior != null) && behavior.AsynchronousLoad)
                    {
                        Debug.LogError("Error: Unable to retrieve GameObject. Properties cannot be mapped while using asynchronous load.");
                        return;
                    }
                }
                if (obj2 == null)
                {
                    Debug.LogError("Error: Unable to find GameObject on " + behaviorSource.behaviorName + " for property mapping with variable " + base.Name);
                }
                else
                {
                    Component firstArgument = obj2.GetComponent(TaskUtility.GetTypeWithinAssembly(strArray[0]));
                    if (firstArgument == null)
                    {
                        Debug.LogError("Error: Unable to find component on " + behaviorSource.behaviorName + " for property mapping with variable " + base.Name);
                    }
                    else
                    {
                        PropertyInfo property = firstArgument.GetType().GetProperty(strArray[1]);
                        if (property != null)
                        {
                            MethodInfo getMethod = property.GetGetMethod();
                            if (getMethod != null)
                            {
                                this.mGetter = (Func<T>) Delegate.CreateDelegate(typeof(Func<T>), firstArgument, getMethod);
                            }
                            getMethod = property.GetSetMethod();
                            if (getMethod != null)
                            {
                                this.mSetter = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), firstArgument, getMethod);
                            }
                        }
                    }
                }
            }
        }

        public override void SetValue(object value)
        {
            if (this.mSetter != null)
            {
                this.mSetter((T) value);
            }
            else
            {
                this.mValue = (T) value;
            }
        }

        public override string ToString() => 
            ((this.Value != null) ? this.Value.ToString() : "(null)");

        public T Value
        {
            get => 
                ((this.mGetter == null) ? this.mValue : this.mGetter());
            set
            {
                if (this.mSetter != null)
                {
                    this.mSetter(value);
                }
                else
                {
                    this.mValue = value;
                }
            }
        }
    }
}

