using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace UI
{
	public class ClientsList : MonoBehaviour
	{
		private TMP_Text _text;
		private string _startText;

		private void Awake()
		{
			_text = GetComponent<TMP_Text>();
			_startText = _text.text;
		}

		private void Start()
		{
			WebsocketServer.Instance.OnRefreshUI += RebuildClientList;
		}

		private void OnEnable()
		{
			if (WebsocketServer.Instance.IDs?.Count == 0)
				_text.text = _startText;
			else
				RebuildClientList(WebsocketServer.Instance.IDs);
		}

		private void RebuildClientList(List<string> ids)
		{
			StringBuilder stringBuilder = new();
			foreach (string id in ids)
			{
				stringBuilder.AppendLine(id);
			}

			_text.text = stringBuilder.ToString();
		}
	}
}
