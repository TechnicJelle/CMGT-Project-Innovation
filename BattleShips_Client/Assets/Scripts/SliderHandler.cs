using UnityEngine;
using UnityEngine.EventSystems;

public class SliderHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private bool log;
    public bool Pressed { get; private set; } = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (log) Debug.Log("PointerDown");
        Pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (log) Debug.Log("PointerUp");
        Pressed = false;
    }
}
