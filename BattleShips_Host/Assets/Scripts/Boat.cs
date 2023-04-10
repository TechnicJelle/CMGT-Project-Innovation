using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
	[SerializeField] [Range(0.0f, 360.0f)] private float startRotation;
	[SerializeField] private float moveSpeed;
	[SerializeField] [Range(0, 0.1f)] private float rotSpeed;
	[SerializeField] private bool go;

	private Rigidbody _rb;
	private float _targetRotation;
	private Vector3 _direction;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_targetRotation = startRotation;
		_direction = Vector3.forward;
	}

	private void FixedUpdate()
	{
		Move();
	}

	private void Move()
	{
		_direction = Quaternion.Euler(new Vector3(0, _targetRotation, 0)) * Vector3.forward;
		Vector3 forward = transform.forward;
		forward = Vector3.Lerp(forward, _direction, rotSpeed);
		transform.forward = forward;
		if (go)
			_rb.AddForce(forward * (moveSpeed * Time.fixedDeltaTime * 100));
	}

	public void SetTargetDirection(float direction)
	{
		_targetRotation = direction;
	}
}
