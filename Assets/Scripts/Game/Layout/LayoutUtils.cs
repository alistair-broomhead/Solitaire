using UnityEngine;

namespace Solitaire.Game.Layout
{
    public struct MinMax
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
    }

    static class LayoutUtils
    {
        public static Vector2 Dimensions(this RectTransform transform)
        {
            return transform.Dimensions(transform.WorldCorners());
        }
        private static Vector2 Dimensions(this RectTransform transform, Vector3[] corners)
        {
            float width = Vector3.Distance(corners[0], corners[3]);
            float height = Vector3.Distance(corners[0], corners[1]);

            return new Vector2(width, height);
        }

        public static MinMax WorldMinMax(this RectTransform transform)
        {
            var corners = transform.WorldCorners();
            return new MinMax
            {
                minX = corners[0].x,
                maxX = corners[3].x,
                minY = corners[0].y,
                maxY = corners[1].y,
            };
        }

        public static Vector3[] WorldCorners(this RectTransform transform)
        {
            // Corners are as such:
            // 1 --- 2
            // |     |
            // 0 --- 3
            Vector3[] corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            return corners;
        }
    }
}
