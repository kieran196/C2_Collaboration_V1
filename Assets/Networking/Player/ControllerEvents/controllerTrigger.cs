using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerTrigger : MonoBehaviour {

    private blockSelectionTask blockSelectionScript;
    public GameObject rootParent;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    public bool VRController; // If the user holding the controller is a VR/AR user. (1 controller each for the experiment)

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        blockSelectionScript = rootParent.GetComponent<blockSelectionTask>();
    }

    void startApplication() {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu) && !blockSelectionScript.hasStarted()) {
            blockSelectionScript.startApp();
        }
    }

    void OnTriggerStay(Collider col) {
        //print("Triggered with obj:" + col.transform.name);
        if(col.tag == "box" && controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            blockSelectionScript.onSelect(col.gameObject);
        }
    }


    public void handlePaintEvents() {
        if(controller != null && rootParent.GetComponent<paintbrush>().enabled) {
            if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
                rootParent.GetComponent<paintbrush>().CmdCreateTexture(this.transform.position, this.transform.eulerAngles);
            }
            if(controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
                rootParent.GetComponent<paintbrush>().disableDrawing();
            }
        }
    }

    public void handleSplashEvents() {
        if(controller != null && rootParent.GetComponent<splashTool>().enabled) {
            if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
                rootParent.GetComponent<splashTool>().CmdCreateTexture(this.transform.position, this.transform.eulerAngles);
            }
            if(controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
                rootParent.GetComponent<splashTool>().disableDrawing();
            }
        }
    }

    public void handlePencilEvents() {
        if(controller != null && rootParent.GetComponent<pencil>().enabled) {
            if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
                rootParent.GetComponent<pencil>().CmdCreateTexture(this.transform.position, this.transform.eulerAngles);
            }
            if(controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
                rootParent.GetComponent<pencil>().disableDrawing();
            }
        }
    }

    public void handleDrawingEvents() {
        handlePaintEvents();
        handleSplashEvents();
        handlePencilEvents();
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
        handleDrawingEvents();
        startApplication();
        OnTriggerPress();
    }

}
