using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VRTK_Switcher : NetworkBehaviour {

    public GameObject VRSimulator_Rig;
    public GameObject SteamVR_Rig;
    public GameObject AR_Rig;
    public GameObject Operator_Panel;

    //[SyncVar]
    public GameObject currentRig;

    [SyncVar]
    public string rigType;

    [SyncVar]
    public string lastRig;

    public GameObject mainPanel;
    public GameObject sidePanel;

    public Text label;
    public Transform player;
    public Transform drawParent;

    public void updateLabel() {
        Debug.Log("Updating these labels..");
        PlayerStorage.players.Add(currentRig.name);
        label.text = "Current Active Rig:" + currentRig.name + " \n" +
                     "Press 1 to activate VR Sim\n" +
                     "Press 2 to activate VR Rig\n" +
                     "Press 3 to activate AR Rig\n" +
                     "Press 4 to activate Operator";
    }

    public GameObject getRig() {
        return Input.GetKeyDown(KeyCode.Alpha1) ? VRSimulator_Rig : Input.GetKeyDown(KeyCode.Alpha2) ? SteamVR_Rig : Input.GetKeyDown(KeyCode.Alpha3) ? AR_Rig : Input.GetKeyDown(KeyCode.Alpha4) ? Operator_Panel : null;
    }

    [Command]
    void CmdLastRig() {
        RpclastRigType();
    }

    [ClientRpc]
    void RpclastRigType() {
        if(rigType != null) {
            lastRig = rigType;
        }
    }

    [Command]
    void CmdAssignRig(string rig) {
        RpcAssignPlayerName(rig);
    }

    [ClientRpc]
    void RpcAssignPlayerName(string rig) {
        if(rig != "OperatorPanel" && rig != "AR_Rig") {
            rigType = rig;
            //GameObject.Find(rigType).SetActive(true);
            print("New rig enabled:" + rigType);
        }
    }

    void operatorPanelRig(GameObject rig) {
        rig.GetComponentInChildren<Camera>().targetDisplay = 1;
    }

    void SwitchClient() {
        if(Input.anyKeyDown) {
            GameObject rig = getRig();
            if(rig == null) return;
            CmdLastRig();
            //if(rig == Operator_Panel) operatorPanelRig(rig);
            if(currentRig == null) currentRig = rig; CmdAssignRig(rig.name); mainPanel.SetActive(false); sidePanel.SetActive(true);
            if(rig.activeInHierarchy == false) {
                currentRig.SetActive(false);
                if(rig == Operator_Panel || rig == AR_Rig) { //Local rigs
                    rig.SetActive(true);
                }
                currentRig = rig;
                CmdAssignRig(rig.name);
                updateLabel();
            }
        }
    }

    public bool VRActivated = false;
    private void Update() {
        /*if(!isServer) {
            return;
        }
        if(!isLocalPlayer) {
            return;
        }*/
        //CmdSwitchClient();
        SwitchClient();
    }

    public override void OnStartLocalPlayer() {
        AR_Rig = GameObject.Find("AR_Rig");
        Operator_Panel = GameObject.Find("OperatorPanel");
        AR_Rig.SetActive(false);
        Operator_Panel.SetActive(false);
        Operator_Panel.GetComponentInChildren<Camera>().enabled = true;

        //PlayerStorage.AddPlayer();
        //Operator_Panel = GameObject.FindGameObjectWithTag("Operator");
        //print("Operator Panel:" + Operator_Panel);
        print("OnStartLocalPlayer called.. IS THIS COMPILING?");
        if(isLocalPlayer) {
            //GetComponentInChildren<Canvas>().enabled = true;
            this.enabled = true;
            this.transform.Find("MenuScreen").GetComponent<Canvas>().enabled = true;
            this.transform.Find("MenuScreen").GetComponentInChildren<Camera>().enabled = true;
            print("Canvas loaded.." + this.transform.Find("MenuScreen").GetComponent<Canvas>().enabled);
            Cursor.visible = true;
        }
        //currentRig = VRSimulator_Rig;
    }

}
