using Shared.Scripts.UI;
using TMPro;
using UnityEngine;

namespace UI
{
	public class ButtonConnect : ButtonViewNavigation
	{
		[SerializeField] private TMP_InputField input;

		private new void Awake()
		{
			base.Awake();
			ThisButton.onClick.AddListener(AttemptConnect);
			input.onSubmit.AddListener(AttemptConnect);
		}

		private void AttemptConnect()
		{
			AttemptConnect(input.text);
		}

		private void AttemptConnect(string link)
		{
			if (WebsocketClient.Instance.Connect(link))
				SwitchPanel();
		}
	}
}
