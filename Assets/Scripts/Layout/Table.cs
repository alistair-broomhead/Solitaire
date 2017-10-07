using System;
using System.Collections.Generic;
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
        private float virtualPortraitWidth = 490.0f;
        [SerializeField]
        private float virtualLandscapeHeight = 600f;
        
        [SerializeField]
        private int numRows;
        [SerializeField]
        private int numColumns;
        [SerializeField]
        private Vector2 currentDimensions;
        [SerializeField]
        private int dimensionStability = 0;
        
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
            foreach (var row in rows)
                row.OnUpdate();

            var parentDimensions = LayoutUtils.Dimensions((RectTransform) transform.parent);

            if (parentDimensions == currentDimensions)
                // Allow for jitteriness
                if (dimensionStability > 100)
                    return;
                else
                    dimensionStability++;
            else
            {
                currentDimensions = parentDimensions;
                dimensionStability = 0;
            }

            Orientation orientation;

            if (parentDimensions.x > parentDimensions.y)
                orientation = Orientation.Landscape;
            else
                orientation = Orientation.Portrait;
            
            gameArea.layout.gameObject.SetActive(false);
            
            if (orientation == Orientation.Portrait)
                ResizePortrait();
            else
                ResizeLandscape();

        }
        private void ResizePortrait()
        {
            gameArea.layout = gameArea.pLayout;
            Resize(virtualPortraitWidth, 0);
        }
        private void ResizeLandscape()
        {
            gameArea.layout = gameArea.lLayout;
            Resize(virtualLandscapeHeight, 1);
        }
        private void ApplyRect(RectTransform source, RectTransform dest)
        {
            dest.sizeDelta = source.sizeDelta;
            dest.position = source.position;
        }
        private void Resize(float toSize, int limitedDimension)
        {
            gameArea.layout.gameObject.SetActive(true);

            var proxyTransform = gameArea.layout.gameProxy.GetComponent<RectTransform>();
            var thisTransform = GetComponent<RectTransform>();

            ApplyRect(proxyTransform, thisTransform);

            // Get the size of this object
            var size = LayoutUtils.Dimensions(proxyTransform);
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
