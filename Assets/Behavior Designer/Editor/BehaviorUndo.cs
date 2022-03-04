namespace BehaviorDesigner.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class BehaviorUndo
    {
        public static Component AddComponent(GameObject undoObject, Type type) => 
            Undo.AddComponent(undoObject, type);

        public static void DestroyObject(UnityEngine.Object undoObject, bool registerScene)
        {
            Undo.DestroyObjectImmediate(undoObject);
        }

        public static void RegisterUndo(string undoName, UnityEngine.Object undoObject)
        {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.UndoRedo))
            {
                Undo.RecordObject(undoObject, undoName);
            }
        }
    }
}

