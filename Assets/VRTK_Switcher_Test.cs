using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class VRTK_Switcher_Test : NetworkBehaviour {

    public GameObject panel;
    public Text nameLabel;

    private bool VRActivated = false;

    public override void OnStartLocalPlayer() {
        print("OnStartLocalPlayer called..");
        if(isLocalPlayer) {
            //GetComponentInChildren<Canvas>().enabled = true;
            this.transform.Find("MenuScreen").GetComponent<Canvas>().enabled = true;
            print("Canvas loaded.." + this.transform.Find("MenuScreen").GetComponent<Canvas>().enabled);
            Cursor.visible = true;
        }
    }

    // Update is called once per frame
    void Update () {
        if(Input.GetKeyDown(KeyCode.Q) && isLocalPlayer && panel.activeInHierarchy) {
            print("Panel function was called..");
            panel.SetActive(false);
        }
    }
}
