using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Island : MonoBehaviour
{
	[SerializeField] private Vector3 origin = new(0, 3, 0);

	private readonly Dictionary<WebsocketServer.ClientEntry, GameObject> _flags = new();

	public void SpawnFlag(WebsocketServer.ClientEntry client)
	{
		GameObject flag = Instantiate(MatchManager.FlagPrefab);
		flag.transform.rotation = Quaternion.Euler(10, -80, -40);
		foreach (Renderer child in flag.GetComponentsInChildren<Renderer>())
		{
			if (child.name == "Flag")
				child.material.color = client.Colour;
		}

		_flags.Add(client, flag);

		RepositionFlags();
	}

	private void RepositionFlags()
	{
		const int flagWidth = 4;

		float startOffset = ((_flags.Count - 1) * flagWidth / 2.0f);
		for (int i = 0; i < _flags.Count; i++)
		{
			(_, GameObject flag) = _flags.ElementAt(i);
			float flagOffset = (i * flagWidth);
			flag.transform.position = transform.position + origin + (Vector3.right * (-startOffset + flagOffset));
		}
	}

	public void RemoveFlag(WebsocketServer.ClientEntry client)
	{
		if (!_flags.ContainsKey(client))
		{
			Debug.LogWarning("Trying to remove flag that doesn't exist");
			return;
		}

		Destroy(_flags[client]);
		_flags.Remove(client);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.grey;
		Gizmos.DrawSphere(transform.position + origin, 0.5f);
	}
}
