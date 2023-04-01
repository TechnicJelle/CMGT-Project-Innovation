using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(RectTransform))]
	public class MainMenu : MonoBehaviour
	{
		private static MainMenu _instance;

		private void Awake()
		{
			if(_instance != null)
				Debug.LogError("There is more than one MainMenu in the scene");
			else
				_instance = this;

			//disable all other panels
			Canvas canvas = GetComponentInParent<Canvas>();
			RectTransform thisPanel = GetComponent<RectTransform>();

			foreach(RectTransform panel in canvas.GetComponentInChildren<RectTransform>())
			{
				if(panel != thisPanel)
					panel.gameObject.SetActive(false);
			}
		}
	}
}
