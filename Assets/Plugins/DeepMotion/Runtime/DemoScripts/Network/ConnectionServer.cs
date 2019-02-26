using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using System;
using UnityEngine.VR;

//For both server and client use
using System.Collections.Generic;


[System.Serializable]
public class AdvancedNetworkConfig
{
    public bool enable = false;
    [Tooltip("For wireless environment, recommend use around 60")]
    public byte NetworkDropThreshold = 5; //For wireless use 60
    [Tooltip("For wireless environment, recommend use around 50")]
    public byte OverflowDropThreshold = 5; //For wireless use 50
    [Tooltip("For wireless environment, recommend use 5000-10000")]
    public uint DisconnectTimeout = 2000; //For wireless use 6500
}

public class ConnectionServer : MonoBehaviour
{
	public bool noSimulation = false;
    public AdvancedNetworkConfig advancedNetConfig;
	[HideInInspector]
	public List<NetworkIdentity> entitiesWithServerAuothority;

    void Awake()
	{
		var settings = FindObjectOfType<ThreePointTrackingSettings>();
		if (settings.multiplayerMode == MultiPlayerMode.Client) {
			DestroyImmediate (gameObject);
			return;
		}

		if (UnityEngine.XR.XRSettings.enabled != settings.VRMode)
			UnityEngine.XR.XRSettings.enabled = settings.VRMode;

		Application.runInBackground = true;
		NetworkServer.RegisterHandler(MsgType.Connect, OnPlayerConnect);
		NetworkServer.RegisterHandler(MsgType.AddPlayer, OnAddPlayer);
		NetworkServer.RegisterHandler(MsgType.Disconnect, OnPlayerDisconnect);

        if (advancedNetConfig.enable)
        {
            ConnectionConfig config = new ConnectionConfig();

            config.AddChannel(QosType.ReliableSequenced);
            config.AddChannel(QosType.Unreliable);
            config.NetworkDropThreshold = advancedNetConfig.NetworkDropThreshold;
            config.OverflowDropThreshold = advancedNetConfig.OverflowDropThreshold;
            config.DisconnectTimeout = advancedNetConfig.DisconnectTimeout;

            NetworkServer.Configure(config, 100);
        }

		entitiesWithServerAuothority = new List<NetworkIdentity> ();
		Invoke ("PopulateServerEntities", 3);

		NetworkServer.Listen(settings.serverPort);
	}

	private void PopulateServerEntities()
	{
		var settings = FindObjectOfType<ThreePointTrackingSettings>();
		for (int i=0; i<settings.serverEntityAssets.Length; i++) {
			EntityAsset asset = settings.serverEntityAssets[i];
			if (asset!=null){
				GameObject networkEntity = (GameObject)Instantiate(asset.networkEntityPrefab);
				networkEntity.name = "NetworkEntity(" + networkEntity.name + ")";
				networkEntity.transform.SetParent(this.transform);
				networkEntity.transform.position = asset.spawnLocation;
				networkEntity.transform.rotation = Quaternion.Euler (asset.spawnRotation);
				networkEntity.GetComponent<ISpawnWithAuthority> ().Spawn (i);
			}
		}
	}
    
    //This will be called when a player(client) disconnects from a server
	private void OnPlayerDisconnect(NetworkMessage netMsg)
	{
        Debug.Log("Client with IP " + netMsg.conn.address + " disconnected from server");
		var playerGamePiece = netMsg.conn.playerControllers[0].gameObject;

		// track the objects that now do not have authority associated with client
		if (netMsg.conn.clientOwnedObjects != null) {
			foreach (NetworkInstanceId obj in netMsg.conn.clientOwnedObjects) {
				GameObject go = NetworkUtility.FindLocalObject (obj, false);
				if (go != null && !go.tag.Equals ("PlayerEntity")) {
					NetworkIdentity ni = go.GetComponent<NetworkIdentity> ();
					if (ni != null) {
						entitiesWithServerAuothority.Add (ni);
					}
				}
			}
		}

        // remove player object from the clients
        NetworkServer.UnSpawn(playerGamePiece); // This doesn't despawn the disconnected player, since it was already disconnected
        
        // remove the player object from server. This is because UnSpawn won't delete the copy on Server
        Destroy(playerGamePiece);
		TransferEntitiesAuthorityToAvailableClient ();
    }

	private void TransferEntitiesAuthorityToAvailableClient() {
		if (NetworkServer.connections.Count > 0) {
			foreach (NetworkConnection connection in  NetworkServer.connections) {
				if (connection != null && connection.playerControllers.Count>0) {
					List<NetworkIdentity> removeList = new List<NetworkIdentity> ();
					foreach (NetworkIdentity id in entitiesWithServerAuothority) {
						if (id.clientAuthorityOwner!=null)
							id.RemoveClientAuthority (id.clientAuthorityOwner);
						if (id.AssignClientAuthority (connection)) {
							removeList.Add (id);
						}
					}
					foreach (NetworkIdentity id in removeList) {
						entitiesWithServerAuothority.Remove (id);
					}
					break;
				}
			}
            
            //Below is to ensure all client's rigid bodies transform sync up
            var networkIdentities = GetComponentsInChildren<NetworkIdentity>();
            if (networkIdentities != null)
            {
                foreach (var identity in networkIdentities)
                {
                    var rbPVDataSync = identity.GetComponentInChildren<NetworkRigidBodyPVDataSync>();
                    if (rbPVDataSync != null)
                    {
                        rbPVDataSync.SvrResetStaticFrameCount();
                    }
                }
            }
		}
	}

    //This will be called when a player(client) connects to a server
    private void OnPlayerConnect(NetworkMessage netMsg)
	{
		Debug.Log("Client with IP " + netMsg.conn.address + " connected!");
	}

    private void OnAddPlayer(NetworkMessage netMsg)
	{
        Debug.Log("Player with IP " + netMsg.conn.address + " was added!");
        Vector3 spawnPos = FindSpawnLocationForPlayer();
        var settings = ThreePointTrackingSettings.GetInstance();
        var networkEntity = Instantiate<GameObject>(settings.playerAsset.networkEntityPrefab);
        networkEntity.name = "NetworkEntity(Player) - server";
        networkEntity.transform.SetParent(this.transform);
        networkEntity.transform.position = spawnPos;
        bool succeed = NetworkServer.AddPlayerForConnection(netMsg.conn, networkEntity, 0);
		if (!succeed) { 
			Destroy (networkEntity); 
		} else {
			TransferEntitiesAuthorityToAvailableClient ();
		}
    }

	// Find the spawn location that is a bit away from other players
	private Vector3 FindSpawnLocationForPlayer(){
		GameObject[] players = GameObject.FindGameObjectsWithTag ("PlayerEntity");
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag ("Respawn");
		Transform safeSP=null;
		float safeDist = -10;


		foreach (GameObject sp in spawnPoints) {

			// find the closes player distance from this spawn point
			float minDist = 100000;
			foreach (GameObject p in players) {
				if (p.GetComponent<NetworkPlayerAssetSpawner> ().isServer) {
					float dist = Vector3.Distance (sp.transform.position, p.transform.position);
					if (dist < minDist){
						minDist = dist;
					}
				}
			}

			// find the spawn point among all from which the closes player is a bit away.
			if (minDist > safeDist) {
				safeDist = minDist;
				safeSP = sp.transform;
			}
		}

		if (safeSP != null)
			return safeSP.position;
		else
			return Vector3.zero;
	}
}
