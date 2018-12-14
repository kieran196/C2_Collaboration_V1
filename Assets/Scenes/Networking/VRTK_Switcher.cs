using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRTK_Switcher : MonoBehaviour {

    public GameObject VRSimulator_Rig;
    public GameObject SteamVR_Rig;
    public Text label;
    public Transform player;
    public Transform drawParent;

    public bool VRActivated = false;
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            VRActivated = !VRActivated;
            VRSimulator_Rig.SetActive(!VRActivated);
            SteamVR_Rig.SetActive(VRActivated);
            player.SetParent((VRActivated) ? SteamVR_Rig.transform : VRSimulator_Rig.transform);
            drawParent.SetParent((VRActivated) ? SteamVR_Rig.transform : VRSimulator_Rig.transform);
            label.text = (VRActivated) ? "Press Q to disable VR" : "Press Q to enable VR";
        }
    }

}
