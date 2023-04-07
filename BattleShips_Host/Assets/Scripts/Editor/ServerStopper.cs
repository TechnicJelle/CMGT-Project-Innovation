using UnityEditor;
using UnityEngine;

// Ensure class initializer is called whenever scripts recompile
namespace Editor
{
	[InitializeOnLoad]
	public static class ServerStopper
	{
		// Register an event handler when the class is initialized
		static ServerStopper()
		{
			EditorApplication.playModeStateChanged += PlayModeStateChanged;
		}

		private static void PlayModeStateChanged(PlayModeStateChange state)
		{
			// Debug.Log($"State changed to: {state}");
			if (state == PlayModeStateChange.ExitingEditMode)
				WebsocketServer.Instance.StopWebserver();
		}
	}
}
