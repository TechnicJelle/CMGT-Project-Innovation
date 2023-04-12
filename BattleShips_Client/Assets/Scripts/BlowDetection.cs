using System.Linq;
using Input;
using Shared.Scripts;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MicrophoneLoudness))]
public class BlowDetection : MonoBehaviour
{
	private MicrophoneLoudness _microphoneLoudness;

	[SerializeField] private TMP_Text logText;
	[SerializeField] private int microphoneNetworkUpdateFrequency = 1;

	private float _accumulatedTime;
	private float _dt;

	private void Awake()
	{
		_microphoneLoudness = GetComponent<MicrophoneLoudness>();
		_dt = 1f / microphoneNetworkUpdateFrequency;
	}

	private void OnEnable()
	{
		_microphoneLoudness.Init();
	}

	private void Update()
	{
		_accumulatedTime += Time.deltaTime;

		float loudness = _microphoneLoudness.GetLoudnessFromMicrophone();

		string allMics = Microphone.devices.Aggregate("", (current, device) => current + device + "\n");
		logText.text = $"{allMics}---\nLoudness: {loudness}\nBlowing: {loudness > SettingsManager.Instance.microphoneThreshold}";

		if (_accumulatedTime < _dt) return;
		_accumulatedTime = 0;

		WebsocketClient.Instance.Send(MessageFactory.CreateBlowingUpdate(loudness > SettingsManager.Instance.microphoneThreshold));
	}

	private void OnDisable()
	{
		_microphoneLoudness.Cleanup();
	}
}
