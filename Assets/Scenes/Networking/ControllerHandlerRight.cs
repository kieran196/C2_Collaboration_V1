using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ControllerHandlerRight : MonoBehaviour {

    public eventHandler events;


    private SteamVR_Controller.Device device;

    private SteamVR_TrackedObject trackedObj;
    public NetworkBehaviour network;

    // Use this for initialization
    void Start () {
        trackedObj = this.GetComponent<SteamVR_TrackedObject>();
	}

    private bool trackedObjValid() {
        return trackedObj.index.ToString() != "None";
    }
	
	// Update is called once per frame
	void Update () {
        //print(network.isLocalPlayer);
        if(!network.isLocalPlayer) {
            return;
        }
        if(trackedObj != null && trackedObjValid()) {
            device = SteamVR_Controller.Input((int)trackedObj.index);
            if(device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                events.triggerDownR.Invoke();
            }
            if(device.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
                events.triggerPressR.Invoke();
            }
        }
        if(Input.GetKey(KeyCode.Space)) {
            print("Pressing space down..");
            events.spaceDown.Invoke();
        }
        if(Input.GetKeyUp(KeyCode.Space)) {
            events.spaceUp.Invoke();
        }
    }
}
