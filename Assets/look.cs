using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class look : MonoBehaviour
{

    public GameObject target;
    // Start is called before the first frame update
    void Start() {
        
    }

    public bool vrEnabled = false;

    // Update is called once per frame
    void Update() {
        if(target != null) {
            this.transform.LookAt(target.transform);
            //print("Distance:" + Vector3.Distance(this.transform.position, target.transform.position));
            var planes = GeometryUtility.CalculateFrustumPlanes(this.transform.parent.GetComponent<Camera>());
            Bounds bounds = (!vrEnabled) ? target.transform.Find("Player").GetComponent<Renderer>().bounds : target.GetComponent<Renderer>().bounds;
            if(GeometryUtility.TestPlanesAABB(planes, bounds)) {
                if(this.transform.GetChild(0).gameObject.activeInHierarchy) {
                    this.transform.GetChild(0).gameObject.SetActive(false);
                }

            } else {
                if(!this.transform.GetChild(0).gameObject.activeInHierarchy) {
                    print("Object not visible");
                    this.transform.GetChild(0).gameObject.SetActive(true);
                }
            }

            /*if(target.transform.Find("Player").GetComponent<Renderer>() != null) {
                print("Is visible from cam:" + target.transform.Find("Player").GetComponent<Renderer>().isVisible);
            }*/
        } else {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players) {
                if (!player.GetComponent<NetworkIdentity>().isLocalPlayer) {
                    GameObject p = (vrEnabled) ? player.GetComponent<cameraRigs>().SteamVR : player.GetComponent<cameraRigs>().VRSim;
                    target = p;
                }
            }
        }
    }
}
