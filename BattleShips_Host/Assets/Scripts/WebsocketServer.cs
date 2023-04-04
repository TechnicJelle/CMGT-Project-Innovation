using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebsocketServer : MonoBehaviour
{
	public static WebsocketServer Instance { get; private set; }
	private const string PATH = "/game";

	[SerializeField] private TMP_Text text;

	[NonSerialized] public bool ShouldUpdateText;
	[NonSerialized] public List<string> IDs;

	private WebSocketServer _server;
	private WebSocketServiceHost _gameHost;

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;

		_server = new WebSocketServer(55555);
		_server.AddWebSocketService<Chat>(PATH);
		_gameHost = _server.WebSocketServices[PATH];
	}

	private void OnEnable()
	{
		Debug.Log("Starting websocket server...");
		IDs = new List<string>();
		_server.Start();
	}

	private void Update()
	{
		if (ShouldUpdateText)
		{
			ShouldUpdateText = false;
			RebuildUI();
		}
	}

	private void RebuildUI()
	{
		Debug.Log("Rebuilding UI...");

		StringBuilder stringBuilder = new();
		foreach (string id in IDs)
		{
			stringBuilder.AppendLine(id);
		}

		text.text = stringBuilder.ToString();
	}

	private void OnDisable()
	{
		//TODO: The panel will probably be disabled when the game starts, so the websocket server will shut down when it shouldn't
		Debug.Log("Stopping websocket server...");

		//disconnect every client
		for (int i = IDs.Count - 1; i >= 0; i--)
		{
			string id = IDs[i];
			Debug.Log($"Disconnecting {id}...");
			_gameHost.Sessions.CloseSession(id);
		}

		_server.Stop();
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
		Server.ShouldUpdateText = true;
	}
}
