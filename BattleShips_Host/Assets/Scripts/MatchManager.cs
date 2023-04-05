using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
	//public static MatchManager Instance { get; private set; }

	[SerializeField] private GameObject playerPrefab;

	[CanBeNull] private Dictionary<string, GameObject> _players;

	private void Awake()
	{
		/*if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;*/

		if (playerPrefab == null)
			Debug.LogError("Player prefab is null");
	}

	public void StartMatch()
	{
		_players = new Dictionary<string, GameObject>();
		foreach (string id in WebsocketServer.Instance.IDs)
		{
			GameObject instance = Instantiate(playerPrefab, transform);
			instance.name = id;
			_players.Add(id, instance);
		}
	}

	public void EndMatch()
	{
		if (_players == null) return; //match was never started
		foreach (GameObject player in _players.Values)
		{
			Destroy(player);
		}
		_players = null;
	}
}
