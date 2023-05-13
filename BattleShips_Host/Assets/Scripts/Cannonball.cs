using System;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
	[NonSerialized] public Boat Shooter;

	private void Start()
	{
		//TODO: Shot smoke effect (should not move along with the cannonball, nor with the ship)
		Debug.DrawRay(transform.position, transform.forward * 5, Color.white, 10);
	}

	private void OnCollisionEnter(Collision other)
	{
		//Ordered by likelihood of collision

		if (other.gameObject.name == "Water")
		{
			Debug.Log("Hit water");

			//TODO: Water sploosh effect
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.blue, 10);

			Destroy(gameObject);
			return;
		}

		if (other.gameObject.CompareTag("Island") || (other.transform.parent != null && other.transform.parent.CompareTag("Island")))
		{
			Debug.Log("Hit an island");

			//TODO: Sand plomf effect
			Debug.DrawRay(transform.position, Vector3.up * 10, Color.yellow, 10);

			Destroy(gameObject);
			return;
		}

		Boat boat = other.gameObject.GetComponent<Boat>();
		if (boat != null)
		{
			if(boat == Shooter) return;

			boat.Damage();

			//TODO: Boat explosion effect
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
