using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class TextIP : MonoBehaviour
	{
		private TMP_Text _text;
		private Button _button;

		private void Awake()
		{
			_text = GetComponent<TMP_Text>();
			_button = GetComponent<Button>();
		}

		private void Start()
		{
			_button.onClick.AddListener(CopyToClipboard);
		}

		private void OnEnable()
		{
			_text.text = WebsocketServer.GetLink();
		}

		private void CopyToClipboard()
		{
			GUIUtility.systemCopyBuffer = _text.text;
		}
	}
}
