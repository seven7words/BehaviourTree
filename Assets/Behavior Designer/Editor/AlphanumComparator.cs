// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.AlphanumComparator`1
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace BehaviorDesigner.Editor
{
  public class AlphanumComparator<T> : IComparer<T>
  {
    public int Compare(T x, T y)
    {
      string empty1 = string.Empty;
      string str1;
      if (x.GetType().IsSubclassOf(typeof (Type)))
      {
        Type t = x as Type;
        string str2 = this.TypePrefix(t) + "/";
        TaskCategoryAttribute[] customAttributes1;
        if ((customAttributes1 = t.GetCustomAttributes(typeof (TaskCategoryAttribute), true) as TaskCategoryAttribute[]).Length > 0)
        {
          string str3 = customAttributes1[0].Category.TrimEnd(TaskUtility.TrimCharacters);
          str2 = str2 + str3 + "/";
        }
        TaskNameAttribute[] customAttributes2;
        str1 = (customAttributes2 = t.GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[]).Length <= 0 ? str2 + BehaviorDesignerUtility.SplitCamelCase(t.Name.ToString()) : str2 + customAttributes2[0].Name;
      }
      else if (x.GetType().IsSubclassOf(typeof (SharedVariable)))
      {
        string s = x.GetType().Name;
        if (s.Length > 6 && s.Substring(0, 6).Equals("Shared"))
          s = s.Substring(6, s.Length - 6);
        str1 = BehaviorDesignerUtility.SplitCamelCase(s);
      }
      else
        str1 = BehaviorDesignerUtility.SplitCamelCase(x.ToString());
      if (str1 == null)
        return 0;
      string empty2 = string.Empty;
      string str4;
      if (y.GetType().IsSubclassOf(typeof (Type)))
      {
        Type t = y as Type;
        string str2 = this.TypePrefix(t) + "/";
        TaskCategoryAttribute[] customAttributes1;
        if ((customAttributes1 = t.GetCustomAttributes(typeof (TaskCategoryAttribute), true) as TaskCategoryAttribute[]).Length > 0)
        {
          string str3 = customAttributes1[0].Category.TrimEnd(TaskUtility.TrimCharacters);
          str2 = str2 + str3 + "/";
        }
        TaskNameAttribute[] customAttributes2;
        str4 = (customAttributes2 = t.GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[]).Length <= 0 ? str2 + BehaviorDesignerUtility.SplitCamelCase(t.Name.ToString()) : str2 + customAttributes2[0].Name;
      }
      else if (y.GetType().IsSubclassOf(typeof (SharedVariable)))
      {
        string s = y.GetType().Name;
        if (s.Length > 6 && s.Substring(0, 6).Equals("Shared"))
          s = s.Substring(6, s.Length - 6);
        str4 = BehaviorDesignerUtility.SplitCamelCase(s);
      }
      else
        str4 = BehaviorDesignerUtility.SplitCamelCase(y.ToString());
      if (str4 == null)
        return 0;
      int length1 = str1.Length;
      int length2 = str4.Length;
      int index1 = 0;
      for (int index2 = 0; index1 < length1 && index2 < length2; ++index2)
      {
        int num;
        if (char.IsDigit(str1[index1]) && char.IsDigit(str1[index2]))
        {
          string empty3 = string.Empty;
          for (; index1 < length1 && char.IsDigit(str1[index1]); ++index1)
            empty3 += str1[index1].ToString(); 
          string empty4 = string.Empty;
          for (; index2 < length2 && char.IsDigit(str4[index2]); ++index2)
            empty4 += str4[index2].ToString(); 
          int result1 = 0;
          int.TryParse(empty3, out result1);
          int result2 = 0;
          int.TryParse(empty4, out result2);
          num = result1.CompareTo(result2);
        }
        else
          num = str1[index1].CompareTo(str4[index2]);
        if (num != 0)
          return num;
        index1++;
      }
      return length1 - length2;
    }

    private string TypePrefix(Type t)
    {
      if (t.IsSubclassOf(typeof (Action)))
        return "Action";
      if (t.IsSubclassOf(typeof (Composite)))
        return "Composite";
      return t.IsSubclassOf(typeof (Conditional)) ? "Conditional" : "Decorator";
    }
  }
}
