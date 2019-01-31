using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VRTK_Switcher : NetworkBehaviour {

    public GameObject VRSimulator_Rig;
    public GameObject SteamVR_Rig;
    public GameObject OptiTracker;
   // public GameObject AR_Rig;
    public GameObject Operator_Panel;

    //[SyncVar]
    public GameObject currentRig;

    //[SyncVar]
    [SyncVar(hook = "OnRigTypeChange")]
    public string rigType;

    [SyncVar]
    public string varRigType;

    [SyncVar]
    public string rigTypeRpcTesting;

    [SyncVar]
    public string lastRig;

    public GameObject mainPanel;
    public GameObject sidePanel;

    public Text label;
    public Transform player;
    public Transform drawParent;
    public enum GAME_STATES {MENU, GAME}
    public GAME_STATES currentState;

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
        return Input.GetKeyDown(KeyCode.Alpha1) ? VRSimulator_Rig : Input.GetKeyDown(KeyCode.Alpha2) ? SteamVR_Rig : Input.GetKeyDown(KeyCode.Alpha3) ? OptiTracker : Input.GetKeyDown(KeyCode.Alpha4) ? Operator_Panel : null;
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

    void OnRigTypeChange(string newRig) {
        if(isLocalPlayer)
            return;
        rigType = newRig;
    }

    [Command]
    void CmdAssignRig(string rig) {
        RpcAssignRig(rig);
    }

    [ClientRpc]
    void RpcAssignRig(string rig) {
        if(rig != "OperatorPanel" && rig != "AR_Rig") {
            rigType = rig;
            //print("New rig enabled:" + rigType);
        }
    }

    void operatorPanelRig(GameObject rig) {
        rig.GetComponentInChildren<Camera>().targetDisplay = 1;
    }

    public void assignRigs(string rigName) {
        if(isServer) {
            RpcAssignRig(rigName);
        } else {
            CmdAssignRig(rigName);
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        OnRigTypeChange(rigType);
    }

    public void loadPrefabs() {
        foreach(var prefab in FindObjectOfType<NetworkManager>().spawnPrefabs) {
            if(prefab != null) {
                ClientScene.RegisterPrefab(prefab);
            }
        }
    }

    void SwitchClient() {
        if(Input.anyKeyDown) {
            GameObject rig = getRig();
            if(rig == null) return;
            print("Activated Rig:"+rig.name);
            CmdLastRig();
            if(currentRig == null) currentRig = rig; CmdAssignRig(rig.name); mainPanel.SetActive(false); sidePanel.SetActive(true); loadPrefabs(); currentState = GAME_STATES.GAME;

            if(rig.activeInHierarchy == false) {
                currentRig.SetActive(false);
                if(rig == Operator_Panel) { //Local rigs
                    rig.SetActive(true);
                    Operator_Panel.transform.GetComponentInChildren<Camera>().enabled = true;
                }
                currentRig = rig;
                CmdAssignRig(rig.name);
                if(rig.GetComponent<cameraController>()) {
                    //this.GetComponent<UserAvatarLoader>().userAvatar.transform.SetParent(rig.GetComponent<cameraController>().cam.transform);
                    //this.GetComponent<UserAvatarLoader>().resetOrientation();
                }
                updateLabel();
            }
        }
    }

    /*[SyncVar]
    public string networkID;

    [ClientRpc]
    void RpcSyncVarWithClients(string varToSync) {
        networkID = varToSync;
        this.transform.name = networkID;
    }*/

    public bool VRActivated = false;
    public bool rpcVarAssigned = false;
    private void Update() {
        /*if(isServer && currentRig != null) {
            RpcSyncVarWithClients(currentRig.name);
        } else if (isServer) {
            RpcSyncVarWithClients("");
        }*/


        if(isLocalPlayer) {
            if(currentRig != null) {
                CmdAssignRig(currentRig.name);
            }
            SwitchClient();
        }
        /*if(isServer) {
            if(rigType != null) {
                RpcSyncVarWithClients(transform.GetComponent<NetworkIdentity>().netId.ToString());
                rpcVarAssigned = true;
            } else {
                RpcSyncVarWithClients("Unassigned..");
            }
        }*/
    }

    public override void OnStartLocalPlayer() {
        Operator_Panel = GameObject.FindGameObjectWithTag("Operator");
        if (Operator_Panel != null)
            Operator_Panel.GetComponent<Camera>().enabled = true;
            Operator_Panel.SetActive(false);

        //PlayerStorage.AddPlayer();
        //Operator_Panel = GameObject.FindGameObjectWithTag("Operator");
        //print("Operator Panel:" + Operator_Panel);
        print("OnStartLocalPlayer called.. IS THIS COMPILING?");
        if(isLocalPlayer) {
            //GetComponentInChildren<Canvas>().enabled = true;
            this.GetComponent<readPythonData>().enabled = true;
            this.enabled = true;
            this.transform.Find("MenuScreen").GetComponent<Canvas>().enabled = true;
            this.transform.Find("MenuScreen").GetComponentInChildren<Camera>().enabled = true;
            print("Canvas loaded.." + this.transform.Find("MenuScreen").GetComponent<Canvas>().enabled);
            Cursor.visible = true;
        }
        //currentRig = VRSimulator_Rig;
    }

}
