namespace Shared.Scripts.UI
{
	public class ButtonAutoViewNavigation : ButtonViewNavigation
	{
		private void OnEnable()
		{
			ThisButton.onClick.AddListener(SwitchPanel);
		}

		private void OnDisable()
		{
			ThisButton.onClick.RemoveListener(SwitchPanel);
		}
	}
}
