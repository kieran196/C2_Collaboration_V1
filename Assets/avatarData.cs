using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class avatarData : NetworkBehaviour {

    [SyncVar(hook = "OnNameChange")]
    public string avatarID;

    [SyncVar(hook = "OnNetworkChange")]
    public string networkID;

    [ClientRpc]
    public void RpcSyncNetworkChange(string varToSync) {
        networkID = varToSync;
        //CmdassignParent();
        /*if (this.transform.parent == null && networkID != "") {
            this.transform.SetParent(findPlayerByNetworkID().GetComponent<VRTK_Switcher>().VRSimulator_Rig.GetComponent<cameraController>().cam.transform);
        }*/
    }

    [Command]
    public void CmdSyncNetworkChange(string varToSync) {
        RpcSyncNetworkChange(varToSync);
    }

    public void assignParent(GameObject parent) {
        //this.transform.SetParent(parent.transform.Find("VRSimulator").transform.Find("[VRSimulator_CameraRig]"));
        this.transform.SetParent(parent.GetComponent<VRTK_Switcher>().VRSimulator_Rig.GetComponent<cameraController>().cam.transform);
        this.transform.localPosition = Vector3.zero;
    }

    public GameObject findPlayerByNetworkID() {
        GameObject[] spawnedPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in spawnedPlayers) {
            if(player.GetComponent<NetworkIdentity>().netId.ToString() == networkID) {
                return player;
            }
        }
        return null;
    }

    public void assignParentHost() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        print("Assigning parent host.." + networkID);
        if(networkID != "") {
            GameObject[] spawnedPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in spawnedPlayers) {
                print("player:" + player + ", netID:" + player.GetComponent<NetworkIdentity>().netId);
                if(player.GetComponent<NetworkIdentity>().netId.ToString() == networkID) {
                    //this.transform.SetParent(player.GetComponent<VRTK_Switcher>().VRSimulator_Rig.GetComponent<cameraController>().cam.transform);
                    this.transform.SetParent(player.GetComponent<VRTK_Switcher>().SteamVR_Rig.GetComponent<cameraController>().cam.transform);
                    this.transform.localPosition = Vector3.zero;
                }
            }
        }
    }



    [Command]
    public void CmdAssignParent(GameObject parent) {
        RpcAssignParent(parent);
        //this.transform.SetParent(parent.transform);
        //parent.transform.localPosition = Vector3.zero;
    }

    [ClientRpc]
    public void RpcAssignParent(GameObject parent) {
        this.transform.SetParent(parent.transform);
        parent.transform.localPosition = Vector3.zero;
    }

    void OnNetworkChange(string newNetworkID) {
        networkID = newNetworkID;
    }

    [ClientRpc]
    public void RpcSyncVarWithClients(string varToSync) {
        avatarID = varToSync;
    }

    [Command]
    public void CmdSyncVarWithClients(string varToSync) {
        RpcSyncVarWithClients(varToSync);
    }

    void OnNameChange(string newAvatarID) {
        avatarID = newAvatarID;
    }

    private bool updatedName = false;
    void Update() {
        if (!isServer && avatarID != "" && updatedName == false) {
            updatedName = true;
            this.name = avatarID;
            this.tag = "networkedAvatar";
        }
       if (avatarID != null && this.transform.parent == null) {
           // GameObject obj = findPlayerByNetworkID();
            //print("Found obj:" + obj);
            //GameObject[] spawnedPlayers = GameObject.FindGameObjectsWithTag("Player");
            //this.transform.SetParent(spawnedPlayers[0].GetComponent<VRTK_Switcher>().VRSimulator_Rig.GetComponent<cameraController>().cam.transform);
        }
    }

}
