namespace BehaviorDesigner.Runtime
{
    using System;
    using UnityEngine;

    public abstract class SharedVariable
    {
        [SerializeField]
        private bool mIsShared;
        [SerializeField]
        private bool mIsGlobal;
        [SerializeField]
        private bool mIsDynamic;
        [SerializeField]
        private string mName;
        [SerializeField]
        private string mToolTip;
        [SerializeField]
        private string mPropertyMapping;
        [SerializeField]
        private GameObject mPropertyMappingOwner;

        protected SharedVariable()
        {
        }

        public abstract object GetValue();
        public virtual void InitializePropertyMapping(BehaviorSource behaviorSource)
        {
        }

        public abstract void SetValue(object value);

        public bool IsShared
        {
            get => 
                this.mIsShared;
            set => 
                this.mIsShared = value;
        }

        public bool IsGlobal
        {
            get => 
                this.mIsGlobal;
            set => 
                this.mIsGlobal = value;
        }

        public bool IsDynamic
        {
            get => 
                this.mIsDynamic;
            set => 
                this.mIsDynamic = value;
        }

        public string Name
        {
            get => 
                this.mName;
            set => 
                this.mName = value;
        }

        public string Tooltip
        {
            get => 
                this.mToolTip;
            set => 
                this.mToolTip = value;
        }

        public string PropertyMapping
        {
            get => 
                this.mPropertyMapping;
            set =>
                this.mPropertyMapping = value;
        }

        public GameObject PropertyMappingOwner
        {
            get => 
                this.mPropertyMappingOwner;
            set => 
                this.mPropertyMappingOwner = value;
        }

        public bool IsNone =>
            (this.mIsShared && string.IsNullOrEmpty(this.mName));
    }
}

