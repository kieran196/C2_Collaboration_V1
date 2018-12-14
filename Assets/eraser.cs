using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eraser : MonoBehaviour {

    internal SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    private void OnTriggerStay(Collider other) {
        controller = SteamVR_Controller.Input((int) trackedObj.index);
        if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
            if(other.gameObject.tag == "drawingMat") {
                Destroy(other.gameObject);
            }
        }
    }

}
