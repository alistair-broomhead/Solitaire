using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Solitaire.Game.Objects;
using System.Collections.Generic;

namespace Solitaire.Game.Objects.Card {

    [System.Serializable]
    public class Card : MonoBehaviour, IMouseHandled
    {
        public List<Transform> Transforms { get { return transforms; } }
        public List<Transform> transforms;
        public MouseHandler<Card> handler;
        
        // Is this object currently accepting mouse events?
        public bool acceptMouseEvents = false;

        void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
                handler = new MouseHandler<Card>(this, canvas);

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

        public void OnDoubleClick(PointerEventData eventData)
        {
            Debug.LogFormat("Double Click @ {0}", eventData);
        }

        public bool OnMove(PointerEventData eventData)
        {
            Debug.LogFormat("Move @ {0}", eventData);

            return false;
        }
    }
}

