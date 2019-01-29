using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class readPythonData : MonoBehaviour {
    public bool HR_DATA_ENABLED;
    private StreamReader reader;
    private float timer = 0f;
    public static readonly float DELAY = 1f;
    Process pyApp = new Process();

    void Awake() {
        if(HR_DATA_ENABLED) {
            pyApp = new Process(); // create process (i.e., the python program
            pyApp.StartInfo.FileName = @"python.exe";
            pyApp.StartInfo.RedirectStandardOutput = true;
            pyApp.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
            pyApp.StartInfo.Arguments = @"pyOut.py"; //PanelsDirectory[j] + "\\powerlink_logs_mrg.py"; // start the python program with two parameters                        
            pyApp.Start();
        }
    }

    void Update() {
        if(HR_DATA_ENABLED) {
            timer += Time.deltaTime;
            if(timer >= DELAY) {
                timer = 0f;
                string path = "output.txt";
                reader = new StreamReader(path);
                UnityEngine.Debug.Log("Reading data:" + reader.ReadToEnd());
                reader.Close();
            }
        }
    }

    void OnApplicationQuit() {
        if(HR_DATA_ENABLED) {
            print("Application quit..");
            pyApp.Close(); //Not closing?
        }
    }

}
