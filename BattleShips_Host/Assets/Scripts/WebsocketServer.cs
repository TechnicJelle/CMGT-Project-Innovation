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
	public static readonly Color[] PlayerColours = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, };
	public static WebsocketServer Instance { get; private set; }
	private const int PORT = 55555;
	private const string PATH = "/game";
	private WebSocketSessionManager Sessions => _server.WebSocketServices[PATH].Sessions;

	// ReSharper disable once InconsistentNaming
	[NonSerialized] public List<string> IDs;
	[NonSerialized] public Dictionary<string, ClientEntry> Clients;

	public delegate void RefreshUI();
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
			Clients = new Dictionary<string, ClientEntry>();

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
		if (_server == null) return; //server was never started
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
			OnRefreshUI.Invoke();
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
	/// Send a message to one specific client
	/// </summary>
	public void Send(string id, byte[] bytes)
	{
		// Debug.Log(BitConverter.ToString(bytes));
		Sessions.SendToAsync(bytes, id, null);
	}

	/// <summary>
	/// Sends a message to all connected clients
	/// </summary>
	public void Broadcast(byte[] bytes)
	{
		Sessions.BroadcastAsync(bytes, null);
	}

	public class ClientEntry
	{
		public string Name;
		public Color Colour;

		public ClientEntry(string name, Color colour)
		{
			Name = name;
			Colour = colour;
		}
	}
}

public class Game : WebSocketBehavior
{
	private static WebsocketServer Server => WebsocketServer.Instance;

	protected override void OnOpen()
	{
		Debug.Log($"Connection Opened with {ID}");
		Server.IDs.Add(ID);
		int colourIndex = (Server.IDs.Count - 1) % WebsocketServer.PlayerColours.Length; //possibly could be a problem with more players? needs testing
		Color colour = WebsocketServer.PlayerColours[colourIndex]; //BUG: This gets de-synced when a player leaves
		Server.Clients.Add(ID, new WebsocketServer.ClientEntry(ID, colour));
		RefreshUI();
	}

	protected override void OnMessage(MessageEventArgs e)
	{
		switch (MessageFactory.CheckMessageType(e.RawData))
		{
			case MessageFactory.MessageType.BoatDirectionUpdate:
				if (!MatchManager.IsMatchRunning) return;
				float direction = MessageFactory.DecodeBoatDirectionUpdate(e.RawData);
				MatchManager.Instance.UpdateBoatDirection(ID, direction);
				break;
			case MessageFactory.MessageType.BlowingUpdate:
				if (!MatchManager.IsMatchRunning) return;
				bool isBlowing = MessageFactory.DecodeBlowingUpdate(e.RawData);
				MatchManager.Instance.SetBoatBlowing(ID, isBlowing);
				break;
			case MessageFactory.MessageType.RequestDockingStatusUpdate:
				if (!MatchManager.IsMatchRunning) return;
				bool requestDockingStatus = MessageFactory.DecodeDockingStatusUpdate(e.RawData);
				if (requestDockingStatus) MatchManager.Instance.RequestDocking(ID);
				else MatchManager.Instance.RequestUndocking(ID);
				break;
			case MessageFactory.MessageType.SearchTreasureSignal:
				if (!MatchManager.IsMatchRunning) return;
				MatchManager.Instance.SearchTreasure(ID);
				break;
			case MessageFactory.MessageType.ShootingUpdate:
				if (!MatchManager.IsMatchRunning) return;
				MessageFactory.ShootingDirection shootingDirection = MessageFactory.DecodeShootingUpdate(e.RawData);
				MatchManager.Instance.BoatShoot(ID, shootingDirection);
				break;
			case MessageFactory.MessageType.RepairingSignal:
				if (!MatchManager.IsMatchRunning) return;
				MatchManager.Instance.RepairBoat(ID);
				break;
			case MessageFactory.MessageType.NameUpdate:
				if (MatchManager.IsMatchRunning) return; //can't change name while match is running
				string newName = MessageFactory.DecodeNameUpdate(e.RawData);
				Debug.Log($"{ID}'s new name: \"{newName}\"");
				Server.Clients[ID].Name = newName.IsNullOrEmpty() ? ID : newName;
				RefreshUI();
				break;
			case MessageFactory.MessageType.DamageBoat:
			case MessageFactory.MessageType.RepairDoneSignal:
			case MessageFactory.MessageType.StartGameSignal:
			case MessageFactory.MessageType.GoBackToLobbySignal:
			case MessageFactory.MessageType.DockingAvailableUpdate:
			case MessageFactory.MessageType.IsDockedUpdate:
			case MessageFactory.MessageType.TreasureResultUpdate:
			case MessageFactory.MessageType.ReloadUpdate:
			default:
				Debug.LogWarning($"Received a message from client {ID} that is not allowed! Ignoring...");
				break;
		}
	}

	protected override void OnClose(CloseEventArgs e)
	{
		Debug.Log($"Connection Closed with {ID}");
		Server.IDs.Remove(ID);
		Server.Clients.Remove(ID);
		RefreshUI();
	}

	private static void RefreshUI()
	{
		Server.SetUpdateUI();
	}
}
