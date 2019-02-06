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

    private GameObject findLocalPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            //print(player.GetComponent<NetworkBehaviour>().isLocalPlayer);
            if (player.GetComponent<NetworkBehaviour>().isLocalPlayer) {
                localPlayer = player;
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

    void Start() {

    }

    void Update() {
        //print("CURR:"+inputType + " OLD:"+oldinputType);
        if (oldinputType != null && oldinputType != inputType) {
            if (oldinputType.name == this.transform.name) {
                this.GetComponent<Image>().color = Color.white;
                oldinputType = inputType;
            }
        }
    }

    public virtual void OnInputClicked(InputClickedEventData eventData) {
        Debug.Log("Clicked button:"+this.transform.name);
        //count = (this.transform.name == "PlusButton") ? count = count+1 : (this.transform.name == "MinusButton") ? count-- : count; ;
        clickButton(this.transform.name);
        //clickButton(this.transform.name);
        eventData.Use();
    }

}
