﻿namespace BehaviorDesigner.Runtime.Tasks
{
    using System;

    public class Decorator : ParentTask
    {
        public override int MaxChildren() => 
            1;
    }
}

