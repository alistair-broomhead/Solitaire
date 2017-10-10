using Solitaire.Game.Objects.Card;
using Solitaire.Game.Objects.Position;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.Game.Objects
{
    [System.Serializable]
    public static class MouseHandler
    {
        // What behaviour are we handling?
        private static CardBehaviour handling;

        // Has there been a Drag since the las PointerDown 
        // event?
        private static bool wasDragged;

        // What camera are we using for the current drag?
        private static Camera eventCamera;

        // Where was the card when the current drag started?
        public static List<Vector3> leftPoints;

        // What's the canvas offset between where the click 
        // started 
        // and the initial local card position?
        private static List<Vector2> pointerOffsets;

        // What position was this attached to?
        private static List<Transform> fromPositions;

        // We will need the canvas' transform in order to 
        // make consistent position calculations
        private static RectTransform canvasTransform;

        private static void SetBehaviour(CardBehaviour handled)
		{
            handling = handled;
			var canvas = handled.GetComponentInParent<Canvas> ();
			canvasTransform = canvas.transform as RectTransform;
		}
        
        // On pointer down we need to set up state to allow
        // for either a click or drag to be processed.
		public static void OnDown(PointerEventData eventData, CardBehaviour handled)
		{
            if (handling != null)
                return;

			SetBehaviour (handled);
			OnDown(eventData);
		}
        private static void OnDown(PointerEventData eventData)
        {
            wasDragged = false;
            eventCamera = eventData.pressEventCamera;

            fromPositions = new List<Transform>();
            leftPoints = new List<Vector3>();
            pointerOffsets = new List<Vector2>();

            foreach (Transform transform in handling.Transforms)
            {
                Vector3 leftPoint;
                Vector2 pointerOffset;

                HandleDown(transform as RectTransform, eventData, out leftPoint, out pointerOffset);

                fromPositions.Add(transform.parent);

                transform.SetParent(Game.HoverParent.transform, true);

                leftPoints.Add(leftPoint);
                pointerOffsets.Add(pointerOffset);
            }
        }
        private static void HandleDown(RectTransform transform, PointerEventData eventData, out Vector3 leftPoint, out Vector2 pointerOffset)
        {
            leftPoint = transform.position;
            
            transform.SetAsLastSibling();

            Vector2 left2D = leftPoint;
            pointerOffset = CanvasPosition(eventData.position) - left2D;
        }

        // On drag event we want to move the image of the 
        // card around in  order to give the player visual 
        // feedback.
        public static void OnDrag(PointerEventData eventData, CardBehaviour handled)
        {
            if (handled == handling)
                OnDrag(eventData);
        }

        private static void OnDrag(PointerEventData eventData)
        {
            int i = 0;
            foreach (Transform transform in handling.Transforms)
                OnDrag(transform as RectTransform, pointerOffsets[i++], eventData);
        }
        private static void OnDrag(RectTransform transform, Vector2 pointerOffset, PointerEventData eventData)
        {
            if (canvasTransform == null) return;

            // Make sure we don't treat this as a click
            wasDragged = true;

            // Don't allow the player to drag the card out
            // of the window entirely
            Vector2 screenPostion = ClampToWindow(eventData);
            // This could be in-line below in C# 7, 
            // apparently  Unity uses C# 4 (Yay Mono?)
            bool inPlane;
            Vector2 canvasPosition = CanvasPosition(screenPostion, out inPlane);

            // If the screen position is within the canvas' 
            // plane we can move the card image to this new 
            // position - it should always be within the 
            // canvas' plane so we'd best warn if it's not
            if (inPlane)
                transform.position = canvasPosition - pointerOffset;
            else
                Debug.LogWarningFormat("Out of canvas plane: {0}", eventData);

        }

        public static void OnPointerUp(PointerEventData eventData, CardBehaviour handled)
        {
            if (handled != handling)
                return;

            OnPointerUp(eventData);

            handling = null;
        }

        private static void OnPointerUp(PointerEventData eventData)
        {
            if (wasDragged)
                HandleDragEnd(eventData);
            else
                HandleClick(eventData);
        }

        private static void HandleDragEnd(PointerEventData eventData)
        {
            var worldPosition = CanvasPosition(eventData.position);

            if (!handling.OnMove(worldPosition))
            {
                var transforms = handling.Transforms;

                for (int i = 0; i < transforms.Count; i++)
                    ResetClick(i, transforms);
            }
        }

        private static void HandleClick(PointerEventData eventData)
        {
            var transforms = handling.Transforms;

            for (int i = 0; i < transforms.Count; i++)
                ResetClick(i, transforms);

            handling.OnTap(eventData);
        }
        private static void ResetClick(int index, List<Transform> transforms)
        {
            var transform = transforms[index];

            transform.SetParent(fromPositions[index]);
            transform.position = leftPoints[index];
        }

        private static Vector2 ClampToWindow(PointerEventData data)
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
        private static Vector3 CanvasPosition(Vector2 screenPosition, out bool inPlane)
        {
            Vector3 output;

            inPlane = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasTransform,
                screenPosition,
                eventCamera,
                out output
            );

            return output;
        }
        /// This overload simply doesn't return to bool to indicate whether
        /// the given screenPosition was within the canvas' plane.
        private static Vector2 CanvasPosition(Vector2 screenPosition)
        {
            bool inPlane;
            Vector2 canvasPosition = CanvasPosition(screenPosition, out inPlane);

            if (!inPlane) Debug.LogWarningFormat("{0} is not within the plane of the canvas!", screenPosition);

            return canvasPosition;
        }
    }
}

