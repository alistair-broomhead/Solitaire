using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Solitaire.Game.Objects.Card {

    [System.Serializable]
    public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        // How long is a double click? See here!
        public static int doubleClickIntervalMs = 500;

        // When was this card last clicked?
        private DateTime lastClicked;
    
        // Has there been a Drag since the las PointerDown event?
        private bool wasDragged;
    
        // What camera are we using for the current drag?
        private Camera eventCamera;
    
        // Where was the card when the current drag started?
        public Vector3 leftPoint;

        // What's the canvas offset between where the click started 
        // and the initial local card position?
        private Vector2 pointerOffset;  
                                    
        // We will need the canvasTransform in order to make consistent
        // position calculations
        private RectTransform canvasTransform;

        void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) canvasTransform = canvas.transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            wasDragged = false;
            leftPoint = transform.localPosition;
        
            eventCamera = eventData.pressEventCamera;

            transform.SetAsLastSibling();

            Vector2 left2D = leftPoint;
            pointerOffset = CanvasPosition(eventData.position) - left2D;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvasTransform == null) return;

            wasDragged = true;

            Vector2 screenPostion = ClampToWindow(eventData);

            // This could be in-line below in C# 7, apparently Unity uses C# 4 (Yay Mono?)
            bool inPlane; 
            Vector2 canvasPosition = CanvasPosition(screenPostion, out inPlane);

            if (inPlane) transform.localPosition = canvasPosition - pointerOffset;
            else Debug.LogWarningFormat(this, "Out of canvas plane: {0}", eventData);

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (wasDragged)
            {
                HandleDragEnd(eventData);
                return;
            }

            DateTime currentClickEnd = DateTime.Now;
            TimeSpan sinceLastClick = currentClickEnd - lastClicked;
            lastClicked = currentClickEnd;

            if (sinceLastClick.TotalMilliseconds < doubleClickIntervalMs)
                HandleDoubleClick(eventData);
            else
                HandleClick(eventData);

        }

        private void HandleDragEnd(PointerEventData eventData)
        {
            Debug.LogFormat(this, "Drag ended @ {0}", eventData);

            transform.localPosition = leftPoint;
        }

        private void HandleClick(PointerEventData eventData)
        {
            Debug.LogFormat(this, "Click @ {0}", eventData);
        }

        private void HandleDoubleClick(PointerEventData eventData)
        {
            Debug.LogFormat(this, "Double Click @ {0}", eventData);
            lastClicked = DateTime.MinValue;
        }

        Vector2 ClampToWindow(PointerEventData data)
        {
            Vector2 rawPointerPosition = data.position;

            Vector3[] canvasCorners = new Vector3[4];
            canvasTransform.GetWorldCorners(canvasCorners);
        
            return new Vector2(
                Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x),
                Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y)
            );
        }

        /// <summary>
        ///     Get the canvas-local position of a point from screen-space.
        /// </summary>
        /// <param name="screenPosition"> 
        ///     A Vecto2 giving a screen-space coordinate 
        /// </param>
        /// <param name="inPlane"> 
        ///     An optional (via overload) output param that gets whether 
        ///     screenPosition was within the plane of the canvas. 
        /// </param>
        /// <returns>
        ///     A Vector2 of the canvas-local position the corresponds to
        ///     screenPosition.
        /// </returns>
        Vector2 CanvasPosition(Vector2 screenPosition, out bool inPlane)
        {
            Vector2 output;

            inPlane = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform,
                screenPosition,
                eventCamera,
                out output
            );

            return output;
        }
        /// This overload simply doesn't return to bool to indicate whether
        /// the given screenPosition was within the canvas' plane.
        Vector2 CanvasPosition(Vector2 screenPosition)
        {
            bool inPlane;
            Vector2 canvasPosition = CanvasPosition(screenPosition, out inPlane);

            if (!inPlane) Debug.LogWarningFormat(this, "{0} is not within the plane of the canvas!", screenPosition);

            return canvasPosition;
        }
    
    }
}

