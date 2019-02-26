using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerIdentity : NetworkBehaviour {

    [SyncVar] public string playerID;
    private NetworkInstanceId playerNetID;
    private Transform myTransform;

    public override void OnStartLocalPlayer() {
        GetIdentity();
        SetIdentity();
    }

    void Awake() {
        myTransform = this.transform;
    }

    void Update() {
        if(myTransform.name == "" || myTransform.name == "VRMultiRigAttempt" || myTransform.name == "VRMultiRigAttempt(Clone)") {
            SetIdentity();
        }
    }

    [Client]
    void GetIdentity() {
        playerNetID = GetComponent<NetworkIdentity>().netId;
        CmdTellServerIdentity(MakeUniqueName());
    }

    void SetIdentity() {
        if (!isLocalPlayer) {
            myTransform.name = playerID;
        } else {
            myTransform.name = MakeUniqueName();
        }
    }

    string MakeUniqueName() {
        string uniqueName = "Player " + playerNetID.ToString();
        return uniqueName;
    }

    [Command]
    void CmdTellServerIdentity(string id) {
        playerID = id;
    }


}
