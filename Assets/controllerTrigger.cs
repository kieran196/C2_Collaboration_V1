using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerTrigger : MonoBehaviour {

    private boxCollision BoxCollision;
    public GameObject rootParent;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

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

    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        startApplication();
    }

}
