using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Solitaire.Game.Extensions;
using Solitaire.Game.Objects.Position;

namespace Solitaire.Game.Objects.Card {

    [System.Serializable]
    public class CardBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [System.NonSerialized]
        FixedPosition fixedPosition;
        
        List<Transform> transforms;

        public List<Transform> Transforms { get { return transforms; } }

        private void Awake()
        {
            gameObject.GetOrAdd<RectTransform>().sizeDelta = new Vector2(67, 100);
        }

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
        public bool AcceptMouseEvents
        {
            get
            {
                return acceptMouseEvents && (
                    gameCard.faceUp ||
                    Game.Instance.options.cheatMoveFaceDown
                );
            }
        }

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
    }
}

