using UnityEngine;

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
    }
}