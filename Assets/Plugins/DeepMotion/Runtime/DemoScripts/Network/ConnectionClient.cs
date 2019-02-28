using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System;
using UnityEngine.VR;

public class ConnectionClient : MonoBehaviour
{
    private NetworkClient _client;
    private GameObject playerEntityPrefab;
    private ThreePointTrackingSettings settings;
    public AdvancedNetworkConfig advancedNetConfig = new AdvancedNetworkConfig();
    private bool firstConnectionFinished = false;

    void Awake() {
        settings = FindObjectOfType<ThreePointTrackingSettings>();
        if (settings.multiplayerMode == MultiPlayerMode.Server) {
            SteamVR_PlayArea pa = FindObjectOfType<SteamVR_PlayArea>();
            if (pa != null)
                DestroyImmediate(pa.gameObject);

            DestroyImmediate(gameObject);
            return;
        }

        if (UnityEngine.XR.XRSettings.enabled != settings.VRMode)
            UnityEngine.XR.XRSettings.enabled = settings.VRMode;
    }

    void Start()
    {
        //on client, this isn't required but is nice for testing.
        Application.runInBackground = true;
    }

    //void Update() //Debugging update
    //{
    //    if (_client != null)
    //    {
    //        Debug.Log("Client(" + Network.player.ipAddress + ") connection status: " + _client.isConnected);
    //    }
    //    Debug.Log("NetworkServer.active: " + NetworkServer.active);
    //}

    public bool GetConnectionStatus()
    {
        return _client.isConnected;
    }

	public NetworkConnection GetConnectionObject()
	{
		return _client.connection;
	}

    public void Connect() {
        ThreePointTrackingGameMgr.GetInstance().canRestart = false;
        settings = FindObjectOfType<ThreePointTrackingSettings>();
        playerEntityPrefab = settings.playerAsset.networkEntityPrefab;
        var networkIdentity = playerEntityPrefab.GetComponent<NetworkIdentity>();
        string IP;

        if (settings.multiplayerMode == MultiPlayerMode.Singleplayer || settings.multiplayerMode == MultiPlayerMode.Host)
        {
            IP = "localhost"; //Should always be local host when in Singleplayer or Host mode
        }
        else
        {
            IP = settings.serverIP;
        }       

        if (firstConnectionFinished)
        {
            if (_client != null)
            {
                _client.Connect(IP, settings.serverPort); //Connect
            }
        }
        else
        {
            ClientScene.RegisterSpawnHandler(networkIdentity.assetId, OnSpawnEntity, OnDespawnEntity);
            foreach (EntityAsset asset in settings.serverEntityAssets)
            {
                ClientScene.RegisterSpawnHandler(asset.networkEntityPrefab.GetComponent<NetworkIdentity>().assetId, OnSpawnEntity, OnDespawnEntity);
            }

            _client = new NetworkClient();

            if (advancedNetConfig.enable)
            {
                ConnectionConfig config = new ConnectionConfig();

                config.AddChannel(QosType.ReliableSequenced);
                config.AddChannel(QosType.Unreliable);
                config.NetworkDropThreshold = advancedNetConfig.NetworkDropThreshold;
                config.OverflowDropThreshold = advancedNetConfig.OverflowDropThreshold;
                config.DisconnectTimeout = advancedNetConfig.DisconnectTimeout;

                _client.Configure(config, 1000);
            }

            _client.Connect(IP, settings.serverPort); //Connect
            _client.RegisterHandler(MsgType.Connect, OnClientConnected);
            _client.RegisterHandler(MsgType.Disconnect, OnClientDisconnected);
            firstConnectionFinished = true;
        }
    }

    public void Disconnect()
    {
        _client.Disconnect();
        //Debug.Log("Disconnecting player with IP: " + Network.player.ipAddress);

        //Delete other clients
        var networkIdentities = GetComponentsInChildren<NetworkIdentity>();
        if (networkIdentities != null)
        {
            foreach (var ni in networkIdentities)
            {
                Destroy(ni.gameObject);
            }
        }

        StartCoroutine(ThreePointTrackingGameMgr.GetInstance().DelayResetRestartFlag(settings.minRestartTime)); //Reset restart flag in certain time
    }

    private void OnClientConnected(NetworkMessage netMsg)
    {
        //Debug.Log("Client with IP " + Network.player.ipAddress + " was connected!");
        ClientScene.Ready(netMsg.conn);
        ClientScene.AddPlayer(0);
    }

    private void OnClientDisconnected(NetworkMessage netMsg)
    {
        //Debug.Log("Client with IP " + Network.player.ipAddress + " was disconnected!");

        var gm = ThreePointTrackingGameMgr.GetInstance();
        gm.StartCoroutine(gm.DelayResetRestartFlag(0.0f)); //Reset restart flag

        //ensure delete all client identities
        var identities = GetComponentsInChildren<NetworkIdentity>();
        if (identities != null)
        {
            foreach (var identity in identities)
            {
                Destroy(identity.gameObject);
            }
        }
    }

    private GameObject OnSpawnEntity(Vector3 position, NetworkHash128 assetId)
	{
		var settings = ThreePointTrackingSettings.GetInstance();
        var networkIdentity = playerEntityPrefab.GetComponent<NetworkIdentity>();
        if (assetId.Equals(networkIdentity.assetId)) {
			var networkEntity = Instantiate<NetworkIdentity> (networkIdentity);
			networkEntity.gameObject.name = "NetworkEntity(Player) - client";
			networkEntity.transform.parent = this.transform;
			networkEntity.transform.position = position;
			return networkEntity.gameObject;
		} else { //other non-player object            
            EntityAsset asset = NetworkUtility.FindAssetWithAssetId(settings.serverEntityAssets, assetId);
			if (asset != null) {
				var networkEntity = Instantiate<GameObject> (asset.networkEntityPrefab);
				networkEntity.name = "NetworkEntity(" + networkEntity.name + ")";
				networkEntity.transform.parent = this.transform;
				return networkEntity.gameObject;
			}
		}
		return null;
	}

	private void OnDespawnEntity(GameObject spawned) //UnSpawnDelegate. Delegate for a function which will handle destruction of objects created with NetworkServer.Spawn.???
    {
		//Debug.Log("Call ConnectionClient.OnDespawnEntity at " + Network.player.ipAddress + " -" + spawned.name);
		Destroy(spawned);
	}
}