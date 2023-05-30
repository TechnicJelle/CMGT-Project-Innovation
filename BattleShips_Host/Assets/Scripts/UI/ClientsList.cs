using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ClientsList : MonoBehaviour
	{
		[SerializeField] private GameObject itemPrefab;

		private readonly List<GameObject> _children = new();

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
			//Clear old ones
			foreach (GameObject item in _children)
				Destroy(item);
			_children.Clear();

			//Add new ones
			for (int i = 0; i < WebsocketServer.Instance.Clients.Count; i++)
			{
				(_, WebsocketServer.ClientEntry client) = WebsocketServer.Instance.Clients.ElementAt(i);

				GameObject item = Instantiate(itemPrefab, transform);
				_children.Add(item);

				//Text
				item.GetComponentInChildren<TextMeshProUGUI>().text = client.Name;

				//Colour
				int colourIndex = i % WebsocketServer.PlayerColours.Length;
				Color colour = WebsocketServer.PlayerColours[colourIndex];
				client.Colour = colour;
				item.GetComponentInChildren<Image>().color = colour;
			}
		}
	}
}
