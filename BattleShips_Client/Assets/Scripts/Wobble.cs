using System.Linq;
using TMPro;
using UnityEngine;

public class Wobble : MonoBehaviour
{
	[Tooltip("Lower is slower")] [Range(0f, 0.2f)] [SerializeField]
	private float turnSpeed = 0.1f;

	[SerializeField] private TMP_Text noGyroText;

	[SerializeField] private TMP_Text logText;

	private int _lastFrameIndex;
	private readonly float[] _frameDeltaTimeArray = new float[50];

	private void Start()
	{
		if (SystemInfo.supportsGyroscope)
		{
			Debug.Log("Gyroscope is supported");
			Input.gyro.enabled = true;
			noGyroText.gameObject.SetActive(false);
			logText.gameObject.SetActive(true);
		}
		else
		{
			Debug.Log("Gyroscope is not supported");
			noGyroText.gameObject.SetActive(true);
			logText.gameObject.SetActive(false);
			enabled = false;
		}
	}

	private void Update()
	{
		Quaternion gyro = Input.gyro.attitude;

		float gyroY = -gyro.eulerAngles.z;
		transform.rotation = Quaternion.Lerp(transform.rotation,
			Quaternion.Euler(0, gyroY, 0), //target rotation
			turnSpeed);

		_frameDeltaTimeArray[_lastFrameIndex] = Time.unscaledDeltaTime;
		_lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;

		logText.text =
			$"FPS: {Mathf.RoundToInt(CalculateFPS())}\nGyro: {gyro}\nEuler: {gyro.eulerAngles}\nRot: {gyroY}\nInterval: {Input.gyro.updateInterval}\nCompass: {Input.compass.magneticHeading}";
	}

	private float CalculateFPS()
	{
		float total = _frameDeltaTimeArray.Sum();
		return _frameDeltaTimeArray.Length / total;
	}
}
