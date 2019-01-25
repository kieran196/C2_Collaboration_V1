using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class readPythonData : MonoBehaviour {
    private StreamReader reader;
    private float timer = 0f;
    public static readonly float DELAY = 1f;

    void Awake() {
        Process p = new Process(); // create process (i.e., the python program
        p.StartInfo.FileName = @"python.exe";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
        p.StartInfo.Arguments = @"pyOut.py"; //PanelsDirectory[j] + "\\powerlink_logs_mrg.py"; // start the python program with two parameters                        
        p.Start();
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer >= DELAY) {
            timer = 0f;
            string path = "output.txt";
            reader = new StreamReader(path);
            UnityEngine.Debug.Log("Reading data:"+reader.ReadToEnd());
            reader.Close();
        }
    }

}
