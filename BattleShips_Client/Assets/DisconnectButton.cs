using Shared.Scripts.UI;

public class DisconnectButton : ButtonViewNavigation
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
