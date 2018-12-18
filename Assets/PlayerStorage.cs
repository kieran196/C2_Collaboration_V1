using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStorage : MonoBehaviour {

    public List<GameObject> playerObjects = new List<GameObject>();

	public void updatePerspective(GameObject currPlayer) {
        foreach (GameObject player in playerObjects) {
            Camera cam = player.GetComponentInChildren<Camera>();
            cam.enabled = false;
        }
    }

}
