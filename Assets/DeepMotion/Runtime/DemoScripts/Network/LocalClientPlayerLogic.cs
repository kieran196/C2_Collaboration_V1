using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalClientPlayerLogic : MonoBehaviour {

    public Transform lHandController = null;
    public Transform rHandController = null;
    public Transform hmd = null;
    public Transform lFootTracker = null;
    public Transform rFootTracker = null;
    public Transform rootTracker = null;

    ThreePointTrackingMonitor monitor;

	private NetworkPlayerGrabDataSync grabDataSync;

	private float grabRange = 0.35f;

    public bool disableRobotBody = false;

	private tntRigidBody lHandGrabbedBody = null;
	private tntRigidBody rHandGrabbedBody = null;
    private bool lHandGrabbedPlayer = false;
    private bool rHandGrabbedPlayer = false;

    //Get the transform of the transform locator of a specific VR tracking object. Inactive(turned off or untracked) gameobject won't be returned
    public static Transform GetTrackedVRObjectTransformLocator(VRTrackerType type)
    {
        Transform retTransform = null; 
        if (type == VRTrackerType.Head)
        {
            var go = GameObject.Find("Main Camera (eye)/Transform Locator");
            if (go == null)
                go = GameObject.Find("Main Camera (eye)");
            if (go != null)
            {
                retTransform = go.transform;
            }
            else
            {
                retTransform = FindObjectOfType<SteamVR_Camera>().transform;
            }
        }
        if (type == VRTrackerType.LeftHand)
        {
            var go = GameObject.Find("Controller (left)/Transform Locator");
            if (go == null)
                go = GameObject.Find("Controller (left)");
            if (go != null)
                retTransform = go.transform;
        }
        if (type == VRTrackerType.RightHand)
        {
            var go = GameObject.Find("Controller (right)/Transform Locator");
            if (go == null)
                go = GameObject.Find("Controller (right)");
            if (go != null)
                retTransform = go.transform;
        }
        if (type == VRTrackerType.Root)
        {
            var go = GameObject.Find("Tracker (root)/Transform Locator");
            if (go == null)
            {
                go = GameObject.Find("Tracker (root)/Rotation Corrector/Transform Locator");
                if (go == null)
                {
                    var parent = GameObject.Find("Tracker (root)");
                    if (parent)
                    {
                        go = new GameObject("Transform Locator"); //If no Transform Locator found, create a new one to keep structure
                        go.transform.SetParent(parent.transform);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.rotation = Quaternion.identity;
                    }
                }
            }
            if (go != null)
                retTransform = go.transform;
            else
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRoot = false;

        }
        if (type == VRTrackerType.LeftFoot)
        {
            var go = GameObject.Find("Tracker (left foot)/Transform Locator");
            if (go == null)
            {
                go = GameObject.Find("Tracker (left foot)/Rotation Corrector/Transform Locator");
                if (go == null)
                {
                    var parent = GameObject.Find("Tracker (left foot)");
                    if (parent)
                    {
                        go = new GameObject("Transform Locator"); //If no Transform Locator found, create a new one to keep structure
                        go.transform.SetParent(parent.transform);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.rotation = Quaternion.identity;
                    }
                }
            }
            if (go != null)
                retTransform = go.transform;
        }
        if (type == VRTrackerType.RightFoot)
        {
            var go = GameObject.Find("Tracker (right foot)/Transform Locator");
            if (go == null)
            {
                go = GameObject.Find("Tracker (right foot)/Rotation Corrector/Transform Locator");
                if (go == null)
                {
                    var parent = GameObject.Find("Tracker (right foot)");
                    if (parent)
                    {
                        go = new GameObject("Transform Locator"); //If no Transform Locator found, create a new one to keep structure
                        go.transform.SetParent(parent.transform);
                        go.transform.localPosition = Vector3.zero;
                        go.transform.rotation = Quaternion.identity;
                    }
                }
            }
            if (go != null)
                retTransform = go.transform;
        }
        return retTransform;
    }

    void Start () {

		var settings = FindObjectOfType<ThreePointTrackingSettings>();

		if (settings.VRMode)
        {
            //Point to VR trackers(if exist or active).
            //If a specific tracker can not be found, auto disable the corresponding tracker in the setting for the demo
            lHandController = GetTrackedVRObjectTransformLocator(VRTrackerType.LeftHand);
            if (lHandController == null)
            {
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackLeftHand = false;
            }

            rHandController = GetTrackedVRObjectTransformLocator(VRTrackerType.RightHand);
            if (rHandController == null)
            {
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRightHand = false;
            }

            hmd = GetTrackedVRObjectTransformLocator(VRTrackerType.Head);
            if (hmd == null)
            {
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackHead = false;
            }

            lFootTracker = GetTrackedVRObjectTransformLocator(VRTrackerType.LeftFoot);
            if (lFootTracker == null)
            {
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackLeftFoot = false;
            }

            rFootTracker = GetTrackedVRObjectTransformLocator(VRTrackerType.RightFoot);
            if (rFootTracker == null)
            {
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRightFoot = false;
            }

            rootTracker = GetTrackedVRObjectTransformLocator(VRTrackerType.Root);
            if (rootTracker == null)
            {
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRoot = false;
            }
        }
        else
        {
			//Normal mouse driven trackers
			Transform tf = transform.Find ("TPTRobot/lHandTracker");
			if (tf != null)
				lHandController = tf;

			tf = transform.Find ("TPTRobot/rHandTracker");
			if (tf != null)
				rHandController = tf;

			tf = transform.Find ("TPTRobot/headTracker");
			if (tf != null)
				hmd = tf;

            tf = transform.Find("TPTRobot/lFootTracker");
            if (tf != null)
                lFootTracker = tf;

            tf = transform.Find("TPTRobot/rFootTracker");
            if (tf != null)
                rFootTracker = tf;

            tf = transform.Find("TPTRobot/pivotTracker");
            if (tf != null)
                rootTracker = tf;
        }

        // setup for interactive hand animation
#if ENABLE_HAND_ANIMATION // Supress warning CS0162 'Unreachable code detected'. TODO Ruudy Liu: enable or remove this code (see commit 607ab3026b345e27533cc7c0989448186e5c2d9a)
        if (false)
        {
            Transform kunckle = transform.GetComponentInChildren<tntHumanoidController>().transform.parent.parent.Find("lKnuckle");
            if (kunckle != null)
            {
                InteractiveHandAnimator animator = kunckle.GetComponent<InteractiveHandAnimator>();
                if (animator != null && lHandController != null)
                {
                    animator.trackedController = lHandController.GetComponentInParent<SteamVR_TrackedController>();
                }
            }

            kunckle = transform.GetComponentInChildren<tntHumanoidController>().transform.parent.parent.Find("rKnuckle");
            if (kunckle != null)
            {
                InteractiveHandAnimator animator = kunckle.GetComponent<InteractiveHandAnimator>();
                if (animator != null && rHandController != null)
                {
                    animator.trackedController = rHandController.GetComponentInParent<SteamVR_TrackedController>();
                }
            }

            Transform lowerKunckle = transform.GetComponentInChildren<tntHumanoidController>().transform.parent.parent.Find("lLowerKuckle");
            if (lowerKunckle != null)
            {
                InteractiveHandAnimator animator = lowerKunckle.GetComponent<InteractiveHandAnimator>();
                if (animator != null && lHandController != null)
                {
                    animator.trackedController = lHandController.GetComponentInParent<SteamVR_TrackedController>();
                }
            }

            lowerKunckle = transform.GetComponentInChildren<tntHumanoidController>().transform.parent.parent.Find("rLowerKunckle");
            if (lowerKunckle != null)
            {
                InteractiveHandAnimator animator = lowerKunckle.GetComponent<InteractiveHandAnimator>();
                if (animator != null && rHandController != null)
                {
                    animator.trackedController = rHandController.GetComponentInParent<SteamVR_TrackedController>();
                }
            }
        }
		//----------
#endif

        monitor = GetComponent<ThreePointTrackingMonitor>();
        if (monitor)
        {
            monitor.SetTargets(lHandController, rHandController, hmd);
        }

		ThreePointTrackingGameMgr gm = FindObjectOfType<ThreePointTrackingGameMgr> ();

		// just disabled for time being. since the grab body part feature is not fully dev tested 
		if (gm.trackedController1 != null && gm.trackedController2 != null)
		{
			gm.trackedController1.TriggerClicked += OnController1TriggerClicked;
			gm.trackedController1.TriggerUnclicked += OnController1TriggerUnclicked;
			gm.trackedController2.TriggerClicked += OnController2TriggerClicked;
			gm.trackedController2.TriggerUnclicked += OnController2TriggerUnclicked;
		}

        grabDataSync = GetComponentInChildren<NetworkPlayerGrabDataSync>();
        if ((disableRobotBody) || (settings.multiplayerMode == MultiPlayerMode.ServerAndClient))
        {
            GetComponent<UpdateAvatarWith3PTData>().enabled = false;
            Transform child = transform.Find("TPTRobot/Robot");
            if (child != null)
                child.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        ThreePointTrackingGameMgr gm = FindObjectOfType<ThreePointTrackingGameMgr>();
        if (gm !=null && gm.trackedController1 != null && gm.trackedController2 != null)
        {
            if (gm.trackedController1 != null && gm.trackedController2 != null)
            {
                gm.trackedController1.TriggerClicked -= OnController1TriggerClicked;
                gm.trackedController1.TriggerUnclicked -= OnController1TriggerUnclicked;
                gm.trackedController2.TriggerClicked -= OnController2TriggerClicked;
                gm.trackedController2.TriggerUnclicked -= OnController2TriggerUnclicked;
            }
        }
    }

    void Update()
    {
        if (monitor)
        {
            monitor.RunUpdate(); //Put it here for organizing purpose, can be put into its own MonoBehavior later if needed

            if (monitor.outOfControl)
            {
                if (lHandGrabbedBody | lHandGrabbedPlayer) Release(GrabHandType.LEFT);
                if (rHandGrabbedBody | rHandGrabbedPlayer) Release(GrabHandType.RIGHT);
            }
        }
    }

	public void Get3PTData (out Vector3 lHandPos, out Quaternion lHandRot, out Vector3 rHandPos, out Quaternion rHandRot, out Vector3 hmdPos, out Quaternion hmdRot) {
		if ((lHandController != null) && (rHandController != null) && (hmd != null)) {
			lHandPos = lHandController.position;
			lHandRot = lHandController.rotation;
			rHandPos = rHandController.position;
			rHandRot = rHandController.rotation;
			hmdPos = hmd.position;
			hmdRot = hmd.rotation;
		} else {
			lHandPos = Vector3.zero;
			lHandRot = Quaternion.identity;
			rHandPos = Vector3.zero;
			rHandRot = Quaternion.identity;
			hmdPos = Vector3.zero;
			hmdRot = Quaternion.identity;
		}
	}

    /*Get the standard 6 point tracking data.
    empty or unmeaningful data for all trackers will still be sent here with this function interface, 
    nullifying operation accross network for certain trackers will be performed somewhere else*/
    public void Get6PTData(
        out Vector3 lHandPos,
        out Quaternion lHandRot,
        out Vector3 rHandPos,
        out Quaternion rHandRot,
        out Vector3 hmdPos,
        out Quaternion hmdRot,
        out Vector3 lFootPos,
        out Quaternion lFootRot,
        out Vector3 rFootPos,
        out Quaternion rFootRot,
        out Vector3 rootPos,
        out Quaternion rootRot)
    {
        if (lHandController != null)
        {
            lHandPos = lHandController.position;
            lHandRot = lHandController.rotation;
        }
        else
        {
            lHandPos = Vector3.zero;
            lHandRot = Quaternion.identity;
        }

        if (rHandController != null)
        {
            rHandPos = rHandController.position;
            rHandRot = rHandController.rotation;
        }
        else
        {
            rHandPos = Vector3.zero;
            rHandRot = Quaternion.identity;
        }

        if (hmd != null)
        {
            hmdPos = hmd.position;
            hmdRot = hmd.rotation;
        }
        else
        {
            hmdPos = Vector3.zero;
            hmdRot = Quaternion.identity;
        }

        if (lFootTracker != null)
        {
            lFootPos = lFootTracker.position;
            lFootRot = lFootTracker.rotation;
        }
        else
        {
            lFootPos = Vector3.zero;
            lFootRot = Quaternion.identity;
        }

        if (rFootTracker != null)
        {
            rFootPos = rFootTracker.position;
            rFootRot = rFootTracker.rotation;
        }
        else
        {
            rFootPos = Vector3.zero;
            rFootRot = Quaternion.identity;
        }

        if (rootTracker != null)
        {
            rootPos = rootTracker.position;
            rootRot = rootTracker.rotation;
        }
        else
        {
            rootPos = Vector3.zero;
            rootRot = Quaternion.identity;
        }
    }

    private Vector3 CalcPointOnSurfaceOfColliderTowardsPoint(Collider thisCollider, Vector3 holdingPointInWorld)
	{
		// Calc raycasting direction
		Vector3 center = thisCollider.transform.position;
		Vector3 direction = center - holdingPointInWorld;
		// Check if we're at the center of the collider
		if (direction.magnitude < Mathf.Epsilon) return center;
		// Check if we're inside the collider
		if (direction.magnitude < 2.0f * thisCollider.bounds.extents.magnitude)
			direction *= 2.0f * thisCollider.bounds.extents.magnitude / direction.magnitude;

		// Raycast
		Ray ray = new Ray(center - direction, direction.normalized);
		RaycastHit hitInfo;
		bool success = thisCollider.Raycast(ray, out hitInfo, direction.magnitude);

        // Return hit point
        return success ? hitInfo.point : center;
        //return center + direction.normalized * thisCollider.bounds.extents.magnitude;
	}



	private void Grab(GrabHandType hand)
	{
        string handName = "lWrist";
		if (hand == GrabHandType.LEFT)
			handName = "rWrist";

        //Transform handProxy = transform.FindChild ("TPTRobot/Robot/" + handName);
        Transform handProxy = null; tntBase rootBase = GetComponentInChildren<tntBase>(); Transform robot = null;
        if (rootBase) { robot = rootBase.transform.parent; }
        if (robot) { handProxy = robot.Find(handName); }

        tntRigidBody rbHand = handProxy.GetComponentInChildren<tntRigidBody> ();
		Vector3 grabPoint = handProxy.position;

		// Get all overlaps within grab distance
		Collider[] overlaps = Physics.OverlapSphere (grabPoint, grabRange);

		// 1b. Filter overlaps to non-kinematic rigid bodies
		List<tntRigidBody> bodies = new List<tntRigidBody> ();
		//List<tntLink> links = new List<tntLink>(); // todo: we don't currently support grabbing links that are not compounds & don't contain child tntRigidBodies
		foreach (var o in overlaps) {
			tntRigidBody rb = 
                System.Array.Find<tntRigidBody>(
                    o.gameObject.GetComponentsInParent<tntRigidBody> (), aBody => !aBody.ContainedByCompoundShape());

            if (rb == null)
            {
                if (o.GetComponentInParent<tntChildLink>() != null)
                {
                    rb = o.GetComponentInParent<tntRigidBody>();
                }
            }

			UpdateAvatarWith3PTData avatar = o.GetComponentInParent<UpdateAvatarWith3PTData> ();

            if ((avatar != null) && (avatar.transform != transform)) {
                if (rb && rb.enabled && rb.mass != 0.0f)
					bodies.Add (rb);
			} else {
				GrabManager gb = o.GetComponentInParent<GrabManager> ();                
                if (gb != null && rb && rb.mass != 0.0f) {
                    bodies.Add (rb);
				}
			}
		}
        tntRigidBody closestRb = null;
		float minDistSqr = Mathf.Infinity;
        Vector3 closestSurfacePoint = Vector3.zero;
        foreach (var rb in bodies) {
			// Heuristics -- use some point on the collider as the closest point
			Vector3 pointOnObjectSurface = CalcPointOnSurfaceOfColliderTowardsPoint (rb.GetComponentInChildren <Collider> (), grabPoint);
			float distSqr = (pointOnObjectSurface - grabPoint).sqrMagnitude;
			if (distSqr < minDistSqr) {
                minDistSqr = distSqr;
				closestRb = rb;
                closestSurfacePoint = pointOnObjectSurface;
            }
		}

		// 2. Create a holding link
		// 
		if (closestRb) {
			tntLink closestLink = closestRb.GetComponentInParent<tntLink> ();
			GrabManager gm = closestRb.GetComponentInParent<GrabManager> ();
            // keep rbs at a minimum dist
            if (closestRb.name == "Entity-BallPrefab(Clone)")
                closestRb.m_transform.position = Vector3.MoveTowards(closestSurfacePoint, grabPoint, Mathf.Sqrt(minDistSqr) - 0.3f);
            else if (closestRb.name == "Entity-Rapier(Clone)")
            {
                Vector3 handle = closestRb.m_transform.position - (closestRb.m_transform.up * 0.47f);
                Vector3 handleTarget = Vector3.MoveTowards(handle, grabPoint, Vector3.Distance(handle, grabPoint) - .17f);
                closestRb.m_transform.position += handleTarget - handle;
            }

            if (gm==null && closestLink && closestLink.enabled) {
				Transform heldObject = closestLink.transform;
				// Calculate a good point on the held body
				Vector3 globalPivotB = CalcPointOnSurfaceOfColliderTowardsPoint (closestRb.GetComponent<Collider> (), grabPoint);
				Vector3 pivotBInLocal = heldObject.InverseTransformDirection (globalPivotB - heldObject.position); // Transform without scale

				Vector3 globalPivotA = CalcPointOnSurfaceOfColliderTowardsPoint (rbHand.GetComponent<Collider> (), heldObject.position);
				Vector3 pivotAInLocal = handProxy.InverseTransformDirection (globalPivotA - handProxy.position); // Transform without scale

                if (hand == GrabHandType.LEFT) lHandGrabbedPlayer = true;
                else rHandGrabbedPlayer = true;

                if (grabDataSync)
                {
                    // now send network request to add links to remote avatar
                    grabDataSync.SendGrabData(heldObject.GetComponentInParent<UpdateAvatarWith3PTData>().gameObject, hand, heldObject, handProxy, pivotAInLocal, pivotBInLocal);
                }                
			} else if (gm != null) { // now try to grab a non avatar rigid body
				tntChildLink handLink = handProxy.GetComponent<tntChildLink> ();
				if (handLink != null) {
					NetworkRigidBodyGrabDataSync gds = gm.gameObject.GetComponentInChildren <NetworkRigidBodyGrabDataSync> ();
					if (gds != null) {
                        bool res = false;
                        if (grabDataSync)
                        {
                            Vector3 rbLocalPos = handProxy.InverseTransformPoint(closestRb.position); //Calculate local pos
                            Quaternion rbLocalRotation = Quaternion.Inverse(handProxy.transform.rotation) * closestRb.rotation; //Calculate local rot
                            var rbLocalTransform = new TransformInfoSimple(rbLocalPos, rbLocalRotation);

                            res = grabDataSync.SendRigidBodyGrabData(gds, transform, handProxy.name, hand, rbLocalTransform);
                        }
						if (res) {
							if (hand == GrabHandType.LEFT)
                            {
								lHandGrabbedBody = closestRb;
                                if (rHandGrabbedBody == lHandGrabbedBody) rHandGrabbedBody = null;
                            }
							else
							{
                            	rHandGrabbedBody = closestRb;
                                if (lHandGrabbedBody == rHandGrabbedBody) lHandGrabbedBody = null;
                            }
						}
					}
				}
			}
		}
	}

	private void Release(GrabHandType hand)
	{
		if (hand == GrabHandType.LEFT && lHandGrabbedBody != null) {
			GrabManager gm = lHandGrabbedBody.gameObject.GetComponentInParent<GrabManager> (); // access the root
			if (gm != null) {
				NetworkRigidBodyGrabDataSync gds = gm.gameObject.GetComponentInChildren <NetworkRigidBodyGrabDataSync> ();
				if (gds != null) {
					bool res = grabDataSync.SendRigidBodyGrabRemovalData (gds,hand);
					if (res) {
						lHandGrabbedBody = null;
					}
				}
			}
		}
		else if (hand == GrabHandType.RIGHT && rHandGrabbedBody != null) {
			GrabManager gm = rHandGrabbedBody.gameObject.GetComponentInParent<GrabManager> (); // access the root
			if (gm != null) {
				NetworkRigidBodyGrabDataSync gds = gm.gameObject.GetComponentInChildren <NetworkRigidBodyGrabDataSync> ();
				if (gds != null) {
					bool res = grabDataSync.SendRigidBodyGrabRemovalData (gds,hand);
					if (res) {
						rHandGrabbedBody = null;
					}
				}
			}
		}
        else if (grabDataSync)
        {
            if (hand == GrabHandType.LEFT) lHandGrabbedPlayer = false;
            else rHandGrabbedPlayer = false;

            // now send network request to clear links to remote avatar
            grabDataSync.SendGrabRemovalData(hand);
        }
	}


	private void OnController1TriggerClicked(object sender, ClickedEventArgs e)
    {
        Grab (GrabHandType.LEFT);
	}

	private void OnController1TriggerUnclicked(object sender, ClickedEventArgs e)
	{
		Release (GrabHandType.LEFT);
	}

	private void OnController2TriggerClicked(object sender, ClickedEventArgs e)
	{
		Grab (GrabHandType.RIGHT);
	}

	private void OnController2TriggerUnclicked(object sender, ClickedEventArgs e)
	{
		Release (GrabHandType.RIGHT);
	}
}