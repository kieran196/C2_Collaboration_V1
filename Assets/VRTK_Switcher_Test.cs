using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRTK_Switcher_Test : NetworkBehaviour {

    public GameObject Sphere;
    public GameObject Cube;

    public bool VRActivated = false;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(isLocalPlayer && Input.GetKeyDown(KeyCode.Q)) {
            print("VR ACTIVED :: " + VRActivated);
            VRActivated = !VRActivated;
            Sphere.SetActive(!VRActivated);
            Cube.SetActive(VRActivated);
        }
    }
}
