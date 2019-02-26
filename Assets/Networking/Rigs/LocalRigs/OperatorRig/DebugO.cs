using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugO : MonoBehaviour {

    public Text debugText;

    public void Out(string message) {
        Debug.Log(message);
        debugText.text = message+"\n";
    }

}
