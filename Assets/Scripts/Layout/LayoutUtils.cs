using UnityEngine;

namespace Solitaire.Game.Layout
{
    static class LayoutUtils
    {
        public static Vector2 Dimensions(RectTransform transform)
        {
            Vector3[] corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            // Corners are as such:
            // 1 --- 2
            // |     |
            // 0 --- 3
            float width = Vector3.Distance(corners[0], corners[3]);
            float height = Vector3.Distance(corners[0], corners[1]);

            return new Vector2(width, height);
        } 
    }
}
