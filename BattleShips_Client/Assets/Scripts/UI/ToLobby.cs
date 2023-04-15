using Shared.Scripts.UI;

namespace UI
{
	public class ToLobby : EventViewNavigation
	{
		private void Start()
		{
			WebsocketClient.Instance.OnGoBackToLobby += SwitchPanel;
		}
	}
}
