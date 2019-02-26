using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableAvatar : MonoBehaviour {
    // Start is called before the first frame update
    void Awake() {
        GameObject world = GameObject.FindGameObjectWithTag("avatarWorld");
        if (world == null) {
            gameObject.SetActive(false);
        }
    }

}
