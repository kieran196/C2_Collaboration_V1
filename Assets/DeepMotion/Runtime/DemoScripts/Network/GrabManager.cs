using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GrabManager : MonoBehaviour {

	private tntChildLink lHand = null;
	private tntChildLink rHand = null;

	private tntRigidBody lBody = null;
	private tntRigidBody rBody = null;

    [System.NonSerialized]
    public bool isGrabbed = false;
    [System.NonSerialized]
    public float lastTimeGrabbed;

	public bool Grab(NetworkInstanceId grabber, string grabberBodyPart, GrabHandType hand, TransformInfoSimple rbLocalTransform)
	{
		NetworkIdentity ni = GetComponentInChildren<NetworkIdentity> ();
		GameObject grabberPlayer = NetworkUtility.FindLocalObject(grabber,ni.isClient);
		if (grabberPlayer != null) {
			Transform root = grabberPlayer.GetComponent<NetworkPlayerAssetSpawner> ().assetRoot.transform;
			tntBase rootBase = root.GetComponentInChildren<tntBase>();
			if (rootBase!=null) {
				Transform robot = rootBase.transform.parent;
				if (robot!=null) {
					Transform ownPart = robot.Find (grabberBodyPart);
					if (ownPart != null) {
						tntChildLink partLink = ownPart.GetComponent<tntChildLink> ();
						if (partLink != null) {
							tntRigidBody rb = GetComponentInChildren <tntRigidBody> ();
							if (rb != null) {
								if (hand == GrabHandType.LEFT) {
									lHand = partLink;
									lBody = rb;
								} else {
									rHand = partLink;
									rBody = rb;
								}
                                rb.position = ownPart.TransformPoint(rbLocalTransform.position); //Restore and set rb global position for matching
                                rb.rotation = ownPart.rotation * rbLocalTransform.rotation; //Restore and set rb global rotation for matching
                                RigidBodyGrabbingUtil.GrabRigidBody(partLink, rb);
                                isGrabbed = true;
								return true;
							}
						}
					}
				}
			}
		}
        isGrabbed = false;
		return false;
	}

	public bool Release(GrabHandType hand){
		if (hand == GrabHandType.LEFT) {
			if (lHand != null && lBody != null) {
                RigidBodyGrabbingUtil.ReleaseRigidBody(lHand, lBody);
				lHand = null;
				lBody = null;
                isGrabbed = false;
                lastTimeGrabbed = Time.time;
				return true;
			}
		} else {
			if (rHand != null && rBody != null) {
                RigidBodyGrabbingUtil.ReleaseRigidBody(rHand, rBody);
				rHand = null;
				rBody = null;
                isGrabbed = false;
                lastTimeGrabbed = Time.time;
				return true;
			}
		}

		return false;
	}
}
