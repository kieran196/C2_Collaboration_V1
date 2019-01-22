using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class avatarData : NetworkBehaviour {

    [SyncVar(hook = "OnColorChanged")]
    public string avatarID;

    [ClientRpc]
    public void RpcSyncVarWithClients(string varToSync) {
        avatarID = varToSync;
    }

    [Command]
    public void CmdSyncVarWithClients(string varToSync) {
        RpcSyncVarWithClients(varToSync);
    }

    void OnColorChanged(string newAvatarID) {
        avatarID = newAvatarID;
    }


}
