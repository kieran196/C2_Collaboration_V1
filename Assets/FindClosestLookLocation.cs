using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindClosestLookLocation : MonoBehaviour {

    public LayerMask interactionLayers;
    private GameObject lastSelectedObject; // holds the selected object

    public Text lookingAtUser;

    public GameObject currentlyPointingAt;
    private Vector3 castingBezierFrom;
    public GameObject controllerRight = null;
    public GameObject controllerLeft = null;

    void checkSurroundingObjects() {

        Vector3 forwardVectorFromRemote = this.transform.forward;
        Vector3 positionOfRemote = this.transform.position;

        // This way is quite innefficient but is the way described for the bendcast.
        // Might make an example of a way that doesnt loop through everything
        var allObjects = FindObjectsOfType<GameObject>();

        float shortestDistance = float.MaxValue;

        GameObject objectWithShortestDistance = null;
        // Loop through objects and look for closest (if of a viable layer)
        for(int i = 0; i < allObjects.Length; i++) {
            // dont have to worry about executing twice as an object can only be on one layer
            if(interactionLayers == (interactionLayers | (1 << allObjects[i].layer))) {
                // Check if object is on plane projecting in front of VR remote. Otherwise ignore it. (we dont want our laser aiming backwards)
                Vector3 forwardParallelToDirectionPointing = Vector3.Cross(forwardVectorFromRemote, this.transform.up);
                Vector3 targObject = this.transform.position - allObjects[i].transform.position;
                Vector3 perp = Vector3.Cross(forwardParallelToDirectionPointing, targObject);
                float side = Vector3.Dot(perp, this.transform.up);
                if(side < 0) {
                    // Object can only have one layer so can do calculation for object here
                    Vector3 objectPosition = allObjects[i].transform.position;

                    // Using vector algebra to get shortest distance between object and vector 
                    Vector3 forwardControllerToObject = this.transform.position - objectPosition;
                    Vector3 controllerForward = forwardVectorFromRemote;
                    float distanceBetweenRayAndPoint = Vector3.Magnitude(Vector3.Cross(forwardControllerToObject, controllerForward)) / Vector3.Magnitude(controllerForward);



                    Vector3 newPoint = new Vector3(forwardVectorFromRemote.x * distanceBetweenRayAndPoint + positionOfRemote.x, forwardVectorFromRemote.y * distanceBetweenRayAndPoint + positionOfRemote.y
                            , forwardVectorFromRemote.z * distanceBetweenRayAndPoint + positionOfRemote.z);

                    if(distanceBetweenRayAndPoint < shortestDistance) {
                        shortestDistance = distanceBetweenRayAndPoint;
                        objectWithShortestDistance = allObjects[i];
                    }
                }

            }
        }
        if(objectWithShortestDistance != null) {

            // setting the object that is being pointed at
            currentlyPointingAt = objectWithShortestDistance;

            castingBezierFrom = this.transform.position;

        } else {
            // Laser didnt reach any object so will disable
            currentlyPointingAt = null;
            lastSelectedObject = null;
        }
    }

    public void Update() {
        checkSurroundingObjects();
        //Bad way of finding the root parents name.. FIX THIS LATER
        string closestUserName = currentlyPointingAt.transform.parent.parent.parent.parent.name;
        lookingAtUser.text = "User Information Settings\nLooking at:"+ closestUserName;
    }

}
