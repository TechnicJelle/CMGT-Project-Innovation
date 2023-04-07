using UnityEngine;

namespace UI
{
	public class DetectCamera : MonoBehaviour
	{
		private void Awake()
		{
			if (WebCamTexture.devices.Length == 0)
				gameObject.SetActive(false);
		}
	}
}
