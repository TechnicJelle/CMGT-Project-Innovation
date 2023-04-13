using Shared.Scripts.UI;

namespace UI
{
	public class ToDockedView : EventViewNavigation
	{
		private void Start()
		{
			WebsocketClient.Instance.OnDocked += SwitchPanel;
		}
	}
}
