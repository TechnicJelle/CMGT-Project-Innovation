using System;
using Shared.Scripts;
using TMPro;
using UnityEngine;

public class SteeringInput : MonoBehaviour
{
	private enum ControlType { Gyro, Accel, Buttons, Slider, }
	[SerializeField] private ControlType controls;

	[SerializeField] private TMP_Text errorText;
	[SerializeField] private GameObject buttons;
	[SerializeField] private GameObject slider;
	[SerializeField] private SliderHandler sliderHandler;

	[SerializeField] private float networkPositionUpdateFrequency = 1;
	private float _dt;
	private float _accumulator;

	private float _boatDirection = 0;

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
			case ControlType.Buttons:
				throw new NotImplementedException(); //TODO
			default:
				throw new ArgumentOutOfRangeException();
		}
		_dt = 1 / networkPositionUpdateFrequency;
	}

	private void OnEnable()
	{
		//reset rotation
		_boatDirection = 0;

		//reset slider position
		slider.transform.rotation = Quaternion.identity;
		sliderHandler.Reset();
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
			case ControlType.Buttons:
				throw new NotImplementedException(); //TODO
			default:
				throw new ArgumentOutOfRangeException();
		}

		_accumulator += Time.deltaTime;
		//To supposedly prevent a spiral of death
		// if (_accumulator > 0.2f)
		// 	_accumulator = 0.2f;

		//Send to server
		while (_accumulator > _dt)
		{
			WebsocketClient.Instance.Send(MessageFactory.CreateBoatDirectionUpdate(_boatDirection));
			_accumulator -= _dt;
		}
	}

	private void HandleGyro()
	{
		Quaternion gyro = Input.gyro.attitude;
		_boatDirection = -gyro.eulerAngles.z + 180;
	}

	private void HandleAccel()
	{
		Vector3 accel = Input.acceleration;
		_boatDirection = Mathf.Atan2(accel.y, accel.x) * Mathf.Rad2Deg + 90;
	}

	private void HandleSlider()
	{
		if (sliderHandler.Pressed)
		{
			Vector3 dir = Input.mousePosition - slider.transform.position;
			_boatDirection = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
			slider.transform.rotation = Quaternion.AngleAxis(_boatDirection, Vector3.forward);
			_boatDirection *= -1;
		}
	}
}
