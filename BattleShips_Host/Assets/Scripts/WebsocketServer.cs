using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebsocketServer : MonoBehaviour
{
	public static WebsocketServer Instance { get; private set; }
	private const int PORT = 55555;
	private const string PATH = "/game";

	[NonSerialized] public bool ShouldUpdateUI;
	// ReSharper disable once InconsistentNaming
	[NonSerialized] public List<string> IDs;

	public delegate void RefreshUI(List<string> ids);
	public RefreshUI OnRefreshUI;

	private WebSocketServer _server;
	private WebSocketServiceHost _gameHost;

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
			_server.AddWebSocketService<Chat>(PATH);
			_gameHost = _server.WebSocketServices[PATH];
			IDs = new List<string>();

			_server.Start();
			Debug.Log("Started websocket server...");
			return true;
		} catch (Exception e)
		{
			Debug.LogWarning($"Connection failed: {e.Message}");
			return false;
		}
	}

	private void OnApplicationQuit() => StopWebserver();

	public void StopWebserver()
	{
		Debug.Log("Stopping websocket server...");
		if(_server == null) return; //server was never started

		//disconnect every client
		for (int i = IDs.Count - 1; i >= 0; i--)
		{
			string id = IDs[i];
			Debug.Log($"Disconnecting {id}...");
			_gameHost.Sessions.CloseSession(id, CloseStatusCode.Normal, "Server is shutting down");
		}

		_server.Stop();
	}

	private void Update()
	{
		if (ShouldUpdateUI)
		{
			ShouldUpdateUI = false;
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
}

public class Chat : WebSocketBehavior
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
		string input = e.Data.Trim();
		string output = $"{ID}: {input}";
		Debug.Log(output);
		Sessions.Broadcast(output);
		Send($"this only gets sent to client {ID}");
	}

	protected override void OnClose(CloseEventArgs e)
	{
		Debug.Log($"Connection Closed with {ID}");
		Server.IDs.Remove(ID);
		RefreshUI();
	}

	private static void RefreshUI()
	{
		Server.ShouldUpdateUI = true;
	}
}
