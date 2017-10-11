using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.Game.Extensions
{
    public static class UnityExtensions
    {
        public static T GetOrAdd<T>(this MonoBehaviour behaviour, out bool isNew) where T : Component
        {
            return GetOrAdd<T>(behaviour.gameObject, out isNew);
        }
        public static T GetOrAdd<T>(this MonoBehaviour behaviour) where T : Component
        {
            bool isNew;
            return GetOrAdd<T>(behaviour.gameObject, out isNew);
        }
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            bool isNew;
            return GetOrAdd<T>(gameObject, out isNew);
        }
        public static T GetOrAdd<T>(this GameObject gameObject, out bool isNew) where T : Component
        {

            T component = gameObject.GetComponent<T>();

            isNew = (component == null);

            if (isNew)
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static Vector3 WorldPosition(this PointerEventData eventData, Canvas canvas)
        {
            bool inPlane;
            Vector2 canvasPosition = eventData.WorldPosition(canvas, out inPlane);

            if (!inPlane)
                Debug.LogWarningFormat("{0} is not within the plane of the canvas!", eventData.position);

            return canvasPosition;
        }
        public static Vector3 WorldPosition(this PointerEventData eventData, Canvas canvas, out bool inPlane)
        {
            Vector3 output;
            var canvasTransform = canvas.gameObject.GetOrAdd<RectTransform>();

            inPlane = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasTransform,
                Clamped(canvas, eventData.position),
                eventData.pressEventCamera,
                out output
            );

            return output;
        }

        private static Vector3 Clamped(Canvas canvas, Vector3 point)
        {
            var canvasCorners = new Vector3[4];
            var canvasTransform = canvas.gameObject.GetOrAdd<RectTransform>();
            canvasTransform.GetWorldCorners(canvasCorners);

            return new Vector2(
                Mathf.Clamp(point.x, canvasCorners[0].x, canvasCorners[2].x),
                Mathf.Clamp(point.y, canvasCorners[0].y, canvasCorners[2].y)
            );
        }
    }
}