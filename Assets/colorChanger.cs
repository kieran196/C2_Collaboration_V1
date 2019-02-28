using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Networking;

public class colorChanger : MonoBehaviour, IInputClickHandler {

    public static int count = 5;
    public static GameObject inputType;
    public static GameObject oldinputType;

    public GameObject localPlayer;
    public GameObject VRPlayer;

    private GameObject findVRPlayer() {
        print("Searching for VR player...");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players) {
            if(player.GetComponent<cameraRigHandler>().rigType == player.GetComponent<cameraRigHandler>().SteamVR_Rig.name) {
                VRPlayer = player;
                print("VR player was found.");
                return player;
            }
        }
        print("VR player was not found.");
        return null;
    }

    private GameObject findLocalPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            if (player.GetComponent<NetworkBehaviour>().isLocalPlayer) {
                localPlayer = player;
                updateTextField();
                return player;
            }
        }
        return null;
    }

    void setValues(float val) {
        if (localPlayer == null) {
            findLocalPlayer();
            print("Found local player:" + localPlayer);
        }
        //count += val;
        //value.text = count.ToString();
        //inputType.GetComponent<InputField>().text = count.ToString();
        if(inputType != null) {
            /*if(val <= 0 && localPlayer.GetComponent<syncHololensData>().destroy_speed <= 0.5f || val <= 0 && localPlayer.GetComponent<syncHololensData>().spawn_speed <= 0.5f) {
                return;
            }*/

            if(inputType.name == "SpawnSpeedInput") {

                localPlayer.GetComponent<syncHololensData>().spawn_speed += val;
                inputType.GetComponent<InputField>().text = localPlayer.GetComponent<syncHololensData>().spawn_speed.ToString();
            } if(inputType.name == "DestroySpeedInput") {
                localPlayer.GetComponent<syncHololensData>().destroy_speed += val;
                inputType.GetComponent<InputField>().text = localPlayer.GetComponent<syncHololensData>().destroy_speed.ToString();
            } if(inputType.name == "SpawnAmountInput") {
                if(val > 0f) {
                    localPlayer.GetComponent<syncHololensData>().spawn_amount += 1;
                } else {
                    localPlayer.GetComponent<syncHololensData>().spawn_amount -= 1;
                }
                inputType.GetComponent<InputField>().text = localPlayer.GetComponent<syncHololensData>().spawn_amount.ToString();
            }
        }
    }

    void setInputType() {
        if(inputType != null) {
            oldinputType = inputType;
        }
        //inputType = this.transform.name;
        inputType = this.gameObject;
        if (oldinputType == null) {
            oldinputType = inputType;
        }
        this.GetComponent<Image>().color = Color.red;
    }

    void clickButton(string button) {
        switch(button) {
            case "PlusButton":
                setValues(0.5f);
                break;
            case "MinusButton":
                setValues(-0.5f);
                break;
            case "SpawnSpeedInput":
                setInputType();
                break;
            case "DestroySpeedInput":
                setInputType();
                break;
            case "SpawnAmountInput":
                setInputType();
                break;
        }
    }

    public void updateTextField() {
        if(localPlayer != null) {
            //print(transform.name + ", " + localPlayer.GetComponent<syncHololensData>().spawn_speed.ToString() + " , " + localPlayer.GetComponent<syncHololensData>().destroy_speed.ToString());
            if(transform.name == "SpawnSpeedInput") {
                this.GetComponent<InputField>().text = localPlayer.GetComponent<syncHololensData>().spawn_speed.ToString();
            } if(transform.name == "DestroySpeedInput") {
                this.GetComponent<InputField>().text = localPlayer.GetComponent<syncHololensData>().destroy_speed.ToString();
            }
            //inputType.GetComponent<InputField>().text = localPlayer.GetComponent<syncHololensData>().spawn_amount.ToString();
        }
    }

    private bool selectable = false;
    private bool usingMouse = false; //Change to true if inputted through the PC instead of Hololens

    void Update() {
        if(usingMouse) {
            if(this.GetComponent<InputField>() != null && this.GetComponent<InputField>().isFocused && selectable == false) {
                selectable = true;
                handleClickEvent();
            } else if(this.GetComponent<InputField>() != null && !this.GetComponent<InputField>().isFocused && selectable == true) {
                selectable = false;
            }
        }
        //print(this.GetComponent<InputField>().isFocused);
        if(VRPlayer == null) {
            findVRPlayer();
        }
        if(localPlayer == null) {
            findLocalPlayer();
        }
        //updateTextField();


        //Handle VR Input
        if(VRPlayer != null && localPlayer != null) {
            AnimatedCursor cursor = localPlayer.GetComponent<cameraRigs>().AR_Rig.transform.Find("DefaultCursor").GetComponent<AnimatedCursor>();
            if(VRPlayer.GetComponent<syncTransformData>().viveARPressed && cursor.hitObject != null && cursor.hitObject.name == this.transform.name) {
                handleClickEvent();
                VRPlayer.GetComponent<syncTransformData>().viveARPressed = false;
            }
        }

        //print("CURR:"+inputType + " OLD:"+oldinputType);
        if (oldinputType != null && oldinputType != inputType) {
            if (oldinputType.name == this.transform.name) {
                this.GetComponent<Image>().color = Color.white;
                oldinputType = inputType;
            }
        }
    }

    public void handleClickEvent() {
        //if (this.GetComponent<Button>().high)
        Debug.Log("Clicked button:" + this.transform.name);
        clickButton(this.transform.name);
    }

    public virtual void OnInputClicked(InputClickedEventData eventData) {
        handleClickEvent();
        eventData.Use();
    }

}
