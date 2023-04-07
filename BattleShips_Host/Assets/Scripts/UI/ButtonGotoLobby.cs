using Shared.Scripts.UI;

namespace UI
{
	public class ButtonGotoLobby : ButtonViewNavigation
	{
		private void Start()
		{
			ThisButton.onClick.AddListener(AttemptStartServer);
		}

		private void AttemptStartServer()
		{
			if(WebsocketServer.Instance.StartWebserver())
				SwitchPanel();
		}
	}
}
