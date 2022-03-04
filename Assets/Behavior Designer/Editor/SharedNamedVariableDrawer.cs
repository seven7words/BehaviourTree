using BehaviorDesigner.Editor;
using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomObjectDrawer(typeof(NamedVariable))]
public class SharedNamedVariableDrawer : ObjectDrawer
{
    private static string[] variableNames;

    public override void OnGUI(GUIContent label)
    {
        NamedVariable variable = base.value as NamedVariable;
        EditorGUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
        if (FieldInspector.DrawFoldout(variable.GetHashCode(), label))
        {
            EditorGUI.indentLevel += 1;
            
            if (variableNames == null)
            {
                List<Type> list = VariableInspector.FindAllSharedVariableTypes(true);
                variableNames = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    variableNames[i] = list[i].Name.Remove(0, 6);
                }
            }
            int index = 0;
            string str = variable.type.Remove(0, 6);
            int num3 = 0;
            while (true)
            {
                if (num3 < variableNames.Length)
                {
                    if (!variableNames[num3].Equals(str))
                    {
                        num3++;
                        continue;
                    }
                    index = num3;
                }
                variable.name = EditorGUILayout.TextField("Name", variable.name, Array.Empty<GUILayoutOption>());
                int num4 = EditorGUILayout.Popup("Type", index, variableNames, BehaviorDesignerUtility.SharedVariableToolbarPopup, Array.Empty<GUILayoutOption>());
                Type type = VariableInspector.FindAllSharedVariableTypes(true)[num4];
                if (num4 != index)
                {
                    index = num4;
                    variable.value = Activator.CreateInstance(type) as SharedVariable;
                }
                GUILayout.Space(3f);
                variable.type = "Shared" + variableNames[index];
                variable.value = FieldInspector.DrawSharedVariable(null, new GUIContent("Value"), null, type, variable.value);
                EditorGUI.indentLevel-=1;
                break;
            }
        }
        EditorGUILayout.EndVertical();
    }
}

