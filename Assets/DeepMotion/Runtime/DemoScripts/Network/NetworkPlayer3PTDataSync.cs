using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkPlayer3PTDataSync: NetworkBehaviour
{
    ConnectionClient selfClient;
    private bool sixPointTrackingSetup = true;
    

    //SyncVars are mainly used here for synchronizing a client(through server to other clients) after the moment the client sending out synchronizatoin command
    //Ex. Client B connects to server after client A connected already. Need to sync client A to B so we need to remember the data here for the postponed synchronizatoin.
    [SyncVar]
    public float rescaleHeight = -1.0f; //HMD height
    [SyncVar]
    public float rescaleWidth = -1.0f; //Controller distance
    [SyncVar]
    public bool adjustedHeadState = false;
    [SyncVar]
    public bool adjustedLeftHandState = false;
    [SyncVar]
    public bool adjustedRightHandState = false;
    [SyncVar]
    public bool adjustedRootState = false;
    [SyncVar]
    public bool adjustedLeftFootState = false;
    [SyncVar]
    public bool adjustedRightFootState = false;

    private SixPointTrackingOptions preTrackingOptions = new SixPointTrackingOptions();

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

	public override void OnStartClient()
	{
		base.OnStartClient();
        if (rescaleHeight > 0)
        { MakeRescale(); }

        //Implemente the postponed synchronization from the remembered data.
        //The delay is needed here to ensure the same avatar will have the identical desired poses across network
        StartCoroutine(DelayMatchTrackersStatus());
    }

    IEnumerator DelayMatchTrackersStatus()
    {
        yield return new WaitForSeconds(.033f);
        MatchTrackersStatus();
    }

    private void MatchTrackersStatus()
    {
        if (adjustedHeadState)
        {
            UpdateTrackerStatus(true, (int)VRTrackerType.Head);
        }
        if (adjustedLeftHandState)
        {
            UpdateTrackerStatus(true, (int)VRTrackerType.LeftHand);
        }
        if (adjustedRightHandState)
        {
            UpdateTrackerStatus(true, (int)VRTrackerType.RightHand);
        }
        if (adjustedRootState)
        {
            UpdateTrackerStatus(true, (int)VRTrackerType.Root);
        }
        if (adjustedLeftFootState)
        {
            UpdateTrackerStatus(true, (int)VRTrackerType.LeftFoot);
        }
        if (adjustedRightFootState)
        {
            UpdateTrackerStatus(true, (int)VRTrackerType.RightFoot);
        }
    }

    public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
        selfClient = FindObjectOfType<ConnectionClient>();

        if (sixPointTrackingSetup)
        {
            InvokeRepeating("Send6PTData", 0, .033f);
        }
        else
        {
            InvokeRepeating("Send3PTData", 0, .033f);
        }  
	}

    [Client]
    private void Send6PTData() //Sync 6 point data throughout the network
    {
        if (!selfClient.GetConnectionStatus())
        {
            CancelInvoke("Send6PTData");
            return;
        }
        Vector3 lHandPos, rHandPos, hmdPos, lFootPos, rFootPos, rootPos;
        Quaternion lHandRot, rHandRot, hmdRot, lFootRot, rFootRot, rootRot;
        GetComponentInParent<LocalClientPlayerLogic>().Get6PTData(out lHandPos, out lHandRot, out rHandPos, out rHandRot, out hmdPos, out hmdRot, out lFootPos, out lFootRot, out rFootPos, out rFootRot, out rootPos, out rootRot);

        // update nullifying change if needed
        var trackingOptions = ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions;
        if (trackingOptions.trackHead != preTrackingOptions.trackHead)
        {
            CmdServerUpdateTrackerStatus(trackingOptions.trackHead, (int)VRTrackerType.Head);
        }
        if (trackingOptions.trackLeftHand != preTrackingOptions.trackLeftHand)
        {
            CmdServerUpdateTrackerStatus(trackingOptions.trackLeftHand, (int)VRTrackerType.LeftHand);
        }
        if (trackingOptions.trackRightHand != preTrackingOptions.trackRightHand)
        {
            CmdServerUpdateTrackerStatus(trackingOptions.trackRightHand, (int)VRTrackerType.RightHand);
        }
        if (trackingOptions.trackRoot != preTrackingOptions.trackRoot)
        {
            CmdServerUpdateTrackerStatus(trackingOptions.trackRoot, (int)VRTrackerType.Root);
        }
        if (trackingOptions.trackLeftFoot != preTrackingOptions.trackLeftFoot)
        {
            CmdServerUpdateTrackerStatus(trackingOptions.trackLeftFoot, (int)VRTrackerType.LeftFoot);
        }
        if (trackingOptions.trackRightFoot != preTrackingOptions.trackRightFoot)
        {
            CmdServerUpdateTrackerStatus(trackingOptions.trackRightFoot, (int)VRTrackerType.RightFoot);
        }
        preTrackingOptions = trackingOptions.GetShallowCopy(); //Remember last step tracking options

        // update local avater
        UpdateAvatar6Point(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot, lFootPos, lFootRot, rFootPos, rFootRot, rootPos, rootRot);

        // send update command to server
        CmdServerUpdateAvatar6Point(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot, lFootPos, lFootRot, rFootPos, rFootRot, rootPos, rootRot);
    }

    [Client]
	private void Send3PTData()
	{
        if (!selfClient.GetConnectionStatus())
        {
            CancelInvoke("Send3PTData");
            return;
        }
		Vector3 lHandPos, rHandPos, hmdPos;
		Quaternion lHandRot, rHandRot, hmdRot;
		GetComponentInParent<LocalClientPlayerLogic> ().Get3PTData (out lHandPos,out lHandRot,out rHandPos,out rHandRot,out hmdPos,out hmdRot);

		// update local avater
		UpdateAvatar (lHandPos,lHandRot,rHandPos,rHandRot,hmdPos,hmdRot);

		// send update command to server
		CmdServerUpdateAvatar(lHandPos,lHandRot,rHandPos,rHandRot,hmdPos,hmdRot);
	}

    [Client]
    public void SetCharacterDead()
    {
        if (!selfClient || !selfClient.GetConnectionStatus())
        {
            return;
        }

        //make dead local
        MakeAvatarDead();

        //Send dead command to server
        CmdServerMakeAvatorDead();
    }

    [Client]
    public void ResyncCharacter()
    {
        if (!selfClient || !selfClient.GetConnectionStatus())
        {
            return;
        }

        //make local resync
        MakeAvatarResync();

        //Send resync command to server
        CmdServerMakeAvatorResync();
    }

    [Client]
    public void MakeOtherRescale(float _h, float _w)
    {
        var self_client = FindObjectOfType<ConnectionClient>();
        if (!self_client.GetConnectionStatus())
        {
            return;
        }

        //Send rescale signal to server
        CmdServerMakeOthersRescale(_h, _w);
    }

    [Command]
    public void CmdServerUpdateAvatar(Vector3 lHandPos, Quaternion lHandRot, Vector3 rHandPos, Quaternion rHandRot, Vector3 hmdPos, Quaternion hmdRot)
    {
        // update server avatar
        UpdateAvatar(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot);

        // update all client avatars
        RpcClientUpdateAvatar(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot);
    }

    [Command]
    public void CmdServerUpdateAvatar6Point(
        Vector3 lHandPos,
        Quaternion lHandRot, 
        Vector3 rHandPos, 
        Quaternion rHandRot, 
        Vector3 hmdPos, 
        Quaternion hmdRot,
        Vector3 lFootPos,
        Quaternion lFootRot,
        Vector3 rFootPos,
        Quaternion rFootRot,
        Vector3 rootPos,
        Quaternion rootRot)
    {
        // update server avatar
        UpdateAvatar6Point(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot, lFootPos, lFootRot, rFootPos, rFootRot, rootPos, rootRot);

        // update all client avatars
        RpcClientUpdateAvatar6Point(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot, lFootPos, lFootRot, rFootPos, rFootRot, rootPos, rootRot);
    }

    [Command]
    public void CmdServerUpdateTrackerStatus(bool state, int trackerType)
    {
        if (state)
        {
            if (trackerType == (int)VRTrackerType.Head)
            {
                adjustedHeadState = true;
            }
            if (trackerType == (int)VRTrackerType.LeftHand)
            {
                adjustedLeftHandState = true;
            }
            if (trackerType == (int)VRTrackerType.RightHand)
            {
                adjustedRightHandState = true;
            }
            if (trackerType == (int)VRTrackerType.Root)
            {
                adjustedRootState = true;
            }
            if (trackerType == (int)VRTrackerType.LeftFoot)
            {
                adjustedLeftFootState = true;
            }
            if (trackerType == (int)VRTrackerType.RightFoot)
            {
                adjustedRightFootState = true;
            }
        }
        // update server
        UpdateTrackerStatus(state, trackerType);

        // update all clients
        RpcClientUpdateTrackerStatus(state, trackerType);
    }

    [Command]
    private void CmdServerMakeAvatorDead()
    {
        // make dead server avatar
        MakeAvatarDead();

        // make dead all client avatar
        RpcClientMakeAvatarDead();
    }

    [Command]
    private void CmdServerMakeAvatorResync()
    {
        // resync server avatar
        MakeAvatarResync();

        // resync all client avatar
        RpcClientMakeAvatarResync();
    }    

    [Command]
    private void CmdServerMakeOthersRescale(float _h, float _w)
    {
        rescaleHeight = _h; rescaleWidth = _w;
        // make use rescale server avatar
        MakeRescale();

        // make use rescale all client avatar
        RpcClientMakeOthersRescale(_h, _w);
    }

    [Command]
    public void CmdSetRescaleData(float _h, float _w)
    {
        rescaleHeight = _h; rescaleWidth = _w;
        var self_client = FindObjectOfType<ConnectionClient>(); //need to use this instead of the member var
        var gm = self_client.GetComponent<ThreePointTrackingGameMgr>();
        if (gm && gm.settings && gm.settings.VRMode && gm.settings.autoRescale)
        {
            MakeOtherRescale(_h, _w);
        }
    }

    [ClientRpc]
	private void RpcClientUpdateAvatar(Vector3 lHandPos, Quaternion lHandRot, Vector3 rHandPos, Quaternion rHandRot, Vector3 hmdPos, Quaternion hmdRot)
	{
		//if it is the same client that sent update avatar command then ignore it. 
		if (!isLocalPlayer)
			UpdateAvatar (lHandPos,lHandRot,rHandPos,rHandRot,hmdPos,hmdRot);
	}

    [ClientRpc]
    private void RpcClientUpdateAvatar6Point(
        Vector3 lHandPos, 
        Quaternion lHandRot, 
        Vector3 rHandPos, 
        Quaternion rHandRot, 
        Vector3 hmdPos, 
        Quaternion hmdRot,
        Vector3 lFootPos,
        Quaternion lFootRot,
        Vector3 rFootPos,
        Quaternion rFootRot,
        Vector3 rootPos,
        Quaternion rootRot)
    {
        //if it is the same client that sent update avatar command then ignore it. 
        if (!isLocalPlayer)
            UpdateAvatar6Point(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot, lFootPos, lFootRot, rFootPos, rFootRot, rootPos, rootRot);
    }

    [ClientRpc]
    private void RpcClientUpdateTrackerStatus(bool state, int trackerType)
    {
        //if it is the same client that sent the update command then ignore it. 
        UpdateTrackerStatus(state, trackerType);
    }

    [ClientRpc]
    private void RpcClientMakeAvatarDead()
    {
        //if it is the same client that sent make dead avatar command then ignore it. 
        if (!isLocalPlayer)
            MakeAvatarDead();
    }

    [ClientRpc]
    private void RpcClientMakeAvatarResync()
    {
        //if it is the same client that sent resync avatar command then ignore it. 
        if (!isLocalPlayer)
            MakeAvatarResync();
    }    

    [ClientRpc]
    private void RpcClientMakeOthersRescale(float _h, float _w)
    {
        //if it is the same client that sent make use rescale command then ignore it. 
        if (!isLocalPlayer)
        {
            rescaleHeight = _h; rescaleWidth = _w;
            MakeRescale();
        }
    }

    private void UpdateAvatar(Vector3 lHandPos, Quaternion lHandRot, Vector3 rHandPos, Quaternion rHandRot, Vector3 hmdPos, Quaternion hmdRot) {
		UpdateAvatarWith3PTData avatar = GetComponentInParent<UpdateAvatarWith3PTData> ();
		if (avatar!=null)
            avatar.UpdateHumanoidController(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot);
    }

    private void UpdateAvatar6Point(
        Vector3 lHandPos, 
        Quaternion lHandRot, 
        Vector3 rHandPos, 
        Quaternion rHandRot, 
        Vector3 hmdPos, 
        Quaternion hmdRot,
        Vector3 lFootPos,
        Quaternion lFootRot,
        Vector3 rFootPos,
        Quaternion rFootRot,
        Vector3 rootPos,
        Quaternion rootRot)
    {
        UpdateAvatarWith3PTData avatar = GetComponentInParent<UpdateAvatarWith3PTData>();
        if (avatar != null)
            avatar.UpdateHumanoidController(lHandPos, lHandRot, rHandPos, rHandRot, hmdPos, hmdRot, lFootPos, lFootRot, rFootPos, rFootRot, rootPos, rootRot);
    }

    private void UpdateTrackerStatus(bool state, int trackerType)
    {
        UpdateAvatarWith3PTData avatar = GetComponentInParent<UpdateAvatarWith3PTData>();
        if (avatar != null)
            avatar.UpdateTrackerStatus(state, trackerType);
    }

    private void MakeAvatarDead()
    {
        UpdateAvatarWith3PTData avatar = GetComponentInParent<UpdateAvatarWith3PTData>();
        if (avatar != null)
            avatar.MakeHumanoidControllerDead();
    }

    private void MakeAvatarResync()
    {
        UpdateAvatarWith3PTData avatar = GetComponentInParent<UpdateAvatarWith3PTData>();
        if (avatar != null)
            avatar.MakeHumanoidControllerResync();
    }

    private void MakeRescale()
    {
        var updateData = GetComponentInParent<UpdateAvatarWith3PTData>();
        GameObject clientPlayer = null;
        if (updateData)
        {
            clientPlayer = updateData.gameObject;
        }
        tntHumanoidController hc = null;
        if (clientPlayer)
        {
            hc = clientPlayer.GetComponentInChildren<tntHumanoidController>();
        }
        if (hc)
        {
            //Reconstruct
            tntBase root = hc.gameObject.GetComponentInParent<tntBase>();
            root.m_world.RemoveArticulationBase(root);
            tntEntityAndJointFactory.DestroyArticulationBase(root);

            float heightScaleFactor = 1.0f;
            float widthScaleFactor = 1.0f;

            ThreePointTrackingGameMgr.GetRescaleFactors(hc, rescaleHeight, rescaleWidth, ref heightScaleFactor, ref widthScaleFactor);

            tntLink lShoulder = null; tntLink rShoulder = null; tntLink lowerback_torse = null;
            GameObject avatar = hc.gameObject.transform.parent.parent.gameObject;
            APEModelScalerUtils.ConfigCheckAPEObject(avatar, out lShoulder, out rShoulder, out lowerback_torse);
            APEModelScalerUtils.ScaleGeometry(avatar, heightScaleFactor, widthScaleFactor, lShoulder, rShoulder, lowerback_torse);
            float geometryScalar = APEModelScalerUtils.getUnformScalar(widthScaleFactor, heightScaleFactor);
            APEModelScalerUtils.ScaleMass(avatar, APEModelScalerUtils.GetMassScalar(geometryScalar));
            APEModelScalerUtils.ScaleMOI(avatar, APEModelScalerUtils.GetMOIScalar(geometryScalar));
            APEModelScalerUtils.ScaleStrengths(avatar, geometryScalar);

            tntEntityAndJointFactory.CreateArticulationBase(root);
            root.m_world.AddArticulationBase(root);
        }
    }
}
