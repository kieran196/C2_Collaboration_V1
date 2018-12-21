using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class operatorSettings : MonoBehaviour {
    public GameObject movementPanel;

    public void movementPanelActivation() {
        movementPanel.SetActive(!movementPanel.activeInHierarchy);
    }

    public GameObject[] getUsers() {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    public void enableAllUsers() {
        foreach (GameObject user in getUsers()) {
            print(user.GetComponent<trackUser>().ToString());
        }
        print("Completed record of all users..");
    }

    void Update() {
        
    }

}
