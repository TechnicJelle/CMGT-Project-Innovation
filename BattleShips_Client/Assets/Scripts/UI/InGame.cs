using Shared.Scripts.UI;

namespace UI
{
	public class InGame : EventViewNavigation
	{
		private void Start()
		{
			WebsocketClient.Instance.OnGoBackToLobby += SwitchPanel;
		}
	}
}
