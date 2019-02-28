using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkRigidBodyPVDataSync: NetworkBehaviour
{
    ConnectionClient selfClient;
	NetworkRigidBodyGrabDataSync grabSync;
	NetworkRigidBodyAssetSpawner assetSpawner;
    GrabManager grabMgr;
    ThreePointTrackingGameMgr threePtGM;

	private bool startUpdating = false;

    private Vector3 initPos;
    private Quaternion initRot;
    private Vector3 rapierSpawnOffset = new Vector3(0.2f, 0, 0.4f);

    [SyncVar]
	private int staticFrameCount = 0;
	private int staticFrameCountThreshold = 120;

	private int sendRate = 60;

	private float receivedSequence = -1;

	private InterpolationBufferPlayer interpolationBuff;

    private float lastRespawnTime = 0f;

	private void Init() {
		if (isClient && selfClient==null)
			selfClient = FindObjectOfType<ConnectionClient>();
		if (grabSync==null)
			grabSync = GetComponent<NetworkRigidBodyGrabDataSync> ();
		if (assetSpawner==null)
			assetSpawner =  GetComponent<NetworkRigidBodyAssetSpawner> ();
		if (interpolationBuff==null)
			interpolationBuff = new InterpolationBufferPlayer (sendRate);
        if (threePtGM==null)
            threePtGM = ThreePointTrackingGameMgr.GetInstance();
        if (grabMgr==null)
            grabMgr = GetComponentInParent<GrabManager>();
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
        if (interpolationBuff != null) {
			interpolationBuff.Reset ();
			receivedSequence = -1;
		}
			
		if (!isClient)
			return;
		
		base.OnStartAuthority();
		Init ();
        
        initPos = transform.parent.position;
        initRot = transform.parent.rotation;

        CancelInvoke("SendPVData");
		InvokeRepeating("SendPVData", 0, (1.0f / sendRate));        
	}

    [Client]
	private void SendPVData()
	{

        if (!selfClient.GetConnectionStatus())
        {
			CancelInvoke("SendPVData");
            return;
        }

		if (isServer)
			return;
		
		// dont send PV data in case the object is in grab state 
		if (grabSync != null) {
			if (!grabSync.grabOwner.Equals(NetworkInstanceId.Invalid))
				return;
		}

		startUpdating = false;

		tntRigidBody rb = assetSpawner.assetRoot.GetComponentInChildren<tntRigidBody> ();
		if (rb == null)
			return;

		if (!hasAuthority) {
			if (!rb.m_IsKinematic)
				rb.SetKinematic (true);

			if (interpolationBuff != null) {
				interpolationBuff.Reset ();
				receivedSequence = -1;
			}

			CancelInvoke ("SendPVData");
			return;
		} else {
			if (rb.m_IsKinematic)
				rb.SetKinematic (false);
		}

        CheckForOutOfBounds();
			
		Vector3 pos = rb.position;
        Quaternion rot = rb.rotation;
		Vector3 velocity = rb.linearVelocity;
		Vector3 angularVelocity = rb.angularVelocity;

		// send update command to server
		CheckForValidVelocity(velocity,angularVelocity);
		if (staticFrameCount < staticFrameCountThreshold) {
			CmdServerUpdateRigidBody (pos, rot, velocity, angularVelocity, NetworkTransport.GetNetworkTimestamp(), Time.time);
		}
	}

    public void ResetStaticFrameCount()
    {
        staticFrameCount = 0;
    }

    //Reset all static frame counter
    [Server]
    public void SvrResetStaticFrameCount()
    {
        if (isServer) //Necessary to supress a warning
        {
            RpcResetStaticFrameCount();
        }
    }

    [ClientRpc]
    private void RpcResetStaticFrameCount()
    {
        ResetStaticFrameCount();
    }

    private void CheckForValidVelocity(Vector3 velocity, Vector3  angularVelocity) {
		if (velocity.magnitude <= 0 && angularVelocity.magnitude <= 0) {
			staticFrameCount ++;
			if (staticFrameCount > staticFrameCountThreshold)
				staticFrameCount = staticFrameCountThreshold;	
		} else {
			staticFrameCount = 0;
		}
	}

    private void CheckForOutOfBounds() {
        if (threePtGM != null && threePtGM.chaperonePoints.Count > 0 && grabMgr != null) {
            Vector3 _point = grabSync.transform.position;
            int ySign = 0;
            for (int i = 0; i < threePtGM.chaperonePoints.Count; i++) {
                Vector3 vecA = threePtGM.chaperonePoints[i] - _point;
                Vector3 vecB = threePtGM.chaperonePoints[(i + 1) % threePtGM.chaperonePoints.Count] - _point;
                int newYSign = Vector3.Cross(vecA, vecB).y > 0 ? 1 : -1;
                if (newYSign != ySign && ySign != 0) {
                    // is out of bounds
                    if (!grabMgr.isGrabbed && (Time.time - grabMgr.lastTimeGrabbed > 5f)) {
                        RespawnRigidBody();
                        return;
                    }
                }
                ySign = newYSign;
            }
        }
    }

	[Command(channel=1)]
	private void CmdServerUpdateRigidBody(Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity, double ts, float sequence)
	{
		// update server rigid body
		UpdateRigidBodyTarget (pos,rot, velocity,angularVelocity, ts, sequence);

		// update all client rigid body
		RpcClientUpdateRigidBody (pos, rot, velocity, angularVelocity, ts, sequence);
	}

	[ClientRpc(channel=1)]
	private void RpcClientUpdateRigidBody(Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity, double ts, float sequence)
	{
		//if it is the same client that sent update rigit body command then ignore it. 
		if (!hasAuthority)
			UpdateRigidBodyTarget (pos, rot, velocity, angularVelocity, ts, sequence);
	}
    
    private void RespawnRigidBody()
    {
        // only owner should attempt to respawn, restrict respawn to once/5 seconds
        if (!hasAuthority | (Time.time - lastRespawnTime < 5f))
            return;

        tntRigidBody theRB = GetComponentInParent<tntRigidBody>();
        theRB.m_IsKinematic = true;

        // spawn location changes slightly based on item (weird offset)
        if (theRB.name == "Entity-Rapier(Clone)")
        {
            if (initPos.x > 0)
            {
                theRB.transform.position = initPos + rapierSpawnOffset;
                theRB.position = initPos + rapierSpawnOffset;
            }
            else
            {
                theRB.transform.position = initPos - rapierSpawnOffset;
                theRB.position = initPos - rapierSpawnOffset;
            }
        }
        else
        {
            theRB.transform.position = initPos;
            theRB.position = initPos;
        }
        theRB.transform.rotation = initRot;
        theRB.rotation = initRot;
        theRB.linearVelocity = Vector3.zero;
        theRB.m_IsKinematic = false;

        lastRespawnTime = Time.time;
    }

	private void UpdateRigidBodyTarget(Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity, double ts, float sequence) {

		startUpdating = true;

		if (sequence > receivedSequence) {
			if (receivedSequence == -1)
				receivedSequence = sequence;
			RigidBodySnap rbs = new RigidBodySnap ();
			rbs.position = pos;
			rbs.orientation = rot;
			rbs.linear_velocity = velocity;
			interpolationBuff.AddSnapshot (sequence, rbs);
			receivedSequence = sequence;
		}
	}
		
	public void FixedUpdate() {

		// dont update in case the object is in grab state 
		if (grabSync != null) {
			if (!grabSync.grabOwner.Equals (NetworkInstanceId.Invalid)) {
				startUpdating = false;
				return;
			}
		}

		if (!startUpdating)
			return;
		
		if (!hasAuthority) {
			RigidBodySnap rbs = interpolationBuff.GetInterpolatedSnap(Time.time);
			if (rbs != null) {
				tntRigidBody rb = null;
				if (assetSpawner != null && assetSpawner.assetRoot != null)
					rb = assetSpawner.assetRoot.GetComponentInChildren<tntRigidBody> ();
				if (rb != null) {
					rb.linearVelocity = rbs.linear_velocity;
					rb.position = rbs.position;
					rb.rotation = rbs.orientation;
				} else {
					transform.position =  rbs.position;
					transform.rotation = rbs.orientation;
				}
			}
		} else {
			return;
		}

	}
}
