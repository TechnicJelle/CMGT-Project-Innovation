using Shared.Scripts;
using UnityEngine;

namespace Input
{
	public class BlowDetection : MonoBehaviour
	{
		[SerializeField] private float threshold = 0.1f;
		[SerializeField] private int sampleWindow = 64;

		[SerializeField] private int micUpdatesPerSecond = 1;

		private AudioClip _microphoneClip;

		private float _accumulatedTime;
		private float _dt;

		private void Start()
		{
			_dt = 1f / micUpdatesPerSecond;
			MicrophoneToAudioClip();
		}

		private void Update()
		{
			_accumulatedTime += Time.deltaTime;

			float loudness = GetLoudnessFromMicrophone();

			if (_accumulatedTime < _dt) return;
			_accumulatedTime = 0;

			WebsocketClient.Instance.Send(MessageFactory.CreateBlowingUpdate(loudness > threshold));
		}

		private void MicrophoneToAudioClip()
		{
			string microphoneName = Microphone.devices[0];
			Debug.Log($"Microphone name: {microphoneName}");
			_microphoneClip = Microphone.Start(microphoneName, true, 20, AudioSettings.outputSampleRate);
		}

		private float GetLoudnessFromMicrophone()
		{
			return GetLoudnessFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), _microphoneClip);
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
