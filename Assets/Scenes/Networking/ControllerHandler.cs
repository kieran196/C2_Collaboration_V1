using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ControllerHandler : NetworkBehaviour {

    public SteamVR_TrackedObject trackedObjR;
    private SteamVR_Controller.Device deviceR;
    public SteamVR_TrackedObject trackedObjL;
    private SteamVR_Controller.Device deviceL;

    public bool triggerDown() {
        return deviceR.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) || deviceL.GetPressDown(SteamVR_Controller.ButtonMask.Trigger);
    }

    public bool OnSpaceDown() {
        return Input.GetKey(KeyCode.Space);
    }


    public bool OnSpaceUp() {
        return Input.GetKeyUp(KeyCode.Space);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
