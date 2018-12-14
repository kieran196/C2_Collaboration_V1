using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class linkIcons : MonoBehaviour {

    //public Transform targetObject1;
    //public Transform targetObject2;

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
    }
	
	// Update is called once per frame
	void Update () {
        if (currentlyLinking == true) {
            linkProcess(targetObject1.transform, hitPos);
        }
    }
}
