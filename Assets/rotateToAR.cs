using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class rotateToAR : MonoBehaviour {

    public Text currHR;
    public Text restHR;
    public Text MaxMinHR;
    public Text sucessRate;
    public Text failRate;

    public GameObject rootParent;
    public bool parentSet = false;
    int count = 0;
    GameObject[] players;
    private string hrVar = "";
    private GameObject hrPlayer;

    int GetConnectionCount() {
        int count = 0;
        foreach(NetworkConnection con in NetworkServer.connections) {
            if(con != null)
                count++;
        }
        return count;
    }

    void updateText() {
        /*players = GameObject.FindGameObjectsWithTag("Player");
        if (NetworkServer.connections.Count >= 2 && hrVar == "") {
            foreach (GameObject player in players) {
                hrVar = player.GetComponent<readPythonData>().data;
                if (hrVar != "") {
                    hrPlayer = player;
                }
            }
        }
        if(hrPlayer != null) {
            label.text = hrPlayer.GetComponent<readPythonData>().data;
        }*/
        readPythonData pyOut = rootParent.GetComponent<readPythonData>();
        if(pyOut.data.ToString() != "") {
            currHR.text = "HR" + rootParent.GetComponent<readPythonData>().data.ToString();
        } else if(pyOut.currData.ToString() != "") {
            currHR.text = "HR" + rootParent.GetComponent<readPythonData>().currData.ToString();
        }
        restHR.text = "";
        MaxMinHR.text = "";
        sucessRate.text = "Success Rate:" +SyncTransperancy.hitBoxes.ToString();
        failRate.text = "Fail Rate:" + SyncTransperancy.missedBoxes.ToString();
    }

    void assignParent() {
        string rigType = rootParent.GetComponent<cameraRigHandler>().rigType;
        if((rigType == "VRSimulator" || rigType == "SteamVR") && rigType != this.transform.parent.name) {
            GameObject rig = (rigType == "VRSimulator") ? rootParent.GetComponent<cameraRigHandler>().VRSimulator_Rig : rootParent.GetComponent<cameraRigHandler>().SteamVR_Rig;
            this.GetComponent<Canvas>().enabled = true;
            parentSet = true;
            transform.SetParent(rig.transform);
        }
    }

    void updatePosition() {

    }

    private void LateUpdate() {
        if(parentSet) {
            //Transform findChild = this.transform.parent.GetChild(0).transform;
            Transform findChild = this.transform.parent.GetComponent<cameraController>().cam.transform;
            //print(findChild.name);
            Vector3 newVec = new Vector3(findChild.localPosition.x, findChild.localPosition.y + 0.5f, findChild.localPosition.z);
            this.transform.localPosition = newVec;
        }
    }

    // Update is called once per frame
    void Update() {
        //this.transform.rotation = Quaternion.Inverse(this.transform.parent.rotation);
        assignParent();
        if(this.GetComponent<Canvas>().enabled) {
            updateText();
        }
    }
}
