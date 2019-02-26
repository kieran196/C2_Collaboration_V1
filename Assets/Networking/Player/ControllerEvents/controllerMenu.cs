using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerMenu : MonoBehaviour {

    public GameObject menu;
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    private void changeVisibility() {
        if(controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            menu.transform.SetParent(this.transform);
            menu.transform.localPosition = new Vector3(0.092f, -0.179f, 0.053f);
            menu.transform.localEulerAngles = new Vector3(0f, -4.107f, 0f);
            menu.SetActive(!menu.activeInHierarchy);
        }
    }

	// Use this for initialization
	void Start () {
        trackedObj = this.GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        changeVisibility();
    }
}
