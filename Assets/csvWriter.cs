using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class csvWriter : MonoBehaviour {

    /// ===============================
    /// AUTHOR: Kieran William May
    /// PURPOSE: Writes to and generates a .csv file
    /// NOTES:
    /// (If CSV file(s) with the same name already exists it will loop through using a counter until it finds a name that doesn't exist)
    /// ===============================

    public bool logData;
    private List<string> logInfo = new List<string>();

    private const string FILE_NAME = "trial";
    private const string DIR = "Logs/";

    public void WriteLine(string line) {
        logInfo.Add(line);
    }

    public void writeToFile() {
        string dest = DIR + FILE_NAME + ".csv";
        StreamWriter writer = null;
        int count = 1;
        bool foundPath = false;
        while(foundPath == false) {
            if(File.Exists(dest)) {
                dest = DIR + FILE_NAME + count + ".csv";
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
