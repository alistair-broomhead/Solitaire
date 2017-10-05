using System;
using UnityEngine;

namespace Solitaire.Game.Layout
{
    internal enum Orientation : sbyte
    {
        Unknown,
        Portrait,
        Landscape
    }

    [Serializable]
    class Table : MonoBehaviour
    {
        [NonSerialized]
        private GameObject cloneArea;
        [NonSerialized]
        private RectTransform cloneTransform;

        private GameArea gameArea;
        [NonSerialized]
        private RectTransform gameTransform;

        private Row[] rows;

        [SerializeField]
        private int numRows;

        [SerializeField]
        private int numColumns;
        
        protected void Awake()
        {
            gameArea = GetComponentInChildren<GameArea>();
            gameTransform = gameArea.GetComponent<RectTransform>();

            if (cloneArea == null)
                cloneArea = new GameObject("cloneArea");

            cloneTransform = cloneArea.GetComponent<RectTransform>();

            if (cloneTransform == null)
                cloneTransform = cloneArea.AddComponent<RectTransform>();

            cloneTransform.SetParent(transform);

            rows = GetComponentsInChildren<Row>();
            numRows = rows.Length;
            numColumns = 0;

            for (int i = 0; i < numRows; i++)
                numColumns = Math.Max(numColumns, rows[i].GetComponentsInChildren<Cell>().Length);

            if (GetComponent<RectTransform>() == null)
                gameObject.AddComponent<RectTransform>();
        }
        public void Update()
        {
            var parentDimensions = LayoutUtils.Dimensions((RectTransform) transform.parent);

            Orientation orientation;

            if (parentDimensions.x > parentDimensions.y)
                orientation = Orientation.Landscape;
            else
                orientation = Orientation.Portrait;

            foreach (var row in rows)
                row.OnUpdate();
            
            if (orientation == Orientation.Portrait)
                ResizePortrait();
            else
                ResizeLandscape();

        }
        private void ResizePortrait()
        {
            Resize(490.0f, 0);
        }
        private void ResizeLandscape()
        {
            Resize(480f, 1);
        }
        private void Resize(float toSize, int limitedDimension)
        {
            // Get the size of this object
            var size = LayoutUtils.Dimensions(GetComponent<RectTransform>());
            // What factor should we multiply the dimensions
            // by in order to set the limitedDimension to
            // the requested size?
            var scale = toSize / size[limitedDimension];
            // These shall be the dimensions of the child object
            var gameSize = new Vector2(
                size.x * scale,
                size.y * scale
            );
            // By resizing another object we can get the
            // Unity scaling factor that must be applied
            cloneTransform.sizeDelta = gameSize;
            var cloneSize = LayoutUtils.Dimensions(cloneTransform);
            // We shall only be interested in the 
            // limitedDimension as this is controlled by
            // orientation, therefore the other dimension
            // should have plenty of breathing room.
            var factor = size[limitedDimension] / cloneSize[limitedDimension];
            // Now that we have the information we need, 
            // actually resize everything.
            gameTransform.sizeDelta = gameSize;
            gameTransform.localScale = new Vector3(factor, factor, factor);
        }
    }
}
