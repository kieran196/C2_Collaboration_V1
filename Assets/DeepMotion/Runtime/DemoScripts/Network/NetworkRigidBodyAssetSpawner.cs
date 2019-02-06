using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class  NetworkRigidBodyAssetSpawner: NetworkBehaviour
{
	[HideInInspector]
	public GameObject assetRoot = null;
	[SyncVar]
	private Vector3 spawnPos;
	[SyncVar]
	private Quaternion spawnRot;
	[SyncVar][HideInInspector]
	public int assetIndex;
	private bool localAssetInitialized = false;

	public override void OnStartServer()
    {
        base.OnStartServer();

		spawnPos = transform.position;
		spawnRot = transform.rotation;

		ConnectionServer serverObj = FindObjectOfType<ConnectionServer> ();
		var settings = ThreePointTrackingSettings.GetInstance();
        if ((!serverObj.noSimulation) && ((settings.multiplayerMode == MultiPlayerMode.Server) ||(settings.multiplayerMode == MultiPlayerMode.ServerAndClient))) {
			EntityAsset asset = settings.serverEntityAssets [assetIndex];
			if (asset != null) {
				var serverEnt = Instantiate<GameObject> (asset.serverAssetPrefab, spawnPos, spawnRot);
				assetRoot = serverEnt;
				transform.SetParent (serverEnt.transform);
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				serverEnt.transform.SetParent (serverObj.transform);
				serverEnt.name = "Entity-" + serverEnt.name;
			}
		}
    }

	public override void OnStartClient()
	{
		base.OnStartClient();

		if (!localAssetInitialized)
			InitAsset ();
		if (assetRoot!=null)
			assetRoot.GetComponent<tntRigidBody> ().SetKinematic (true);

    }

	private void InitAsset() {
		var settings = ThreePointTrackingSettings.GetInstance();
		EntityAsset asset =  settings.serverEntityAssets [assetIndex];
		if (asset != null) {
			var clientEnt = Instantiate<GameObject> (asset.clientAssetPrefab, spawnPos, spawnRot);

			ConfigureClientPrefab (clientEnt);
		}
		localAssetInitialized = true;
	}

	public override void OnStartAuthority()
	{
        if (!isClient)
			return;
		
		base.OnStartAuthority();

        if (!localAssetInitialized)
			InitAsset();
		if (assetRoot!=null)
			assetRoot.GetComponent<tntRigidBody> ().SetKinematic (false);
    }


    private void ConfigureClientPrefab(GameObject clientEnt)
    {
        assetRoot = clientEnt;
        transform.SetParent(clientEnt.transform);
        clientEnt.transform.SetParent(FindObjectOfType<ConnectionClient>().transform);
        transform.localPosition = Vector3.zero;
		clientEnt.name = "Entity-" + clientEnt.name;
    }

	void OnDestroy()
	{
		Destroy(assetRoot);
	}

}
