using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Shared.Scripts;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
	[SerializeField] private TMP_Text txtName;
	[SerializeField] private Slider sldHealth;

	[Header("Movement")]
	[SerializeField] [Range(0.0f, 360.0f)] private float startRotation;
	[SerializeField] private float moveSpeed = 15;
	[SerializeField] private float blowingSpeed = 30;
	[SerializeField] [Range(0, 0.1f)] private float rotSpeed;
	[SerializeField] public bool go;

	[Header("Shooting")]
	[SerializeField] private GameObject cannonballPrefab;
	[SerializeField] private float shootingPower = 1000;
	[SerializeField] private int startHealth = 10;
	[SerializeField] private float reloadSpeed = 2f;
	[SerializeField] private int shotsInOneGo = 3;

	// == Not visible in inspector ==
	// Properties
	private string _id;

	// Movement
	private Rigidbody _rigidbody;
	private float _targetRotation;
	private Vector3 _movementDirection;
	private bool _isBlowing;
	[CanBeNull] public GameObject CollidingIsland { get; private set; }

	// Shooting
	private MessageFactory.ShootingDirection? _shouldShoot;
	private int _health;
	private readonly Dictionary<MessageFactory.ShootingDirection, float> _reloadTimers = new();

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_targetRotation = startRotation;
		_movementDirection = Vector3.forward;
		_health = startHealth;
		sldHealth.maxValue = startHealth;
		sldHealth.value = _health;

		foreach (MessageFactory.ShootingDirection dir in Enum.GetValues(typeof(MessageFactory.ShootingDirection)))
			_reloadTimers.Add(dir, 0f);
	}

	public void Setup(string id, Camera cam)
	{
		_id = id;
		txtName.text = id;
		GetComponentInChildren<PointToCamera>().SetCamera(cam);
	}

	private void Update()
	{
		for (byte i = 0; i < _reloadTimers.Count; i++)
		{
			_reloadTimers[(MessageFactory.ShootingDirection) i] += Time.deltaTime;
		}


		if (_shouldShoot != null)
		{
			if(_reloadTimers[_shouldShoot.Value] >= reloadSpeed) Shoot(_shouldShoot.Value);
			_shouldShoot = null;
		}
	}

	private void Shoot(MessageFactory.ShootingDirection shootingDirection)
	{
		Transform t = transform;
		Vector3 right = t.right;
		Vector3 forward = t.forward;
		Vector3 direction = shootingDirection switch
		{
			MessageFactory.ShootingDirection.Port => -right,
			MessageFactory.ShootingDirection.Starboard => right,
			_ => throw new ArgumentOutOfRangeException(nameof(shootingDirection), shootingDirection, null),
		};

		Bounds boatBounds = GetComponent<Collider>().bounds;
		Vector3 ballRadius = cannonballPrefab.GetComponentInChildren<SphereCollider>().radius * cannonballPrefab.transform.localScale;
		float offset = boatBounds.size.x / 2f + ballRadius.x;
		for (int i = 0; i < shotsInOneGo; i++)
		{
			float alongSide = boatBounds.size.z / shotsInOneGo * i + 0.5f;
			Vector3 fromBack = boatBounds.size.z / 2 * forward;
			GameObject cannonball = Instantiate(cannonballPrefab, t.position + direction * offset - fromBack + alongSide * forward + t.up, Quaternion.LookRotation(direction));
			float rand = Random.Range(0.9f, 1.1f);
			cannonball.GetComponent<Rigidbody>().AddForce(direction * (shootingPower * rand));
			cannonball.GetComponent<Cannonball>().Shooter = this;
		}

		_reloadTimers[shootingDirection] = 0;
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
		if (other.gameObject.CompareTag("IslandDocking"))
		{
			CollidingIsland = other.gameObject;
			WebsocketServer.Instance.Send(_id, MessageFactory.CreateDockingAvailableUpdate(true));
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("IslandDocking"))
		{
			WebsocketServer.Instance.Send(_id, MessageFactory.CreateDockingAvailableUpdate(false));
			CollidingIsland = null;
		}
	}

	public void Damage()
	{
		_health--;
		if (_health <= 0) Die();
		sldHealth.value = _health;
	}

	private void Die()
	{
		transform.position = MatchManager.Instance.GetValidSpawnLocation();
		//TODO: Drop treasure(s?)
		_health = startHealth;
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
