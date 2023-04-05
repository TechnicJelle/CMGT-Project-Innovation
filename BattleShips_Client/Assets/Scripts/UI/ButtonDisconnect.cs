using Shared.Scripts.UI;

namespace UI
{
	public class ButtonDisconnect : ButtonViewNavigation
	{
		private void Start()
		{
			ThisButton.onClick.AddListener(AttemptDisconnect);
		}

		private void AttemptDisconnect()
		{
			WebsocketClient.Instance.Disconnect();
			SwitchPanel();
		}
	}
}
