using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class operatorSettings : MonoBehaviour {
    public GameObject movementPanel;
    public GameObject perspectivesPanel;

    public void persectivesPanelActivation() {
        perspectivesPanel.SetActive(!perspectivesPanel.activeInHierarchy);
    }

    public void movementPanelActivation() {
        movementPanel.SetActive(!movementPanel.activeInHierarchy);
    }

    public GameObject[] getUsers() {
        return GameObject.FindGameObjectsWithTag("Player");
    }

    public void enableMovementAllUsers() {
        foreach (GameObject user in getUsers()) {
            print(user.GetComponent<trackUser>().ToString());
            user.transform.Find("WalkParent").gameObject.SetActive(true);
        }
        print("Completed record of all users..");
    }

    public void enableMovementUser(int count) {
        foreach (GameObject user in getUsers()) {
            if (user.GetComponent<determineLocalPlayer>().playerName == "Player "+count) {
                user.transform.Find("WalkParent").gameObject.SetActive(true);
            } else {
                user.transform.Find("WalkParent").gameObject.SetActive(false);
            }
        }
    }

    void Update() {
        
    }

}
