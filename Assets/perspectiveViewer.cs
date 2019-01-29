using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class perspectiveViewer : NetworkBehaviour {

    public GameObject rootParent;

    public GameObject[] getUsers() {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    public GameObject findPlayer() {
        GameObject[] activeUsers = getUsers();
        int count = 0;
        foreach(GameObject user in getUsers()) {
            if(!user.GetComponent<NetworkIdentity>().isLocalPlayer && count == 0) {
                count += 1;
                return user;
            }
        }
        return null;
    }

    public Transform getNonLocalPlayerHead(GameObject player, bool VRMode) {
        print("Player Found:" + player);
        userCameras cameras = player.GetComponent<userCameras>();
        return (VRMode) ? cameras.VR_Camera.transform.parent : cameras.VRSim_Camera.transform.parent;
    }

    public Transform getLocalPlayerHead(bool VRMode) {
        userCameras cameras = rootParent.GetComponent<userCameras>();
        return (VRMode) ? cameras.VR_Camera.transform.parent : cameras.VRSim_Camera.transform.parent;
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.P)) { // For testing ..
            print("Changing user perspective..");
            Transform nonLocalPlayerPerspective = getNonLocalPlayerHead(findPlayer(), false); //Not in VR
            if(nonLocalPlayerPerspective != null) {
                Transform localPlayerPerspective = getLocalPlayerHead(false); //In VR
                nonLocalPlayerPerspective.position = nonLocalPlayerPerspective.position;
                nonLocalPlayerPerspective.eulerAngles = nonLocalPlayerPerspective.eulerAngles;
            }
        }
    }
}
