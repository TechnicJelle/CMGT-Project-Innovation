using System;
using System.Collections.Generic;
using Input;
using TMPro;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
	public static SettingsManager Instance;

	[SerializeField] private TMP_Dropdown dropdown;

	public Steering.ControlType controls = Steering.ControlType.Slider;
	public float microphoneThreshold = 0.1f;

	private void Awake()
	{
		if (Instance != null)
			Debug.LogError($"There is more than one {this} in the scene");
		else
			Instance = this;
	}

	private void Start()
	{
		dropdown.value = (int) controls;
		dropdown.options = new List<TMP_Dropdown.OptionData>();
		foreach (Steering.ControlType controlType in Enum.GetValues(typeof(Steering.ControlType)))
		{
			dropdown.options.Add(new TMP_Dropdown.OptionData(controlType.ToString()));
		}
	}
}
