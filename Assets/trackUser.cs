using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class trackUser : MonoBehaviour {

    public bool tracking = true;

    public Vector3 lastLocation;
    public Text user1;
    public GameObject texture;

    public float totalDistance = 0f;
    public float timeElapsed = 0f;
    public Transform walkParent;

    // Use this for initialization
    void Start () {
        lastLocation = this.transform.position;
	}

    private float addDistance() {
        if (tracking) {
            timeElapsed += Time.deltaTime;
            user1.text = "User 1\nDistance moved:" + totalDistance + "m\nElapsed time:"+(int)timeElapsed+"s";
            float distanceMoved = Vector3.Distance(this.transform.position, lastLocation);
            if (distanceMoved != 0f) {
                GameObject newTexture;
                newTexture = Instantiate(texture, new Vector3(this.transform.position.x, -0.066f, this.transform.position.z), new Quaternion(0f, 0f, 0f, 0f));
                newTexture.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
                newTexture.transform.localScale += new Vector3(this.transform.position.x - lastLocation.x, (this.transform.position.z - lastLocation.z), 0f);
                newTexture.transform.SetParent(walkParent);
                newTexture.tag = "drawingMat";
            }
            return distanceMoved;
        }
        return 0f;
    }

    public void resetProperties() {
        totalDistance = 0;
        timeElapsed = 0;
    }
	
	// Update is called once per frame
	void Update () {
        //print(totalDistance);
        totalDistance += addDistance();
        lastLocation = this.transform.position;
    }
}
