using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Solitaire.Menu
{
    public class HiddenButton : Selectable, IPointerClickHandler, ISubmitHandler, IEventSystemHandler
    {
        public Button.ButtonClickedEvent onClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            onClick.Invoke();
        }
    }
}
