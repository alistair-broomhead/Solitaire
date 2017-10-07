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
        private Orientation orientation;

        [SerializeField]
        private float virtualPortraitWidth = 490.0f;
        [SerializeField]
        private float virtualLandscapeHeight = 600f;
        
        [SerializeField]
        private int numRows;
        [SerializeField]
        private int numColumns;

        private LayoutTemplate[] layouts;
        private LayoutTemplate ActiveLayout
        {
            get
            {
                if (gameArea == null)
                    return null;

                return gameArea.layout;
            }
            set
            {
                if (gameArea == null)
                    return;

                if (layouts == null)
                    layouts = new LayoutTemplate[] {
                        gameArea.portrait,
                        gameArea.landscape
                    };

                foreach (var layout in layouts)
                    if (layout != value)
                        layout.gameObject.SetActive(false);

                gameArea.layout = value;
                gameArea.layout.gameObject.SetActive(true);
            }
        }

        protected void Awake()
        {

            orientation = Orientation.Unknown;
            gameArea = GetComponentInChildren<GameArea>();
            gameTransform = gameArea.GetComponent<RectTransform>();

            if (cloneArea == null)
            {
                cloneArea = new GameObject("cloneArea");
                cloneTransform = cloneArea.AddComponent<RectTransform>();
                cloneTransform.SetParent(transform);
                cloneTransform.localScale = new Vector3(1, 1, 1);
                cloneTransform.position = new Vector3();
            }

            rows = GetComponentsInChildren<Row>();
            numRows = rows.Length;
            numColumns = 0;

            for (int i = 0; i < numRows; i++)
                numColumns = Math.Max(numColumns, rows[i].GetComponentsInChildren<Cell>().Length);

            if (GetComponent<RectTransform>() == null)
                gameObject.AddComponent<RectTransform>();

            Resize();
        }
        public void Update()
        {
            if (orientation == Orientation.Unknown)
                Resize();

            foreach (var row in rows)
                row.OnUpdate();
        }
        private void OnRectTransformDimensionsChange()
        {
            Resize();
        }
        private void Resize()
        {
            if (gameArea == null)
                return;

            var parentDimensions = LayoutUtils.Dimensions((RectTransform)transform.parent);
            
            if (parentDimensions.x > parentDimensions.y)
                orientation = Orientation.Landscape;
            else
                orientation = Orientation.Portrait;
            
            if (orientation == Orientation.Portrait)
                ResizePortrait();
            else
                ResizeLandscape();
        }
        private void ResizePortrait()
        {
            ActiveLayout = gameArea.portrait;
            Resize(virtualPortraitWidth, 0);
        }
        private void ResizeLandscape()
        {
            ActiveLayout = gameArea.landscape;
            Resize(virtualLandscapeHeight, 1);
        }
        private void ApplyRect(RectTransform source, RectTransform dest)
        {
            dest.sizeDelta = source.sizeDelta;
            dest.position = source.position;
        }
        private void Resize(float toSize, int limitedDimension)
        {
            var proxyTransform = ActiveLayout.gameProxy.GetComponent<RectTransform>();
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
