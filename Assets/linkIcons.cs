using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class linkIcons : NetworkBehaviour {

    //public Transform targetObject1;
    //public Transform targetObject2;

    public GameObject[] players;
    public GameObject rootParent;

    public bool currentlyLinking;
    public GameObject targetObject1;
    public Vector3 hitPos;

    public void linkProcess(Transform targetObj1, Vector3 hitPosition) {
        float hitDist = Vector3.Distance(targetObj1.position, hitPosition);
        this.transform.position = Vector3.Lerp(targetObj1.position, hitPosition, .5f);
        this.transform.LookAt(hitPosition);
        this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, hitDist);
    }

    public void linkObject(Transform targetObj1, Transform targetObj2) {
        float hitDist = Vector2.Distance(targetObj1.position, targetObj2.position);
        this.transform.position = Vector3.Lerp(targetObj1.position, targetObj2.transform.position, .5f);
        this.transform.LookAt(targetObj2.position);
        this.transform.localScale = new Vector3(this.transform.localScale.x, this.transform.localScale.y, Vector3.Distance(targetObj1.position, targetObj2.position));
    }

	// Use this for initialization
	void Start () {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    int netCounter = 0;

    IEnumerator Wait(float duration, int netId) {
        //This is a coroutine
        Debug.Log("Start Wait() function. The time is: " + Time.time);
        yield return new WaitForSeconds(duration);   //Wait
        players = GameObject.FindGameObjectsWithTag("Player");
        print("Players count:" + players.Length);
        targetObject1 = players[netId].GetComponent<VRTK_Switcher>().VRSimulator_Rig.transform.Find("[VRSimulator_CameraRig]").gameObject;
        currentlyLinking = true;
    }

    // Update is called once per frame
    void Update() {
        if(NetworkServer.connections.Count >= 2 && NetworkServer.connections.Count != netCounter && rootParent.GetComponent<NetworkBehaviour>().isServer) { // Second player (or more) has connected to the server..
            netCounter = NetworkServer.connections.Count;
            StartCoroutine(Wait(2, 1));
        } else if(!rootParent.GetComponent<NetworkBehaviour>().isServer) {
            StartCoroutine(Wait(2, 0));
        }
        if (currentlyLinking == true) {
            linkProcess(targetObject1.transform, this.transform.parent.transform.position);
        }
    }
}
