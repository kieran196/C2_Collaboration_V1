using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour {

    public GameObject feet;
	
	// Update is called once per frame
	void Update () {
        feet.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
        feet.transform.localEulerAngles = new Vector3(-90f, this.transform.localEulerAngles.y, this.transform.localEulerAngles.z);
    }
}
