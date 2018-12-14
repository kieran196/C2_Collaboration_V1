using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class globalSettings : MonoBehaviour {

    //Settings
    private float upTime;
    public Text upTimeLabel;

    public Toggle generateReport; // Determine if generate report is toggled

    //Report
    private string reportName = "test";
    public List<string> reportData = new List<string>();

    public void generateCSVReport() {
        string dest = reportName + ".csv";
        StreamWriter writer = null;
        int count = 1;
        bool foundPath = false;
        while (foundPath == false) { // Avoids making duplicate file names
            if(File.Exists(dest)) { 
                dest = reportName+count+".csv";
                count++;
            } else {
                Debug.Log("Generated file:" + dest);
                writer = new StreamWriter(dest, true) as StreamWriter;
                foundPath = true;
            }
        }
        //Created the CSV report, now writing to the CSV file..
        writeCSVReport(writer);
    }

    public void writeCSVReport(StreamWriter writer) {
        Debug.Log("Writing CSV report");
        for (int i=0; i < reportData.Count; i++) {
            writer.Write(reportData[i]);
            writer.WriteLine();
        }
        Debug.Log("Written to the path:" + Application.dataPath);
        writer.Close();
    }

    private void OnApplicationQuit() {
        if(generateReport.isOn) {
            Debug.Log("Exiting application .. Generating a CSV Report");
            reportData.Add(upTime.ToString());
            generateCSVReport();
        }
    }

    // Use this for initialization
    void Start () {
        reportData.Add("Uptime");
    }
	
	// Update is called once per frame
	void Update () {
        upTime += Time.deltaTime;
        upTimeLabel.text = "Current Uptime:"+ upTime;
    }
}
