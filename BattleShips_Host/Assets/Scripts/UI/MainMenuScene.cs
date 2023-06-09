using UnityEngine;

namespace UI
{
	public class MainMenuScene : MonoBehaviour
	{
		[SerializeField] private GameObject menuBackgroundScene;

		private void OnEnable()
		{
			menuBackgroundScene.SetActive(true);
		}

		private void OnDisable()
		{
			if (menuBackgroundScene != null)
				menuBackgroundScene.SetActive(false);
		}
	}
}
