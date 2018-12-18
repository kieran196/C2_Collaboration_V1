using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class paintbrush : NetworkBehaviour {

    private SteamVR_Controller.Device device;
    private bool drawing = false;
    public GameObject texture;
    public Transform drawParent;
    private SteamVR_TrackedObject trackedObj;
    private int drawingCounter;

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
    public void CmdCreateTexture() {
        if(this.isActiveAndEnabled && this.transform.parent.parent.parent.GetComponent<NetworkBehaviour>().isLocalPlayer) {
            if (drawing == false) {
                AssignParent();
            }
            print("Drawing = true");
            var newTexture = (GameObject)Instantiate(texture, this.transform.position, new Quaternion(0f, 0f, 0f, 0f));
            newTexture.transform.eulerAngles = this.transform.eulerAngles;
            newTexture.transform.SetParent(newParent);
            newTexture.tag = "drawingMat";
            NetworkServer.Spawn(newTexture);
            drawing = true;
        }
    }

    public void disableDrawing() {
        if(this.isActiveAndEnabled) {
            print("Drawing = false");
            drawing = false;
        }
    }

	// Use this for initialization
	void Start () {
        trackedObj = this.GetComponent<SteamVR_TrackedObject>();
    }

    private void Update() {
        if (!drawParent.gameObject.activeInHierarchy) {
            drawParent.SetParent(this.transform.parent.parent); //Set it to SteamVR / VRSimulator Parent
        }
    }

}
