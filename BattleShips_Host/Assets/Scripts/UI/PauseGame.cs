using UnityEngine;

namespace UI
{
	public class PauseGame : MonoBehaviour
	{
		public static void Pause()
		{
			Time.timeScale = 0;
		}

		public static void Resume()
		{
			Time.timeScale = 1;
		}
	}
}
