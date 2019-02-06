using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkRigidBodySpawnWithAuthority : NetworkBehaviour,ISpawnWithAuthority {

	[Server]
	public void Spawn (int assetIndex) {
		GetComponent<NetworkRigidBodyAssetSpawner> ().assetIndex = assetIndex;
		InvokeRepeating ("DoSpawn", 1, 3);
	}

	[Server]
	private void DoSpawn() {

		float minDist = Mathf.Infinity;
		NetworkConnection closestPlayer = null;
		if (NetworkServer.connections.Count > 0) {
			//Debug.Log (NetworkServer.connections.Count);
			foreach (NetworkConnection connection in  NetworkServer.connections) {
				if (connection != null && connection.playerControllers.Count>0) {
					float dist = Vector3.Distance (connection.playerControllers [0].gameObject.transform.position, transform.position);
					if (dist < minDist) {
						minDist = dist;
						closestPlayer = connection;
					}
				}
			}
		}

		if (closestPlayer != null) {
			NetworkServer.SpawnWithClientAuthority (gameObject, closestPlayer);
			CancelInvoke ("DoSpawn");
		}
	}
}
