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

    public float spawn_speed = 0;
    public float destroy_speed = 0;
    public int spawn_amount = 0;

    void Start() {
        spawn_speed = this.GetComponent<blockSelectionTask>().SPAWN_SPEED;
        destroy_speed = this.GetComponent<blockSelectionTask>().DESTROY_SPEED;
        spawn_amount = this.GetComponent<blockSelectionTask>().SPAWN_AMOUNT;
        TaskType = this.GetComponent<blockSelectionTask>().Task_Type.ToString();
    }

    void Update() {
        //CmdSyncTaskType(GetComponent<boxCollision>().Task_Type.ToString());
        if(isLocalPlayer) {
            if (!foundHololens) {
                findHololensUser();
            } else if (foundHololens && NetworkServer.connections.Count > 1) {
                syncHololensData holoScript = hololensUser.GetComponent<syncHololensData>();
                holoScript.CmdSyncTaskType(this.GetComponent<blockSelectionTask>().Task_Type.ToString());
                CmdSyncAllVarsWithClient(holoScript.SpawnSpeedSync, holoScript.DestroySpeedSync, holoScript.SpawnAmountSync);
            }
        }
    }

    private bool foundHololens = false;
    private GameObject hololensUser = null;

    void findHololensUser() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (player.GetComponent<cameraRigHandler>().rigType == CONSTANTS.AR_RIG) {
                hololensUser = player;
                foundHololens = true;
            }
        }
    }

    //this will sync var from server to all clients by calling the "SyncVarWithClientsRpc" funtion on the clients with the value of the variable "varToSync" equals to the value of "example1"
    [ClientRpc]
    public void RpcSyncAllVarsWithClient(float varSS, float varDS, int varSA) {
        blockSelectionTask blockSelectionScript = this.GetComponent<blockSelectionTask>();
        if(blockSelectionScript.Task_Type != blockSelectionTask.TASK_TYPE.DEFAULT_VALUES) {
            if(varSS != -1) {
                SpawnSpeedSync = varSS;
                blockSelectionScript.setSpawnSpeed(SpawnSpeedSync);
            }
            if(varDS != -1) {
                DestroySpeedSync = varDS;
                blockSelectionScript.setDestroySpeed(DestroySpeedSync);
            }
            if(varSA != -1) {
                SpawnAmountSync = varSA;
                blockSelectionScript.setSpawnAmount(SpawnAmountSync);
            }
        }
    }

    [Command]
    public void CmdSyncAllVarsWithClient(float varSS, float varDS, int varSA) {
        RpcSyncAllVarsWithClient(varSS, varDS, varSA);
    }

    [ClientRpc]
    public void RpcSyncTaskType(string task) {
        if(task != TaskType) {
            TaskType = task;
        }
    }

    [Command]
    public void CmdSyncTaskType(string task) {
        RpcSyncTaskType(task);
    }

}