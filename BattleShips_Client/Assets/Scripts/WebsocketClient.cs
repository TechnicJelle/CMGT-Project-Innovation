using System;
using Shared.Scripts;
using Shared.Scripts.UI;
using UnityEngine;
using WebSocketSharp;

public class WebsocketClient : MonoBehaviour
{
	public static WebsocketClient Instance { get; private set; }

	public Action OnConnected;
	public Action OnGoBackToLobby;

	[SerializeField] private View mainMenu;

	private WebSocket _webSocket;

	private bool _shouldDisconnect;
	private bool _shouldStartGame;
	private bool _shouldGoBackToLobby;

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;
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
			_webSocket.OnOpen += (_, _) => { Debug.Log("WS Connected"); };
			_webSocket.OnMessage += (object _, MessageEventArgs e) =>
			{
				switch (MessageFactory.CheckMessageType(e.RawData))
				{
					case MessageFactory.MessageType.BoatDirectionUpdate:
						Debug.LogWarning("Received boat direction update from server, which is not allowed! Ignoring...");
						break;
					case MessageFactory.MessageType.StartGameSignal:
						Debug.Log("Start game signal received from server!");
						_shouldStartGame = true;
						break;
					case MessageFactory.MessageType.GoBackToLobbySignal:
						Debug.Log("Go back to lobby signal received from server!");
						_shouldGoBackToLobby = true;
						break;
					default:
						throw new ArgumentOutOfRangeException();
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

		if (_shouldStartGame)
		{
			_shouldStartGame = false;
			OnConnected.Invoke();
		}

		if (_shouldGoBackToLobby)
		{
			_shouldGoBackToLobby = false;
			OnGoBackToLobby.Invoke();
		}
	}
}
