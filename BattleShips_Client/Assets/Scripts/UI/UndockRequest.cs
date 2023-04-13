using Shared.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UndockRequest : MonoBehaviour
	{

		private void Awake()
		{
			Button thisButton = GetComponent<Button>();
			thisButton.onClick.AddListener(RequestUndock);
		}

		private static void RequestUndock()
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateDockingStatusUpdate(false));
		}
	}
}
