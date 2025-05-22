using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public bool isPressed = false;

    void Start()
    {
        //PauseManager.instance.OnPause.AddListener(UnTouch);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    void UnTouch()
    {
        isPressed = false; // When revive menu comes up the touch controls continue to hold the left or right so the player would go unintentionally. this function is for that.
    }
}
