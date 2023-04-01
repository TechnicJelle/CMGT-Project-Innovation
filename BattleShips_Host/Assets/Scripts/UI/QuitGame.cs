using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Button))]
	public class QuitGame : MonoBehaviour
	{
		public static void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
			Application.Quit();
		}
	}
}
