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
			_text.text = _startText;
		}

		private void RebuildClientList(List<string> ids)
		{
			Debug.Log("Rebuilding Client List...");

			StringBuilder stringBuilder = new();
			foreach (string id in ids)
			{
				stringBuilder.AppendLine(id);
			}

			_text.text = stringBuilder.ToString();
		}
	}
}
