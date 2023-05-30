using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using JetBrains.Annotations;
using Shared.Scripts;
using Shared.Scripts.UI;
using TMPro;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;


public class MatchManager : MonoBehaviour
{
	public static MatchManager Instance { get; private set; }
	public static bool IsMatchRunning => Instance._players != null;
	public static GameObject FlagPrefab => Instance.flagPrefab;

	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private CinemachineVirtualCamera menuCamera;
	[SerializeField] private CinemachineVirtualCamera matchCamera;
	[SerializeField] private CinemachineTargetGroup targetGroup;
	[SerializeField] private View gameView;
	[SerializeField] private View endMatchView;
	[SerializeField] private TMP_Text winnerText;

	[SerializeField] private Transform boundaries;
	[SerializeField] private Transform water;
	[SerializeField] private GameObject flagPrefab;

	[SerializeField] private float distanceBetweenSpawns = 8f;

	[SerializeField] private int maxTreasure = 3;
	[SerializeField] private float timeToGatherTreasure = 5f;
	[SerializeField] private float timeToRepair = 5f;
	[SerializeField] private int healthRepaired = 2;

	[CanBeNull] private Dictionary<string, PlayerData> _players;

	private Bounds _spawnBounds;

	[SerializeField] private EventReference music;
	[SerializeField] private EventReference startMusic;
	[SerializeField] private EventReference endMusic;
	[SerializeField] private EventReference click;
	private EventInstance _music;
	private EventInstance _startMusic;
	private EventInstance _endMusic;
	private EventInstance _click;
	private float _ending = 0f;
	private bool _onceToChorus = true;

	private enum Cameras
	{
		Menu,
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
		ChangeCamera(Cameras.Menu);

		//Boundaries
		List<Transform> children = boundaries.Cast<Transform>().ToList();
		_spawnBounds = new Bounds(children[0].position, Vector3.zero);
		foreach (Transform t in children)
		{
			_spawnBounds.Encapsulate(t.position);
		}
		_spawnBounds.Expand(-7f); //To ensure no boats get stuck in the boundary
	}

	public Vector3 GetValidSpawnLocation()
	{
		//random point in bounds
		Vector3 randomPoint = new(
			Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
			30,
			Random.Range(_spawnBounds.min.z, _spawnBounds.max.z));

		//check if point is above a collider
		if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 40f))
		{
			if (hit.transform == water)
			{
				if (_players != null)
				{
					//if point is too close to another player, try again
					if (_players.Values.Any(player => Vector3.Distance(player.Boat.transform.position, hit.point) < distanceBetweenSpawns))
					{
						return GetValidSpawnLocation();
					}
				}

				//safe!
				Debug.DrawLine(randomPoint, hit.point, Color.blue, 100f);
				return hit.point;
			}
		}

		//not safe, or didn't hit anything at all somehow... try again
		return GetValidSpawnLocation();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
	}

	private void Start()
	{
		_music = RuntimeManager.CreateInstance(music);
		_startMusic = RuntimeManager.CreateInstance(startMusic);
		_endMusic = RuntimeManager.CreateInstance(endMusic);
		_click = RuntimeManager.CreateInstance(click);
		_music.start();
	}

	public void StartMatch()
	{
		_onceToChorus = true;
		_startMusic.start();
		_music.setParameterByName("Sea sound", 1);
		_music.setParameterByName("chorus", 1);
		_music.setParameterByName("verse", 1);
		_music.setParameterByName("end", 0);
		StartCoroutine(StartMatchMusic());
		// music.setParameterByName("Sea sound", 0);  <-- For if we don't want the start music


		_players = new Dictionary<string, PlayerData>();
		WebsocketServer.Instance.Broadcast(MessageFactory.CreateSignal(MessageFactory.MessageType.StartGameSignal));
		foreach (string id in WebsocketServer.Instance.IDs)
		{
			GameObject instance = Instantiate(playerPrefab, transform);
			instance.name = id;
			instance.transform.position = GetValidSpawnLocation();
			Boat boat = instance.GetComponent<Boat>();
			WebsocketServer.ClientEntry client = WebsocketServer.Instance.Clients[id];
			boat.Setup(id, client, matchCamera.transform);
			_players.Add(id, new PlayerData(boat, client.Name));
			targetGroup.AddMember(boat.transform, 1, 1);
		}

		ChangeCamera(Cameras.Match);
	}

	/// <summary>
	/// This function should always be called when stopping the match
	/// </summary>
	public void Cleanup()
	{
		if (_players == null) return; //match was never started

		_music.setParameterByName("Sea sound", 0);
		_music.setParameterByName("chorus", 0);
		_music.setParameterByName("verse", 0);

		StartCoroutine(EndMatchMusic());

		WebsocketServer.Instance.Broadcast(MessageFactory.CreateSignal(MessageFactory.MessageType.GoBackToLobbySignal));
		foreach (PlayerData player in _players.Values)
		{
			player.Points = 0;
			Destroy(player.Boat.gameObject);
		}

		foreach (PlayerData player in _players.Values)
		{
			targetGroup.RemoveMember(player.Boat.transform);
		}
		_players = null;
		ChangeCamera(Cameras.Menu);
	}

	/// <summary>
	/// This function should only be called when a match is stopped by winning
	/// </summary>
	/// <param name="winner"></param>
	private void WinMatch(PlayerData winner)
	{
		Cleanup();

		StartCoroutine(PlayVictoryTune());

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
		menuCamera.enabled = mode == Cameras.Menu;
		matchCamera.enabled = mode == Cameras.Match;
	}

	public void SetBoatBlowing(string id, bool blowing)
	{
		_players?[id].Boat.SetBlowing(blowing);
	}

	public void BoatShoot(string id, MessageFactory.ShootingDirection shootingDirection)
	{
		_players?[id].Boat.SetShoot(shootingDirection);
	}

	public void RequestDocking(string id)
	{
		PlayerData player = _players?[id];
		if (player == null) return;
		Boat boat = player.Boat;
		if (player.IsDocked || boat.CollidingIsland == null)
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
		if (!player.IsDocked || boat.CollidingIsland == null)
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
		if (!player.IsDocked || player.Boat.CollidingIsland == null)
		{
			Debug.LogWarning($"Player {id} requested to search for treasure, but is not docked, or even colliding with an island!");
			return;
		}

		Debug.Log($"Player {id} is searching for treasure!");
		player.IsSearchingTreasure = true;
		player.ShouldStartSearchingTreasure = true;
	}

	public void RepairBoat(string id)
	{
		PlayerData player = _players?[id];
		if (player == null) return;
		if (!player.IsDocked || player.Boat.CollidingIsland == null)
		{
			Debug.LogWarning($"Player {id} requested to repair, but is not docked, or even colliding with an island!");
			return;
		}

		Debug.Log($"Player {id} is repairing!");
		player.ShouldStartRepairing = true;
		player.IsRepairing = true;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_click.start();
			StartCoroutine(ClickWait());
		}

		if (_players == null) return;
		foreach ((string id, PlayerData player) in _players)
		{
			if (player.ShouldStartSearchingTreasure)
			{
				StartCoroutine(SearchTreasureCoroutine(id, player));
				player.ShouldStartSearchingTreasure = false;
			}
		}

		foreach ((string id, PlayerData player) in _players)
		{
			if (player.ShouldStartRepairing)
			{
				StartCoroutine(RepairCoroutine(id, player));
				player.ShouldStartRepairing = false;
			}
		}
	}


	private IEnumerator SearchTreasureCoroutine(string id, PlayerData player)
	{
		yield return new WaitForSeconds(timeToGatherTreasure);
		if (!player.IsSearchingTreasure) yield break;

		player.IsSearchingTreasure = false;

		if (_onceToChorus)
		{
			_onceToChorus = false;
			_music.setParameterByName("verse", 0);
		}

		bool success = player.Boat.CollidingIsland == player.Boat.TreasureIsland;
		WebsocketServer.Instance.Send(id, MessageFactory.CreateTreasureResultUpdate(success));
		if (success)
		{
			SetPlayerPoints(id, player.Points+1);
			Debug.Log($"Player {id} found treasure! Points: {player.Points}");
			player.Boat.ChooseNewTreasureIsland();
		}
		else
		{
			Debug.Log($"Player {id} didn't find treasure!");
		}

	}

	private IEnumerator RepairCoroutine(string id, PlayerData player)
	{
		yield return new WaitForSeconds(timeToRepair);
		if (!player.IsRepairing) yield break;

		player.IsRepairing = false;

		if (_onceToChorus)
		{
			_onceToChorus = false;
			_music.setParameterByName("verse", 0);
		}

		WebsocketServer.Instance.Send(id, MessageFactory.CreateSignal(MessageFactory.MessageType.RepairDoneSignal));

		player.Boat.Heal(healthRepaired);

		Debug.Log($"Player {id} repaired their boat!");
	}

	private IEnumerator StartMatchMusic()
	{
		yield return new WaitForSeconds(4f);
		_startMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		_music.setParameterByName("Sea sound", 0);
	}

	private IEnumerator PlayVictoryTune()
	{
		yield return new WaitForSeconds(1f);
		_endMusic.start();
		StartCoroutine(EndVictoryTune());
	}

	private IEnumerator EndVictoryTune()
	{
		yield return new WaitForSeconds(3f);
		_endMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	private IEnumerator EndMatchMusic()
	{
		yield return new WaitForSeconds(0.1f);
		_ending += 0.025f;
		_music.setParameterByName("end", _ending);
		if (_ending < 1f)
		{
			StartCoroutine(EndMatchMusic());
		}
	}

	private IEnumerator ClickWait()
	{
		yield return new WaitForSeconds(0.3f);
		_click.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public int GetPlayerPoints(string playerId)
	{
		PlayerData player = _players?[playerId];
		if (player == null)
		{
			Debug.Log("Id not found when getting points. Id: "+playerId);
			return -1;
		}

		return _players[playerId].Points;
	}

	public void SetPlayerPoints(string playerId, int playerPoints)
	{
		PlayerData player = _players?[playerId];
		if (player == null)
		{
			Debug.Log("Id not found when setting points. Id: "+playerId);
			return;
		}

		player.Points = playerPoints;

		if (playerPoints >= maxTreasure)
		{
			WinMatch(player);
		}
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
	public bool IsRepairing;
	public bool ShouldStartRepairing;

	public PlayerData(Boat boat, string name)
	{
		Boat = boat;
		Name = name;
		Points = 0;
		IsDocked = false;
		IsSearchingTreasure = false;
	}
}
