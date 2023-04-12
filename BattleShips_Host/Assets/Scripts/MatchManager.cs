using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Shared.Scripts;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
	public static MatchManager Instance { get; private set; }
	public static bool IsMatchRunning => Instance._players != null;

	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera gameCamera;

	[CanBeNull] private Dictionary<string, PlayerData> _players;
	
	private enum Cameras
	{
		Main,
		Game
	}

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
		ChangeCamera(Cameras.Main);
	}

	public void StartMatch()
	{
		_players = new Dictionary<string, PlayerData>();
		WebsocketServer.Instance.Broadcast(MessageFactory.CreateStartGameSignal());
		foreach (string id in WebsocketServer.Instance.IDs)
		{
			GameObject instance = Instantiate(playerPrefab, transform);
			instance.name = id;
			Boat boat = instance.GetComponent<Boat>();
			_players.Add(id, new PlayerData(boat));
			ImmersiveCamera.instance.AddPlayer(boat.transform);
		}
		ChangeCamera(Cameras.Game);
	}

	public void EndMatch()
	{
		if (_players == null) return; //match was never started
		WebsocketServer.Instance.Broadcast(MessageFactory.CreateGoBackToLobbySignal());
		foreach (PlayerData player in _players.Values)
		{
			Destroy(player.Boat.gameObject);
		}

		_players = null;
		ImmersiveCamera.instance.ClearPlayers();
		ChangeCamera(Cameras.Main);
	}

	public void UpdateBoatDirection(string id, float direction)
	{
		_players?[id].Boat.SetTargetDirection(direction);	
	}

	private void ChangeCamera(Cameras mode)
	{
		mainCamera.enabled = mode == Cameras.Main;
		gameCamera.enabled = mode == Cameras.Game;
	}

	public void AddPoint(Boat boat)
	{
		if (_players == null) return;
		foreach (PlayerData player in _players.Values)
		{
			if (player.Boat == boat)
			{
				player.Points++;
				if (player.Points > 3)
					EndMatch(); //TODO: Return to another screen when the game is over
				Debug.Log($"Player {player} got to an island and now has {player.Points}");
				return;
			}
		}
	}
}

class PlayerData
{
	public readonly Boat Boat;
	public int Points;
	public PlayerData (Boat pBoat)
	{
		Boat = pBoat;
		Points = 0;
	}
}