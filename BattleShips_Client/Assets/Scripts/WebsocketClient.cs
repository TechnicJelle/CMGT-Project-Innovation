using System;
using UnityEngine;
using WebSocketSharp;

public class WebsocketClient : MonoBehaviour
{
	public static WebsocketClient Instance { get; private set; }

	private WebSocket _webSocket;

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;

		// Debug.Log("Connection: " + Connect("127.0.0.1", 55555));
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
			_webSocket.OnMessage += (object _, MessageEventArgs e) => { Debug.Log($"WS Received message: {e.Data}"); };
			_webSocket.Connect();
			return _webSocket.IsAlive;
		}
		catch (Exception e)
		{
			Debug.LogWarning($"WS Connection failed: {e.Message}");
			return false;
		}
	}

	public void Disconnect()
	{
		Debug.Log("Disconnecting...");
		_webSocket?.Close();
	}

	private void FixedUpdate()
	{
		//every second
		if (Time.timeSinceLevelLoad % 1 < Time.fixedDeltaTime && (_webSocket?.IsAlive ?? false))
			_webSocket?.Send($"Hello {Time.timeSinceLevelLoad}"); //TODO: Remove this and send more sensible data
	}
}
