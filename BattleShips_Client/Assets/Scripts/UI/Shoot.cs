using Shared.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Shoot : MonoBehaviour
	{
		[SerializeField] private MessageFactory.ShootingDirection shootingDirection;
		[SerializeField] private Slider slider;
		public Slider Slider => slider;

		private Button _thisButton;
		public bool CanShoot {get; set; }

		private void Awake()
		{
			_thisButton = GetComponent<Button>();
			_thisButton.onClick.AddListener(ShootCannon);
		}

		private void ShootCannon()
		{
			Debug.Log($"CanShoot: {CanShoot}");
			if (CanShoot)
			{
				CanShoot = false;
				SoundManager.Instance.PlaySound(SoundManager.Sound.Shooting);
			}
			
			WebsocketClient.Instance.Send(MessageFactory.CreateShootingUpdate(shootingDirection));
		}
	}
}
