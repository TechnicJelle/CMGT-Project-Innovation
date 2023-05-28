using System;
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
		Debug.DrawRay(transform.position, transform.forward * 5, Color.white, 10);
	}

	private void OnCollisionEnter(Collision other)
	{
		//Ordered by likelihood of collision

		if (other.gameObject.name == "Water")
		{
			Debug.Log("Hit water");

			pWater.Play();
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.blue, 10);

			Destroy(gameObject);
			return;
		}

		if (other.gameObject.CompareTag("Island") || (other.transform.parent != null && other.transform.parent.CompareTag("Island")))
		{
			Debug.Log("Hit an island");

			pSand.Play();
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.yellow, 10);

			Destroy(gameObject);
			return;
		}

		Boat boat = other.gameObject.GetComponent<Boat>();
		if (boat != null)
		{
			if(boat == Shooter) return;

			boat.Damage();

			pBoat.Play();
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.red, 10);

			Destroy(gameObject);
			return;
		}

		Cannonball otherCannonball = other.gameObject.GetComponent<Cannonball>();
		if (otherCannonball != null)
		{
			if (this.Shooter == otherCannonball.Shooter) return; //don't hit other own cannon balls
			Debug.Log("Hit other cannonball");

			//TODO: Cannonball clink effect
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.grey, 10);

			Destroy(gameObject);
			return;
		}

		Debug.Log("other type of hit " + other.gameObject.name);
	}
}
