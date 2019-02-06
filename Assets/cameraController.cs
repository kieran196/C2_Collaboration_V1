using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class cameraController : NetworkBehaviour {

    public GameObject avatarHead;
    public Camera cam;
    public Camera camRenderPerspective;
    public Canvas canvas;

    // Update is called once per frame
    void Update() {
        if(isLocalPlayer && !cam.enabled && !canvas.GetComponent<Canvas>().enabled) {
            cam.enabled = true;
            canvas.GetComponent<Canvas>().enabled = true;
            print("Enabled camera for:" + this.name);
        }
        if(camRenderPerspective != null && isLocalPlayer && !camRenderPerspective.enabled) {
            camRenderPerspective.enabled = true;
        }
    }
}
