using JetBrains.Annotations;
using Shared.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
	[SerializeField] [Range(0.0f, 360.0f)] private float startRotation;
	[SerializeField] private float moveSpeed = 15;
	[SerializeField] private float blowingSpeed = 30;
	[SerializeField] [Range(0, 0.1f)] private float rotSpeed;
	[SerializeField] public bool go;

	public string ID { private get; set; }

	[CanBeNull] public GameObject collidingIsland;

	private Rigidbody _rb;
	private float _targetRotation;
	private Vector3 _direction;
	private bool _blowing;

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
		{
			_rb.AddForce(forward * ((_blowing ? blowingSpeed : moveSpeed) * Time.fixedDeltaTime * 100));
		}
	}

	public void SetTargetDirection(float direction)
	{
		_targetRotation = direction;
	}


	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Island"))
		{
			collidingIsland = other.gameObject;
			WebsocketServer.Instance.Send(ID, MessageFactory.CreateDockingAvailableUpdate(true));
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Island"))
		{
			WebsocketServer.Instance.Send(ID, MessageFactory.CreateDockingAvailableUpdate(false));
			collidingIsland = null;
		}
	}

	public void SetBlowing(bool blowing)
	{
		_blowing = blowing;
	}
}
