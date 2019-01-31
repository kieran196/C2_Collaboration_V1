using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class rotateToAR : MonoBehaviour {

    public Text label;
    public GameObject rootParent;
    private bool parentSet = false;
    public string currRig = "";
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
        label.text = "HR"+rootParent.GetComponent<readPythonData>().data.ToString();
    }

    void assignParent() {
        string rigType = rootParent.GetComponent<VRTK_Switcher>().rigType;
        if((rigType == "VRSimulator" || rigType == "SteamVR") && rigType != this.transform.parent.name) {
            GameObject rig = (rigType == "VRSimulator") ? rootParent.GetComponent<VRTK_Switcher>().VRSimulator_Rig : rootParent.GetComponent<VRTK_Switcher>().SteamVR_Rig;
            this.GetComponent<Canvas>().enabled = true;
            parentSet = true;
            transform.SetParent(rig.transform);
        }
    }

    void updatePosition() {

    }

    private void LateUpdate() {
        if(parentSet) {
            Transform findChild = this.transform.parent.GetChild(0).transform;
            Vector3 newVec = new Vector3(findChild.localPosition.x, findChild.localPosition.y + 2f, findChild.localPosition.z);
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
