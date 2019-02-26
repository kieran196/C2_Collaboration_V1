using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Sy
/// </summary>

public class NetworkTransformHandler : NetworkBehaviour {

    public enum TRANSFORM_TYPE {SCALE = 0, COLOR = 1}

    [SyncVar]
    public Vector3 syncScale;

    [SyncVar]
    public Vector3 colorType;

    [ClientRpc]
    public void RpcSyncTransform(Vector3 vector, int transformType) {
        if(transformType == (int)TRANSFORM_TYPE.SCALE) {
            syncScale = vector;
            transform.localScale = syncScale;
        } else if(transformType == (int)TRANSFORM_TYPE.COLOR) {
            colorType = vector;
            //X = R, Y = G, Z = B (Used Vector for color since colors can't be syncvars)
            GetComponent<Renderer>().material.color = new Color(vector.x, vector.y, vector.z);
        }
    }

    //False = Color, True = Scale
    [Command]
    public void CmdSyncTransform(Vector3 vector, int transformType) {
        RpcSyncTransform(vector, transformType);
    }
    
}
