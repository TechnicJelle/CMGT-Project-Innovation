using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera matchCamera;
	[SerializeField] private View gameView;
	[SerializeField] private View endMatchView;
	[SerializeField] private TMP_Text winnerText;

	[SerializeField] private Transform boundaries;
	[SerializeField] private Transform water;

	[SerializeField] private float distanceBetweenSpawns = 8f;

	[SerializeField] private int maxTreasure = 3;
	[SerializeField] private float timeToGatherTreasure = 5f;

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

		//Boundaries
		List<Transform> children = boundaries.Cast<Transform>().ToList();
		_spawnBounds = new Bounds(children[0].position, Vector3.zero);
		foreach (Transform t in children)
		{
			_spawnBounds.Encapsulate(t.position);
		}
		_spawnBounds.Expand(-7f); //To ensure no boats get stuck in the boundary
	}

	private Vector3 GetValidSpawnLocation()
	{
		//random point in bounds
		Vector3 randomPoint = new(
			UnityEngine.Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
			30,
			UnityEngine.Random.Range(_spawnBounds.min.z, _spawnBounds.max.z));

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
		StartCoroutine(StartMusic());
		// music.setParameterByName("Sea sound", 0);  <-- For if we don't want the start music


		_players = new Dictionary<string, PlayerData>();
		WebsocketServer.Instance.Broadcast(MessageFactory.CreateSignal(MessageFactory.MessageType.StartGameSignal));
		foreach (string id in WebsocketServer.Instance.IDs)
		{
			GameObject instance = Instantiate(playerPrefab, transform);
			instance.name = id;
			instance.transform.position = GetValidSpawnLocation();
			Boat boat = instance.GetComponent<Boat>();
			boat.ID = id;
			_players.Add(id, new PlayerData(boat, "Joe"));
			// ImmersiveCamera.Instance.AddPlayer(boat.transform); //TODO
		}

		// ImmersiveCamera.Instance.FitAllPlayers(); //TODO
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

		// ImmersiveCamera.Instance.ClearPlayers(); //TODO
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
	}

	private IEnumerator SearchTreasureCoroutine(string id, PlayerData player)
	{
		yield return new WaitForSeconds(timeToGatherTreasure);
		if (!player.IsSearchingTreasure) yield break;

		player.IsSearchingTreasure = false;
		player.Points++;

		if (_onceToChorus)
		{
			_onceToChorus = false;
			_music.setParameterByName("verse", 0);
		}

		Debug.Log($"Player {id} found treasure! Points: {player.Points}");

		WebsocketServer.Instance.Send(id, MessageFactory.CreateSignal(MessageFactory.MessageType.FoundTreasureSignal));

		if (player.Points >= maxTreasure)
		{
			_music.setParameterByName("Sea sound", 0);
			_music.setParameterByName("chorus", 0);
			_music.setParameterByName("verse", 0);

			StartCoroutine(EndMusic());
			StartCoroutine(EndingM());

			EndMatch(player);
		}
	}

	private IEnumerator StartMusic()
	{
		yield return new WaitForSeconds(4f);
		_startMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		_music.setParameterByName("Sea sound", 0);
	}


	private IEnumerator EndMusic()
	{
		yield return new WaitForSeconds(1f);
		_endMusic.start();
		StartCoroutine(EndEndMusic());
	}

	private IEnumerator EndEndMusic()
	{
		yield return new WaitForSeconds(3f);
		_endMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	private IEnumerator EndingM()
	{
		yield return new WaitForSeconds(0.1f);
		_ending += 0.025f;
		_music.setParameterByName("end", _ending);
		if (_ending < 1f)
		{
			StartCoroutine(EndingM());
		}
	}

	private IEnumerator ClickWait()
	{
		yield return new WaitForSeconds(0.3f);
		_click.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
