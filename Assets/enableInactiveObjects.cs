using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enableInactiveObjects : MonoBehaviour
{

    GameObject[] FindInActiveObjectsByTag(string tag) {
        List<GameObject> validTransforms = new List<GameObject>();
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for(int i = 0; i < objs.Length; i++) {
            if(objs[i].hideFlags == HideFlags.None) {
                if(objs[i].gameObject.CompareTag(tag)) {
                    validTransforms.Add(objs[i].gameObject);
                }
            }
        }
        return validTransforms.ToArray();
    }

    void Start() {
        GameObject[] objects = FindInActiveObjectsByTag("avatar");
        print("Inactive objects:" + objects.Length);
        foreach (GameObject obj in objects) {
            obj.SetActive(true);
        }
    }
}
