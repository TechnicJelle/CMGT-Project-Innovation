using Shared.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Shoot : MonoBehaviour
	{
		[SerializeField] private MessageFactory.ShootingDirection shootingDirection;

		private Button _thisButton;

		private void Awake()
		{
			_thisButton = GetComponent<Button>();
			_thisButton.onClick.AddListener(ShootCannon);
		}

		private void ShootCannon()
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateShootingUpdate(shootingDirection));
			SoundManager.Instance.PlaySound(SoundManager.Sound.Shooting);
		}
	}
}
