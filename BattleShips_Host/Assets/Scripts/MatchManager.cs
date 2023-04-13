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

	public void AddPoint(Boat boat)
	{
		if (_players == null) return;
		PlayerData player = GetPlayerData(boat);
		player.Points++;
		if (player.Points > maxTreasure)
			EndMatch(player);
		Debug.Log($"Player {player} got to an island and now has {player.Points}");
	}

	private PlayerData GetPlayerData(Boat boat)
	{
		if (_players == null) return null;
		foreach (PlayerData player in _players.Values)
		{
			if (player.Boat == boat)
				return player;
		}

		Debug.LogError("Could not find boat in list");
		return null;
	}

	public void SetBoatBlowing(string id, bool blowing)
	{
		_players?[id].Boat.SetBlowing(blowing);
	}
}

public class PlayerData
{
	public readonly Boat Boat;
	public readonly string Name;
	public int Points;
	public PlayerData (Boat pBoat, string pName)
	{
		Boat = pBoat;
		Name = pName;
		Points = 0;
	}
}
