using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControllerHandlerLeft : ControllerHandler {

    public eventHandler events;

    private SteamVR_Controller.Device device;
    private SteamVR_TrackedObject trackedObj;

    // Use this for initialization
    void Start () {
        trackedObj = this.GetComponent<SteamVR_TrackedObject>();
	}

    private bool trackedObjValid() {
        return trackedObj.index.ToString() != "None";
    }

    // Update is called once per frame
    void Update () {
        if(trackedObjValid()) {
            device = SteamVR_Controller.Input((int)trackedObj.index);
            if(device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                events.triggerDownL.Invoke();
            }
            if(device.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
                events.triggerPressL.Invoke();
            }
        }
	}
}
