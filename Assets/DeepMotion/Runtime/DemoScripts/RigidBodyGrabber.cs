using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PhysicsAPI;

// for now, grabber is loosely connected to the parent by TransformConstraint
[RequireComponent(typeof(TransformConstraint))]
public class RigidBodyGrabber : MonoBehaviour, ITntTriggerListener
{
    public GameObject targetParent;
    tntRigidBody curHolding;
    List<tntRigidBody> grabTargets = new List<tntRigidBody>();
    tntRigidBody parentRigidBody;
    tntChildLink parentLink;
    tntBase parentBase;
    tntRigidBody grabberRigidBody;

	// Use this for initialization
    void Start () {
        grabberRigidBody = GetComponent<tntRigidBody>();
        if (grabberRigidBody != null)
        {
            grabberRigidBody.AddListener(this);
        }
        parentRigidBody = targetParent.GetComponent<tntRigidBody>();
        parentLink = targetParent.GetComponent<tntChildLink>();
        parentBase = targetParent.GetComponent<tntBase>();
        if (parentRigidBody != null)
        {
            parentRigidBody.AddListener(this);
        }
        else if (parentLink != null)
        {
            parentLink.AddListener(this);
        }
        else if (parentBase != null)
        {
            parentBase.AddListener(this);
        }
    }

    // Update is called once per frame
    void Update () {
    }      

    public void OnTntTriggerEnter(GameObject go)
    {
        tntRigidBody rb = go.GetComponent<tntRigidBody> ();
        RigidBodyGrabber grabber = go.GetComponent<RigidBodyGrabber>();
//        print("Enter " + name + " " + grabTargets.Count);
        if (rb == null || grabber != null)
            return;
        for (int i = 0; i < grabTargets.Count; i++)
            if (grabTargets[i] == rb)
                return;
        grabTargets.Add(rb);
    }

    public void OnTntTriggerStay(GameObject go)
    {
    }

    public void OnTntTriggerExit(GameObject go)
    {
        tntRigidBody rb = go.GetComponent<tntRigidBody>();
//        print("Exit " + name + " " + grabTargets.Count);
        grabTargets.Remove(rb);
    }

    public bool TryGrab()
    {
        // for now, choose the first rigidbody touched
        if (grabTargets.Count == 0)
            return false;
        curHolding = grabTargets[0];
        if (curHolding != null)
        {
            if (parentRigidBody != null)
                RigidBodyGrabbingUtil.GrabRigidBody(parentRigidBody, curHolding);
            else if (parentLink != null)
                RigidBodyGrabbingUtil.GrabRigidBody(parentLink, curHolding);
            else if (parentBase != null)
                RigidBodyGrabbingUtil.GrabRigidBody(parentBase, curHolding);
        }
        grabTargets.Clear();
        return true;
    }

    public void Release()
    {
        if (curHolding != null)
        {
            if (parentRigidBody != null)
                RigidBodyGrabbingUtil.ReleaseRigidBody(parentRigidBody, curHolding);
            else if (parentLink != null)
                RigidBodyGrabbingUtil.ReleaseRigidBody(parentLink, curHolding);
            else if (parentBase != null)
                RigidBodyGrabbingUtil.ReleaseRigidBody(parentBase, curHolding);
        }
        curHolding = null;
        grabTargets.Clear();
    }
}
