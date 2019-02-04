using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class determineLocalPlayer : NetworkBehaviour {

    private Camera cam;
    private GameObject rig;

    public GameObject menuCanvas;


    [SyncVar(hook = "onNameChange")]
    public string playerName;

    private void Start() {
        rig = GetComponent<VRTK_Switcher>().currentRig;
        this.transform.SetParent(GameObject.Find("Parent").transform);
    }

    public GameObject[] getUsers() {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    void Update() {
        if(isLocalPlayer && playerName != "") {
            CmdAssignPlayerName("Player:" + netId);
        }

        if(isLocalPlayer && !menuCanvas.GetComponent<Canvas>().enabled) {
            menuCanvas.GetComponent<Canvas>().enabled = true;
        }


        //Here
        int count = 0;
        foreach(GameObject user in getUsers()) {
            //print(playerName + "  | Current Rig:" + user.GetComponent<VRTK_Switcher>().rigType + " | Last Rig: " + user.GetComponent<VRTK_Switcher>().lastRig);

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


        if(isLocalPlayer && GetComponentInChildren<cameraController>() != null) {
            if(cam == null) {
                cam = GetComponentInChildren<cameraController>().cam;
            }
            this.GetComponentInChildren<cameraController>().cam.enabled = true;
            //print("local player:" + this.GetComponentInChildren<cameraController>().cam.enabled);
        }
    }

    [Command]
    void CmdAssignPlayerName(string name) {
        RpcAssignPlayerName(name);
    }

    [ClientRpc]
    void RpcAssignPlayerName(string name) {
        playerName = name;
        this.transform.name = playerName;
    }

    public void onNameChange(string newName) {
        playerName = newName;
        //Debug.Log("Name has been changed on client: updated?" + playerName);
    }

    public override void OnStartLocalPlayer() {
        print("Trying to assign players name...");
        CmdAssignPlayerName("Player:" + netId);
        /*foreach(GameObject user in getUsers()) {
            user.transform.Find(user.GetComponent<VRTK_Switcher>().rigType).gameObject.SetActive(true);
        }*/
    }

}