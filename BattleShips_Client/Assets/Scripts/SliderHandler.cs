using UnityEngine;
using UnityEngine.EventSystems;

public class SliderHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public bool Pressed { get; private set; }

	private void Awake()
	{
		//ensure that the handle always actually fits fully on screen
		RectTransform rectTransform = GetComponent<RectTransform>();
		Vector3 rectTransformPosition = rectTransform.localPosition;
		rectTransformPosition.y = Mathf.Min(Screen.width, Screen.height) * 0.3f;
		rectTransform.localPosition = rectTransformPosition;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Pressed = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		Pressed = false;
	}

	public void Reset()
	{
		Pressed = false;
	}
}
