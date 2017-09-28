using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.Game.Objects
{
    public interface IMouseHandled : IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        List<Transform> Transforms
        {
            get;
        }

        void OnDoubleClick(PointerEventData eventData);

        bool OnMove(PointerEventData eventData);
    }
}