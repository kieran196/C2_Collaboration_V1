using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
	void Start ()
    {
        StartCoroutine(DelayDestroy());
	}

    IEnumerator DelayDestroy()
    {
        yield return null;
        yield return null;
        Destroy(gameObject);
    }
}
