using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class  NetworkRigidBodyGrabDataSync: NetworkBehaviour
{
    ConnectionClient selfClient;
	[HideInInspector]
	public NetworkInstanceId grabOwner = NetworkInstanceId.Invalid;
	[HideInInspector]
	public GrabHandType grabHand;

	private NetworkRigidBodyAssetSpawner assetSpawner;

	private void Init() {
		if (isClient && selfClient==null)
			selfClient = FindObjectOfType<ConnectionClient>();
		if (assetSpawner==null)
			assetSpawner = GetComponent<NetworkRigidBodyAssetSpawner> ();
	}

	public override void OnStartServer()
    {
        base.OnStartServer();
		Init ();
    }

	public override void OnStartClient()
	{
		base.OnStartClient();
		Init ();
	}

	public override void OnStartAuthority()
	{
        if (!isClient)
			return;

        base.OnStartAuthority ();
		Init ();
	}

	// Add Links
    [Server]
	public void CmdServerUpdateGrabData(NetworkInstanceId grabber, string grabberBodyPart, GrabHandType hand, TransformInfoSimple rbLocalTransform)
    {
        //update server grabber simulation
        Grab(grabber, grabberBodyPart, hand, rbLocalTransform);

		// update all client grab simulation
		RpcClientUpdateGrabData (grabber, grabberBodyPart, hand, rbLocalTransform);

		// set the object's ownership to grabber
		GameObject go = NetworkUtility.FindLocalObject (grabber, false);
		if (go != null && go.tag.Equals("PlayerEntity")) {
			NetworkIdentity ni = go.GetComponent<NetworkIdentity> ();
			if (ni != null) {
				NetworkIdentity objNI = GetComponent<NetworkIdentity> ();
				if (objNI!=null) {
					bool setAuth = true;
					if (objNI.clientAuthorityOwner != null && objNI.clientAuthorityOwner==ni.connectionToClient) {
						setAuth = false;
					}
					if (setAuth) {
						if (objNI.clientAuthorityOwner != null)
							objNI.RemoveClientAuthority (objNI.clientAuthorityOwner);
						Debug.Log (objNI.AssignClientAuthority (ni.connectionToClient));
					}
					//logging
					if (NetworkServer.connections.Count > 0) {
						foreach (NetworkConnection connection in  NetworkServer.connections) {
							if (connection != null && connection.playerControllers.Count>0) {
								if (connection.clientOwnedObjects != null) {
									Debug.Log (connection.connectionId.ToString() + ": " + connection.clientOwnedObjects.Count.ToString());
								}
							}
						}
					}

				}
	
			}
		}

	}

	[ClientRpc]
	private void RpcClientUpdateGrabData(NetworkInstanceId grabber, string grabberBodyPart, GrabHandType hand, TransformInfoSimple rbLocalTransform)
    {
        Grab(grabber, grabberBodyPart, hand, rbLocalTransform);
	}

	// Remove Links
	[Command]
	public void CmdServerUpdateGrabRemovalData(GrabHandType hand)
	{
		Release (hand);
		// update all client grab simulation
		RpcClientUpdateGrabRemovalData (hand);
	}

	[ClientRpc]
	private void RpcClientUpdateGrabRemovalData(GrabHandType hand)
	{
		Release (hand);
	}

	private void Grab(NetworkInstanceId grabber, string grabberBodyPart, GrabHandType hand, TransformInfoSimple rbLocalTransform) {
        // release the previous grab first
        Release(grabHand);

		// now grab
		if (assetSpawner != null && assetSpawner.assetRoot != null) {
			assetSpawner.assetRoot.GetComponent<tntRigidBody> ().SetKinematic (false);
		}

		// update grab simulation
		GrabManager gm = GetComponentInParent<GrabManager>();
		if (gm != null)
			gm.Grab(grabber, grabberBodyPart, hand, rbLocalTransform);

		grabOwner = grabber;
		grabHand = hand;
	}

	private void Release(GrabHandType hand) {
		if (assetSpawner != null && assetSpawner.assetRoot != null) {
			if (hasAuthority)
				assetSpawner.assetRoot.GetComponent<tntRigidBody> ().SetKinematic (false);
			else
				assetSpawner.assetRoot.GetComponent<tntRigidBody> ().SetKinematic (true);
		}

		// update grab simulation
		GrabManager gm = GetComponentInParent<GrabManager> ();
		if (gm != null)
        {
            if (gm.Release (hand))
                grabOwner = NetworkInstanceId.Invalid; // only invalidate the grabOwner if we actually released something
        }
	}
}
