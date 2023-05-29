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
#if UNITY_EDITOR
			if (WebsocketServer.Instance == null)
			{
				Debug.LogWarning("Make sure to start the game from the Main Menu, not from the Lobby! ;D");
				return;
			}
#endif
			RebuildClientList();
		}

		private void RebuildClientList()
		{
			StringBuilder stringBuilder = new();
			foreach (WebsocketServer.ClientEntry client in WebsocketServer.Instance.Clients.Values)
			{
				stringBuilder.AppendLine(client.Name);
			}

			_text.text = stringBuilder.ToString();
		}
	}
}
