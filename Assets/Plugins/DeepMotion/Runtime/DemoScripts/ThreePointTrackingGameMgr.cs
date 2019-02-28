using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.Networking;

/**
 * This is a manager for 3 point tracking demo
 * This manager does couple things:
 * 1. tracks the input from the vive controllers
 * 2. automatically remapping VR device
 * 3  Auto rescaling avatar to match you when you are in VR mode
 */
public class ThreePointTrackingGameMgr : MonoBehaviour
{
    public static ThreePointTrackingGameMgr instance;
    [System.NonSerialized]
    public ThreePointTrackingSettings settings;
    [System.NonSerialized]
    public bool canRestart = true;
    // left and right controllers
    public SteamVR_TrackedController trackedController1;
    public SteamVR_TrackedController trackedController2;
    private Transform leftController;
    private Transform rightController;
    private SteamVR_PlayArea playArea;
    private GameObject controllerModel1;
    private GameObject controllerModel2;
    private Vector3 vecUtilBuf = Vector3.zero;
    [System.NonSerialized]
    public List<Vector3> chaperonePoints;
    private int clickCount;
    private int oldClickCount;
    private Transform hmd;
    [System.NonSerialized]
    public float rescaleHeight = -1.0f; //HMD height
    [System.NonSerialized]
    public float rescaleWidth = -1.0f; //Controller distance
    private Camera vrCam;
    [HideInInspector]
    public Camera deadCam;
    [HideInInspector]
    public tntHumanoidController humanoidController;
    private bool sceneViewMode = false;

    private float padPressDuration = 0.0f;

    public static ThreePointTrackingGameMgr GetInstance()
    {
        return instance;
    }

    public enum SwitchType
    {
        Toggle,
        ToHMD,
        ToSecondCamera,
    }

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        //Debug.Log("LAN IP address is: " + Network.player.ipAddress);
        settings = FindObjectOfType<ThreePointTrackingSettings>();
        playArea = FindObjectOfType<SteamVR_PlayArea>();
        if (trackedController1 != null && trackedController2 != null)
        {
            trackedController1.Gripped += OnGripped;
            trackedController1.Ungripped += OnUngripped;
            trackedController2.Gripped += OnGripped;
            trackedController2.Ungripped += OnUngripped;
            trackedController1.TriggerClicked += OnTriggerClicked;
            trackedController1.TriggerUnclicked += OnTriggerUnclicked;
            trackedController2.TriggerClicked += OnTriggerClicked;
            trackedController2.TriggerUnclicked += OnTriggerUnclicked;
            trackedController1.PadClicked += OnPadClicked;
            trackedController2.PadClicked += OnPadClicked;
            trackedController1.PadUnclicked += OnPadUnClicked;
            trackedController2.PadUnclicked += OnPadUnClicked;
            trackedController1.MenuButtonClicked += OnMenuButtonClicked;
            trackedController2.MenuButtonClicked += OnMenuButtonClicked;

            controllerModel1 = trackedController1.GetComponentInChildren<SteamVR_RenderModel>().gameObject;
            controllerModel2 = trackedController2.GetComponentInChildren<SteamVR_RenderModel>().gameObject;
        }

        SetControllerModelsActive(true);

        clickCount = 0;
        oldClickCount = 0;

        chaperonePoints = new List<Vector3>(new Vector3[4]);

        chaperonePoints = new List<Vector3>(new Vector3[4]);

        if (settings.VRMode)
        {
            GameObject go = GameObject.Find("Main Camera (eye)");
            if (go != null)
            { hmd = go.transform; }
        }
        else
        {
            hmd = FindObjectOfType<SteamVR_Camera>().transform;
        }
			
        if (hmd != null)       
            vrCam = hmd.GetComponent<Camera>();        
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedController1.padPressed || trackedController2.padPressed)
        {
            padPressDuration += Time.unscaledDeltaTime;
        }

        if ((clickCount == 2 && oldClickCount != 2)
#if UNITY_EDITOR
            || Input.GetKeyDown(KeyCode.R)
#endif
            )
        {
            if (settings.VRMode)
            {
                AutoVRDeviceIndexRemapping();
            }

            var connectionClient = FindObjectOfType<ConnectionClient>();

            if (canRestart && (ClientScene.readyConnection == null || !ClientScene.readyConnection.isConnected))
            {
                SetControllerModelsActive(false);
                connectionClient.Connect();
            }
            else if (!canRestart && ClientScene.readyConnection != null && ClientScene.readyConnection.isConnected)
            {
                SetControllerModelsActive(true);
                connectionClient.Disconnect();
            }
        }
        oldClickCount = clickCount;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (settings.VRMode) { SwitchCamera(); }
        }
#endif

        if (sceneViewMode)
        {
            if (vrCam && deadCam)
            {
                vrCam.transform.localPosition = deadCam.transform.localPosition;
                vrCam.transform.localRotation = deadCam.transform.localRotation;
            }
        }
    }

    //When player is in T pose, this function will automatically match each tracker to the correct scene gameobject(must be steamVRtrackedObject) by changing the vr device index.
    private void AutoVRDeviceIndexRemapping()
    {
        var steamVRCtrlManager = FindObjectOfType<SteamVR_ControllerManager>();
        var steamVRCam = FindObjectOfType<SteamVR_Camera>();

        //Setup all possible trackers
        GameObject leftHandController = null;
        GameObject rightHandController = null;
        GameObject rootTracker = null;
        GameObject leftFootTracker = null;
        GameObject rightFootTracker = null;

        leftHandController = steamVRCtrlManager.left.activeSelf ? steamVRCtrlManager.left : null;
        rightHandController = steamVRCtrlManager.right.activeSelf ? steamVRCtrlManager.right : null;
        var rootFeetTrackers = new List<GameObject>();

        foreach (var go in steamVRCtrlManager.objects)
        {
            if (go.gameObject.name == "Tracker (root)")
            {
                rootTracker = go;
                rootFeetTrackers.Add(go);
            }
            else if (go.gameObject.name == "Tracker (left foot)")
            {
                leftFootTracker = go;
                rootFeetTrackers.Add(go);
            }
            else if (go.gameObject.name == "Tracker (right foot)")
            {
                rightFootTracker = go;
                rootFeetTrackers.Add(go);
            }
        }

        //If both vr hand controllers are active(usually means turned on)
        if (leftHandController && rightHandController && leftHandController.activeInHierarchy && rightHandController.activeInHierarchy)
        {
            if (Vector3.Dot(leftHandController.transform.position - rightHandController.transform.position, steamVRCam.transform.right) < 0)
            {
                //Do necessary swaps
                SwapTrackedObjectIndex(leftHandController.GetComponent<SteamVR_TrackedObject>(), rightHandController.GetComponent<SteamVR_TrackedObject>());
                SwapTrackedControllerIndex();
                SwapRobotFistControllerIndex();
            }
        }

        //Setup other trackers
        if (rootFeetTrackers.Count > 0 && rootFeetTrackers.Count <= 3)
        {
            //Prepare variables
            var rootDeviceIndex = SteamVR_TrackedObject.EIndex.None;
            var feetDeviceIndexList = new List<KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>>();
            foreach (var go in rootFeetTrackers)
            {
                var deviceIndex = go.GetComponent<SteamVR_TrackedObject>().index;
                if (go.transform.position.y < steamVRCam.transform.position.y * 0.2f && feetDeviceIndexList.Count < 2)
                {
                    feetDeviceIndexList.Add(new KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>(deviceIndex, go.transform.position));
                }
                else
                {
                    if (rootDeviceIndex == SteamVR_TrackedObject.EIndex.None)
                    {
                        rootDeviceIndex = deviceIndex;
                    }
                    else
                    {
                        if (feetDeviceIndexList.Count < 2)
                        {
                            feetDeviceIndexList.Add(new KeyValuePair<SteamVR_TrackedObject.EIndex, Vector3>(deviceIndex, go.transform.position));
                        }
                    }
                }
            }

            //Root
            if (rootDeviceIndex != SteamVR_TrackedObject.EIndex.None)
            {
                if (rootTracker != null)
                {
                    if (!rootTracker.activeSelf)
                    {
                        rootTracker.SetActive(true);
                    }
                    rootTracker.GetComponent<SteamVR_TrackedObject>().index = rootDeviceIndex;
                }
            }

            //Feet
            if (feetDeviceIndexList.Count == 0)
            {
                if (leftFootTracker != null)
                {
                    leftFootTracker.SetActive(false);
                }
                if (rightFootTracker != null)
                {
                    rightFootTracker.SetActive(false);
                }
            }
            else if (feetDeviceIndexList.Count == 1)
            {
                if (Vector3.Dot((feetDeviceIndexList[0].Value - steamVRCam.transform.position), steamVRCam.transform.right) > 0)
                {
                    leftFootTracker.SetActive(true);
                    leftFootTracker.GetComponent<SteamVR_TrackedObject>().index = feetDeviceIndexList[0].Key;
                    rightFootTracker.SetActive(false);
                }
                else
                {
                    rightFootTracker.SetActive(true);
                    rightFootTracker.GetComponent<SteamVR_TrackedObject>().index = feetDeviceIndexList[0].Key;
                    leftFootTracker.SetActive(false);
                }
            }
            else if (feetDeviceIndexList.Count == 2)
            {
                if (leftFootTracker != null || !leftFootTracker.activeSelf)
                {
                    leftFootTracker.SetActive(true);
                }
                if (rightFootTracker != null || !rightFootTracker.activeSelf)
                {
                    rightFootTracker.SetActive(true);
                }

                bool flag = Vector3.Dot((feetDeviceIndexList[0].Value - feetDeviceIndexList[1].Value), steamVRCam.transform.right) > 0;

                leftFootTracker.GetComponent<SteamVR_TrackedObject>().index = feetDeviceIndexList[flag ? 0 : 1].Key;
                rightFootTracker.GetComponent<SteamVR_TrackedObject>().index = feetDeviceIndexList[flag ? 1 : 0].Key;
            }
        }
    }

    void UpdateTrackerVisibility()
    {
        if (ThreePointTrackingSettings.GetInstance().displayTrackers)
        {
            Transform trackerTransform = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.LeftHand);
            if (leftController &&
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackLeftHand &&
                leftController.transform.Find("Visualizer") &&
                trackerTransform.Find("Visualizer"))
            {
                trackerTransform.Find("Visualizer").gameObject.SetActive(true);
                leftController.transform.Find("Visualizer").gameObject.SetActive(true);
            }
            trackerTransform = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.RightHand);
            if (rightController &&
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRightHand &&
                rightController.transform.Find("Visualizer") &&
                trackerTransform.Find("Visualizer"))
            {
                trackerTransform.Find("Visualizer").gameObject.SetActive(true);
                rightController.transform.Find("Visualizer").gameObject.SetActive(true);
            }
            trackerTransform = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.Head);
            if (hmd &&
                ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackHead &&
                hmd.transform.Find("Visualizer") &&
                trackerTransform.Find("Visualizer"))
            {
                trackerTransform.Find("Visualizer").gameObject.SetActive(true);
                hmd.transform.Find("Visualizer").gameObject.SetActive(true);
            }
            foreach (GameObject go in FindObjectOfType<SteamVR_ControllerManager>().objects)
            {
                trackerTransform = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.Root);
                if (go.name == "Tracker (root)" &&
                    ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRoot &&
                    go.transform.Find("Visualizer") &&
                    trackerTransform.Find("Visualizer"))
                {
                    trackerTransform.Find("Visualizer").gameObject.SetActive(true);
                    go.transform.Find("Visualizer").gameObject.SetActive(true);
                }
                trackerTransform = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.LeftFoot);
                if (go.name == "Tracker (left foot)" &&
                    ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackLeftFoot &&
                    go.transform.Find("Visualizer") &&
                    trackerTransform.Find("Visualizer"))
                {
                    trackerTransform.Find("Visualizer").gameObject.SetActive(true);
                    go.transform.Find("Visualizer").gameObject.SetActive(true);
                }
                trackerTransform = LocalClientPlayerLogic.GetTrackedVRObjectTransformLocator(VRTrackerType.RightFoot);
                if (go.name == "Tracker (right foot)" &&
                    ThreePointTrackingSettings.GetInstance().sixPointTrackingOptions.trackRightFoot &&
                    go.transform.Find("Visualizer") &&
                    trackerTransform.Find("Visualizer"))
                {
                    trackerTransform.Find("Visualizer").gameObject.SetActive(true);
                    go.transform.Find("Visualizer").gameObject.SetActive(true);
                }
            }
        }
    }

    public void SwitchCamera(SwitchType type = SwitchType.Toggle)
    {
        Debug.Log("Switch Camera");
        if (vrCam && deadCam)
        {
            if (type == SwitchType.Toggle) // Toggle switch
            {
                vrCam.enabled = !vrCam.enabled;
                deadCam.enabled = !vrCam.enabled;
            }
            else if (type == SwitchType.ToSecondCamera && !sceneViewMode) // Ensure switch to scene cam
            {
                vrCam.enabled = false;
                deadCam.enabled = true;
            }
            else if (type == SwitchType.ToHMD && sceneViewMode) // Ensure switch back to vr cam
            {
                vrCam.enabled = true;
                deadCam.enabled = false;
            }

            if (vrCam.enabled)
            {
                sceneViewMode = false;
            }
            else // Each time switch to scene view, auto adjust camera aimming to the scene center from scene cam locator
            {
                sceneViewMode = true;

                Vector3 deadCamLocalPos = vrCam.transform.Find("Dead Camera Transform").localPosition;
                Vector3 deadCamPos = vrCam.transform.Find("Dead Camera Transform").position;
                deadCamPos.y = vrCam.transform.position.y;
                deadCamPos = vrCam.transform.position + (vrCam.transform.position - deadCamPos).normalized * deadCamLocalPos.z;

                deadCam.transform.parent.position = deadCamPos;

                Vector3 target = vrCam.transform.position;

                Vector3 deadCamToTgtVec_world = target - deadCam.transform.parent.position;
                Vector3 deadCamToTgtVecXZ_world = new Vector3(deadCamToTgtVec_world.x, 0.0f, deadCamToTgtVec_world.z);

                Vector3 VRCamforwardVec_local = vrCam.transform.parent.InverseTransformVector(vrCam.transform.forward);

                Vector3 deadCamforwardVec_world = deadCam.transform.parent.TransformVector(VRCamforwardVec_local);         
                Vector3 deadCamforwardVecXZ_world = new Vector3(deadCamforwardVec_world.x, 0.0f, deadCamforwardVec_world.z);

                Quaternion rotQuat = Quaternion.FromToRotation(deadCamforwardVecXZ_world, deadCamToTgtVecXZ_world);

                deadCam.transform.parent.rotation = rotQuat * deadCam.transform.parent.rotation;

                Vector3 correctionVec = -deadCam.transform.parent.TransformVector(UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head));

                deadCam.transform.parent.position += correctionVec;
            }
        }
    }

    public void ShowStart(GameObject clientEnt)
    {
        deadCam = GetDeadCam(clientEnt);
        humanoidController = clientEnt.GetComponentInChildren<tntHumanoidController>();

        if (settings || settings.VRMode)
        {
            //Make sure the vr camera is put on the same position(XZ only) with the character head and aim to the scene center(Y axis aim only)
            Transform hcHead = humanoidController.m_limbs.m_neck.transform;
            Vector3 hmdFwdXZ = hmd.forward; hmdFwdXZ.y = 0.0f;
            Vector3 hcHeadFwdXZ = hcHead.forward; hcHeadFwdXZ.y = 0.0f;
            Quaternion rot = Quaternion.FromToRotation(hmdFwdXZ, hcHeadFwdXZ);
            playArea.transform.rotation = rot * playArea.transform.rotation; //Adjust rotation

            Vector3 diff = hcHead.position - hmd.position;
            Vector3 diffXZ = new Vector3(diff.x, 0.0f, diff.z);
            playArea.transform.position += diffXZ; //Match VR HMD position to character head

            //Always start at VR camera
            sceneViewMode = false;
            if (vrCam)
                vrCam.enabled = true;
            if (deadCam)
                deadCam.enabled = false;
        }

        //scale the model according to hmd position
        if (settings && settings.autoRescale && settings.VRMode)
        {
            Rescale();
        }

        GetChaperonePoints();
        UpdateTrackerVisibility();

        SetupAPEVisTool(clientEnt);
    }

    //This should be called only under vr mode
    public void Rescale()
    {
        if (leftController == null && rightController == null)
        {
            //Vive trackers
            GameObject go = GameObject.Find("Controller (left)");
            if (go != null)
                leftController = go.transform;
            go = GameObject.Find("Controller (right)");
            if (go != null)
                rightController = go.transform;
        }

        rescaleHeight = hmd.position.y; //Record
        if (leftController != null && rightController != null)
        {
            rescaleWidth = (leftController.position - rightController.position).magnitude; //Record
        }
        else
        {
            ThreePointTrackingSettings.GetInstance().rescaleWidth = false;
        }

        //Reconstruct
        tntBase root = humanoidController.gameObject.GetComponentInParent<tntBase>();
        root.m_world.RemoveArticulationBase(root);
        tntEntityAndJointFactory.DestroyArticulationBase(root);

        float heightScaleFactor = 1.0f;
        float widthScaleFactor = 1.0f;

        GetRescaleFactors(humanoidController, rescaleHeight, rescaleWidth, ref heightScaleFactor, ref widthScaleFactor);

        tntLink lShoulder = null; tntLink rShoulder = null; tntLink lowerback_torse = null;
        GameObject avatar = humanoidController.gameObject.transform.parent.parent.gameObject;
        APEModelScalerUtils.ConfigCheckAPEObject(avatar, out lShoulder, out rShoulder, out lowerback_torse);
        APEModelScalerUtils.ScaleGeometry(avatar, heightScaleFactor, widthScaleFactor, lShoulder, rShoulder, lowerback_torse);
        float geometryScalar = APEModelScalerUtils.getUnformScalar(widthScaleFactor, heightScaleFactor);
        APEModelScalerUtils.ScaleMass(avatar, APEModelScalerUtils.GetMassScalar(geometryScalar));
        APEModelScalerUtils.ScaleMOI(avatar, APEModelScalerUtils.GetMOIScalar(geometryScalar));
        APEModelScalerUtils.ScaleStrengths(avatar, geometryScalar);
 
        tntEntityAndJointFactory.CreateArticulationBase(root);
        root.m_world.AddArticulationBase(root);

        var playerTemplatePrefab = GetComponentInChildren<LocalClientPlayerLogic>();
        NetworkPlayer3PTDataSync dataSync = null;
        if (playerTemplatePrefab)
        {
            var playerNetworkEntity = playerTemplatePrefab.GetComponentInChildren<NetworkIdentity>();
            if (playerNetworkEntity)
            {
                if (playerNetworkEntity.gameObject.tag == "PlayerEntity")
                {
                    dataSync = playerNetworkEntity.GetComponent<NetworkPlayer3PTDataSync>();
                }
            }
        }
        if (dataSync)
        {
            dataSync.CmdSetRescaleData(rescaleHeight, rescaleWidth);
        }
    }

    public static void GetRescaleFactors(tntHumanoidController hc, float rescaleHeight, float rescaleWidth, ref float _heightScaleFactor, ref float _widthScaleFactor)
    {
        if (rescaleHeight < 0.0f)
            rescaleHeight = 0.0f;
        if (rescaleWidth < 0.0f)
            rescaleWidth = 0.0f;

        float w = 0.0f, h = 0.0f;

        //rescale width
        if (ThreePointTrackingSettings.GetInstance().rescaleWidth)
        {
            var ratio = rescaleWidth / (rescaleHeight + 0.1f);
            if (ThreePointTrackingSettings.GetInstance().rescaleWidthAutoCorrect)
            {
                if (ratio > 0.5f && ratio < 2.0f)
                {
                    w = rescaleWidth;
                }
                else //If hand distance is way off
                {
                    w = rescaleHeight + 0.08f; //Give it the human height value as a rough guess
                }
            }
            else
            {
                w = rescaleWidth;
            }
            //compensate for distance between controller tracker and hand
            Vector3 lpivotA = ((tntChildLink)hc.m_limbs.m_lHand).PivotAToWorld();
            float leftPalmDist = Vector3.Distance(lpivotA, hc.m_limbs.m_lHand.transform.position);
            Vector3 rpivotA = ((tntChildLink)hc.m_limbs.m_rHand).PivotAToWorld();
            float rightPalmDist = Vector3.Distance(rpivotA, hc.m_limbs.m_rHand.transform.position);
            _widthScaleFactor = w / (Vector3.Distance(lpivotA, rpivotA) + 2 * leftPalmDist + 2 * rightPalmDist);
        }

        //rescale height
        if (rescaleHeight > 1.0f) //Limit the height to be greater than 1 meter
        {
            h = rescaleHeight;
        }
        else //If player height is way too short
        {
            h = 1.0f;

            if (ThreePointTrackingSettings.GetInstance().rescaleWidth && ThreePointTrackingSettings.GetInstance().rescaleWidthAutoCorrect)
            {
                _widthScaleFactor = h / (hc.m_limbs.m_lHand.transform.position - hc.m_limbs.m_rHand.transform.position).magnitude; //use height as width to calculate here
            }                
        }
        float headDist = hc.m_limbs.m_neck.transform.position.y - h;
        _heightScaleFactor = (hc.m_limbs.m_neck.transform.position.y - headDist) / hc.m_limbs.m_neck.transform.position.y;
    }

    private Camera GetDeadCam(GameObject clientEnt)
    {
        Camera[] cams = clientEnt.transform.GetComponentsInChildren<Camera>();
        if (cams != null)
        {
            foreach (var cam in cams)
            {
                if (cam.gameObject.name == "Main Camera (player dead)")
                {
                    return cam;
                }
            }
        }
        return null;
    }

    private void SwapTrackedObjectIndex(SteamVR_TrackedObject left, SteamVR_TrackedObject right)
    {
        var temp = right.index;
        right.index = left.index;
        left.index = temp;
    }

    private void SwapTrackedControllerIndex()
    {
        var temp = trackedController2.controllerIndex;
        trackedController2.controllerIndex = trackedController1.controllerIndex;
        trackedController1.controllerIndex = temp;
    }

    private void SwapRobotFistControllerIndex()
    {
        var robotFistController1 = trackedController1.GetComponent<MTG_SteamVR_RobotFist_Controller>();
        var robotFistController2 = trackedController2.GetComponent<MTG_SteamVR_RobotFist_Controller>();
        if (robotFistController1 && robotFistController2)
        {
            var temp = robotFistController2.index;
            robotFistController2.index = robotFistController1.index;
            robotFistController1.index = temp;
        }
    }

    private void OnTriggerClicked(object sender, ClickedEventArgs e)
    {
    }

    private void OnTriggerUnclicked(object sender, ClickedEventArgs e)
    {
    }

    private void OnGripped(object sender, ClickedEventArgs e)
    {
//        clickCount++;
    }

    private void OnUngripped(object sender, ClickedEventArgs e)
    {
//        clickCount--;
    }

    private void OnMenuButtonClicked(object sender, ClickedEventArgs e)
    {
#if UNITY_EDITOR
        if (settings.VRMode)
        {
            SwitchCamera();
        }
#endif
    }

    private void OnPadClicked(object sender, ClickedEventArgs e)
    {
        clickCount++;
    }

    private void OnPadUnClicked(object sender, ClickedEventArgs e)
    {
        clickCount--;
    }

    public static void ScaleGeometryForSingleObject(GameObject obj, float theGeometryScalar)
    {
        Component[] links;

        links = obj.GetComponentsInChildren<tntLink>();

        foreach (tntLink link in links)
        {
            link.transform.localScale *= theGeometryScalar;
            link.transform.localPosition *= theGeometryScalar;
        }

        foreach (tntLink link in links)
        {
            if (link as tntBallLink)
            {
                tntBallLink ball = link as tntBallLink;
                ball.PivotA *= theGeometryScalar;
                ball.AutoFillPivotB();
            }
            else if (link as tntHingeLink)
            {
                tntHingeLink hinge = link as tntHingeLink;
                hinge.PivotA *= theGeometryScalar;
                hinge.AutoFillPivotB();
            }
            else if (link as tntUniversalLink)
            {
                tntUniversalLink universal = link as tntUniversalLink;
                universal.PivotA *= theGeometryScalar;
                universal.AutoFillPivotB();
            }
        }
    }

    public IEnumerator DelayResetRestartFlag(float duration)
    {
        if (canRestart) //Only reset flag when it was previously false        
            yield break;
        
        yield return new WaitForSeconds(duration);
        canRestart = true;
    }

    private void SetControllerModelsActive(bool active)
    {
        if (controllerModel1 != null && controllerModel2 != null)
        {
            controllerModel1.SetActive(active);
            controllerModel2.SetActive(active);
        }
    }

    private void GetChaperonePoints()
    {
        var rect = new Valve.VR.HmdQuad_t();
        if (playArea != null && SteamVR_PlayArea.GetBounds(playArea.size, ref rect))
        {
            vecUtilBuf.Set(rect.vCorners0.v0, rect.vCorners0.v1, rect.vCorners0.v2);
            chaperonePoints[0] = playArea.transform.TransformPoint(vecUtilBuf);

            vecUtilBuf.Set(rect.vCorners1.v0, rect.vCorners1.v1, rect.vCorners1.v2);
            chaperonePoints[1] = playArea.transform.TransformPoint(vecUtilBuf);

            vecUtilBuf.Set(rect.vCorners2.v0, rect.vCorners2.v1, rect.vCorners2.v2);
            chaperonePoints[2] = playArea.transform.TransformPoint(vecUtilBuf);

            vecUtilBuf.Set(rect.vCorners3.v0, rect.vCorners3.v1, rect.vCorners3.v2);
            chaperonePoints[3] = playArea.transform.TransformPoint(vecUtilBuf);
        }
    }

    //Do setup logic if APEVisualization tool present
    private void SetupAPEVisTool(GameObject clientEnt)
    {
        var configAPEVis = GetComponentInChildren<ConfigAPEVisualization>();
        if (configAPEVis && !APEVisualization.Instance)
        {
            var hc = clientEnt.GetComponentInChildren<tntHumanoidController>();
            if (hc)
            {
                var APEvis = Instantiate<APEVisualization>(configAPEVis.prefab);
                APEvis.avatar = hc.transform.parent.parent.gameObject;
                APEvis.transform.SetParent(transform, true);

                //if non-vr mode
                if (!settings.VRMode)
                {
                    //hide VR origin and vr camera
                    var VRArea = GameObject.Find("Main Camera (eye)").transform.GetComponentInParent<SteamVR_PlayArea>();
                    if (VRArea != null)
                    {
                        VRArea.gameObject.SetActive(false);
                    }

                    //create new camera
                    var camGo = new GameObject("Camera");
                    camGo.transform.position = hc.transform.position + (hc.transform.forward + hc.transform.right).normalized * 2.5f;
                    camGo.transform.LookAt(hc.transform);
                    APEvis.canvasCam = camGo.AddComponent<Camera>();
                }
            }
        }
    }
}
