using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Shared.Scripts;
using Shared.Scripts.UI;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
	public static MatchManager Instance { get; private set; }
	public static bool IsMatchRunning => Instance._players != null;

	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera matchCamera;
	[SerializeField] private View gameView;
	[SerializeField] private View endMatchView;
	[SerializeField] private TMP_Text winnerText;

	[SerializeField] private int maxTreasure = 1;
	[SerializeField] private float timeToGatherTreasure = 5f;

	[CanBeNull] private Dictionary<string, PlayerData> _players;

	private enum Cameras
	{
		Main,
		Match,
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
		WebsocketServer.Instance.Broadcast(MessageFactory.CreateSignal(MessageFactory.MessageType.StartGameSignal));
		foreach (string id in WebsocketServer.Instance.IDs)
		{
			GameObject instance = Instantiate(playerPrefab, transform);
			instance.name = id;
			Boat boat = instance.GetComponent<Boat>();
			boat.ID = id;
			_players.Add(id, new PlayerData(boat, "Joe"));
			ImmersiveCamera.Instance.AddPlayer(boat.transform);
		}

		ImmersiveCamera.Instance.FitAllPlayers();
		ChangeCamera(Cameras.Match);
	}

	public void Cleanup()
	{
		if (_players == null) return; //match was never started
		WebsocketServer.Instance.Broadcast(MessageFactory.CreateSignal(MessageFactory.MessageType.GoBackToLobbySignal));
		foreach (PlayerData player in _players.Values)
		{
			player.Points = 0;
			Destroy(player.Boat.gameObject);
		}

		ImmersiveCamera.Instance.ClearPlayers();
		_players = null;
		ChangeCamera(Cameras.Main);
	}

	private void EndMatch(PlayerData winner)
	{
		Cleanup();
		winnerText.text = $"Winner:\n{winner.Name}";
		gameView.Hide();
		endMatchView.Show();
	}

	public void UpdateBoatDirection(string id, float direction)
	{
		_players?[id].Boat.SetTargetDirection(direction);
	}

	private void ChangeCamera(Cameras mode)
	{
		mainCamera.enabled = mode == Cameras.Main;
		matchCamera.enabled = mode == Cameras.Match;
	}

	public void SetBoatBlowing(string id, bool blowing)
	{
		_players?[id].Boat.SetBlowing(blowing);
	}

	public void RequestDocking(string id)
	{
		PlayerData player = _players?[id];
		if (player == null) return;
		Boat boat = player.Boat;
		if (player.IsDocked || boat.collidingIsland == null)
		{
			Debug.LogWarning($"Player {id} requested to dock, but is already docked, or not even colliding with an island!");
			return;
		}

		Debug.Log($"Player {id} is docking!");

		boat.go = false;
		player.IsDocked = true;
		WebsocketServer.Instance.Send(id, MessageFactory.CreateIsDockedUpdate(true));
	}

	public void RequestUndocking(string id)
	{
		PlayerData player = _players?[id];
		if (player == null) return;
		Boat boat = player.Boat;
		if (!player.IsDocked || boat.collidingIsland == null)
		{
			Debug.LogWarning($"Player {id} requested to undock, but is not docked, or even colliding with an island!");
			return;
		}

		Debug.Log($"Player {id} is undocking!");

		boat.go = true;
		player.IsDocked = false;
		player.IsSearchingTreasure = false;
		WebsocketServer.Instance.Send(id, MessageFactory.CreateIsDockedUpdate(false));
	}

	public void SearchTreasure(string id)
	{
		PlayerData player = _players?[id];
		if (player == null) return;
		if (!player.IsDocked || player.Boat.collidingIsland == null)
		{
			Debug.LogWarning($"Player {id} requested to search for treasure, but is not docked, or even colliding with an island!");
			return;
		}

		Debug.Log($"Player {id} is searching for treasure!");
		player.IsSearchingTreasure = true;
		player.ShouldStartSearchingTreasure = true;
	}

	private void Update()
	{
		if (_players == null) return;
		foreach ((string id, PlayerData player) in _players)
		{
			if (player.ShouldStartSearchingTreasure)
			{
				StartCoroutine(SearchTreasureCoroutine(id, player));
				player.ShouldStartSearchingTreasure = false;
			}
		}
	}

	private IEnumerator SearchTreasureCoroutine(string id, PlayerData player)
	{
		Debug.Log("pre-wait");
		yield return new WaitForSeconds(timeToGatherTreasure);
		Debug.Log("post-wait");
		if (!player.IsSearchingTreasure) yield break;

		player.IsSearchingTreasure = false;
		player.Points++;

		Debug.Log($"Player {id} found treasure! Points: {player.Points}");

		WebsocketServer.Instance.Send(id, MessageFactory.CreateSignal(MessageFactory.MessageType.FoundTreasureSignal));

		if (player.Points >= maxTreasure)
			EndMatch(player);
	}
}

public class PlayerData
{
	public readonly Boat Boat;
	public readonly string Name;
	public int Points;
	public bool IsDocked;
	public bool ShouldStartSearchingTreasure;
	public bool IsSearchingTreasure;

	public PlayerData(Boat pBoat, string pName)
	{
		Boat = pBoat;
		Name = pName;
		Points = 0;
		IsDocked = false;
		IsSearchingTreasure = false;
	}
}
