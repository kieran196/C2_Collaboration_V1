using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkTime : NetworkBehaviour
{
	[HideInInspector]
	public bool isNetworkTimeSynced = false;

	// timestamp received from server
	private int networkTimestamp;

	// server to client delay
	private int networkTimestampDelayMS;

	// when did we receive timestamp from server
	private float timeReceived;

	ConnectionClient selfClient;

	protected virtual void Start()
	{
		SyncTime ();
	}

	[Client]
	public void SyncTime() {
		if (isLocalPlayer)
		{
			CmdRequestTime();
		}
	}

	[Command]
	private void CmdRequestTime()
	{
		int timestamp = NetworkTransport.GetNetworkTimestamp();
		RpcNetworkTimestamp(timestamp);
	}

	[ClientRpc]
	private void RpcNetworkTimestamp(int timestamp)
	{
		if (isLocalPlayer) {
			isNetworkTimeSynced = true;
			networkTimestamp = timestamp;
			timeReceived = Time.time;

			// if client is a host, assume that there is 0 delay
			if (isServer) {
				networkTimestampDelayMS = 0;
			} else {
				byte error;
				if (isClient && selfClient == null)
					selfClient = FindObjectOfType<ConnectionClient> ();
			
				networkTimestampDelayMS = NetworkTransport.GetRemoteDelayTimeMS (
					selfClient.GetConnectionObject ().hostId,
					selfClient.GetConnectionObject ().connectionId,
					timestamp,
					out error);
			}
			NetworkUtility.networkTime = this;
		}
	}

	[Client]
	public float GetServerTime()
	{ 
		if (isNetworkTimeSynced)
			return networkTimestamp + (networkTimestampDelayMS / 1000f) + (Time.time - timeReceived);
		else
			return 0f;
	}
}