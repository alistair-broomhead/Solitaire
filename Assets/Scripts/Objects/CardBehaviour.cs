using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using Solitaire.Game.Objects.Position;

namespace Solitaire.Game.Objects.Card {

    [System.Serializable]
    public class CardBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public List<Transform> Transforms { get { return transforms; } }
        [SerializeField]
        private List<Transform> transforms;
        [SerializeField]
        private MouseHandler handler;
        [SerializeField]
        internal Card gameCard;

        private Game game;

        // Is this object currently accepting mouse events?
        public bool acceptMouseEvents = false;

        public void Awake()
        {
            game = GetComponentInParent<Game>();
            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
                handler = new MouseHandler(this, canvas);

            transforms = new List<Transform>();
            if (acceptMouseEvents) transforms.Add(transform);
        }

        void Update()
        {
            if (acceptMouseEvents && Transforms.Count == 0)
                transforms.Add(transform as RectTransform);

            else if (!acceptMouseEvents && Transforms.Count > 0)
                transforms.Clear();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (handler != null) handler.OnDown(eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (handler != null) handler.OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (handler != null) handler.OnPointerUp(eventData);
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
            game.MoveCard(gameCard);
        }

        public bool OnMove(Vector2 point)
        {
            var position = PositionRegistry.PositionAt(point);

            bool valid = game.MoveCardToPosition(gameCard, position);

            if (valid)
                Destroy(gameObject);

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

