namespace BehaviorDesigner.Runtime.Tasks
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class UnknownTask : Task
    {
        [HideInInspector]
        public string JSONSerialization;
        [HideInInspector]
        public List<int> fieldNameHash = new List<int>();
        [HideInInspector]
        public List<int> startIndex = new List<int>();
        [HideInInspector]
        public List<int> dataPosition = new List<int>();
        [HideInInspector]
        public List<UnityEngine.Object> unityObjects = new List<UnityEngine.Object>();
        [HideInInspector]
        public List<byte> byteData = new List<byte>();
    }
}

