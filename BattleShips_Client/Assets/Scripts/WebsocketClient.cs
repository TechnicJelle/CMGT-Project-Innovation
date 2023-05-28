using System;
using System.Collections.Generic;
using Shared.Scripts;
using Shared.Scripts.UI;
using UI;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class WebsocketClient : MonoBehaviour
{
	public static WebsocketClient Instance { get; private set; }

	public Action OnMatchStart;
	public Action OnGoBackToLobby;
	public Action OnDockingAvailable;
	public Action OnDockingUnavailable;
	public Action OnDocked;
	public Action OnUndocked;
	public Action OnFoundTreasure;

	[SerializeField] private View mainMenu;

	[SerializeField] private Shoot portButton;
	[SerializeField] private Shoot starboardButton;

	private WebSocket _webSocket;

	private bool _shouldDisconnect;
	private bool _shouldStartMatch;
	private bool _shouldGoBackToLobby;
	private bool _shouldUpdateDockingAvailable;
	private bool _isDockingAvailable;
	private bool _shouldUpdateDocked;
	private bool _isDocked;
	private bool _shouldUpdateFoundTreasure;
	private bool _shouldDamage;
	private bool _shouldDie;

	private enum ReloadSoundState
	{
		Ready,
		Start,
		Playing,
	}

	private readonly Dictionary<MessageFactory.ShootingDirection, float> _reloadTimers = new();
	private readonly Dictionary<MessageFactory.ShootingDirection, ReloadSoundState> _reloadSounds = new();

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;

		foreach (MessageFactory.ShootingDirection dir in Enum.GetValues(typeof(MessageFactory.ShootingDirection)))
		{
			_reloadTimers.Add(dir, 0f);
			_reloadSounds.Add(dir, ReloadSoundState.Ready);
		}
	}

	public bool Connect(string ip, int port) =>
		Connect($"ws://{ip}:{port}");

	public bool Connect(string link)
	{
		Debug.Log($"Connecting to {link}...");
		_webSocket?.Close();
		try
		{
			_webSocket = new WebSocket(link);
			_webSocket.OnOpen += (_, _) =>
			{
				Debug.Log("WS Connected");
				SoundManager.Instance.PlaySound(SoundManager.Sound.Joining);
			};

			_webSocket.OnMessage += (object _, MessageEventArgs e) =>
			{
				// Debug.Log(BitConverter.ToString(e.RawData));
				switch (MessageFactory.CheckMessageType(e.RawData))
				{
					case MessageFactory.MessageType.StartGameSignal:
						Debug.Log("Start game signal received from server!");
						_shouldStartMatch = true;
						break;
					case MessageFactory.MessageType.GoBackToLobbySignal:
						Debug.Log("Go back to lobby signal received from server!");
						_shouldGoBackToLobby = true;
						break;
					case MessageFactory.MessageType.DockingAvailableUpdate:
						Debug.Log("Docking available update received from server!");
						_shouldUpdateDockingAvailable = true;
						_isDockingAvailable = MessageFactory.DecodeDockingAvailableUpdate(e.RawData);
						break;
					case MessageFactory.MessageType.IsDockedUpdate:
						_shouldUpdateDocked = true;
						_isDocked = MessageFactory.DecodeIsDockedUpdate(e.RawData);
						break;
					case MessageFactory.MessageType.FoundTreasureSignal:
						Debug.Log("Found treasure signal received from server!");
						_shouldUpdateFoundTreasure = true;
						break;
					case MessageFactory.MessageType.ReloadUpdate:
						Debug.Log("Reload update received from server!");
						(MessageFactory.ShootingDirection dir, float progress) = MessageFactory.DecodeReloadUpdate(e.RawData);
						_reloadTimers[dir] = progress;

						//Reload sound, start playing at 20% progress, to put the final click of the sound at 100%
						//(reload is 2s, sound is 2s with the click at 1.5s)
						if (_reloadSounds[dir] == ReloadSoundState.Ready && progress >= 0.2f) _reloadSounds[dir] = ReloadSoundState.Start;
						if (progress >= 1f) _reloadSounds[dir] = ReloadSoundState.Ready;
						break;
					case MessageFactory.MessageType.DamageBoat:
						bool shouldDie = MessageFactory.DecodeDamageBoat(e.RawData);
						_shouldDie = shouldDie;
						_shouldDamage = !shouldDie;
						Debug.Log("Boat HIT, should rumble");
						break;
					
					case MessageFactory.MessageType.BoatDirectionUpdate:
					case MessageFactory.MessageType.BlowingUpdate:
					case MessageFactory.MessageType.RequestDockingStatusUpdate:
					case MessageFactory.MessageType.SearchTreasureSignal:
					case MessageFactory.MessageType.ShootingUpdate:
					default:
						Debug.LogWarning("Received a message from the server that is not allowed! Ignoring...");
						break;
				}
			};
			_webSocket.OnClose += (object _, CloseEventArgs e) =>
			{
				Debug.Log($"WS Disconnected from server-side: {e.Reason}");
				DisconnectClient();
			};
			_webSocket.Connect();
			return _webSocket.IsAlive;
		}
		catch (Exception e)
		{
			Debug.LogWarning($"WS Connection failed: {e.Message}");
			return false;
		}
	}

	public void Send(byte[] bytes)
	{
#if UNITY_EDITOR
		if (_webSocket == null)
		{
			Debug.LogWarning("Make sure to start the game from the Main Menu! ;D");
			return;
		}
#endif
		_webSocket.SendAsync(bytes, null);
	}

	private void OnApplicationQuit() => DisconnectClient();

	public void DisconnectClient()
	{
		Debug.Log("Disconnecting...");
		_webSocket?.Close();
		_shouldDisconnect = true;
	}

	private void Update()
	{
		if (_shouldDisconnect)
		{
			_shouldDisconnect = false;
			foreach (View view in FindObjectsOfType<View>(true))
			{
				if (view == mainMenu) view.Show(); //TODO: And show popup for reason
				else view.Hide();
			}
		}

		if (_shouldStartMatch)
		{
			_shouldStartMatch = false;
			OnMatchStart.Invoke();
		}

		if (_shouldGoBackToLobby)
		{
			_shouldGoBackToLobby = false;
			OnGoBackToLobby.Invoke();
		}

		if (_shouldUpdateDockingAvailable)
		{
			_shouldUpdateDockingAvailable = false;
			if (_isDockingAvailable) OnDockingAvailable.Invoke();
			else OnDockingUnavailable.Invoke();
		}

		if (_shouldUpdateDocked)
		{
			_shouldUpdateDocked = false;
			if (_isDocked)
			{
				OnDocked.Invoke();
				SoundManager.Instance.PlaySound(SoundManager.Sound.Docking);
			}
			else OnUndocked.Invoke();
		}

		if (_shouldUpdateFoundTreasure)
		{
			_shouldUpdateFoundTreasure = false;
			OnFoundTreasure.Invoke();
		}

		if (_shouldDamage)
		{
			_shouldDamage = false;
			Vibrator.Vibrate(500);
			SoundManager.Instance.PlaySound(SoundManager.Sound.Damaged);
		}

		if (_shouldDie)
		{
			_shouldDie = false;
			Vibrator.Vibrate(1000);
			SoundManager.Instance.PlaySound(SoundManager.Sound.Death);
		}

		UpdateDir(portButton, MessageFactory.ShootingDirection.Port);
		UpdateDir(starboardButton, MessageFactory.ShootingDirection.Starboard);

		//Reload sound
		for (byte i = 0; i < _reloadSounds.Count; i++)
		{
			MessageFactory.ShootingDirection dir = (MessageFactory.ShootingDirection) i;
			if (_reloadSounds[dir] == ReloadSoundState.Start)
			{
				SoundManager.Instance.PlaySound(SoundManager.Sound.Reloading);
				_reloadSounds[dir] = ReloadSoundState.Playing;
			}
		}
	}

	private void UpdateDir(Shoot button, MessageFactory.ShootingDirection direction)
	{
		if (_reloadTimers[direction] > 1f && _reloadTimers[direction] < 1.5f) button.CanShoot = true;
		button.Slider.value = _reloadTimers[direction] > 1f ? 0f : _reloadTimers[direction]; //hide bar when not reloading
		Debug.Log(_reloadTimers[direction]);
	}
}
