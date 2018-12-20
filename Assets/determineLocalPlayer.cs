using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class determineLocalPlayer : NetworkBehaviour {

    private Camera cam;
    private GameObject rig;

    [SyncVar]
    public string playerName;

    private void Start() {
        rig = GetComponent<VRTK_Switcher>().currentRig;
        this.transform.SetParent(GameObject.Find("Parent").transform);
    }

    void Update() {
        if (isLocalPlayer && GetComponentInChildren<cameraController>() != null) {
            if(cam == null) {
                cam = GetComponentInChildren<cameraController>().cam;
            }
            this.GetComponentInChildren<cameraController>().cam.enabled = true;
        }
    }

    [Command]
    void CmdSendNameToServer(string nameToSend) {
        RpcSetPlayerName(nameToSend);
    }

    [ClientRpc]
    void RpcSetPlayerName(string name) {
        print(name);
    }

}
