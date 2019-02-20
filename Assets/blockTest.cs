using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockTest : MonoBehaviour {

    public bool enabled = false;
    public SteamVR_TrackedController leftController;
    public SteamVR_TrackedController rightController;

    private void updateCubePos(Vector3 position, Vector3 scale) {
        if(!this) return;
        this.transform.position = position;
        this.transform.localScale = scale;
    }

    private void Update() {
        if(enabled) {
            if(leftController == null && rightController == null) {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                GameObject host = players[0].GetComponent<VRTK_Switcher>().SteamVR_Rig.transform.Find("[CameraRig]").gameObject;
                leftController = host.GetComponent<SteamVR_ControllerManager>().left.GetComponent<SteamVR_TrackedController>();
                rightController = host.GetComponent<SteamVR_ControllerManager>().right.GetComponent<SteamVR_TrackedController>();
            }
            Vector3 left = leftController.transform.position;
            Vector3 right = rightController.transform.position;
            updateCubePos((left + right) / 2, right - left);
        }
    }
}
