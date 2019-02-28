using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

/// ===============================
/// AUTHOR: Kieran William May
/// PURPOSE: This class is responsible for handling synchronization of 3D avatars between the server-client
/// NOTES:
/// 
/// ===============================

public class UserAvatarLoader : NetworkBehaviour {

    public GameObject userAvatar;

    [SyncVar]
    public GameObject syncVarAvatar;
    public GameObject avatar;
    public string avatarID;
    public GameObject headParent;

    public GameObject headPrefab;
    public bool hideHead = false; // Make the head invisible to the local player (So they can see better)

    [SyncVar]
    public string avatarName;

    private void Awake() {
        print("CODE:" + AvatarInfo.STORED_CODE);
        if(AvatarInfo.STORED_AVATAR != null) {
            AvatarInfo.STORED_AVATAR.SetActive(false);
        }
        /*foreach(GameObject obj in AvatarInfo.AVATARS) {
            obj.SetActive(true);
            playerStorage.spawnPrefabs.Add(obj);
        }*/
    }

    public void resetOrientation() {
        userAvatar.transform.localEulerAngles = Vector3.zero;
        userAvatar.transform.localPosition = Vector3.zero;
        userAvatar.SetActive(true);
    }

    public override void OnStartClient() {
        base.OnStartClient();
        CmdSyncVarWithClients(AvatarInfo.STORED_CODE);
        //CmdSpawnHead();
    }



    [ClientRpc]
    void RpcSyncVarWithClients(string varToSync) {
        avatarName = varToSync;
    }

    [Command]
    void CmdSyncVarWithClients(string varToSync) {
        RpcSyncVarWithClients(varToSync);
    }

    [ClientRpc]
    public void RpcSpawnHead() {
        print("Trying to spawn head");
        userAvatar = Instantiate(headPrefab,
                                Vector3.zero,
                                new Quaternion(0f, 0f, 0f, 0f));
        userAvatar.name = avatarName;
        userAvatar.tag = "networkedAvatar";
        //userAvatar.transform.SetParent(headParent.transform); // Causes client crash?
        ClientScene.RegisterPrefab(userAvatar);
        NetworkServer.Spawn(userAvatar);
        syncVarAvatar = userAvatar;
        CmdUpdateData();
        //initializeHead();
    }

    [Command]
    public void CmdSpawnHead() {
        print("Calling RpcSpawnHead");
        RpcSpawnHead();
    }

    public GameObject Find(GameObject objToFind, GameObject[] gameObjects) {
        foreach(GameObject obj in gameObjects) {
            if(objToFind.name == obj.name) {
                return obj;
            }
        }
        return null;
    }

    [Command]
    public void CmdUpdateData() {
        RpcinitializeHead();
    }


    private GameObject[] FindInActiveObjectsByTag(string tag) {
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

    [ClientRpc]
    public void RpcinitializeHead() {
        //GameObject[] findByName = GameObject.Find("")
        print("Initializing head..");
        GameObject[] allAvatars = GameObject.FindGameObjectsWithTag("avatar");
        GameObject[] spawnedAvatars = FindInActiveObjectsByTag("networkedAvatar");
        print("LISTS:" + allAvatars.Length + " , " + spawnedAvatars.Length);
        print("Set avatar name:" + avatarName);
        userAvatar.name = avatarName;
        foreach(GameObject head in spawnedAvatars) {
            GameObject obj = Find(head, allAvatars);
            SkinnedMeshRenderer newRend = head.GetComponent<SkinnedMeshRenderer>();
            print("SkinnedMeshRenderer added to:" + head.name);
            if(obj != null) {
                SkinnedMeshRenderer avatarRend = obj.GetComponentInChildren<SkinnedMeshRenderer>();
                newRend.sharedMesh = avatarRend.sharedMesh;
                newRend.material = avatarRend.material;
            }
            head.GetComponent<avatarData>().assignParentHost();
        }
        if(!isServer) {
            userAvatar.GetComponent<avatarData>().assignParent(gameObject);
        } else {
            //StartCoroutine(Wait(4));
            //userAvatar.transform.SetParent(rig);
        }

        /*if(avatarName == "" || avatarName.Length <= 1) {
            Destroy(this.gameObject);
            return;
        }*/
        //assignParent();
    }

    IEnumerator Wait(float duration) {
        //This is a coroutine
        Debug.Log("Start Wait() function. The time is: " + Time.time);
        Debug.Log("Float duration = " + duration);
        yield return new WaitForSeconds(duration);   //Wait
        Debug.Log("End Wait() function and the time is: " + Time.time);
        Transform rigg = this.transform.Find("SteamVR").GetComponent<cameraController>().avatarHead.transform;
        userAvatar.transform.SetParent(rigg);
        userAvatar.transform.localPosition = Vector3.zero;
        userAvatar.transform.localEulerAngles = Vector3.zero;
        //Make the head invisible to the local player
        if (hideHead && isLocalPlayer) {
            userAvatar.SetActive(false);
        }
        print("rig:" + rigg);
        parentSet = true;
    }


    private bool findAvatar = false;

    public void setupAvatar() {
        if(avatarName != null && avatar == null && isLocalPlayer && findAvatar == false) {
            findAvatar = true;
            avatar = GameObject.Find(AvatarInfo.STORED_CODE);
            bool foundAvatar = (avatar == null ? false : true);
            print(avatar == null ? "Couldn't find avatar:" + AvatarInfo.STORED_CODE : "Successfully found avatar..");
            GameObject[] allAvatars = GameObject.FindGameObjectsWithTag("avatar");
            print("LENGTH:" + allAvatars.Length);
            if(foundAvatar && avatar != null) {
                print("Successfully created avatar:" + AvatarInfo.STORED_AVATAR);
                //playerStorage.spawnPrefabs.Add(avatar);
                //ClientScene.RegisterPrefab(avatar);
                CmdSpawnHead();
            }
        }
    }

    private bool parentSet = false;
    // Update is called once per frame
    void Update() {
        if(NetworkServer.connections.Count >= 2 && isServer && !parentSet && userAvatar != null) {
            print("Setting parent..");
            StartCoroutine(Wait(2));
        }

        //print(NetworkServer.connections.Count);
        if(isLocalPlayer && AvatarInfo.STORED_CODE != null) {
            CmdSyncVarWithClients(AvatarInfo.STORED_CODE);
            if(userAvatar != null && isServer) {
                userAvatar.GetComponent<avatarData>().CmdSyncVarWithClients(avatarName);
                userAvatar.GetComponent<avatarData>().CmdSyncNetworkChange(this.GetComponent<NetworkIdentity>().netId.ToString());
            }
        }

        if (isLocalPlayer && !findAvatar) {
            print("Spawned a head?");
            findAvatar = true;
            CmdSpawnHead();
        }
    }

}