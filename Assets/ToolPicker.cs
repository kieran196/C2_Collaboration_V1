using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolPicker : MonoBehaviour {

    private GameObject lastHoveredTool;
    private GameObject lastSelectedTool;
    private SteamVR_Controller.Device device;
    public GameObject eraser;
    public GameObject rootParent;

    public void hoverTool(GameObject tool, SteamVR_TrackedObject trackedObj, Vector3 hitPoint) {
        if(tool.transform.tag == "UI_Icons") {
            device = SteamVR_Controller.Input((int)trackedObj.index);
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                selectTool(tool, trackedObj, hitPoint);
            }
            if(lastHoveredTool == null) {
                lastHoveredTool = tool;
            }
            if(lastHoveredTool != tool) { //New object has been hovered .. 0.092, -0.179, 0.053 | -4.107
                lastHoveredTool.transform.GetChild(0).transform.localScale = new Vector3(1f, 1f, 0.1f);
                lastHoveredTool.transform.GetChild(0).transform.localPosition = new Vector3(0f, 0f, 0.06f);
                lastHoveredTool.transform.localPosition = new Vector3(lastHoveredTool.transform.localPosition.x, lastHoveredTool.transform.localPosition.y, 0f);
                tool.transform.GetChild(0).transform.localScale = new Vector3(1f, 1f, 0.5f);
                tool.transform.GetChild(0).transform.localPosition = new Vector3(0f, 0f, 0.3f);
                tool.transform.localPosition = new Vector3(tool.transform.localPosition.x, tool.transform.localPosition.y, -0.3f);
                lastHoveredTool = tool;
            }
        }
    }

    private void disableTools(SteamVR_TrackedObject trackedObj) {
        rootParent.GetComponent<paintbrush>().enabled = false;
        rootParent.GetComponent<pencil>().enabled = false;
        rootParent.GetComponent<splashTool>().enabled = false;
        eraser.SetActive(false);
    }

    private void resetRotation() {
        this.GetComponent<Animator>().SetBool("rotate", false);
        this.GetComponent<Animator>().SetBool("rotateOpposite", false);
    }

    public void rotateToolPicker(SteamVR_Controller.Device controller, SteamVR_TrackedObject trackedObj) {
        if (this.gameObject.activeInHierarchy == true) {
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) {
                Vector2 touchpadAxis = (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
                if (touchpadAxis.x > 0.7f) {
                    this.GetComponent<Animator>().SetBool("rotate", true);
                } else if (touchpadAxis.x < -0.7f) {
                    this.GetComponent<Animator>().SetBool("rotateOpposite", true);
                }
            }
        }
    }

    public GameObject cubePrefab;
    public GameObject cylinderPrefab;
    public GameObject spherePrefab;
    public GameObject planePrefab;

    public void createObject(SteamVR_TrackedObject trackedObj, Vector3 hitPoint, GameObject prefab) {
        GameObject obj = Instantiate(prefab);
        obj.transform.localPosition = hitPoint;
        obj.transform.SetParent(trackedObj.transform);
        obj.layer = LayerMask.NameToLayer("InteractableGameObjectGen");
    }

    public void selectTool(GameObject tool, SteamVR_TrackedObject trackedObj, Vector3 hitPoint) {
        if (lastSelectedTool == null) {
            lastSelectedTool = tool;
        }
        disableTools(trackedObj);
        tool.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
        if(lastSelectedTool != tool) {
            lastSelectedTool.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.white;
            lastSelectedTool = tool;
        }
        if(tool.transform.name == "ReturnIcon") {
            this.gameObject.SetActive(false);
        } else if(tool.transform.name == "PaintBrush") {
            rootParent.GetComponent<paintbrush>().enabled = true;
        } else if(tool.transform.name == "Eraser") {
            eraser.GetComponent<eraser>().trackedObj = trackedObj;
            eraser.SetActive(true);
            eraser.transform.SetParent(trackedObj.transform);
            eraser.transform.localPosition = Vector3.zero;
            eraser.transform.localEulerAngles = Vector3.zero;
        } else if(tool.transform.name == "Pencil") {
            rootParent.GetComponent<pencil>().enabled = true;
        } else if(tool.transform.name == "Splatter") {
            rootParent.GetComponent<splashTool>().enabled = true;
        } else if(tool.transform.name == "CubeIcon") {
            createObject(trackedObj, hitPoint, cubePrefab);
        } else if(tool.transform.name == "CylinderIcon") {
            createObject(trackedObj, hitPoint, cylinderPrefab);
        } else if(tool.transform.name == "SphereIcon") {
            createObject(trackedObj, hitPoint, spherePrefab);
        } else if(tool.transform.name == "PlaneIcon") {
            createObject(trackedObj, hitPoint, planePrefab);
        }
    } 
}
