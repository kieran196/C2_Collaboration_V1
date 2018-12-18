using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour {

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    public GameObject mirroredCube;
    public GameObject selectedObject;

    private void ShowLaser(RaycastHit hit) {
        //mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(this.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);
    }

    void mirroredObject() {
        Vector3 controllerPos = this.transform.forward;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        Vector3 mirroredPos = this.transform.position;

        mirroredPos.x = mirroredPos.x + (100f / (distance_formula_on_vector)) * controllerPos.x;
        mirroredPos.y = mirroredPos.y + (100f / (distance_formula_on_vector)) * controllerPos.y;
        mirroredPos.z = mirroredPos.z + (100f / (distance_formula_on_vector)) * controllerPos.z;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = this.transform.rotation;
    }
    // Use this for initialization
    void Start () {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    private GameObject last2DObject;
    public void Convert2DTo3D(GameObject obj) {
        if (obj.tag == "2DObject" ) {
            last2DObject = obj;
            if(last2DObject != null && last2DObject == selectedObject) {
                //last2DObject.GetComponent<MeshRenderer>().enabled = true;
                Color color = last2DObject.GetComponent<Renderer>().material.color;
                color.a = 1f;
                last2DObject.GetComponent<Renderer>().material.color = color;
            } else if(last2DObject != null && last2DObject != selectedObject) {
                //last2DObject.GetComponent<MeshRenderer>().enabled = false;
                Color color = last2DObject.GetComponent<Renderer>().material.color;
                color.a = 0f;
                last2DObject.GetComponent<Renderer>().material.color = color;
            }
        }
    }

    public void Convert2DTo3D() {
        if(last2DObject != null) {
            Color color = last2DObject.GetComponent<Renderer>().material.color;
            color.a = 0f;
            last2DObject.GetComponent<Renderer>().material.color = color;
        }
    }

    void Update() {
        //mirroredObject();
        //ShowLaser();
        Ray ray = Camera.main.ScreenPointToRay(this.transform.position);
        RaycastHit hit;
        if(Physics.Raycast(this.transform.position, this.transform.forward, out hit, 100)) {
            selectedObject = hit.transform.gameObject;
            hitPoint = hit.point;
            ShowLaser(hit);
            Convert2DTo3D(hit.transform.gameObject);
        } else {
            laser.SetActive(false);
            Convert2DTo3D();
            selectedObject = null;
        }
    }

}
