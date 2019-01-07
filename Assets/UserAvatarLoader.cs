using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UserAvatarLoader : NetworkBehaviour {

    public GameObject userAvatar;
    private PlayerStorage playerStorage;

    private void Awake() {
        playerStorage = GameObject.Find("NetworkManager").GetComponent<PlayerStorage>();
        AvatarInfo.STORED_AVATAR.SetActive(true);
    }

    public void resetOrientation() {
        userAvatar.transform.localEulerAngles = Vector3.zero;
        userAvatar.transform.localPosition = Vector3.zero;
        userAvatar.SetActive(true);
    }

    public override void OnStartClient() {
        
    }

    [Command]
    public void CmdSpawnAvatar() {
        userAvatar = (GameObject)Instantiate(
                      AvatarInfo.STORED_AVATAR,
                      Vector3.zero,
                      new Quaternion(0f, 0f, 0f, 0f));
        userAvatar.AddComponent<NetworkIdentity>();
        userAvatar.AddComponent<NetworkTransform>();
        userAvatar.transform.SetParent(this.transform);
        RpcUpdateNetworkSpawn(userAvatar);
        //ClientScene.RegisterPrefab(userAvatar);
        if(isServer)
            FindObjectOfType<NetworkManager>().spawnPrefabs.Add(userAvatar);
        ClientScene.RegisterPrefab(userAvatar);
        resetOrientation();
        NetworkServer.Spawn(userAvatar);
        print("Spawned User Avatar at:" + userAvatar.transform);
    }

    [ClientRpc]
    public void RpcUpdateNetworkSpawn(GameObject avatar) {
        playerStorage.spawnPrefabs.Add(avatar);
        ClientScene.RegisterPrefab(avatar);
    }

    // Use this for initialization
    void Start () {
		if (AvatarInfo.STORED_AVATAR != null) {
            print("Successfully created avatar:" + AvatarInfo.STORED_AVATAR);
            playerStorage.spawnPrefabs.Add(AvatarInfo.STORED_AVATAR);
            ClientScene.RegisterPrefab(AvatarInfo.STORED_AVATAR);
            CmdSpawnAvatar();
        } else {
            print("Unable to generate avatar...");
        }
	}
	
	// Update is called once per frame
	void Update () {
        print(AvatarInfo.STORED_AVATAR + ", "+userAvatar);
	}
}
