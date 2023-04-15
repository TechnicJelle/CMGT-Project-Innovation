using Shared.Scripts.UI;

namespace UI
{
	public class ToUndockView : EventViewNavigation
	{
		private void Start()
		{
			WebsocketClient.Instance.OnUndocked += SwitchPanel;
			WebsocketClient.Instance.OnGoBackToLobby += HideThisPanel;
		}
	}
}
