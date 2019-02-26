using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableAvatar : MonoBehaviour {

    private void Awake() {
        GameObject world = GameObject.FindGameObjectWithTag("avatarWorld");
        if (world == null) {
            gameObject.SetActive(false);
        }
    }
}
