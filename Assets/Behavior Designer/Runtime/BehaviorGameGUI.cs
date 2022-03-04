namespace BehaviorDesigner.Runtime
{
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("Behavior Designer/Behavior Game GUI")]
    public class BehaviorGameGUI : MonoBehaviour
    {
        private BehaviorManager behaviorManager;
        private Camera mainCamera;

        public  void OnGUI()
        {
            if (this.behaviorManager == null)
            {
                this.behaviorManager = BehaviorManager.instance;
            }
            if ((this.behaviorManager != null) && (this.mainCamera != null))
            {
                List<BehaviorManager.BehaviorTree> behaviorTrees = this.behaviorManager.BehaviorTrees;
                int num = 0;
                while (num < behaviorTrees.Count)
                {
                    BehaviorManager.BehaviorTree tree = behaviorTrees[num];
                    string str = string.Empty;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= tree.activeStack.Count)
                        {
                            Transform transform = tree.behavior.transform;
                            Vector2 vector2 = GUIUtility.ScreenToGUIPoint(Camera.main.WorldToScreenPoint(transform.position));
                            GUIContent content = new GUIContent(str);
                            Vector2 vector3 = GUI.skin.label.CalcSize(content);
                            vector3.x += 14f;
                            vector3.y += 5f;
                            GUI.Box(new Rect(vector2.x - (vector3.x / 2f), (Screen.height - vector2.y) + (vector3.y / 2f), vector3.x, vector3.y), content);
                            num++;
                            break;
                        }
                        Stack<int> stack = tree.activeStack[num2];
                        if ((stack.Count != 0) && (tree.taskList[stack.Peek()] is Tasks.Action))
                        {
                            str = str + tree.taskList[tree.activeStack[num2].Peek()].FriendlyName + ((num2 >= (tree.activeStack.Count - 1)) ? string.Empty : "\n");
                        }
                        num2++;
                    }
                }
            }
        }

        public void Start()
        {
            this.mainCamera = Camera.main;
        }
    }
}

