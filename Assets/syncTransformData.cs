using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This class provides the Hololens with the positions and rotations of the controllers and Vive HMD.
/// The original Vectors are being read from the Hololens per frame and is used to calibrate the Vive coordinate system inside the Hololens.
/// </summary>

public class syncTransformData : NetworkBehaviour {

    //Enable this if project is running the Vive
    public bool isVive = false;

    private bool inputConfirmed = false;
    public bool vivePressed;

    //The positions and rotations of the right/left controller and head
    [SyncVar]
    public Vector3 positionR;

    [SyncVar]
    public Vector3 rotationR;

    [SyncVar]
    public Vector3 positionL;

    [SyncVar]
    public Vector3 rotationL;

    [SyncVar]
    public Vector3 positionH;

    [SyncVar]
    public Vector3 rotationH;

    [SyncVar]
    public bool viveARPressed;

    [ClientRpc]
    public void RpcSyncControllerPress() {
        vivePressed = true;
    }

    [Command]
    public void CmdSyncControllerPress() {
        RpcSyncControllerPress();
    }

    [ClientRpc]
    public void RpcSyncARControllerPress(bool arController, bool enable) {
        if(arController == true) {
            viveARPressed = enable;
        }
    }

    [Command]
    public void CmdSyncARControllerPress(bool arController, bool enable) {
        RpcSyncARControllerPress(arController, enable);
    }

    [ClientRpc]
    public void RpcSyncData(Vector3 posR, Vector3 rotR, Vector3 posL, Vector3 rotL, Vector3 posH, Vector3 rotH) {
        positionR = posR;
        rotationR = rotR;
        positionL = posL;
        rotationL = rotL;
        positionH = posH;
        rotationH = rotH;
    }

    [Command]
    public void CmdSyncData(Vector3 posR, Vector3 rotR, Vector3 posL, Vector3 rotL, Vector3 posH, Vector3 rotH) {
        RpcSyncData(posR, rotR, posL, rotL, posH, rotH);
    }

    public void Update() {
        /*if (viveARPressed) {
            print("Vive AR controller input pressed..");
            viveARPressed = false;
        }*/
        if (vivePressed && !inputConfirmed) {
            GetComponent<cameraRigHandler>().SteamVR_Rig.transform.Find("[CameraRig]").GetComponent<calibrationManager>().assignValue = true;
            inputConfirmed = true;
        }
    }
}
