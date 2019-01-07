using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UserAvatarLoader : MonoBehaviour {

    public GameObject userAvatar;

    private void Awake() {
        //DontDestroyOnLoad(AvatarInfo.STORED_AVATAR);
    }

    public void resetOrientation() {
        userAvatar.transform.localEulerAngles = Vector3.zero;
        userAvatar.transform.localPosition = Vector3.zero;
        userAvatar.SetActive(true);
    }

    public void CmdSpawnAvatar() {
        userAvatar = (GameObject)Instantiate(
                      AvatarInfo.STORED_AVATAR,
                      Vector3.zero,
                      new Quaternion(0f, 0f, 0f, 0f));
        userAvatar.AddComponent<NetworkIdentity>();
        userAvatar.AddComponent<NetworkTransform>();
        userAvatar.transform.SetParent(this.transform);
        //resetOrientation();
        NetworkServer.Spawn(userAvatar);
    }

    // Use this for initialization
    void Start () {
		if (AvatarInfo.STORED_AVATAR != null) {
            CmdSpawnAvatar();
            print("Successfully created avatar:" + AvatarInfo.STORED_AVATAR);
        } else {
            print("Unable to generate avatar...");
        }
	}
	
	// Update is called once per frame
	void Update () {
        //print(AvatarInfo.STORED_AVATAR);
	}
}
