using JetBrains.Annotations;
using UnityEngine;

namespace Input
{
	public class MicrophoneLoudness : MonoBehaviour
	{
		[SerializeField] private int sampleWindow = 64;

		private AudioClip _microphoneClip;
		[CanBeNull] private string _microphoneName = null;

		public void Init()
		{
			_microphoneName = Microphone.devices[0];
			// Debug.Log($"Microphone name: {_microphoneName}");
			_microphoneClip = Microphone.Start(_microphoneName, true, 20, AudioSettings.outputSampleRate);
		}

		public float GetLoudnessFromMicrophone()
		{
			if (_microphoneName == null) Debug.LogError("Microphone not running!");
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

		public void Cleanup()
		{
			Microphone.End(_microphoneName);
			_microphoneName = null;
		}
	}
}
