using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class calibrationManager : MonoBehaviour {

    public GameObject rootParent;
    private GameObject ghostController;
    private SteamVR_TrackedObject leftController;
    private SteamVR_TrackedObject rightController;
    private GameObject head;
    private bool offSetAssigned = false;
    public bool autoCalibrate; // Calibrate based on manually inputted values
    public Vector3 offset;
    public bool assignValue = false;
    private GameObject table;

    private void Awake() {
        if (!autoCalibrate) {
            offset = Vector3.zero;
        }
    }

    public void assignOffset(SteamVR_TrackedObject controller) {
        float x = -controller.transform.position.x - ghostController.transform.position.x;
        float y = controller.transform.position.y - ghostController.transform.position.y;
        float z = -controller.transform.position.z - ghostController.transform.position.z;
        //print("Assigning Values:" + x + ", " + y + ", " + z);
        offset = new Vector3(x, y, z);
        table.transform.position = new Vector3(-table.transform.position.x - x, -1, -table.transform.position.z - z);
        table.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        offSetAssigned = true;
        //Change camera settings
        GameObject holoPlayer = findLocalPlayer();
        holoPlayer.GetComponent<cameraRigs>().AR_Cam.GetComponent<Camera>().nearClipPlane = 0.85f;
        ghostController.SetActive(false); //Hide ghost controller
    }

    public void assignControllers() {
        leftController = (leftController == null) ? GetComponent<SteamVR_ControllerManager>().left.GetComponent<SteamVR_TrackedObject>() : null;
        rightController = (rightController == null) ? GetComponent<SteamVR_ControllerManager>().right.GetComponent<SteamVR_TrackedObject>() : null;
        head = (head == null) ? transform.parent.GetComponent<cameraController>().cam.gameObject : null;
    }

    private void Start() {
        ghostController = GameObject.FindGameObjectWithTag("ghostController");
        table = GameObject.FindGameObjectWithTag("table");
        //Remove later just for testing purposes
        if (offset != Vector3.zero && autoCalibrate) {
            table.transform.position = new Vector3(-table.transform.position.x - offset.x, -1f, -table.transform.position.z - offset.z);
            table.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        }
    }

    private GameObject findLocalPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (player.GetComponent<NetworkBehaviour>().isLocalPlayer) {
                return player;
            }
        }
        return null;
    }

    void Update() {
        //Getting the child vive controllers
        if (leftController == null || rightController == null) {
            assignControllers();
        }
        if(assignValue == true && offSetAssigned == false) {
            assignOffset(rightController); // Just using right controller as default..
            assignValue = false;
        }
        if(offset != Vector3.zero) {
            //print(offset);
            //Transform rightModel = rightController.transform.Find("vr_controller_vive_1_5");
            //Inversing X-Axis..
            if(rightController != null) {
                Vector3 rightOriginPos = rootParent.GetComponent<syncTransformData>().positionR;
                Vector3 rightOriginRot = rootParent.GetComponent<syncTransformData>().rotationR;
                rightController.transform.position = new Vector3(-rightOriginPos.x - offset.x, rightOriginPos.y - offset.y, -rightOriginPos.z - offset.z);
                rightController.transform.localEulerAngles = new Vector3(-rightOriginRot.x, rightOriginRot.y, -rightOriginRot.z);
            } if (leftController != null) {
                Vector3 leftOriginPos = rootParent.GetComponent<syncTransformData>().positionL;
                Vector3 leftOriginRot = rootParent.GetComponent<syncTransformData>().rotationL;
                leftController.transform.position = new Vector3(-leftOriginPos.x - offset.x, leftOriginPos.y - offset.y, -leftOriginPos.z - offset.z);
                leftController.transform.localEulerAngles = new Vector3(-leftOriginRot.x, leftOriginRot.y, -leftOriginRot.z);
            } if (head != null) {
                Vector3 headOriginPos = rootParent.GetComponent<syncTransformData>().positionH;
                Vector3 headOriginRot = rootParent.GetComponent<syncTransformData>().rotationH;
                head.transform.position = new Vector3(-headOriginPos.x - offset.x, headOriginPos.y - offset.y, -headOriginPos.z - offset.z);
                head.transform.localEulerAngles = new Vector3(-headOriginRot.x, headOriginRot.y, -headOriginRot.z);
            }
        }
    }
}
