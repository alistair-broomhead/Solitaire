using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Game.Objects.Position
{
    public class PositionRegistry: MonoBehaviour
    {
        public static PositionRegistry instance = null;

        // TODO: Remove this and usages
        public List<Position> positionToShow;

        protected void Awake()
        {
            positionToShow = new List<Position>();

            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            // Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
            
            foreach (Position pos in instances)
                if (pos != null)
                    positionToShow.Add(pos);

            positionToShow.Sort(ComparePositions);
        }

        protected static HashSet<Position> instances = new HashSet<Position>();
        public static void RegisterInstance(Position position)
        {
            if (instances.Contains(position)) return;

            instances.Add(position);

            if (instance == null) return;

            instance.positionToShow.Add(position);
            instance.positionToShow.Sort((a, b) => a.name.CompareTo(b.name));
        }

        private int ComparePositions(Position a, Position b)
        {
            return a.name.CompareTo(b.name);
        }

        public static Position PositionAt(Vector2 worldPosition)
        {
            foreach (Position instance in instances)
                if (ContainsPoint(instance.gameObject, worldPosition))
                    return instance;

            // Will need to check for this wherever it is called
            return null;
        }

        private static bool ContainsPoint(GameObject position, Vector2 point)
        {
            RectTransform rectTransform = position.transform as RectTransform;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (Vector3 corner in corners)
            {
                minX = Mathf.Min(minX, corner.x);
                minY = Mathf.Min(minY, corner.y);
                maxX = Mathf.Max(maxX, corner.x);
                maxY = Mathf.Max(maxY, corner.y);
            }

            return (
                minX <= point.x &&
                minY <= point.y &&
                maxX >= point.x &&
                maxY >= point.y
            );
        }
    }
}
