using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class splashTool : NetworkBehaviour {

    private bool drawing = false;
    public GameObject texture;
    public Transform drawParent;
    private int drawingCounter;

    private Transform currDeviceTransform;

    private Transform newParent;

    public void AssignParent() {
        drawingCounter++;
        GameObject parent = new GameObject();
        parent.name = "DrawingParent" + drawingCounter;
        parent.transform.SetParent(drawParent.transform);
        newParent = parent.transform;
        print("Pressing a trigger down..");
    }

    [Command]
    public void CmdChangeVisibility(bool enabled) {
        print("Changed visibility:" + enabled);
        this.enabled = enabled;
    }

    [Command]
    public void CmdCreateTexture(Vector3 position, Vector3 eulerAngles) {
        print("Called CmdCreateTexture()");
        if(this.isActiveAndEnabled) {
            if(drawing == false) {
                AssignParent();
            }
            print("Drawing = true");
            var newTexture = (GameObject)Instantiate(texture, position, new Quaternion(0f, 0f, 0f, 0f));
            newTexture.transform.eulerAngles = eulerAngles;
            newTexture.transform.SetParent(newParent);
            newTexture.transform.localPosition += new Vector3(0f, 0f, -0.176614f);
            newTexture.tag = "drawingMat";
            print("Called on server: " + isServer);
            NetworkServer.Spawn(newTexture);
            drawing = true;
        }
    }

    public void disableDrawing() {
        if(this.isActiveAndEnabled) {
            drawing = false;
        }
    }

    private void Update() {
        if(!drawParent.gameObject.activeInHierarchy) {
            drawParent.SetParent(this.transform); //Set it to SteamVR / VRSimulator Parent
        }
    }

}