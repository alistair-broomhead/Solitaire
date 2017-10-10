using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Solitaire.Game.Layout;

namespace Solitaire.Menu
{
    [Serializable]
    public class MenuHandle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private RectTransform handleTransform;
        [SerializeField]
        private RectTransform menuTransform;
        [SerializeField]
        private RectTransform parentTransform;
        [SerializeField]
        private bool shown = false;
        private bool inDrag = false;

        private void Move()
        {
            var parentMinMax = parentTransform.WorldMinMax();

            float x;

            if (shown)
                x = parentMinMax.minX;
            else
                x = parentMinMax.maxX - handleTransform.Dimensions().x;

            menuTransform.position = new Vector2(
                x, menuTransform.position.y
            );
        }

        private void Update()
        {
            if (!inDrag)
                Move();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            shown = !shown;
            Move();
        }
    }
}
