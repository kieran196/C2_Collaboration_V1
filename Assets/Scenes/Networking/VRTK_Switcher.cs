using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VRTK_Switcher : NetworkBehaviour {

    public GameObject VRSimulator_Rig;
    public GameObject SteamVR_Rig;
    public GameObject AR_Rig;

    public GameObject currentRig;
    public GameObject menuPanel;
    public GameObject sidePanel;

    public Text label;
    public Transform player;
    public Transform drawParent;

    public void updateLabel() {
        label.text = "Current Active Rig:" + currentRig.name + " \n" +
                     "Press 1 to activate VR Sim\n" +
                     "Press 2 to activate VR Rig\n" +
                     "Press 3 to activate AR Rig";
    }

    public GameObject getRig() {
        return Input.GetKeyDown(KeyCode.Alpha1) ? VRSimulator_Rig : Input.GetKeyDown(KeyCode.Alpha2) ? SteamVR_Rig : Input.GetKeyDown(KeyCode.Alpha3) ? AR_Rig : null;
    }

    [Command]
    public void CmdswitchClient() {
        if (Input.anyKeyDown) {
            GameObject rig = getRig();
            if(rig == null) return;
            if(currentRig == null) currentRig = rig; menuPanel.SetActive(false); sidePanel.SetActive(true);
            if (rig.activeInHierarchy == false) {
                currentRig.SetActive(false);
                rig.SetActive(true);
                currentRig = rig;
                updateLabel();
            }
        }
    }

    public bool VRActivated = false;
    private void Update() {
        if(!isLocalPlayer) {
            return;
        }
        CmdswitchClient();
        /*if (Input.GetKeyDown(KeyCode.Q)) {
            CmdswitchClient();
        }*/
    }

    public override void OnStartLocalPlayer() {
        //GetComponent<MeshRenderer>().material.color = Color.blue;
        currentRig = VRSimulator_Rig;
    }

}
