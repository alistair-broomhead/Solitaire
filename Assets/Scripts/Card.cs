using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // Set in OnPointerDown
    public Camera usedCamera;
    public Vector3 leftPoint;
    public Vector2 pointerCardOffset;
    public Vector2 initialCanvasOffset;

    // Set in Awake
    public RectTransform canvasTransform;
    public RectTransform cardTransform;

    void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasTransform = canvas.transform as RectTransform;
            cardTransform = transform as RectTransform;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer down: " + eventData.ToString(), this);

        leftPoint = cardTransform.localPosition;
        
        usedCamera = eventData.pressEventCamera;

        cardTransform.SetAsLastSibling();

        LocalPoint(cardTransform, eventData.position, out pointerCardOffset);
        LocalPoint(canvasTransform, eventData.position, out initialCanvasOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (cardTransform == null)
            return;

        Debug.Log("Dragging: " + eventData.ToString(), this);

        Vector2 pointerPostion = ClampToWindow(eventData);

        Vector2 localPointerPosition;
        bool inCanvasPlane = LocalPoint(canvasTransform, pointerPostion, out localPointerPosition);

        if (inCanvasPlane) cardTransform.localPosition = localPointerPosition - pointerCardOffset - initialCanvasOffset;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Pointer up" + eventData.ToString(), this);

        cardTransform.localPosition = leftPoint;
    }

    Vector2 ClampToWindow(PointerEventData data)
    {
        Vector2 rawPointerPosition = data.position;

        Vector3[] canvasCorners = new Vector3[4];
        canvasTransform.GetWorldCorners(canvasCorners);
        
        return new Vector2(
            Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x),
            Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y)
        );
    }

    bool LocalPoint(RectTransform relativeTo, Vector2 screenPoint, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            relativeTo,
            screenPoint,
            usedCamera,
            out localPoint
        );
    }
    
}
