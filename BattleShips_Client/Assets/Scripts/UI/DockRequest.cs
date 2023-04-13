using Shared.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DockRequest : MonoBehaviour
	{
		private Button _thisButton;

		private void Awake()
		{
			_thisButton = GetComponent<Button>();
			_thisButton.onClick.AddListener(RequestDock);
		}

		private void Start()
		{
			// Disable when match starts
			WebsocketClient.Instance.OnMatchStart += () => { _thisButton.interactable = false; };

			// But because this Start method gets called _after_ the OnMatchStart event is fired for the first time,
			// we need to disable it here for the first match in the app's session as well
			_thisButton.interactable = false;

			WebsocketClient.Instance.OnDockingAvailable += () => { _thisButton.interactable = true; };

			WebsocketClient.Instance.OnDockingUnavailable += () => { _thisButton.interactable = false; };
		}
		private static void RequestDock()
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateDockingStatusUpdate(true));
		}
	}
}
