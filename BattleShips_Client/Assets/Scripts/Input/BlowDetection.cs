using System.Linq;
using Shared.Scripts;
using TMPro;
using UnityEngine;

namespace Input
{
	public class BlowDetection : MonoBehaviour
	{
		[SerializeField] private int sampleWindow = 64;

		[SerializeField] private TMP_Text logText;

		[SerializeField] private int micUpdatesPerSecond = 1;

		private AudioClip _microphoneClip;
		private string _microphoneName;

		private float _accumulatedTime;
		private float _dt;

		private void Start()
		{
			_dt = 1f / micUpdatesPerSecond;
		}

		private void Update()
		{
			_accumulatedTime += Time.deltaTime;

			float loudness = GetLoudnessFromMicrophone();

			string allMics = Microphone.devices.Aggregate("", (current, device) => current + device + "\n");
			logText.text = $"{allMics}---\nLoudness: {loudness}\nBlowing: {loudness > SettingsManager.Instance.microphoneThreshold}";

			if (_accumulatedTime < _dt) return;
			_accumulatedTime = 0;

			WebsocketClient.Instance.Send(MessageFactory.CreateBlowingUpdate(loudness > SettingsManager.Instance.microphoneThreshold));
		}

		private void OnEnable()
		{
			MicrophoneToAudioClip();
		}

		private void OnDisable()
		{
			Microphone.End(_microphoneName);
		}

		private void MicrophoneToAudioClip()
		{
			_microphoneName = Microphone.devices[0];
			// Debug.Log($"Microphone name: {_microphoneName}");
			_microphoneClip = Microphone.Start(_microphoneName, true, 20, AudioSettings.outputSampleRate);
		}

		private float GetLoudnessFromMicrophone()
		{
			if (_microphoneName == null) MicrophoneToAudioClip(); //setup mic if it's not already
			return GetLoudnessFromAudioClip(Microphone.GetPosition(_microphoneName), _microphoneClip);
		}

		private float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
		{
			int startPosition = clipPosition - sampleWindow;
			if (startPosition < 0) return 0;

			float[] waveData = new float[sampleWindow];
			clip.GetData(waveData, startPosition);

			//compute loudness
			float totalLoudness = 0;
			for (int i = 0; i < sampleWindow; i++)
			{
				totalLoudness += Mathf.Abs(waveData[i]);
			}

			return totalLoudness / sampleWindow;
		}
	}
}
