using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerHandler : MonoBehaviour {

    public bool OnSpaceDown() {
        return Input.GetKey(KeyCode.Space);
    }


    public bool OnSpaceUp() {
        return Input.GetKeyUp(KeyCode.Space);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
