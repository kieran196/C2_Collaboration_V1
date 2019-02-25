using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Diagnostics;

/// ===============================
/// AUTHOR: Kieran William May
/// PURPOSE: The class handles retrieving HR data from the Python script
/// NOTES:
/// Basically how this works is when the Unity app starts a process of the Python program is created.
/// The Python program outputs the Zephyr Bioharness HR data into a .txt file (each 1s)
/// The .txt file is read from this unity C# script (each 1s) and the HR data is retrieved.
/// It's then synced across the server/client using commands and RPC calls.
/// ===============================

public class readPythonData : NetworkBehaviour {

    public bool HR_DATA_ENABLED;

    public string currData = "";
    private StreamReader reader;
    private float timer = 0f;
    public static readonly float DELAY = 1f; // This is how long the .txt file containing HR data is read (default = 1 p/second)
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
        //An instance of the Python script is created automatically through our C# script. (Or it can just be started manually)
        if(HR_DATA_ENABLED && !processStarted) {
            processStarted = true;
            pyApp = new Process(); // create process (i.e., the python program
            pyApp.StartInfo.FileName = @"python.exe";
            pyApp.StartInfo.RedirectStandardOutput = true;
            pyApp.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
            pyApp.StartInfo.Arguments = @"pyOut.py"; //PanelsDirectory[j] + "\\powerlink_logs_mrg.py"; // start the python program with two parameters                        
            pyApp.Start();
        }
        //Reads the output.txt file every second and calls a command to synchronize the HR data
        counter++;
        if(HR_DATA_ENABLED && processStarted) {
            timer += Time.deltaTime;
            if(timer >= DELAY) {
                timer = 0f;
                string path = "output.txt";
                reader = new StreamReader(path);
                currData = reader.ReadToEnd(); // This is the retrieved HR data
                //UnityEngine.Debug.Log("Reading data:" + currData);
                CmdAssignRig(currData.ToString());
                reader.Close();
            }
        }
        if(isLocalPlayer && counter != 0) {
            CmdAssignRig(currData.ToString());
        }
    }

    int counter = 0;

    //Automatically kills our Python script when our Unity program exists. (So we don't have 100 python processes running in the background)
    void OnApplicationQuit() {
        if(HR_DATA_ENABLED && processStarted) {
            print("Application quit..");
            pyApp.Kill();
            pyApp.CloseMainWindow();
            pyApp.Close(); //Not closing?
        }
    }

}
