using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class determineLocalPlayer : NetworkBehaviour {

    private Camera cam;
    private GameObject rig;

    public GameObject menuCanvas;

    [SyncVar]
    public string playerName;

    private void Start() {
        rig = GetComponent<VRTK_Switcher>().currentRig;
        this.transform.SetParent(GameObject.Find("Parent").transform);
    }

    public GameObject[] getUsers() {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    void Update() {
        if(isLocalPlayer && !menuCanvas.GetComponent<Canvas>().enabled) {
            menuCanvas.GetComponent<Canvas>().enabled = true;
        }


        //Here
        int count = 0;
        foreach(GameObject user in getUsers()) {
            print("Player:" + count + "  | Current Rig:" + user.GetComponent<VRTK_Switcher>().rigType + " | Last Rig: " + user.GetComponent<VRTK_Switcher>().lastRig);

            //enableAllUsers.GetComponent<VRTK_Switcher>()
            if(user.GetComponent<VRTK_Switcher>().rigType != null && user.GetComponent<VRTK_Switcher>().rigType != "" && user.transform.Find(user.GetComponent<VRTK_Switcher>().rigType).gameObject.activeInHierarchy == false) {
                user.transform.Find(user.GetComponent<VRTK_Switcher>().rigType).gameObject.SetActive(true);
                if(user.GetComponent<VRTK_Switcher>().lastRig != null && user.GetComponent<VRTK_Switcher>().lastRig != "") {
                    print(user.GetComponent<VRTK_Switcher>().lastRig.Length);
                    user.transform.Find(user.GetComponent<VRTK_Switcher>().lastRig).gameObject.SetActive(false);
                }
            }
            count++;
        }


        if (isLocalPlayer && GetComponentInChildren<cameraController>() != null) {
            if(cam == null) {
                cam = GetComponentInChildren<cameraController>().cam;
            }
            this.GetComponentInChildren<cameraController>().cam.enabled = true;
            print("local player:" + this.GetComponentInChildren<cameraController>().cam.enabled);
        }
    }

    [Command]
    void CmdAssignPlayerName() {
        RpcAssignPlayerName();
    }

    [ClientRpc]
    void RpcAssignPlayerName() {
        playerName = "Player " + netId;
        print("Player joined the session: " + playerName);
    }

    public void assignPlayerName() {
        if(!isServer) {
            return;
        }
        playerName = "Player " + netId;
    }

    public override void OnStartLocalPlayer() {
        CmdAssignPlayerName();
        /*foreach(GameObject user in getUsers()) {
            user.transform.Find(user.GetComponent<VRTK_Switcher>().rigType).gameObject.SetActive(true);
        }*/
    }

}
