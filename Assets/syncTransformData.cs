using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// This class provides the Hololens with the positions and rotations of the controllers and Vive HMD.
/// The original Vectors are bieng read from the Hololens per frame and is used to calibrate the Vive coordinate system inside the Hololens.
/// </summary>

public class syncTransformData : NetworkBehaviour {

    //Enable this if project is running the Vive
    public bool isVive = false;

    public GameObject leftController;
    public GameObject rightController;
    public GameObject head;

    [SyncVar]
    public bool vivePressed;

    [SyncVar]
    public bool viveARPressed;

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

    [SerializeField]
    float angX, angY, angZ;

    [Command]
    public void CmdSyncData(Vector3 posR, Vector3 rotR, Vector3 posL, Vector3 rotL, Vector3 posH, Vector3 rotH) {
        RpcSyncData(posR, rotR, posL, rotL, posH, rotH);
    }

    private Vector3 WrapAngles(Vector3 angles) {
        float angX = angles.x %= 360;
        float angY = angles.y %= 360;
        float angZ = angles.z %= 360;
        if (angX > 180) {
            angX -= 360f;
        } if(angY > 180) {
            angY -= 360f;
        } if(angZ > 180) {
            angZ -= 360f;
        }
        return new Vector3(angX, angY, angZ);
    }

    void Update() {
        if (isVive && isLocalPlayer) {
            CmdSyncData(rightController.transform.position, WrapAngles(rightController.transform.localEulerAngles), leftController.transform.position, WrapAngles(leftController.transform.localEulerAngles), head.transform.position, WrapAngles(head.transform.localEulerAngles));
        }
    }
}
