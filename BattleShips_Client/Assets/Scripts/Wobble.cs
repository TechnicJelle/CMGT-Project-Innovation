using System;
using System.Linq;
using System.Net.Sockets;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Wobble : MonoBehaviour
{
	[Tooltip("Lower is slower")] [Range(0f, 0.2f)] [SerializeField]
	private float turnSpeed = 0.1f;

	[SerializeField] private TMP_Text errorText;
	[SerializeField] private TMP_Text logText;

	[SerializeField] private GameObject buttons;
	[SerializeField] private GameObject slider;
	[SerializeField] private SliderHandler sliderHandler;

	private enum ControlType { Gyro, Accel, Buttons, Slider }
	[SerializeField] private ControlType controls;

	private float _boatDirection = 0f;

	private int _lastFrameIndex;
	private readonly float[] _frameDeltaTimeArray = new float[50];

	private void Start()
	{
		errorText.gameObject.SetActive(false);
		buttons.gameObject.SetActive(false);
		slider.gameObject.SetActive(false);
		switch (controls)
		{
			case ControlType.Gyro:
				if (SystemInfo.supportsGyroscope)
				{
					Debug.Log("Gyroscope is supported");
					Input.gyro.enabled = true;
				} else {
					Debug.Log("Gyroscope not supported, switching to alternative controls.");
					controls = ControlType.Slider;
					errorText.gameObject.SetActive(true);
				}
				break;
			case ControlType.Accel:
				Debug.Log("Accelerometer controls enabled");
				break;
			case ControlType.Slider:
				slider.gameObject.SetActive(true);
				Debug.Log("Slider controls enabled");
				break;
		}
	}

	private void Update()
	{
		switch (controls)
		{
			case ControlType.Gyro:
				HandleGyro();
				break;
			case ControlType.Accel:
				HandleAccel();
				break;
			case ControlType.Slider:
				HandleSlider();
				break;
		}
		
		transform.rotation = Quaternion.Lerp(transform.rotation,
			Quaternion.Euler(0, _boatDirection, 0), //target rotation
			turnSpeed);
		
		_frameDeltaTimeArray[_lastFrameIndex] = Time.unscaledDeltaTime;
		_lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;
	}

	private void CheckControls()
	{
		
	}

	private void HandleGyro()
	{
		Quaternion gyro = Input.gyro.attitude;
		_boatDirection = -gyro.eulerAngles.z + 180;
		
		logText.text =
			$"FPS: {Mathf.RoundToInt(CalculateFPS())}\nGyro: {gyro}\nEuler: {gyro.eulerAngles}\nRot: {_boatDirection}\nInterval: {Input.gyro.updateInterval}\nCompass: {Input.compass.magneticHeading}";
	}

	private void HandleAccel()
	{
		Vector3 accel = Input.acceleration;
		_boatDirection = Mathf.Atan2(accel.y, accel.x) * Mathf.Rad2Deg + 90;

		logText.text =
			$"FPS:{Mathf.RoundToInt(CalculateFPS())}\nAccel: {accel}\nZ Accel after mod: {_boatDirection}";
	}

	private void HandleSlider()
	{
		if (sliderHandler.Pressed)
		{
			var dir = Input.mousePosition - slider.transform.position;
			_boatDirection = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
			slider.transform.rotation = Quaternion.AngleAxis(_boatDirection, Vector3.forward);
			_boatDirection *= -1;
		}
		
		logText.text =
			$"FPS: {Mathf.RoundToInt(CalculateFPS())}\nRot: {_boatDirection}";
	}

	private float CalculateFPS()
	{
		float total = _frameDeltaTimeArray.Sum();
		return _frameDeltaTimeArray.Length / total;
	}
}
