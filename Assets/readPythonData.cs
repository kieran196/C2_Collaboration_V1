using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Diagnostics;
 
public class readPythonData : NetworkBehaviour {

    public bool HR_DATA_ENABLED;
    public string currData = "";
    private StreamReader reader;
    private float timer = 0f;
    public static readonly float DELAY = 1f;
    private bool processStarted = false;
    Process pyApp = new Process();
 
    [SyncVar(hook = "OnValTypeChange")]
    public string data;
 
    [Command]
    void CmdAssignRig(string val) {
        RpcAssignRig(val);
    }
 
    [ClientRpc]
    void RpcAssignRig(string val) {
        data = val;
    }
 
    void OnValTypeChange(string val) {
        if(isLocalPlayer)
            return;
        data = val;
    }
 
    void Update() {
        if(HR_DATA_ENABLED && !processStarted) {
            processStarted = true;
            pyApp = new Process(); // create process (i.e., the python program
            pyApp.StartInfo.FileName = @"python.exe";
            pyApp.StartInfo.RedirectStandardOutput = true;
            pyApp.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
            pyApp.StartInfo.Arguments = @"pyOut.py"; //PanelsDirectory[j] + "\\powerlink_logs_mrg.py"; // start the python program with two parameters                        
            pyApp.Start();
        }
 
        counter++;
        if(HR_DATA_ENABLED && processStarted) {
            timer += Time.deltaTime;
            if(timer >= DELAY) {
                timer = 0f;
                string path = "output.txt";
                reader = new StreamReader(path);
                currData = reader.ReadToEnd();
                //UnityEngine.Debug.Log("Reading data:" + currData);
                //CmdAssignRig(currData.ToString());
                reader.Close();
            }
        }
        if(isLocalPlayer && HR_DATA_ENABLED) {
            CmdAssignRig(currData.ToString());
        }
    }
 
    int counter = 0;
 
 
    void OnApplicationQuit() {
        if(HR_DATA_ENABLED && processStarted) {
            print("Application quit..");
            pyApp.Kill();
            pyApp.CloseMainWindow();
            pyApp.Close(); //Not closing?
        }
    }
 
}