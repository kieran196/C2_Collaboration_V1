using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class  NetworkPlayerGrabDataSync: NetworkBehaviour
{

	public override void OnStartServer()
    {
        base.OnStartServer();
    }

	public override void OnStartClient()
	{
		base.OnStartClient();
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
	}

	[Client]
	public bool SendRigidBodyGrabData(NetworkRigidBodyGrabDataSync rb, Transform grabber, string grabberBodyPart, GrabHandType hand, TransformInfoSimple rbLocalTransform)
	{
        if ((!rb.grabOwner.Equals(NetworkInstanceId.Invalid)) && (!rb.grabOwner.Equals(netId)))
			return false;

		// send grab command to server
		CmdServerUpdateRigidBodyGrabData(rb.gameObject.GetComponent<NetworkIdentity> ().netId, grabberBodyPart, hand, rbLocalTransform);
		return true;
	}

	[Command]
	public void CmdServerUpdateRigidBodyGrabData(NetworkInstanceId rb, string grabberBodyPart, GrabHandType hand, TransformInfoSimple rbLocalTransform)
    {
        GameObject rbObject = NetworkUtility.FindLocalObject(rb,false);
		if (rbObject != null) {
			rbObject.GetComponent<NetworkRigidBodyGrabDataSync> ().CmdServerUpdateGrabData (netId, grabberBodyPart, hand, rbLocalTransform);
		}
	}

	[Client]
	public bool SendRigidBodyGrabRemovalData(NetworkRigidBodyGrabDataSync rb, GrabHandType hand)
	{
		if ((rb.grabOwner.Equals(NetworkInstanceId.Invalid)) || (!rb.grabOwner.Equals(netId)) || (rb.grabHand != hand))
			return false;
		
		// send grab removal command to server
		CmdServerUpdateRigidBodyGrabRemovalData(rb.gameObject.GetComponent<NetworkIdentity> ().netId,hand);
		return true;
	}

	[Command]
	public void CmdServerUpdateRigidBodyGrabRemovalData(NetworkInstanceId rb,GrabHandType hand)
	{
		GameObject rbObject = NetworkUtility.FindLocalObject(rb,false);
		if (rbObject != null) {
			rbObject.GetComponent<NetworkRigidBodyGrabDataSync> ().CmdServerUpdateGrabRemovalData (hand);
		}
	}

    [Client]
    public void MonitorLinkBreak()
    {
        if (!isLocalPlayer)
        {
            //Debug.Log("Not local player");
            return;
        }
        P2PLinkManager pm = GetComponentInParent<P2PLinkManager>();
        pm.MonitoringLinkBreakCondition();
    }

	// Add Links
    [Client]
	public void SendGrabData(GameObject other, GrabHandType hand, Transform otherBodyPart, Transform grabberBodyPart, Vector3 pivotA, Vector3 pivotB)
	{
        // update local client Grab simulation
        P2PLinkManager pm = GetComponentInParent<P2PLinkManager>();
        if (pm != null)
            pm.AddLinksFromData(other.GetComponentInChildren<NetworkIdentity>().netId, hand, otherBodyPart.name, grabberBodyPart.name, pivotA, pivotB);

        // send grab command to server
        CmdServerUpdateGrabData(other.GetComponentInChildren<NetworkIdentity>().netId, netId, hand, otherBodyPart.name, grabberBodyPart.name, pivotA, pivotB);
    }

    [Command]
	public void CmdServerUpdateGrabData(NetworkInstanceId other, NetworkInstanceId grabber, GrabHandType hand, string otherBodyPart, string grabberBodyPart, Vector3 pivotA, Vector3 pivotB)
	{
		// update server grab simulation
		GameObject grabberPlayer = ClientScene.FindLocalObject(grabber);
		if (grabberPlayer != null) {
			Transform root = grabberPlayer.GetComponent<NetworkPlayerAssetSpawner> ().assetRoot.transform;
			P2PLinkManager pm = root.GetComponent<P2PLinkManager> ();
			if (pm !=null)
				pm.AddLinksFromData (other, hand, otherBodyPart, grabberBodyPart, pivotA, pivotB);
		}

		// update all client grab simulation
		RpcClientUpdateGrabData (other, grabber, hand, otherBodyPart, grabberBodyPart, pivotA, pivotB);
	}

	[ClientRpc]
	private void RpcClientUpdateGrabData(NetworkInstanceId other, NetworkInstanceId grabber, GrabHandType hand, string otherBodyPart, string grabberBodyPart, Vector3 pivotA, Vector3 pivotB)
	{
		//if it is the same client that sent Grab command then ignore it. 
		if (!isLocalPlayer) {

			// update server grab simulation
			GameObject grabberPlayer = ClientScene.FindLocalObject(grabber);
			if (grabberPlayer != null) {
				Transform root = grabberPlayer.GetComponent<NetworkPlayerAssetSpawner> ().assetRoot.transform;
				P2PLinkManager pm = root.GetComponent<P2PLinkManager> ();
				if (pm !=null)
					pm.AddLinksFromData (other, hand, otherBodyPart, grabberBodyPart, pivotA, pivotB);
			}

		}
	}

	// Remove Links
	[Client]
	public void SendGrabRemovalData(GrabHandType hand)
	{
        // update local client Grab simulation
        P2PLinkManager pm = GetComponentInParent<P2PLinkManager>();
        if (pm != null)
        {
            pm.ClearLinks(hand);
        }
        // send grab removal command to server
        CmdServerUpdateGrabRemovalData(netId,hand);
	}

	[Command]
	public void CmdServerUpdateGrabRemovalData(NetworkInstanceId grabber, GrabHandType hand)
	{
		// update server grab simulation
		GameObject grabberPlayer = ClientScene.FindLocalObject(grabber);
		if (grabberPlayer != null) {
			Transform root = grabberPlayer.GetComponent<NetworkPlayerAssetSpawner> ().assetRoot.transform;
			P2PLinkManager pm = root.GetComponent<P2PLinkManager> ();
			if (pm !=null)
				pm.ClearLinks (hand);
		}

		// update all client grab simulation
		RpcClientUpdateGrabRemovalData (grabber, hand);
	}

	[ClientRpc]
	private void RpcClientUpdateGrabRemovalData(NetworkInstanceId grabber, GrabHandType hand)
	{
        //if it is the same client that sent Grab removal command then ignore it. 
        if (!isLocalPlayer)
        {
            // update server grab simulation
            GameObject grabberPlayer = ClientScene.FindLocalObject(grabber);
			if (grabberPlayer != null) {
				Transform root = grabberPlayer.GetComponent<NetworkPlayerAssetSpawner> ().assetRoot.transform;
				P2PLinkManager pm = root.GetComponent<P2PLinkManager> ();
				if (pm !=null)
					pm.ClearLinks (hand);
			}
        }
    }
}
