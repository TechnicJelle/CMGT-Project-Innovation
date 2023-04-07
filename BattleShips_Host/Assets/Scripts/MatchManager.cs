using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
	public static MatchManager Instance { get; private set; }
	public static bool IsMatchRunning => Instance._players != null;

	[SerializeField] private GameObject playerPrefab;

	[CanBeNull] private Dictionary<string, Boat> _players;

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;

		if (playerPrefab == null)
			Debug.LogError("Player prefab is null");

		if (playerPrefab.GetComponent<Boat>() == null)
			Debug.LogError("Player prefab does not have a Boat component");
	}

	public void StartMatch()
	{
		_players = new Dictionary<string, Boat>();
		foreach (string id in WebsocketServer.Instance.IDs)
		{
			GameObject instance = Instantiate(playerPrefab, transform);
			instance.name = id;
			Boat boat = instance.GetComponent<Boat>();
			_players.Add(id, boat);
		}
	}

	public void EndMatch()
	{
		if (_players == null) return; //match was never started
		foreach (Boat player in _players.Values)
		{
			Destroy(player.gameObject);
		}

		_players = null;
	}

	public void UpdateBoatDirection(string id, float direction)
	{
		_players?[id].SetTargetDirection(direction);
	}
}
