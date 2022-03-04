namespace BehaviorDesigner.Runtime
{
    using System;

    [Serializable]
    public class SharedGenericVariable : SharedVariable<GenericVariable>
    {
        public SharedGenericVariable()
        {
            base.mValue = new GenericVariable();
        }

        public static implicit operator SharedGenericVariable(GenericVariable value) => 
            new SharedGenericVariable { mValue = value };
    }
}

