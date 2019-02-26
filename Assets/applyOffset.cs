using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class applyOffset : MonoBehaviour {

    public bool setTransformOffset;
    // Start is called before the first frame update
    void Start() {
        if(setTransformOffset) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Vector3 pos = transform.position;
            calibrationManager calibrationSettings = players[0].GetComponent<VRTK_Switcher>().SteamVR_Rig.transform.Find("[CameraRig]").GetComponent<calibrationManager>();
            if(calibrationSettings.offSetAssigned) {
                Vector3 offset = calibrationSettings.offset;
                this.transform.position = new Vector3(-pos.x - offset.x, pos.y - offset.y, -pos.z - offset.z);
            }
        }
    }
}
