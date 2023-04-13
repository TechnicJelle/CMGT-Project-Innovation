using InputSystems;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(MicrophoneLoudness), typeof(Slider))]
	public class MicrophoneSlider : MonoBehaviour
	{
		[SerializeField] private Image backgroundImage;
		[SerializeField] private Color blowingColor;
		[SerializeField] private Color notBlowingColor;

		private MicrophoneLoudness _microphoneLoudness;
		private Slider _slider;

		private void Awake()
		{
			_microphoneLoudness = GetComponent<MicrophoneLoudness>();
			_slider = GetComponent<Slider>();
		}

		private void OnEnable()
		{
			_microphoneLoudness.Init();
		}

		private void Update()
		{
			_slider.value = _microphoneLoudness.GetLoudnessFromMicrophone();
			backgroundImage.color = _slider.value > SettingsManager.Instance.microphoneThreshold
				? blowingColor : notBlowingColor;
		}

		private void OnDisable()
		{
			_microphoneLoudness.Cleanup();
		}
	}
}
