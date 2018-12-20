using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class paintbrush : NetworkBehaviour {

    private SteamVR_Controller.Device device;
    private bool drawing = false;
    public GameObject texture;
    public Transform drawParent;
    private SteamVR_TrackedObject trackedObj;
    private int drawingCounter;
    public GameObject controllerR;
    private NetworkBehaviour root;

    public SteamVR_TrackedObject trackedObjR;
    private SteamVR_Controller.Device deviceR;
    public SteamVR_TrackedObject trackedObjL;
    private SteamVR_Controller.Device deviceL;

    private Transform currDeviceTransform;

    private Transform newParent;

    public void AssignParent() {
        drawingCounter++;
        GameObject parent = new GameObject();
        parent.name = "DrawingParent" + drawingCounter;
        parent.transform.SetParent(drawParent.transform);
        newParent = parent.transform;
        print("Pressing a trigger down..");
    }

    [Command]
    public void CmdFire() {
        if(isLocalPlayer) {
            print("Fired bullet");
            var bullet = (GameObject)Instantiate(
                texture,
                controllerR.transform.position,
                controllerR.transform.rotation);

            bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6f;
            NetworkServer.Spawn(bullet);
            Destroy(bullet, 2f);
        }
    }

    //[Command]
    public void CmdCreateTexture() {
        //print("Called CmdCreateTexture()");
        if(this.isActiveAndEnabled) {
            if (drawing == false) {
                AssignParent();
            }
            print("Drawing = true");
            var newTexture = (GameObject)Instantiate(texture, currDeviceTransform.position, new Quaternion(0f, 0f, 0f, 0f));
            newTexture.transform.eulerAngles = currDeviceTransform.eulerAngles;
            newTexture.transform.SetParent(newParent);
            newTexture.tag = "drawingMat";
            print("Called on server: "+isServer);
            NetworkServer.Spawn(newTexture);
            drawing = true;
        }
    }

    public void disableDrawing() {
        if(this.isActiveAndEnabled) {
            //print("Drawing = false");
            drawing = false;
        }
    }

    public void handleInput() {
        //if(isLocalPlayer) {
            //Testing
            if(Input.GetKey(KeyCode.Space)) {
            currDeviceTransform = controllerR.transform;
                CmdCreateTexture();
            } else if(Input.GetKeyUp(KeyCode.Space)) {
                disableDrawing();
            }

        //VR
        if(deviceR != null && deviceR.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
            currDeviceTransform = trackedObjR.transform;
            CmdCreateTexture();
        }
        if(deviceR != null && deviceR.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            disableDrawing();
        }
        if(deviceL != null && deviceL.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
            currDeviceTransform = trackedObjL.transform;
            CmdCreateTexture();
        }
        if(deviceL != null && deviceL.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            disableDrawing();
        }
    }

    private void Update() {
        if(trackedObjR != null && (int)trackedObjR.index != -1) {
            deviceR = SteamVR_Controller.Input((int)trackedObjR.index);
        }
        if(trackedObjL != null && (int)trackedObjL.index != -1) {
            deviceL = SteamVR_Controller.Input((int)trackedObjL.index);
        }

        handleInput();
        if (!drawParent.gameObject.activeInHierarchy) {
            drawParent.SetParent(this.transform); //Set it to SteamVR / VRSimulator Parent
        }
    }

}
