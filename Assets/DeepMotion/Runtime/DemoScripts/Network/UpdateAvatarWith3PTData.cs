using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateAvatarWith3PTData : MonoBehaviour {
	private tntHumanoidController humanoidController;
    Vector3 rotLeftBy = new Vector3(225, -5, 34);
    Vector3 rotRightBy = new Vector3(225, -5, -34);

    void Start () {
		humanoidController = GetComponentInChildren<tntHumanoidController>();
	}

    //Three point tracking
    public void UpdateHumanoidController(Vector3 lHandPos, Quaternion lHandRot, Vector3 rHandPos, Quaternion rHandRot, Vector3 hmdPos, Quaternion hmdRot)
    {
        if (humanoidController != null)
        {
            if (humanoidController.m_limbs.m_lHandTarget &&
                humanoidController.m_limbs.m_rHandTarget &&
                humanoidController.m_limbs.m_headTarget)
            {
                humanoidController.m_limbs.m_lHandTarget.transform.position = lHandPos;
                humanoidController.m_limbs.m_lHandTarget.transform.rotation = lHandRot;
                humanoidController.m_limbs.m_rHandTarget.transform.position = rHandPos;
                humanoidController.m_limbs.m_rHandTarget.transform.rotation = rHandRot;
                humanoidController.m_limbs.m_headTarget.transform.position = hmdPos;
                humanoidController.m_limbs.m_headTarget.transform.rotation = hmdRot;
            }
        }
    }

    //Six point tracking
    public void UpdateHumanoidController(Vector3 lHandPos, Quaternion lHandRot, Vector3 rHandPos, Quaternion rHandRot, Vector3 hmdPos, Quaternion hmdRot, Vector3 lFootPos, Quaternion lFootRot, Vector3 rFootPos, Quaternion rFootRot, Vector3 rootPos, Quaternion rootRot)
    {
        if (humanoidController != null)
        {
            if (humanoidController.m_limbs.m_lHandTarget)
            {
                humanoidController.m_limbs.m_lHandTarget.transform.position = lHandPos;
                humanoidController.m_limbs.m_lHandTarget.transform.rotation = lHandRot * Quaternion.AngleAxis(rotRightBy.z, Vector3.forward) * Quaternion.AngleAxis(rotRightBy.y, Vector3.up) * Quaternion.AngleAxis(rotRightBy.x, Vector3.right);
            }
            if (humanoidController.m_limbs.m_rHandTarget)
            {
                humanoidController.m_limbs.m_rHandTarget.transform.position = rHandPos;
                humanoidController.m_limbs.m_rHandTarget.transform.rotation = rHandRot * Quaternion.AngleAxis(rotLeftBy.z, Vector3.forward) * Quaternion.AngleAxis(rotLeftBy.y, Vector3.up) * Quaternion.AngleAxis(rotLeftBy.x, Vector3.right);
            }
            if (humanoidController.m_limbs.m_headTarget)
            {
                humanoidController.m_limbs.m_headTarget.transform.position = hmdPos;
                humanoidController.m_limbs.m_headTarget.transform.rotation = hmdRot;
            }
            if (humanoidController.m_limbs.m_lFootTarget)
            {
                humanoidController.m_limbs.m_lFootTarget.transform.position = lFootPos;
                humanoidController.m_limbs.m_lFootTarget.transform.rotation = lFootRot;
            }
            if (humanoidController.m_limbs.m_rFootTarget)
            {
                humanoidController.m_limbs.m_rFootTarget.transform.position = rFootPos;
                humanoidController.m_limbs.m_rFootTarget.transform.rotation = rFootRot;
            }
            if (humanoidController.m_limbs.m_rootTarget)
            {
                humanoidController.m_limbs.m_rootTarget.transform.position = rootPos;
                humanoidController.m_limbs.m_rootTarget.transform.rotation = rootRot;
            }
        }
    }

    //Dynamically link and unlink a specific tracking target among the 6 point tracking setup for the avatar
    public void UpdateTrackerStatus(bool state, int trackerType)
    {
        if (humanoidController == null)
        {
            humanoidController = GetComponentInChildren<tntHumanoidController>();
        }
        var type = (VRTrackerType)trackerType;
        if (type == VRTrackerType.Head)
        {
            humanoidController.m_limbs.m_headTarget = state ? GetCorrespondingTarget(type) : null;
        }
        if (type == VRTrackerType.LeftHand)
        {
            humanoidController.m_limbs.m_lHandTarget = state ? GetCorrespondingTarget(type) : null;
        }
        if (type == VRTrackerType.RightHand)
        {
            humanoidController.m_limbs.m_rHandTarget = state ? GetCorrespondingTarget(type) : null;
        }
        if (type == VRTrackerType.Root)
        {
            humanoidController.m_limbs.m_rootTarget = state ? GetCorrespondingTarget(type) : null;
        }
        if (type == VRTrackerType.LeftFoot)
        {
            humanoidController.m_limbs.m_lFootTarget = state ? GetCorrespondingTarget(type) : null;
        }
        if (type == VRTrackerType.RightFoot)
        {
            humanoidController.m_limbs.m_rFootTarget = state ? GetCorrespondingTarget(type) : null;
        }
    }

    private GameObject GetCorrespondingTarget(VRTrackerType type)
    {
        ThreePointTrackingLocator[] locators = GetComponentsInChildren<ThreePointTrackingLocator>();
        foreach (var loc in locators)
        {
            if (loc.locatorType == type)
            {
                return loc.gameObject;
            }
        }
        return null;
    }

    public void MakeHumanoidControllerDead()
    {
        humanoidController.SetDead(true);
    }

    public void MakeHumanoidControllerResync()
    {
        tntBase rootBase = humanoidController.GetComponentInParent<tntBase>();
        Transform robotContainer = rootBase.transform.parent;
        Transform robotHead = humanoidController.m_limbs.m_neck.transform;
        Transform headTracker = humanoidController.m_limbs.m_headTarget.transform;

        float yDiffRecord = (headTracker.position - robotHead.position).y; //Used to keep character head height
        rootBase.SetKinematic(true); //Set root kinematic so that it can be moved

        //Order is important
        Quaternion rot = Quaternion.FromToRotation(robotHead.forward, headTracker.forward);
        robotContainer.rotation = rot * robotContainer.rotation;
        Vector3 trans = headTracker.position - robotHead.position;
        robotContainer.position += trans;

        yDiffRecord = Mathf.Clamp(yDiffRecord, -1.0f, 1.0f); //Moderately limit the y difference
        robotContainer.position = new Vector3(robotContainer.position.x, robotContainer.position.y - yDiffRecord, robotContainer.position.z); //Keep character head height but to certain extend

        //Delay set back to non-kinematic state
        StartCoroutine(DelaySetKinematic(rootBase, false));
    }

    IEnumerator DelaySetKinematic(tntBase _rootBase, bool b)
    {
        yield return null;
        yield return null;
        yield return null; //Need to wait for 3 frames to make it work
        //yield return new WaitForSeconds(0.15f);
        _rootBase.SetKinematic(b);

        //Reset the flag for local player so that next time it can be triggered again
        var monitor = GetComponent<ThreePointTrackingMonitor>();
        if (monitor != null)
        { monitor.outOfControl = false; }
    }
}
