using Shared.Scripts;
using TMPro;
using UnityEngine;

namespace UI
{
	public class NameChoice : MonoBehaviour
	{
		private TMP_InputField _inputField;

		private void Awake()
		{
			_inputField = GetComponent<TMP_InputField>();
			_inputField.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnEnable()
		{
			//In case this is a repeat join, the name is already set, so just send again what's already filled in.
			WebsocketClient.Instance.Send(MessageFactory.CreateNameUpdate(_inputField.text));
		}

		private static void OnValueChanged(string value)
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateNameUpdate(value));
		}
	}
}
