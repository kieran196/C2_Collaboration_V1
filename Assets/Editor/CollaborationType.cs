using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollaborationType : MonoBehaviour {

    public enum COLLAB_TYPE {LOCAL_COLLABORATION, REMOTE_COLLABORATION}
    public COLLAB_TYPE collabType;

    public GameObject avatarWorld;

    public void updateSettings() {
        print("Updating " + collabType + " Settings..");
        if (collabType == COLLAB_TYPE.LOCAL_COLLABORATION) {
            avatarWorld.SetActive(false);
        } else {
            avatarWorld.SetActive(true);
        }
    }

    private COLLAB_TYPE old;
    // Update is called once per frame
    void Update() {
        if (old != collabType) {
            old = collabType;
            updateSettings();

        }
    }
}
