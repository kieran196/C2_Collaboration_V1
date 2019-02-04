using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLoadScreen : MonoBehaviour {

    public GameObject netManager;
	
	public void HideLoadScreen() {
		gameObject.SetActive(false);
        netManager.SetActive(true);
	}
	
	IEnumerator loadHeads(float duration) {
		yield return new WaitForSeconds(duration);
		HideLoadScreen();
	}
	
	void Start() {
		StartCoroutine(loadHeads(2f));
	}
	
}
