using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class  NetworkPlayerAssetSpawner: NetworkBehaviour
{
	[HideInInspector]
	public GameObject assetRoot = null;
	private Vector3 spawnPos;
    ThreePointTrackingSettings settings;

    public override void OnStartServer()
    {
        base.OnStartServer();

		ConnectionServer serverObj = FindObjectOfType<ConnectionServer> ();
        if (settings == null)
            settings = FindObjectOfType<ThreePointTrackingSettings>();
		if ((!serverObj.noSimulation) && ((settings.multiplayerMode == MultiPlayerMode.Server) ||(settings.multiplayerMode == MultiPlayerMode.ServerAndClient))) {
			spawnPos = transform.position;
			var serverEnt = Instantiate<GameObject> (settings.playerAsset.serverAssetPrefab,spawnPos,Quaternion.identity);

            PrefabReplacement pr = serverEnt.GetComponentInChildren<PrefabReplacement>();
            if (pr)
            {
                GameObject serverAvatar = settings.selfAvatar; // Stay here just for unified structure, not tested yet
                pr.prefab = serverAvatar;
                //pr.enabled = true;
                pr.DoSelfReplacing();
            }

            assetRoot = serverEnt;
			transform.SetParent (serverEnt.transform);
			transform.localPosition = Vector3.zero;
			serverEnt.transform.SetParent (serverObj.transform);
			serverEnt.name = "Player";
		}
    }

	public override void OnStartClient()
	{
		base.OnStartClient();
        spawnPos = transform.position;

        if (settings == null)
            settings = FindObjectOfType<ThreePointTrackingSettings>();
        var clientEnt = Instantiate<GameObject>(settings.playerAsset.clientAssetPrefab,spawnPos, GetInitialRotation());

        PrefabReplacement pr = clientEnt.GetComponentInChildren<PrefabReplacement>();
        if (pr)
        {
            if (settings.opponentAvatar != null)
            {
                pr.prefab = settings.opponentAvatar;
            }
            else
            {
                pr.prefab = settings.selfAvatar;
            }

            //pr.enabled = true;
            pr.DoSelfReplacing(true, settings.opponentDefaultMaterial);
        }

        ConfigTrackersInitialPosition(clientEnt);
        ConfigureClientPrefab (clientEnt);
    }

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
        if (settings == null)
            settings = FindObjectOfType<ThreePointTrackingSettings>();
        var clientEnt = Instantiate<GameObject>(settings.playerAsset.localClientAssetPrefab, spawnPos, GetInitialRotation());

        PrefabReplacement pr = clientEnt.GetComponentInChildren<PrefabReplacement>();
        if (pr)
        {
            pr.prefab = settings.selfAvatar;
            //pr.enabled = true;
            pr.DoSelfReplacing();
        }

        //At the time of OnStartClient, we couldn't know that this was a LocalPlayer.
        //We do now so we simply remove the old prefab and put in the new one with the e.g. camera.
        var oldParent = this.transform.parent;
        if (ThreePointTrackingSettings.GetInstance().VRMode)
        {
            AutoRootAndFeetVRTrackersRotationCorrection();
            ForceAdjustFeetTrackerHeight(clientEnt);
        }
        ConfigTrackersInitialPosition(clientEnt);
        ConfigureClientPrefab(clientEnt);
        Destroy(oldParent.gameObject);
        if (ThreePointTrackingSettings.GetInstance().VRMode)
            NullifyUncheckedTrackers(clientEnt);   //This is necessary to avoid unwanted motion due to the huge difference in initial frame for some trackers      
        ConfigStartOperations(clientEnt);
    }

    private Quaternion GetInitialRotation()
    {
        var scnCtr = GameObject.FindGameObjectWithTag("SceneCenter");
        Quaternion rot = Quaternion.identity;
        if (!scnCtr)
        {
            Debug.Log("Scene Center object doesn't exist!");
        }
        else
        {
            var aimVec = scnCtr.transform.position - spawnPos;
            aimVec.y = 0.0f;
            rot = Quaternion.LookRotation(aimVec, Vector3.up);
        }
        return rot;
    }

    private void ConfigureClientPrefab(GameObject clientEnt)
    {
        assetRoot = clientEnt;
        transform.SetParent(clientEnt.transform);
        clientEnt.transform.SetParent(FindObjectOfType<ConnectionClient>().transform);
        transform.localPosition = Vector3.zero;
        clientEnt.name = "Player";
    }

    void ConfigStartOperations(GameObject clientEnt)
    {
        ThreePointTrackingGameMgr gmInstance = ThreePointTrackingGameMgr.GetInstance();
        gmInstance.ShowStart(clientEnt);
    }

    //This is to configure trackers initial position to avoid unwanted avatar motion snap
    private void ConfigTrackersInitialPosition(GameObject clientEnt)
    {
        var update3ptData = clientEnt.GetComponentInChildren<UpdateAvatarWith3PTData>();
        var hc = clientEnt.GetComponentInChildren<tntHumanoidController>();
        var rootBase = hc.GetComponentInParent<tntBase>();
        if (hc && update3ptData)
        {
            var locators = update3ptData.transform.GetComponentsInChildren<ThreePointTrackingLocator>();
            if (locators != null)
            {
                foreach (var loc in locators)
                {
                    if (loc.locatorType == VRTrackerType.Head)
                    {
                        //hc.m_limbs.m_headTarget = loc.gameObject;
                        loc.transform.position = hc.m_limbs.m_neck.transform.position; //Make sure line up
                        loc.transform.rotation = hc.m_limbs.m_neck.transform.rotation; //Make sure line up
                    }
                    else if (loc.locatorType == VRTrackerType.LeftHand)
                    {
                        //hc.m_limbs.m_lHandTarget = loc.gameObject;
                        loc.transform.position = hc.m_limbs.m_lHand.transform.position; //Make sure line up
                        loc.transform.rotation = hc.m_limbs.m_lHand.transform.rotation; //Make sure line up
                    }
                    else if (loc.locatorType == VRTrackerType.RightHand)
                    {
                        //hc.m_limbs.m_rHandTarget = loc.gameObject;
                        loc.transform.position = hc.m_limbs.m_rHand.transform.position; //Make sure line up
                        loc.transform.rotation = hc.m_limbs.m_rHand.transform.rotation; //Make sure line up
                    }
                    else if (loc.locatorType == VRTrackerType.Root)
                    {
                        //hc.m_limbs.m_rootTarget = loc.gameObject;
                        loc.transform.position = rootBase.transform.position; //Make sure line up
                        loc.transform.rotation = rootBase.transform.rotation; //Make sure line up
                    }
                    else if (loc.locatorType == VRTrackerType.LeftFoot)
                    {
                        //hc.m_limbs.m_lFootTarget = loc.gameObject;
                        loc.transform.position = hc.m_limbs.m_lToes.transform.position; //Make sure line up
                        loc.transform.rotation = hc.m_limbs.m_lToes.transform.rotation; //Make sure line up
                    }
                    else if (loc.locatorType == VRTrackerType.RightFoot)
                    {
                        //hc.m_limbs.m_rFootTarget = loc.gameObject;
                        loc.transform.position = hc.m_limbs.m_rToes.transform.position; //Make sure line up
                        loc.transform.rotation = hc.m_limbs.m_rToes.transform.rotation; //Make sure line up
                    }
                }
            }
        }
    }

    //The height of the feet tracker transform locator will be
    //forcibly define here to ensure the avatar doesn't let its feet
    //floating in the air in some cases. 
    //This seems to be useful for irregular footshape
    private void ForceAdjustFeetTrackerHeight(GameObject clientEnt)
    {
        if (!ThreePointTrackingSettings.GetInstance().VRMode)
        {
            return;
        }

        var leftFootTransformLocator = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.LeftFoot);
        var rightFootTransformLocator = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.RightFoot);

        if (leftFootTransformLocator != null)
        {
            leftFootTransformLocator.position = new Vector3(leftFootTransformLocator.position.x, 0.0f, leftFootTransformLocator.position.z);
        }


        if (rightFootTransformLocator != null)
        {
            rightFootTransformLocator.position = new Vector3(rightFootTransformLocator.position.x, 0.0f, rightFootTransformLocator.position.z);
        }        
    }

    //Auto correct feet and root tracker orientation. When in 6 point tracking setup, waist need to be kept straight up and the feet need to aim forward as the face.
    private void AutoRootAndFeetVRTrackersRotationCorrection()
    {
        var localPlayerLogic = FindObjectOfType<LocalClientPlayerLogic>();
        if (localPlayerLogic)
        {
            var VRHeadSet = GameObject.Find("Main Camera (eye)").transform;

            var trackerLFootLocator = GameObject.Find("Tracker (left foot)/Transform Locator");
            var trackerRFootLocator = GameObject.Find("Tracker (right foot)/Transform Locator");
            var trackerRootLocator = GameObject.Find("Tracker (root)/Transform Locator");

            var forwardVec = VRHeadSet.forward; forwardVec.y = 0.0f; forwardVec.Normalize();
            var targetRot = Quaternion.LookRotation(forwardVec, Vector3.up);

            RotateToTargetAroundParentOrigin(trackerLFootLocator, targetRot);
            RotateToTargetAroundParentOrigin(trackerRFootLocator, targetRot);
            RotateToTargetAroundParentOrigin(trackerRootLocator, targetRot);
        }
    }

    private void NullifyUncheckedTrackers(GameObject clientEnt)
    {
        var hc = clientEnt.GetComponentInChildren<tntHumanoidController>();
        var trackOptions = ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions;
        if (!trackOptions.trackHead || !LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.Head))
        {
            hc.m_limbs.m_headTarget = null;
        }
        if (!trackOptions.trackLeftHand || !LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.LeftHand))
        {
            hc.m_limbs.m_lHandTarget = null;
        }
        if (!trackOptions.trackRightHand || !LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.RightHand))
        {
            hc.m_limbs.m_rHandTarget = null;
        }
        if (!trackOptions.trackRoot || !LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.Root))
        {
            hc.m_limbs.m_rootTarget = null;
        }
        if (!trackOptions.trackLeftFoot || !LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.LeftFoot))
        {
            hc.m_limbs.m_lFootTarget = null;
        }
        if (!trackOptions.trackRightFoot || !LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.RightFoot))
        {
            hc.m_limbs.m_rFootTarget = null;
        }
    }

    //Rotate a game object to a target rotation around its parent origin
    void RotateToTargetAroundParentOrigin(GameObject go, Quaternion targetRot)
    {
        if (go == null)
        {
            return;
        }
        var parentGo = go.transform.parent.gameObject;
        Quaternion relative = targetRot * Quaternion.Inverse(parentGo.transform.rotation);

        var rotationCorrector = new GameObject("Rotation Corrector");
        rotationCorrector.transform.SetParent(parentGo.transform);
        rotationCorrector.transform.localPosition = Vector3.zero;
        rotationCorrector.transform.localRotation = Quaternion.identity;
        rotationCorrector.transform.localScale = Vector3.one;
        go.transform.SetParent(rotationCorrector.transform);
        rotationCorrector.transform.rotation = relative * rotationCorrector.transform.rotation;

        /* Those two lines will delete the Rotation Corrector gameobject to make the structure cleaner, 
         * but will be harder to debug without this gameobject*/
        //go.transform.SetParent(parentGo.transform);
        //DestroyImmediate(rotationCorrector);
    }

    void OnDestroy()
	{
        Destroy(assetRoot);
	}
}
