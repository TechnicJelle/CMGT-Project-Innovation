using Shared.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Shoot : MonoBehaviour
	{
		[SerializeField] private MessageFactory.ShootingDirection shootingDirection;
		[SerializeField] private Slider slider;

		public float ReloadProgress
		{
			set => slider.value = value;
		}

		private Button _thisButton;
		private float _timeAtDisable;

		private void Awake()
		{
			_thisButton = GetComponent<Button>();
			_thisButton.onClick.AddListener(ShootCannon);
		}

		private void ShootCannon()
		{
			SoundManager.Instance.PlaySound(SoundManager.Sound.Shooting);
			WebsocketClient.Instance.Send(MessageFactory.CreateShootingUpdate(shootingDirection));
			Disable();
		}

		private void Disable()
		{
			_thisButton.interactable = false;
			_timeAtDisable = Time.timeSinceLevelLoad;
		}

		public bool IsNotEnabled()
		{
			return !_thisButton.interactable;
		}

		public void ReEnable()
		{
			if (Time.timeSinceLevelLoad - _timeAtDisable < 1f) return; //prevent button immediately re-enabling due to sync difference between host and client.
			_thisButton.interactable = true;
		}
	}
}
