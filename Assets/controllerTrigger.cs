using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerTrigger : MonoBehaviour {

    private boxCollision BoxCollision;
    public GameObject rootParent;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    public bool VRController; // If the user holding the controller is a VR/AR user. (1 controller each for the experiment)

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        BoxCollision = rootParent.GetComponent<boxCollision>();
    }

    void startApplication() {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu) && !BoxCollision.hasStarted()) {
            BoxCollision.startApp();
        }
    }

    void OnTriggerStay(Collider col) {
        //print("Triggered with obj:" + col.transform.name);
        if(col.tag == "box" && controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            BoxCollision.onSelect(col.gameObject);
        }
    }

    public void OnTriggerPress() {
        if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            if(VRController == false) {
                rootParent.GetComponent<syncTransformData>().CmdSyncARControllerPress(true, true);
            }

            if(rootParent.GetComponent<syncTransformData>().vivePressed == false && CONSTANTS.CALIBRATION_ENABLED) {
                rootParent.GetComponent<syncTransformData>().CmdSyncControllerPress();
            }
        }
    }

    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        startApplication();
        OnTriggerPress();
    }

}
