using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetVariables : MonoBehaviour {

    public bool resetvariables;

    // Update is called once per frame
    void Update() {
        if (resetvariables) {
            SyncTransperancy.hitBoxes = 0;
            SyncTransperancy.missedBoxes = 0;
            SyncTransperancy.spawnedBoxes = 0;
            resetvariables = false;
        }
    }
}
