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
			WebsocketClient.Instance.OnMatchStart += () => { _thisButton.interactable = false; }; //TODO: This is not happening for some reason

			WebsocketClient.Instance.OnDockingAvailable += () => { _thisButton.interactable = true; };

			WebsocketClient.Instance.OnDockingUnavailable += () => { _thisButton.interactable = false; };
		}
		private static void RequestDock()
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateDockingStatusUpdate(true));
		}
	}
}
