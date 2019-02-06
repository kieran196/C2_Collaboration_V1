using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum GrabHandType {
	LEFT = 0,
	RIGHT = 1
}

public class P2PLinkManager : MonoBehaviour {

    private tntArticulationP2PConstraint[] holdingApeLinksLHand;
    private tntArticulationP2PConstraint[] holdingApeLinksRHand;
    private int numP2pLinks = 1;
    private float linkMaxImpulse = 1.0f;
    private float linkBreakImpulse = 2.75f; //Recommend to be a little lower than linkMaxImpulse

    private NetworkPlayerGrabDataSync grabDataSync;

    void Start () {
		holdingApeLinksLHand = new tntArticulationP2PConstraint[numP2pLinks];
		holdingApeLinksRHand = new tntArticulationP2PConstraint[numP2pLinks];
        grabDataSync = GetComponentInChildren<NetworkPlayerGrabDataSync>();
    }
	
	void Update ()
    {
        //if (grabDataSync)
        //{
        //    grabDataSync.MonitorLinkBreak();
        //}
	}

    private void CalcTripleP2PPivots(Transform holdingBodyA, Transform heldBodyB, Vector3 pivotA, Vector3 pivotB, out Vector3[] otherPivotsA, out Vector3[] otherPivotsB)
	{
		float distanceBetweenPivots = 0.5f;// 0.5f;
		Transform a = holdingBodyA;
		Transform b = heldBodyB;

		Vector3 commonOffset = Vector3.zero;

		//otherPivotsA = new Vector3[3] { pivotA, pivotA + Vector3.right * distanceBetweenPivots * 0.0f, pivotA + Vector3.up * distanceBetweenPivots }; // Vector3.right breaks & get's reversed on bodyB !?
		Vector3[] pivotValues = new Vector3[5]{ pivotA, pivotA + Vector3.forward * distanceBetweenPivots, pivotA + Vector3.up * distanceBetweenPivots,
			pivotA - Vector3.forward * distanceBetweenPivots, pivotA - Vector3.up * distanceBetweenPivots}; // works okay !!
		otherPivotsA = new Vector3[numP2pLinks]; 
		otherPivotsB = new Vector3[numP2pLinks];
		for (int i = 0; i < numP2pLinks; i++)
		{
			otherPivotsA[i] = i < 5 ? pivotValues[i] : pivotA;
			Vector3 pivotInWorld = a.TransformDirection(otherPivotsA[i]) + a.position; // Transform without scale
			otherPivotsB[i] = b.InverseTransformDirection(pivotInWorld - b.position); // Transform without scale

			if (i == 0)
			{
				commonOffset = pivotB - otherPivotsB[i];
				otherPivotsB[i] = pivotB;
			}
			else
			{
				otherPivotsB[i] += commonOffset;
			}
		}
	}

	public void AddLinksFromData(NetworkInstanceId other, GrabHandType hand, string otherBodyPart, string grabberBodyPart, Vector3 pivotA, Vector3 pivotB) {

		GameObject otherPlayer = ClientScene.FindLocalObject(other);
		if (otherPlayer == null)
			return;
        Transform otherRoot = otherPlayer.GetComponent<NetworkPlayerAssetSpawner>().assetRoot.transform;

        //Transform otherPart = otherRoot.FindChild("TPTRobot/Robot/" + otherBodyPart);
        //Transform ownPart = transform.FindChild ("TPTRobot/Robot/" + grabberBodyPart);
        Transform ownPart = null;
        Transform otherPart = null;
        tntBase rootBase;
        Transform robot = null;
        tntBase otherRootBase = null;
        Transform otherRobot = null;

        rootBase = GetComponentInChildren<tntBase>();
        if (rootBase) { robot = rootBase.transform.parent; }
        if (robot)
        {
            ownPart = robot.Find(grabberBodyPart);
        }

        if (otherRoot) { otherRootBase = otherRoot.GetComponentInChildren<tntBase>(); }
        if (otherRootBase) { otherRobot = otherRootBase.transform.parent; }
        if (otherRobot)
        {
            otherPart = otherRobot.Find(otherBodyPart);
        }

        AddLinks (hand, ownPart, otherPart.GetComponent<tntLink> (), pivotA, pivotB);
	}

	public void AddLinks(GrabHandType hand, Transform handProxy, tntLink heldObject, Vector3 pivotAInLocal, Vector3 pivotBInLocal) {
		ClearLinks (hand);

		Vector3[] pivotsA, pivotsB;
		CalcTripleP2PPivots (handProxy, heldObject.transform, pivotAInLocal, pivotBInLocal, out pivotsA, out pivotsB);

		if (hand == GrabHandType.LEFT) {
			for (int i = 0; i < numP2pLinks; i++) {
				// Create the link
				holdingApeLinksLHand [i] = handProxy.gameObject.AddComponent<tntArticulationP2PConstraint> ();
				holdingApeLinksLHand [i].m_pivotA = pivotsA [i];
				holdingApeLinksLHand [i].m_pivotB = pivotsB [i];

				// Set bodies
				holdingApeLinksLHand [i].m_linkA = handProxy.GetComponent<tntLink> ();
				holdingApeLinksLHand [i].m_linkB = heldObject;
				holdingApeLinksLHand [i].m_useBodyB = false;

				holdingApeLinksLHand [i].m_maxImpulse = linkMaxImpulse;
                //holdingApeLinksLHand [i].m_breakingImpulse = linkBreakImpulse;
                holdingApeLinksLHand [i].enabled = true;

            }
		} else {
			for (int i = 0; i < numP2pLinks; i++) {
				// Create the link
				holdingApeLinksRHand [i] = handProxy.gameObject.AddComponent<tntArticulationP2PConstraint> ();
				holdingApeLinksRHand [i].m_pivotA = pivotsA [i];
				holdingApeLinksRHand [i].m_pivotB = pivotsB [i];

				// Set bodies
				holdingApeLinksRHand [i].m_linkA = handProxy.GetComponent<tntLink> ();
				holdingApeLinksRHand [i].m_linkB = heldObject;
				holdingApeLinksRHand [i].m_useBodyB = false;

				holdingApeLinksRHand [i].m_maxImpulse = linkMaxImpulse;
                //holdingApeLinksRHand [i].m_breakingImpulse = linkBreakImpulse;
                holdingApeLinksRHand [i].enabled = true;
			}
		}
	}

	public void ClearLinks(GrabHandType hand)
	{
		if (hand == GrabHandType.LEFT) {
			for (int i = 0; i < numP2pLinks; i++)
				if (holdingApeLinksLHand [i] && holdingApeLinksLHand [i].enabled) {
					Destroy (holdingApeLinksLHand [i]);
					holdingApeLinksLHand [i] = null;
				}
        } else {
			for (int i = 0; i < numP2pLinks; i++)
				if (holdingApeLinksRHand [i] && holdingApeLinksRHand [i].enabled) {
					Destroy (holdingApeLinksRHand [i]);
					holdingApeLinksRHand [i] = null;
				}
        }
	}

    public void MonitoringLinkBreakCondition()
    {
        for (int i = 0; i < numP2pLinks; i++)
        {
            if (holdingApeLinksLHand[i] && holdingApeLinksLHand[i].enabled)
            {
                if (holdingApeLinksLHand[i].m_feedback.m_appliedImpulse > linkBreakImpulse)
                {
                    Debug.Log("Break Left Hand Link!!");
                    grabDataSync.SendGrabRemovalData(GrabHandType.LEFT);
                    break;
                }
            }
        }

        for (int i = 0; i < numP2pLinks; i++)
        {
            if (holdingApeLinksRHand[i] && holdingApeLinksRHand[i].enabled)
            {
                if (holdingApeLinksRHand[i].m_feedback.m_appliedImpulse > linkBreakImpulse)
                {
                    Debug.Log("Break Right Hand Link!!");
                    grabDataSync.SendGrabRemovalData(GrabHandType.RIGHT);
                    break;
                }
            }
        }
    }
}
