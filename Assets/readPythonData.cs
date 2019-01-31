using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using UnityEngine.Networking;

public class readPythonData : NetworkBehaviour {

    public bool HR_DATA_ENABLED;
    public string currData = "";
    private StreamReader reader;
    private float timer = 0f;
    public static readonly float DELAY = 1f;

    [SyncVar(hook = "OnValTypeChange")]
    public string data;

    [ClientRpc]
    public void RpcAssignRig(string val) {
        data = val;
    }

    [Command]
    public void CmdAssignRig(string val) {
        RpcAssignRig(val);
    }

    void OnValTypeChange(string val) {
        if(isLocalPlayer)
            return;
        data = val;
    }

    int counter = 0;

    void Update() {
        counter++;
        /*if(HR_DATA_ENABLED) {
            timer += Time.deltaTime;
            if(timer >= DELAY) {
                timer = 0f;
                string path = "output.txt";
                reader = new StreamReader(path);
                currData = reader.ReadToEnd();
                UnityEngine.Debug.Log("Reading data:" + currData);
                CmdAssignRig(counter.ToString());
                reader.Close();
            }
        }*/
        if (isLocalPlayer && counter != 0) {
            CmdAssignRig(currData.ToString());
        }
    }

    /*public override void OnStartClient() {
        base.OnStartClient();
        CmdSyncVarWithClients(currData);
    }*/

    void OnApplicationQuit() {
        if(HR_DATA_ENABLED) {
            print("Application quit..");
            //pyApp.Kill();
            //pyApp.CloseMainWindow();
            //pyApp.Close();
        }
    }

}
