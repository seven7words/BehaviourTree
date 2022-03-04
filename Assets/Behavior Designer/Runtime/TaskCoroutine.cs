// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Runtime.TaskCoroutine
// Assembly: BehaviorDesigner.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 84396848-9F85-4A31-BDD9-270D59C9C087
// Assembly location: D:\StudyProject\BehaviourTree\Assets\Behavior Designer\Runtime\BehaviorDesigner.Runtime.dll

using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    public class TaskCoroutine
    {
        private IEnumerator mCoroutineEnumerator;
        private Coroutine mCoroutine;
        private Behavior mParent;
        private string mCoroutineName;
        private bool mStop;

        public TaskCoroutine(Behavior parent, IEnumerator coroutine, string coroutineName)
        {
            this.mParent = parent;
            this.mCoroutineEnumerator = coroutine;
            this.mCoroutineName = coroutineName;
            this.mCoroutine = parent.StartCoroutine(this.RunCoroutine());
        }

        public Coroutine Coroutine => this.mCoroutine;

        public void Stop() => this.mStop = true;

        public IEnumerator RunCoroutine()
        {
           yield return Coroutine;
        }
    }
}