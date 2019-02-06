using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public static class NetworkUtility {

	public static NetworkTime networkTime = null;

	public static EntityAsset FindAssetWithAssetId (EntityAsset[] assetGroup, NetworkHash128 assetId) {
		foreach (EntityAsset asset in assetGroup) {
			if (assetId.Equals (asset.networkEntityPrefab.GetComponent<NetworkIdentity> ().assetId)) {
				return asset;
			}
		}
		return null;
	}

	public static GameObject FindLocalObject (NetworkInstanceId id, bool inClient = true) {
		NetworkIdentity[] nis = GameObject.FindObjectsOfType<NetworkIdentity> ();

		foreach (NetworkIdentity ni in nis) {
			if (ni.netId.Equals (id)) {
				if (inClient && ni.isClient)
					return ni.gameObject;
				else if (!inClient && ni.isServer)
					return ni.gameObject;
			}
		}
		return null;
	}

	public static float SecondsSince1970()
	{
		DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		float currentEpochTime = (float)((DateTime.UtcNow - epochStart).TotalMilliseconds/1000.0f);

		return currentEpochTime;
	}
}
