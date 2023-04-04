using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttemptConnect : MonoBehaviour
{
	[SerializeField] private TMP_InputField input;
	[SerializeField] private Button joinButton;

	private Button _thisButton;

	private void Awake()
	{
		_thisButton = GetComponent<Button>();
		_thisButton.onClick.AddListener(Connect);
		input.onSubmit.AddListener(Connect);

		joinButton.gameObject.SetActive(false);
	}

	private void Connect(string link)
	{
		joinButton.gameObject.SetActive(WebsocketClient.Instance.Connect(link));
	}

	private void Connect()
	{
		joinButton.gameObject.SetActive(WebsocketClient.Instance.Connect(input.text));
	}
}
