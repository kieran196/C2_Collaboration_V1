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

    public float spawn_speed = 0;
    public float destroy_speed = 0;
    public int spawn_amount = 0;

    void Start() {
        spawn_speed = this.GetComponent<boxCollision>().SPAWN_SPEED;
        destroy_speed = this.GetComponent<boxCollision>().DESTROY_SPEED;
        spawn_amount = this.GetComponent<boxCollision>().SPAWN_AMOUNT;
    }

    void Update() {
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
}