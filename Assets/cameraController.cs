using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class cameraController : NetworkBehaviour {

    public Camera cam;
	
	// Update is called once per frame
	void Update () {
        if(isLocalPlayer && !cam.enabled) {
            cam.enabled = true;
            print("Enabled camera for:" + this.name);
        }
    }
}
