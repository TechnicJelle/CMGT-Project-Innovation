using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Shared.Scripts;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebsocketServer : MonoBehaviour
{
	public static WebsocketServer Instance { get; private set; }
	private const int PORT = 55555;
	private const string PATH = "/game";
	private WebSocketSessionManager Sessions => _server.WebSocketServices[PATH].Sessions;

	// ReSharper disable once InconsistentNaming
	[NonSerialized] public List<string> IDs;

	public delegate void RefreshUI(List<string> ids);

	public RefreshUI OnRefreshUI;

	private WebSocketServer _server;
	private bool _shouldUpdateUI;

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;
	}

	public bool StartWebserver()
	{
		try
		{
			_server = new WebSocketServer(PORT);
			_server.AddWebSocketService<Game>(PATH);
			IDs = new List<string>();

			_server.Start();
			Debug.Log("Started websocket server...");
			return true;
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Connection failed: {e.Message}");
			return false;
		}
	}

	private void OnApplicationQuit() => StopWebserver();

	public void StopWebserver()
	{
		if(_server == null) return; //server was never started
		Debug.Log("Stopping websocket server...");

		//disconnect every client
		for (int i = IDs.Count - 1; i >= 0; i--)
		{
			string id = IDs[i];
			Debug.Log($"Disconnecting {id}...");
			Sessions.CloseSession(id, CloseStatusCode.Normal, "Server is shutting down");
		}

		_server.Stop();
	}

	public void SetUpdateUI() => _shouldUpdateUI = true;

	private void Update()
	{
		if (_shouldUpdateUI)
		{
			_shouldUpdateUI = false;
			OnRefreshUI.Invoke(IDs);
		}
	}

	public static string GetLink()
	{
		return $"ws://{GetIPAddress()}:{PORT}{PATH}";
	}

	private static string GetIPAddress()
	{
		// https://stackoverflow.com/a/27376368/8109619
		using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
		socket.Connect("", 65530);
		if (socket.LocalEndPoint is not IPEndPoint endPoint)
			throw new Exception("Could not get local IP address");

		return endPoint.Address.ToString();
	}

	/// <summary>
	/// Sends a message to all connected clients
	/// </summary>
	public void Broadcast(byte[] bytes)
	{
		Sessions.BroadcastAsync(bytes, null);
	}
}

public class Game : WebSocketBehavior
{
	private static WebsocketServer Server => WebsocketServer.Instance;

	protected override void OnOpen()
	{
		Debug.Log($"Connection Opened with {ID}");
		Server.IDs.Add(ID);
		RefreshUI();
	}

	protected override void OnMessage(MessageEventArgs e)
	{
		switch (MessageFactory.CheckMessageType(e.RawData))
		{
			case MessageFactory.MessageType.BoatDirectionUpdate:
				if(!MatchManager.IsMatchRunning) return;
				float direction = MessageFactory.DecodeBoatDirectionUpdate(e.RawData);
				MatchManager.Instance.UpdateBoatDirection(ID, direction);
				break;
			case MessageFactory.MessageType.StartGameSignal:
				Debug.LogWarning("Received start game signal from client, which is not allowed! Ignoring...");
				break;
			case MessageFactory.MessageType.GoBackToLobbySignal:
				Debug.LogWarning("Received go back to lobby signal from client, which is not allowed! Ignoring...");
				break;
			case MessageFactory.MessageType.BlowingUpdate:
				bool isBlowing = MessageFactory.DecodeBlowingUpdate(e.RawData);
				MatchManager.Instance.SetBoatBlowing(ID, isBlowing);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	protected override void OnClose(CloseEventArgs e)
	{
		Debug.Log($"Connection Closed with {ID}");
		Server.IDs.Remove(ID);
		RefreshUI();
	}

	private static void RefreshUI()
	{
		Server.SetUpdateUI();
	}
}
