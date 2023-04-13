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
			_thisButton.interactable = false;
			_thisButton.onClick.AddListener(RequestDock);
		}

		private void Start()
		{
			WebsocketClient.Instance.OnDockingAvailable += () =>
			{
				_thisButton.interactable = true;
			};

			WebsocketClient.Instance.OnDockingUnavailable += () =>
			{
				_thisButton.interactable = false;
			};
		}

		private static void RequestDock()
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateDockingStatusUpdate(true));
		}
	}
}
