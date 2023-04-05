using UnityEngine;
using UnityEngine.UI;

namespace Shared.Scripts.UI
{
	[RequireComponent(typeof(Button))]
	public class ButtonPanelNavigation : MonoBehaviour
	{
		[SerializeField] private View enableView;

		private Button _thisButton;
		private View _parentView;

		private void Awake()
		{
			if(enableView == null)
				Debug.LogError("Enable View is null");
			_thisButton = GetComponent<Button>();
			_parentView = transform.parent.GetComponentInParent<View>();
		}

		private void OnEnable()
		{
			_thisButton.onClick.AddListener(SwitchPanel);
		}

		private void OnDisable()
		{
			_thisButton.onClick.RemoveListener(SwitchPanel);
		}

		public void SwitchPanel()
		{
			_parentView.Hide();
			enableView.Show();
		}
	}
}
