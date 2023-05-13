using System;
using JetBrains.Annotations;
using Shared.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] [Range(0.0f, 360.0f)] private float startRotation;
	[SerializeField] private float moveSpeed = 15;
	[SerializeField] private float blowingSpeed = 30;
	[SerializeField] [Range(0, 0.1f)] private float rotSpeed;
	[SerializeField] public bool go;

	[Header("Shooting")]
	[SerializeField] private GameObject cannonballPrefab;
	[SerializeField] private float shootingPower = 1000;

	// == Not visible in inspector ==
	// Properties
	public string ID { private get; set; }

	// Movement
	private Rigidbody _rigidbody;
	private float _targetRotation;
	private Vector3 _movementDirection;
	private bool _isBlowing;
	[CanBeNull] public GameObject CollidingIsland { get; private set; }

	// Shooting
	private MessageFactory.ShootingDirection? _shouldShoot;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_targetRotation = startRotation;
		_movementDirection = Vector3.forward;
	}

	private void Update()
	{
		if (_shouldShoot != null)
		{
			Shoot(_shouldShoot.Value);
			_shouldShoot = null;
		}
	}

	private void Shoot(MessageFactory.ShootingDirection shootingDirection)
	{
		Transform t = transform;
		Vector3 right = t.right;
		Vector3 direction = shootingDirection switch
		{
			MessageFactory.ShootingDirection.Port => -right,
			MessageFactory.ShootingDirection.Starboard => right,
			_ => throw new ArgumentOutOfRangeException(nameof(shootingDirection), shootingDirection, null),
		};

		Bounds boatBounds = GetComponent<Collider>().bounds;
		Vector3 ballRadius = cannonballPrefab.GetComponentInChildren<SphereCollider>().radius * cannonballPrefab.transform.localScale;
		float offset = boatBounds.size.x / 2f + ballRadius.x;
		GameObject cannonball = Instantiate(cannonballPrefab, t.position + direction * offset + t.up, Quaternion.identity);
		cannonball.GetComponent<Rigidbody>().AddForce(direction * shootingPower);
	}

	private void FixedUpdate()
	{
		Move();
	}

	private void Move()
	{
		_movementDirection = Quaternion.Euler(new Vector3(0, _targetRotation, 0)) * Vector3.forward;
		Vector3 forward = transform.forward;
		forward = Vector3.Lerp(forward, _movementDirection, rotSpeed);
		transform.forward = forward;
		if (go)
		{
			_rigidbody.AddForce(forward * ((_isBlowing ? blowingSpeed : moveSpeed) * Time.fixedDeltaTime * 100));
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
			CollidingIsland = other.gameObject;
			WebsocketServer.Instance.Send(ID, MessageFactory.CreateDockingAvailableUpdate(true));
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Island"))
		{
			WebsocketServer.Instance.Send(ID, MessageFactory.CreateDockingAvailableUpdate(false));
			CollidingIsland = null;
		}
	}

	public void SetBlowing(bool blowing)
	{
		_isBlowing = blowing;
	}

	public void SetShoot(MessageFactory.ShootingDirection shootingDirection)
	{
		_shouldShoot = shootingDirection;
	}
}
