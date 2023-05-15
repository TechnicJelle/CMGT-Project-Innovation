using UnityEngine;

namespace UI
{
	public class PointToCamera : MonoBehaviour
	{
		private Transform _camera;

		public void SetCamera(Transform cam)
		{
			_camera = cam;
		}

		private void Update()
		{
			Quaternion rotation = _camera.transform.rotation;
			transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
		}
	}
}
