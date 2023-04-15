using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace UI
{
	public class ClientsList : MonoBehaviour
	{
		private TMP_Text _text;

		private void Awake()
		{
			_text = GetComponent<TMP_Text>();
		}

		private void Start()
		{
			WebsocketServer.Instance.OnRefreshUI += RebuildClientList;
		}

		private void OnEnable()
		{
			if (WebsocketServer.Instance == null) Debug.LogWarning("Make sure to start the game from the Main Menu, not from the Lobby! ;D");
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
