//Attach this to the GameObject you would like to spawn (the player).
//Make sure to create a NetworkManager with an HUD component in your Scene. To do this, create a GameObject, click on it, and click on the Add Component button in the Inspector window.  From there, Go to Network>NetworkManager and Network>NetworkManagerHUD respectively.
//Assign the GameObject you would like to spawn in the NetworkManager.
//Start the server and client for this to work.

//Use this script to send and update variables between Networked GameObjects
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {

    //this will sync the variables value from client to server
    [SyncVar]
    public string networkID;
    public int counter = 0;

    void Update() {
        if(isLocalPlayer) {
            CmdSyncVarWithClients("val:" + counter.ToString());
            if(counter != 0) {
                CmdSyncVarWithClients("val:" + counter.ToString());
            }
            /*if(Input.anyKeyDown && isLocalPlayer) {
                CmdSyncVarWithClients("val:" + counter.ToString());
                counter++;
            }*/
        }
            /*if(isLocalPlayer) {
                setValues();
            }*/

            /*
            if(isServer) {
                RpcSyncVarWithClients("val:" + counter.ToString());
                if(Input.anyKeyDown && isLocalPlayer) {
                    counter++;
                }
            } else {
                if(Input.anyKeyDown && isLocalPlayer) {
                    counter++;
                    CmdSyncVarWithClients("val:" + counter.ToString());
                }
            }*/
        }

    //this will sync var from server to all clients by calling the "SyncVarWithClientsRpc" funtion on the clients with the value of the variable "varToSync" equals to the value of "example1"
    [ClientRpc]
    void RpcSyncVarWithClients(string varToSync) {
        networkID = varToSync;
        this.transform.name = networkID;
    }

    [Command]
    void CmdSyncVarWithClients(string varToSync) {
        RpcSyncVarWithClients(varToSync);
    }

    public override void OnStartClient() {
        base.OnStartClient();
        CmdSyncVarWithClients("val:" + counter.ToString());
    }

}