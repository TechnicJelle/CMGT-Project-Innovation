using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Docking : MonoBehaviour
	{
		private Button _thisButton;

		private void Awake()
		{
			_thisButton = GetComponent<Button>();
			_thisButton.interactable = false;
			_thisButton.onClick.AddListener(Dock);
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

		private void Dock()
		{
			// WebsocketClient.Instance.Send(MessageFactory.EncodeDockingRequest());
		}
	}
}
