using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class csvWriter : MonoBehaviour {

    public bool logData;
    private List<string> logInfo = new List<string>();


    void Start() {
        //Basic example logging data to .csv file
        //logInfo.Add("Dataset1, Dataset2, Dataset3");
        if(logData) {
            writeToFile();
        }
    }

    public void WriteLine(string line) {
        logInfo.Add(line);
    }

    public void writeToFile() {
        string dest = "Logs/trial" + ".csv";
        StreamWriter writer = null;
        int count = 1;
        bool foundPath = false;
        while(foundPath == false) {
            if(File.Exists(dest)) {
                dest = "Logs/trial" + count + ".csv";
                count++;
            } else {
                print("Found path:" + dest);
                writer = new StreamWriter(dest, true) as StreamWriter;
                foundPath = true;
            }
        }
        print("Writing..");
        for(int i = 0; i < logInfo.Count; i++) {
            //print(logInfo[i]);
            writer.Write(logInfo[i]);
            writer.WriteLine();
        }
        print("Writen to file:" + Application.dataPath);
        writer.Close();
    }
}
