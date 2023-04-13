using System.Linq;
using InputSystems;
using Shared.Scripts;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MicrophoneLoudness))]
public class BlowDetection : MonoBehaviour
{
	private MicrophoneLoudness _microphoneLoudness;

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

		if (_accumulatedTime < _dt) return;
		_accumulatedTime = 0;

		float loudness = _microphoneLoudness.GetLoudnessFromMicrophone();
		bool blowing = loudness > SettingsManager.Instance.microphoneThreshold;
		WebsocketClient.Instance.Send(MessageFactory.CreateBlowingUpdate(blowing));
	}

	private void OnDisable()
	{
		_microphoneLoudness.Cleanup();
	}
}
