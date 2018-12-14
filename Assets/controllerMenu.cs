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
            menu.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            menu.transform.localEulerAngles = Vector3.zero;
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
