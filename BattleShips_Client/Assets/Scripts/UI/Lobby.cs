using Shared.Scripts.UI;

namespace UI
{
	public class Lobby : EventViewNavigation
	{
		private void Start()
		{
			WebsocketClient.Instance.OnConnected += SwitchPanel;
		}
	}
}
