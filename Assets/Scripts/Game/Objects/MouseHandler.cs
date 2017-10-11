using Solitaire.Game.Extensions;
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
        private static Canvas canvas;

        private static void SetBehaviour(CardBehaviour handled)
		{
            handling = handled;
			canvas = handled.GetComponentInParent<Canvas> ();
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
            Vector2 worldPosition = eventData.WorldPosition(canvas);

            pointerOffset = worldPosition - left2D;
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
            if (canvas == null)
                return;

            // Make sure we don't treat this as a click
            wasDragged = true;

            // This could be in-line below in C# 7, 
            // apparently  Unity uses C# 4 (Yay Mono?)
            bool inPlane;
            Vector2 canvasPosition = eventData.WorldPosition(canvas, out inPlane);

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
            var worldPosition = eventData.WorldPosition(canvas);

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
    }
}

