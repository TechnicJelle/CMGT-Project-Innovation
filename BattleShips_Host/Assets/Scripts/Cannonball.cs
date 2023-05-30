using System;
using System.Collections;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
	[NonSerialized] public Boat Shooter;
	public ParticleSystem pWater;
	public ParticleSystem pSand;
	public ParticleSystem pBoat;

	private void Start()
	{
		if (!pBoat.isPaused)
			pBoat.Pause();
		if (!pWater.isPaused)
			pWater.Pause();

		//pSand (as smoke) plays automatically
	}

	private void OnCollisionEnter(Collision other)
	{
		//Ordered by likelihood of collision

		if (other.gameObject.name == "Water")
		{
			pWater.Play();

			Kill();
			return;
		}

		if (other.gameObject.CompareTag("Island") || (other.transform.parent != null && other.transform.parent.CompareTag("Island")))
		{
			pSand.Play();

			Kill();
			return;
		}

		Boat boat = other.gameObject.GetComponent<Boat>();
		if (boat != null)
		{
			if(boat == Shooter) return;

			boat.Damage();

			pBoat.Play();

			Kill();
			return;
		}

		Cannonball otherCannonball = other.gameObject.GetComponent<Cannonball>();
		if (otherCannonball != null)
		{
			if (this.Shooter == otherCannonball.Shooter) return; //don't hit other own cannon balls
			Debug.Log("Hit other cannonball");

			//TODO: Cannonball clink effect
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.grey, 10);

			Kill();
			return;
		}

		if(other.gameObject.name.Contains("Boundary", StringComparison.OrdinalIgnoreCase)) return; //ignore boundary collisions

		Debug.LogWarning("other type of hit " + other.gameObject.name);
	}

	private void Kill()
	{
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		rb.AddForce(Physics.gravity * 50, ForceMode.Acceleration); //push down faster
		rb.drag = 1;
		rb.freezeRotation = true;
		rb.rotation = Quaternion.identity;
		Destroy(gameObject.GetComponentInChildren<SphereCollider>());
		StartCoroutine(KillInABit());
	}

	private IEnumerator KillInABit()
	{
		yield return new WaitForSeconds(4);
		Destroy(gameObject);
	}
}
