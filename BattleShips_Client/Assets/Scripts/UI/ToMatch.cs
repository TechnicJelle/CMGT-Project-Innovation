using Shared.Scripts.UI;

namespace UI
{
	public class ToMatch : EventViewNavigation
	{
		private void Start()
		{
			WebsocketClient.Instance.OnMatchStart += SwitchPanel;
		}
	}
}
