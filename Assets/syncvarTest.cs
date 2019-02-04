using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class syncvarTest : NetworkBehaviour {

    [SyncVar(hook = "OnRigTypeChange")]
    public int rigType;

    void OnRigTypeChange(int newRig) {
        if(isLocalPlayer)
            return;
        rigType = newRig;
    }

    [Command]
    void CmdAssignRig(int rig) {
        RpcAssignRig(rig);
    }

    [ClientRpc]
    void RpcAssignRig(int rig) {
        rigType = rig;
    }

    public override void OnStartClient() {
        base.OnStartClient();
        OnRigTypeChange(rigType);
    }
}
