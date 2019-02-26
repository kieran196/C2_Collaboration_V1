using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkTransformHandler : NetworkBehaviour {

    [SyncVar]
    public Vector3 syncScale;

    [SyncVar]
    public Vector3 colorType;

    [ClientRpc]
    public void RpcSyncTransform(Vector3 vector, bool scale) {
        if(scale) {
            syncScale = vector;
            transform.localScale = syncScale;
        } else {
            colorType = vector;
            //X = R, Y = G, Z = B (Used Vector for color since colors can't be syncvars)
            GetComponent<Renderer>().material.color = new Color(vector.x, vector.y, vector.z);
        }
    }

    //False = Color, True = Scale
    [Command]
    public void CmdSyncTransform(Vector3 vector, bool scale) {
        RpcSyncTransform(vector, scale);
    }

}