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

            if (instance == null) instance = this;
            else if (instance != this)
            {

                Destroy(gameObject);
            }

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

        public static Position PositionAt(Vector2 screenPostion)
        {
            foreach (Position instance in instances)
                if (TransformCollidesWithPoint(instance.Transform, screenPostion))
                    return instance;

            // Will need to check for this wherever it is called
            return null;
        }

        private static bool TransformCollidesWithPoint(RectTransform transform, Vector2 point)
        {
            throw new System.NotImplementedException();
        }
    }
}
