﻿// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.RectExtensions
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using UnityEngine;

namespace BehaviorDesigner.Editor
{
    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect) => new Vector2(rect.xMin, rect.yMin);

        public static Rect ScaleSizeBy(this Rect rect, float scale) => rect.ScaleSizeBy(scale, rect.center);

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect rect1 = rect;
            rect1.x -= pivotPoint.x;
            rect1.y -= pivotPoint.y;
            rect1.xMin *= scale;
            rect1.xMax *= scale;
            rect1.yMin *= scale;
            rect1.yMax *= scale;
            rect1.x += pivotPoint.x;
            rect1.y += pivotPoint.y;
            return rect1;
        }
    }
}