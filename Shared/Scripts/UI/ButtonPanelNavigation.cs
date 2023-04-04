using UnityEngine;
using UnityEngine.UI;

namespace Shared.Scripts.UI
{
	[RequireComponent(typeof(Button))]
	public class ButtonPanelNavigation : MonoBehaviour
	{
		[SerializeField] private RectTransform enablePanel;

		private Button _thisButton;
		private RectTransform _parentPanel;

		private void Awake()
		{
			if(enablePanel == null)
				Debug.LogError("Enable Panel is null");
			_thisButton = GetComponent<Button>();
			_parentPanel = transform.parent.GetComponent<RectTransform>();
		}

		private void OnEnable()
		{
			_thisButton.onClick.AddListener(EnablePanel);
		}

		private void OnDisable()
		{
			_thisButton.onClick.RemoveListener(EnablePanel);
		}

		private void EnablePanel()
		{
			_parentPanel.gameObject.SetActive(false);
			enablePanel.gameObject.SetActive(true);
		}
	}
}
