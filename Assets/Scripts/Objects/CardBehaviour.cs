using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using Solitaire.Game.Objects.Position;

namespace Solitaire.Game.Objects.Card {

    [System.Serializable]
    public class CardBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [System.NonSerialized]
        FixedPosition fixedPosition;
        
        List<Transform> transforms;

        public List<Transform> Transforms { get { return transforms; } }

        private void RefreshTransforms()
        {
            fixedPosition = GetComponentInParent<FixedPosition>();

            if (transforms == null)
                transforms = new List<Transform>();

            transforms.Clear();

            if (!acceptMouseEvents)
                return;

            if (fixedPosition == null)
            {   // This happens when taking the top exposed 
                // card from the deck, there can be no 
                // stacked cards in this case.
                transforms.Add(transform);
                return;
            }

            foreach (var card in fixedPosition.GetComponentsInChildren<CardBehaviour>())
                if (card == this)
                    transforms.Add(card.transform);
                else if (transforms.Count > 0)
                    transforms.Add(card.transform);
        }

        [SerializeField]
        internal Card gameCard;

        // Is this object currently accepting mouse events?
        public bool acceptMouseEvents = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            RefreshTransforms();

            MouseHandler.OnDown(eventData, this);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            MouseHandler.OnDrag(eventData, this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            MouseHandler.OnPointerUp(eventData, this);
        }

        internal void SetTexture(Texture2D texture)
        {
            if (name == texture.name) return;

            name = texture.name;
            Image image = gameObject.GetComponent<Image>();

            image.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                (gameObject.transform as RectTransform).pivot
            );
            image.preserveAspect = true;
        }

        public void OnTap(PointerEventData eventData)
        {
            Game.Instance.MoveCard(gameCard);
        }

        public bool OnMove(Vector2 point)
        {
            var position = PositionRegistry.PositionAt(point);

            bool valid = Game.Instance.MoveCardToPosition(gameCard, position);

            return valid;
        }

        private RectTransform MyRectTransform
        {
            get
            {
                RectTransform rt = GetComponent<RectTransform>();

                if (rt == null)
                {
                    gameObject.AddComponent<RectTransform>();
                    rt = GetComponent<RectTransform>();
                }

                return rt;
            }
        }

        public void SetParent(GameObject parent)
        {

            transform.SetParent(parent.transform, false);

            MyRectTransform.sizeDelta = new Vector2(67, 100);
        }
    }
}

