namespace BehaviorDesigner.Runtime
{
    using System;

    [Serializable]
    public class SharedNamedVariable : SharedVariable<NamedVariable>
    {
        public SharedNamedVariable()
        {
            base.mValue = new NamedVariable();
        }

        public static implicit operator SharedNamedVariable(NamedVariable value) => 
            new SharedNamedVariable { mValue = value };
    }
}

