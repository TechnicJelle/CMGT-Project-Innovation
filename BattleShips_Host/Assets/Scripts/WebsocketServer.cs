using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebsocketServer : MonoBehaviour
{
	public static WebsocketServer Instance { get; private set; }
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
			_server = new WebSocketServer(55555);
			_server.AddWebSocketService<Chat>(PATH);
			_gameHost = _server.WebSocketServices[PATH];
			IDs = new List<string>();

			_server.Start(); //TODO: Move this to the button that goes to the LobbyScreen, with a try-catch (Example: Address already in use)
			Debug.Log("Started websocket server...");
			return true;
		} catch (Exception e)
		{
			Debug.LogWarning($"Connection failed: {e.Message}");
			return false;
		}
	}

	public void StopWebserver()
	{
		Debug.Log("Stopping websocket server...");

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
