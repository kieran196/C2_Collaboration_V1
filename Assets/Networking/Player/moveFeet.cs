using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Small footsteps of the VR user
public class moveFeet : MonoBehaviour {

    public GameObject feet;
	
	// Update is called once per frame
	void Update () {
        feet.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
        feet.transform.localEulerAngles = new Vector3(-90f, this.transform.localEulerAngles.y, this.transform.localEulerAngles.z);
    }
}
