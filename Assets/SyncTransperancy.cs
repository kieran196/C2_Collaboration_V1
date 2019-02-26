using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SyncTransperancy : NetworkBehaviour {

    public static int missedBoxes;
    public static int spawnedBoxes;
    public static int hitBoxes;

    private void Start() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Vector3 pos = transform.position;
        Vector3 offset = players[0].GetComponent<VRTK_Switcher>().SteamVR_Rig.transform.Find("[CameraRig]").GetComponent<calibrationManager>().offset;
        this.transform.position = new Vector3(-pos.x - offset.x, 0f, -pos.z - offset.z);
        this.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        spawnedBoxes += 1;
    }

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

    public void OnDestroy() {
        if(Application.isEditor) {
            if (colorAlpha <= 0) {
                missedBoxes += 1;
            } else {
                hitBoxes += 1;
            }
         }
    }



}
