using System;
using Shared.Scripts;
using UnityEngine;

namespace InputSystems
{
	public class Steering : MonoBehaviour
	{
		[SerializeField] private GameObject buttons;
		[SerializeField] private GameObject slider;
		[SerializeField] private SliderHandler sliderHandler;

		[SerializeField] private float positionNetworkUpdateFrequency = 1;

		private float _dt;
		private float _accumulator;

		private float _boatDirection = 0;

		private void Awake()
		{
			_dt = 1 / positionNetworkUpdateFrequency;
		}

		private void OnEnable()
		{
			//reset rotation
			_boatDirection = 0;

			//reset slider position
			slider.transform.localRotation = Quaternion.identity;

			Debug.Log($"Controls: {SettingsManager.Instance.Controls.ToString()}");

			//controls
			buttons.gameObject.SetActive(false);
			slider.gameObject.SetActive(false);
			switch (SettingsManager.Instance.Controls)
			{
				case SettingsManager.ControlType.Gyro:
					Debug.Log("Gyroscope is supported");
					Input.gyro.enabled = true;
					break;
				case SettingsManager.ControlType.Accel:
					Debug.Log("Accelerometer controls enabled");
					break;
				case SettingsManager.ControlType.Slider:
					slider.gameObject.SetActive(true);
					Debug.Log("Slider controls enabled");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void Update()
		{
			switch (SettingsManager.Instance.Controls)
			{
				case SettingsManager.ControlType.Gyro:
					HandleGyro();
					break;
				case SettingsManager.ControlType.Accel:
					HandleAccel();
					break;
				case SettingsManager.ControlType.Slider:
					HandleSlider();
					break;
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
			if (!sliderHandler.Pressed) return;

			Vector3 dir = Input.mousePosition - slider.transform.position;
			_boatDirection = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
			slider.transform.rotation = Quaternion.AngleAxis(_boatDirection, Vector3.forward);
			_boatDirection *= -1;
		}
	}
}
