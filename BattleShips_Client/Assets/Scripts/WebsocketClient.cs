using System;
using Shared.Scripts.UI;
using UnityEngine;
using WebSocketSharp;

public class WebsocketClient : MonoBehaviour
{
	public static WebsocketClient Instance { get; private set; }

	[SerializeField] private View mainMenu;

	private bool _shouldDisconnect;

	private WebSocket _webSocket;

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
			_webSocket.OnMessage += (object _, MessageEventArgs e) => { Debug.Log($"WS Received message: {e.Data}"); }; //TODO: Handle message
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
				if (view == mainMenu) view.Show(); //TODO: Show popup for reason
				else view.Hide();
			}
		}
	}

	private void FixedUpdate()
	{
		//every second
		if (Time.timeSinceLevelLoad % 1 < Time.fixedDeltaTime && (_webSocket?.IsAlive ?? false))
			_webSocket?.Send($"Hello {Time.timeSinceLevelLoad}"); //TODO: Remove this and send more sensible data
	}
}
