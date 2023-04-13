using Shared.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SearchTreasure : MonoBehaviour
	{
		private Button _thisButton;
		private TMP_Text _buttonText;

		private string _defaultText;
		[SerializeField] private string searchingText = "Searching...";
		[SerializeField] private string foundText = "Found!";

		private void Awake()
		{
			_thisButton = GetComponent<Button>();
			_buttonText = GetComponentInChildren<TMP_Text>();
			_defaultText = _buttonText.text;

			_thisButton.onClick.AddListener(RequestSearch);
			WebsocketClient.Instance.OnFoundTreasure += FoundTreasure;
		}

		private void OnEnable()
		{
			_thisButton.interactable = true;
			_buttonText.text = _defaultText;
		}

		private void RequestSearch()
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateSignal(MessageFactory.MessageType.SearchTreasureSignal));
			_thisButton.interactable = false;
			_buttonText.text = searchingText;
		}

		private void FoundTreasure()
		{
			_buttonText.text = foundText;
		}
	}
}