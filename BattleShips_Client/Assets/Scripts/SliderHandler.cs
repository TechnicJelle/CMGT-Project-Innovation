using UnityEngine;
using UnityEngine.EventSystems;

public class SliderHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] private bool updateInEditor;
	[Range(0f, 1f)] [SerializeField] private float offsetFactor = 0.4f;

	public bool Pressed { get; private set; }

	private void Awake()
	{
		//ensure that the handle always actually fits fully on screen
		RectTransform rectTransform = GetComponent<RectTransform>();
		Vector3 rectTransformPosition = rectTransform.localPosition;
		rectTransformPosition.y = Mathf.Min(Screen.width, Screen.height) * offsetFactor;
		rectTransform.localPosition = rectTransformPosition;
	}

	private void OnValidate()
	{
		if (updateInEditor)
			Awake();
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
