using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Solitaire.Game.Layout;
using Solitaire.Game.Extensions;
using System.Collections;

namespace Solitaire.Menu
{
    [Serializable]
    public class MenuHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField]
        private RectTransform handleTransform;
        [SerializeField]
        private RectTransform menuTransform;
        [SerializeField]
        private RectTransform parentTransform;
        [SerializeField]
        private bool shown = false;
        [SerializeField]
        private bool inDrag = false;

        private Canvas ParentCanvas { get { return GetComponentInParent<Canvas>(); } }

        private MinMax HandleBounds()
        {
            var parent = parentTransform.WorldMinMax();
            var handle = handleTransform.WorldMinMax();

            return new MinMax {
                minX = parent.minX,
                maxX = parent.maxX - handleTransform.Dimensions().x,
                minY = menuTransform.position.y,
                maxY = menuTransform.position.y
            };
        }
        
        private void Awake()
        {
            StartCoroutine(MoveNextFrame());
        }
        public IEnumerator MoveNextFrame()
        {
            // Defer this one frame so everything has
            // dinemensions etc
            yield return new WaitForEndOfFrame();
            Move();
        }

        public void Move()
        {
            var bounds = HandleBounds();

            menuTransform.position = new Vector2(
                shown ? bounds.minX : bounds.maxX,
                menuTransform.position.y
            );
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            inDrag = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            inDrag = true;

            bool inPlane;
            Vector2 pointer = eventData.WorldPosition(ParentCanvas, out inPlane);

            if (inPlane)
            {
                var bounds = HandleBounds();
                var handleDimensions = handleTransform.Dimensions();

                float x = Mathf.Clamp(
                    pointer.x - (handleDimensions.x / 2),
                    bounds.minX, bounds.maxX
                );

                menuTransform.position = new Vector2(x, menuTransform.position.y);
            }
            else
                Debug.LogError("Argh!!");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (inDrag)
            {
                shown = HandlePosition();
                inDrag = false;
            }
            else
                shown = !shown;

            Move();
        }

        private bool HandlePosition()
        {
            // Left is lesser, show. Right is greater, hide
            var bounds = HandleBounds();
            var x = menuTransform.position.x;
            
            // Which extreme is closer?
            float distanceFromShow = x - bounds.minX;
            float distanceFromHide = bounds.maxX - x;

            shown = distanceFromHide > distanceFromShow;
            return shown;
        }
    }
}
