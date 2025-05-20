using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableButton : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public bool isEditMode = false;

    private RectTransform rectTransform;
    private Vector2 offset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isEditMode) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );

        offset = (Vector2)rectTransform.localPosition - offset;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isEditMode) return;

        Vector2 pointerPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pointerPos))
        {
            rectTransform.localPosition = pointerPos + offset;
        }
    }
}
