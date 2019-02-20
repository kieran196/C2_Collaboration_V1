using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class createOffset : MonoBehaviour {

    public SteamVR_TrackedObject trackedObj;
    private bool offSetAssigned = false;
    private Vector3 offset;
    public bool assignValue = false;

    public void assignOffset() {
        float x = trackedObj.transform.position.x - this.transform.position.x;
        float y = trackedObj.transform.position.y - this.transform.position.y;
        float z = trackedObj.transform.position.z - this.transform.position.z;
        print("Assigning Values:" + x + ", " + y + ", " + z);
        offset = new Vector3(x, y, z);
        //offset = new Vector3(trackedObj.transform.position.x - x, trackedObj.transform.position.y - y, trackedObj.transform.position.z - z);
        offSetAssigned = true;
    }

    private void assignController() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        trackedObj = players[0].GetComponent<VRTK_Switcher>().SteamVR_Rig.transform.Find("[CameraRig]").GetComponent<SteamVR_ControllerManager>().right.GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update() {
        if (trackedObj == null) {
            assignController();
        }
        if (assignValue == true) {
            assignValue = false;
            assignOffset();
        }
        if(trackedObj != null && offSetAssigned == true) {
            Vector3 newVals = new Vector3(trackedObj.transform.position.x - offset.x, trackedObj.transform.position.y - offset.y, trackedObj.transform.position.z - offset.z);
            print(this.transform.position + " . " + newVals);
            //print(newVals);
            //this.transform.rotation = new Quaternion(trackedObj.transform.rotation.x, trackedObj.transform.rotation.y + 180f, trackedObj.transform.rotation.z, trackedObj.transform.rotation.w);
            this.transform.localEulerAngles = new Vector3(trackedObj.transform.localEulerAngles.x, trackedObj.transform.localEulerAngles.y + 180f, trackedObj.transform.localEulerAngles.z);
            this.transform.position = new Vector3(trackedObj.transform.position.x - offset.x, trackedObj.transform.position.y - offset.y, trackedObj.transform.position.z - offset.z);
            //print("After" + this.transform.position);
            //this.transform.position = trackedObj.transform.position - offset;
            //this.transform.localEulerAngles = new Vector3(trackedObj.transform.localEulerAngles.x, trackedObj.transform.localEulerAngles.y + 180f, trackedObj.transform.localEulerAngles.z);
            //print();
        }
    }
}
