﻿namespace BehaviorDesigner.Runtime.Tasks
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class TaskCategoryAttribute : Attribute
    {
        public readonly string mCategory;

        public TaskCategoryAttribute(string category)
        {
            this.mCategory = category;
        }

        public string Category =>
            this.mCategory;
    }
}

