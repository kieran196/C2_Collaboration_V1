//Attach this to the GameObject you would like to spawn (the player).
//Make sure to create a NetworkManager with an HUD component in your Scene. To do this, create a GameObject, click on it, and click on the Add Component button in the Inspector window.  From there, Go to Network>NetworkManager and Network>NetworkManagerHUD respectively.
//Assign the GameObject you would like to spawn in the NetworkManager.
//Start the server and client for this to work.

//Use this script to send and update variables between Networked GameObjects
using UnityEngine;
using UnityEngine.Networking;

public class syncHololensData : NetworkBehaviour {

    //this will sync the variables value from client to server
    [SyncVar]
    public float SpawnSpeedSync;

    [SyncVar]
    public float DestroySpeedSync;

    [SyncVar]
    public int SpawnAmountSync;

    [SyncVar]
    public string TaskType;
    private string oldTaskType = "";

    public float spawn_speed = 0;
    public float destroy_speed = 0;
    public int spawn_amount = 0;

    private GameObject VRPlayer;

    private GameObject findVRPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players) {
            if(player.GetComponent<cameraRigHandler>().rigType == player.GetComponent<cameraRigHandler>().SteamVR_Rig.name) {
                VRPlayer = player;
                return player;
            }
        }
        return null;
    }

    void Start() {
        spawn_speed = this.GetComponent<blockSelectionTask>().SPAWN_SPEED;
        destroy_speed = this.GetComponent<blockSelectionTask>().DESTROY_SPEED;
        spawn_amount = this.GetComponent<blockSelectionTask>().SPAWN_AMOUNT;
    }

    void Update() {
        if (VRPlayer == null) {
            findVRPlayer();
        } else if (VRPlayer != null) {
            if (oldTaskType != TaskType && TaskType.Length > 1) {
                oldTaskType = TaskType;
                SyncTransperancy.missedBoxes = 0;
                SyncTransperancy.spawnedBoxes = 0;
                SyncTransperancy.hitBoxes = 0;
                print("New task type has been set:" + TaskType);
                if(TaskType == "HOLOLENS_MANIPULATION_FULL") {
                    VRPlayer.GetComponent<cameraRigHandler>().SteamVR_Rig.transform.Find("UICanvas").gameObject.SetActive(true);
                } else if(TaskType == "HOLOLENS_MANIPULATION_LIMITED") {
                    VRPlayer.GetComponent<cameraRigHandler>().SteamVR_Rig.transform.Find("UICanvas").gameObject.SetActive(false);
                }
            }
        }

        if(isLocalPlayer) {
            CmdSyncAllVarsWithClient(spawn_speed, destroy_speed, spawn_amount);
        }
    }

    //this will sync var from server to all clients by calling the "SyncVarWithClientsRpc" funtion on the clients with the value of the variable "varToSync" equals to the value of "example1"
    [ClientRpc]
    public void RpcSyncAllVarsWithClient(float varSS, float varDS, int varSA) {
        SpawnSpeedSync = varSS;
        DestroySpeedSync = varDS;
        SpawnAmountSync = varSA;
    }

    [Command]
    public void CmdSyncAllVarsWithClient(float varSS, float varDS, int varSA) {
        RpcSyncAllVarsWithClient(varSS, varDS, varSA);
    }

    [ClientRpc]
    public void RpcSyncTaskType(string task) {
        if (task != TaskType) {
            TaskType = task;
        }
    }

    [Command]
    public void CmdSyncTaskType(string task) {
        RpcSyncTaskType(task);
    }
}