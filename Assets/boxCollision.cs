using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxCollision : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    private GameObject[] InteractableCubes;

    public float globalTimer = 0;
    public float localTimer = 0f;

    public float reactionTime = 10f;

    public GameObject selectedCube;

    void OnCollisionEnter(Collision col) {
        print("Collided with obj:" + col.transform.name);
    }

    void OnTriggerStay(Collider col) {
        print("Triggered with obj:" + col.transform.name);
        if(col.tag == "box") {
            onSelect(col.gameObject);
        }
    }

    void Start() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        InteractableCubes = GameObject.FindGameObjectsWithTag("box");
        localTimer = reactionTime;
    }


    public GameObject randomlySelectCube() {
        int randomNum = Random.Range(0, InteractableCubes.Length);
        return InteractableCubes[randomNum];
    }


    public void initializeCube() {

    }

    void onSelect(GameObject obj) {
        if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
            //Destroy(obj);
            print("Selected object:" + obj.name);
            obj.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    /*
            if (selectedCube != null) {
            selectedCube.GetComponent<Renderer>().material.color = new Color(selectedCube.GetComponent<Renderer>().material.color.r, selectedCube.GetComponent<Renderer>().material.color.g, selectedCube.GetComponent<Renderer>().material.color.b, 255);
        }

    */

    private int secondCounter = 0;

    public void addSecond() {
        secondCounter = (int)globalTimer;
    }

    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        if (globalTimer == 0) {
            print("Spawn first item..");
            selectedCube = randomlySelectCube();
        }
        if (selectedCube != null) {
            //addSecond();
            //localTimer -= Time.deltaTime;
            //print(localTimer);
            selectedCube.GetComponent<Renderer>().material.color = new Color(selectedCube.GetComponent<Renderer>().material.color.r, selectedCube.GetComponent<Renderer>().material.color.g, selectedCube.GetComponent<Renderer>().material.color.b, selectedCube.GetComponent<Renderer>().material.color.a - Time.deltaTime/10);
        }


        globalTimer += Time.deltaTime;

    }
    

}
