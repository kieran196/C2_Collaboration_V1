using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FishingReel : MonoBehaviour {

    /* Fishing Reel implementation by Kieran May
     * University of South Australia
     * 
     * */

    public LayerMask interactionLayers;
    private Transform oldParent;

    public GameObject controllerRight = null;
    public GameObject controllerLeft = null;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private GameObject mirroredCube;

    public ToolPicker toolPicker;

    public enum InteractionType {Selection, Manipulation_Movement, Manipulation_Full};
    public InteractionType interacionType;

    public enum ControllerPicked { Left_Controller, Right_Controller };
    public ControllerPicked controllerPicked;

    internal bool objectSelected = false;

    public UnityEvent selectedObject; // Invoked when an object is selected

    public UnityEvent droppedObject; // Invoked when object is dropped

	public UnityEvent hovered; // Invoked when an object is hovered by technique
	public UnityEvent unHovered; // Invoked when an object is no longer hovered by the technique

    private void ShowLaser(RaycastHit hit) {
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);
    }

    private Valve.VR.EVRButtonId trigger = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

    public GameObject hitPointObj;
    public GameObject cameraRig;
    public Material greenMat, redMat;


    public void teleportOnFloor(RaycastHit hit) {
        // When the laser hits the floor, a small bubble(hitPointObj) showing the user the location they'll teleport to.
        if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Floor")) {
            laserTransform.GetComponent<MeshRenderer>().material = greenMat;
            if(hitPointObj.activeInHierarchy == false) {
                hitPointObj.SetActive(true);
            }
            hitPointObj.transform.position = hit.point;
            // If trigger is pressed, the user will teleport to the given location
            if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                print("Teleported to:" + hit.transform.position);
                //Vector3 offset = cameraRig.transform.position - this.transform.position;
                Vector3 offset = Vector3.zero;
                cameraRig.transform.position = new Vector3(hitPoint.x + offset.x, hitPoint.y, hitPoint.z + offset.z);
            }
        } else {
            //Laser changes from green to red
            laserTransform.GetComponent<MeshRenderer>().material = redMat;
            if(hitPointObj.activeInHierarchy == true) {
                hitPointObj.SetActive(false);
            }
        }
    }

    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    public GameObject lastSelectedObject;
    public void PickupObject(GameObject obj) {
        if (interactionLayers != (interactionLayers | (1 << obj.layer))) {
            // object is wrong layer so return immediately 
            return;
        }
        if(lastSelectedObject != obj) {
            // is a different object from the currently highlighted so unhover
            unHovered.Invoke();
        }
        hovered.Invoke();
        Vector3 controllerPos = trackedObj.transform.forward;
        if (trackedObj != null) {
            if (controller.GetPressDown(trigger) && pickedUpObject == false) {
                if (interacionType == InteractionType.Manipulation_Movement) {
                    oldParent = obj.transform.parent;
                    obj.transform.SetParent(trackedObj.transform);
                    extendDistance = Vector3.Distance(controllerPos, obj.transform.position);
                    lastSelectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                    pickedUpObject = true;
                } else if (interacionType == InteractionType.Manipulation_Full && this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    lastSelectedObject = obj;
                    objectSelected = true;
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                } else if (interacionType == InteractionType.Selection) {
                    lastSelectedObject = obj;
                    objectSelected = true;
                }
                selectedObject.Invoke();
            }
            if (controller.GetPressUp(trigger) && pickedUpObject == true) {
                if (interacionType == InteractionType.Manipulation_Movement) {
                    lastSelectedObject.transform.SetParent(oldParent);
                    pickedUpObject = false;
                    droppedObject.Invoke();
                }
                objectSelected = false;               
            }
        }
    }

    private float extendDistance = 0f;
    public float reelSpeed = 40f; // Decrease to make faster, Increase to make slower

    private void PadScrolling(GameObject obj) {
        if (obj.transform.name == "Mirrored Cube") {
            return;
        }
        Vector3 controllerPos = trackedObj.transform.forward;
        if (controller.GetAxis().y != 0) {
            extendDistance += controller.GetAxis().y / reelSpeed;
            reelObject(obj);
        }
    }

    void reelObject(GameObject obj) {
        Vector3 controllerPos = trackedObj.transform.forward;
        Vector3 pos = trackedObj.transform.position;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        pos.x += (extendDistance / (distance_formula_on_vector)) * controllerPos.x;
        pos.y += (extendDistance / (distance_formula_on_vector)) * controllerPos.y;
        pos.z += (extendDistance / (distance_formula_on_vector)) * controllerPos.z;

        obj.transform.position = pos;
        obj.transform.rotation = trackedObj.transform.rotation;
    }

    void mirroredObject() {
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        Vector3 mirroredPos = trackedObj.transform.position;

        mirroredPos.x = mirroredPos.x + (100f / (distance_formula_on_vector)) * controllerPos.x;
        mirroredPos.y = mirroredPos.y + (100f / (distance_formula_on_vector)) * controllerPos.y;
        mirroredPos.z = mirroredPos.z + (100f / (distance_formula_on_vector)) * controllerPos.z;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
    }

    private GameObject manipulationIcons;
    public whiteboardTool whiteboard;

    public void configureWhiteboardTool(RaycastHit hit) {
        if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
            if(hit.point != Vector3.zero) {
                this.whiteboard.SetColor(Color.blue);
                this.whiteboard.SetDrawPos(hit.textureCoord.x, hit.textureCoord.y);
                this.whiteboard.ToggleDrawing(true);
                Debug.Log("Is drawing currently..");
            }
        } else if(controller.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
            this.whiteboard.ToggleDrawing(false);
        }       
    }

    void Awake() {
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;
        if (controllerPicked == ControllerPicked.Right_Controller) {
            print(controllerRight);
            trackedObj = controllerRight.GetComponent<SteamVR_TrackedObject>();
        } else if (controllerPicked == ControllerPicked.Left_Controller) {
            trackedObj = controllerLeft.GetComponent<SteamVR_TrackedObject>();
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
        if (interacionType == InteractionType.Manipulation_Full) {

            //this.gameObject.AddComponent<ColorPicker>();
            //this.GetComponent<ColorPicker>().trackedObj = trackedObj;
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
            manipulationIcons = GameObject.Find("Manipulation_Icons");
            this.GetComponent<SelectionManipulation>().manipulationIcons = manipulationIcons;
        }

    }

    void Start() {
        //print("joystick names:" + Valve.VR.iN);
        laser = Instantiate(laserPrefab);
        laser.transform.SetParent(this.transform);
        laserTransform = laser.transform;
    }

    private GameObject lastHoveredButton;
    private GameObject lastSelectedButton;
    public linkIcons linking;
    void Hover2DButtons(GameObject obj) {
        if(obj.transform.tag == "UI_Buttons") {
            if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                obj.transform.GetComponent<Image>().color = Color.green;
                linking.currentlyLinking = true;
                linking.targetObject1 = obj;
                if (lastSelectedButton != null) lastSelectedButton.GetComponent<Image>().color = Color.white;
                lastSelectedButton = obj;
            }
            if(lastHoveredButton == null) {
                lastHoveredButton = obj;
                obj.transform.GetComponent<Image>().color = Color.yellow;
            }
            if(lastHoveredButton != obj) { //New object has been hovered
                if(lastHoveredButton != lastSelectedButton) {
                    lastHoveredButton.transform.GetComponent<Image>().color = Color.white;
                }
                obj.transform.GetComponent<Image>().color = Color.yellow;
                lastHoveredButton = obj;
            }
        }
        if (obj.transform.tag == "UI_User") {
            if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && linking.currentlyLinking == true) {
                linking.currentlyLinking = false;
                linking.linkObject(lastSelectedButton.transform, obj.transform);
            }
        }
    }

    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        toolPicker.rotateToolPicker(controller, trackedObj);
        mirroredObject();
        ShowLaser();
        Ray ray = Camera.main.ScreenPointToRay(trackedObj.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit, 100)) {
            hitPoint = hit.point;
            if(whiteboard != null) {
                configureWhiteboardTool(hit);
            }
            if (linking != null && linking.currentlyLinking == true) {
                linking.hitPos = hitPoint;
            }
            teleportOnFloor(hit);
            Hover2DButtons(hit.transform.gameObject);
            PickupObject(hit.transform.gameObject);
            toolPicker.hoverTool(hit.transform.gameObject, trackedObj, hitPoint);
            if (pickedUpObject == true && lastSelectedObject == hit.transform.gameObject) {
                PadScrolling(hit.transform.gameObject);
            }
            ShowLaser(hit);
        }
    }

}
