using System;
using Shared.Scripts;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wobble : MonoBehaviour
{
	private enum ControlType { Gyro, Accel, Buttons, Slider, }
	[SerializeField] private ControlType controls;

	[SerializeField] private TMP_Text errorText;
	[SerializeField] private GameObject buttons;
	[SerializeField] private GameObject slider;
	[SerializeField] private SliderHandler sliderHandler;

	private float _boatDirection = 1;

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

		//Send to server
		if(Random.Range(0, 20) == 5) //TODO: Replace with Actual timing procedure
			WebsocketClient.Instance.Send(MessageFactory.CreateBoatDirectionUpdate(_boatDirection));
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
