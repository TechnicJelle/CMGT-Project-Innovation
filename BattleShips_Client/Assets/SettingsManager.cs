using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
	public static SettingsManager Instance;

	public enum ControlType
	{
		Slider,
		// Buttons, //TODO: implement
		Accel,
		Gyro,
	}
	[NonSerialized] public ControlType Controls = ControlType.Slider;

	public float microphoneThreshold = 0.1f;

	[SerializeField] private Button btnSlider;
	[SerializeField] private Button btnButtons;
	[SerializeField] private Button btnAccel;
	[SerializeField] private Button btnGyro;


	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;

		//set buttons
		btnSlider.onClick.AddListener(() => SwitchControls(ControlType.Slider, btnSlider));
		btnButtons.interactable = false;
		btnAccel.onClick.AddListener(() => SwitchControls(ControlType.Accel, btnAccel));
		btnGyro.onClick.AddListener(() => SwitchControls(ControlType.Gyro, btnGyro));

		//set panel
		if (!SystemInfo.supportsAccelerometer) btnAccel.interactable = false;
		if (!SystemInfo.supportsGyroscope) btnGyro.interactable = false;

		//set default (for the formatting)
		SwitchControls(ControlType.Slider, btnSlider);
	}

	private void SwitchControls(ControlType newControls, Button thisButton)
	{
		Controls = newControls;
		foreach (Button button in new[] {btnSlider, btnButtons, btnAccel, btnGyro,})
		{
			button.GetComponentInChildren<TMP_Text>().fontStyle = button == thisButton ? FontStyles.Bold : FontStyles.Normal;
		}
	}
}
