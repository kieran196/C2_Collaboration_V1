using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SyncTransperancy : NetworkBehaviour {

    [SyncVar (hook = "OnColorChanged")]
    public float colorAlpha;

    [ClientRpc]
    public void RpcSyncVarWithClients(float varToSync) {
        colorAlpha = varToSync;
        GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b, varToSync);
        GetComponentInChildren<Text>().text = varToSync.ToString();
    }

    [Command]
    public void CmdSyncVarWithClients(float varToSync) {
        RpcSyncVarWithClients(varToSync);
    }

    void OnColorChanged(float alpha) {
        colorAlpha = alpha;
        GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b, colorAlpha);
    }



}
