using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class UserAvatarLoader : NetworkBehaviour {

    public GameObject userAvatar;
    [SyncVar]
    public GameObject syncVarAvatar;
    public GameObject avatar;
    public string avatarID;
    private PlayerStorage playerStorage;
    public GameObject headParent;

    public GameObject headPrefab;

    [SyncVar]
    public string avatarName;

    private void Awake() {
        print("CODE:" + AvatarInfo.STORED_CODE);
        playerStorage = GameObject.Find("NetworkManager").GetComponent<PlayerStorage>();
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
        //CmdTest();
    }



    [ClientRpc]
    void RpcSyncVarWithClients(string varToSync) {
        avatarName = varToSync;
        //setupAvatar();
    }

    [Command]
    void CmdSyncVarWithClients(string varToSync) {
        RpcSyncVarWithClients(varToSync);
    }

    [ClientRpc]
    void RpcassignInfo() {
        userAvatar.tag = "networkedAvatar";
        userAvatar.name = AvatarInfo.STORED_CODE;
    }

    [Command]
    public void CmdTest() {
        userAvatar = Instantiate(headPrefab,
                        Vector3.zero,
                        new Quaternion(0f, 0f, 0f, 0f));
        RpcassignInfo();
        ClientScene.RegisterPrefab(userAvatar);
        NetworkServer.Spawn(userAvatar);
    }

    [ClientRpc]
    public void RpcSpawnHead() {
        print("Trying to spawn head");
        userAvatar = Instantiate(headPrefab,
                                Vector3.zero,
                                new Quaternion(0f, 0f, 0f, 0f));
        /*userAvatar.transform.SetParent(headParent.transform);
        //userAvatar.transform.localPosition = new Vector3(0f, -0.117f, -0.1f);
        userAvatar.transform.localPosition = new Vector3(0f, -0.117f, 1f);
        userAvatar.transform.localScale = new Vector3(1f, 1f, 1f);
        userAvatar.transform.localEulerAngles = Vector3.zero;*/
        userAvatar.name = avatarName;
        userAvatar.tag = "networkedAvatar";
        //userAvatar.transform.SetParent(headParent.transform); // Causes client crash?
        ClientScene.RegisterPrefab(userAvatar);
        NetworkServer.Spawn(userAvatar);
        syncVarAvatar = userAvatar;
        CmdUpdateData();
        //initializeHead();
    }

    public void setHeadParent() {
        if(userAvatar != null && userAvatar.transform.parent == null) {
            print("Setting head parent..");
            userAvatar.transform.SetParent(headParent.transform);
        }
    }

    [Command]
    public void CmdSpawnHead() {
        RpcSpawnHead();
    }



    [Command]
    public void CmdSpawnAvatar() {
        //RpcSpawnAvatar();
        userAvatar = Instantiate(avatar,
                                 Vector3.zero,
                                 new Quaternion(0f, 0f, 0f, 0f));
        userAvatar.name = avatar.name;
        userAvatar.AddComponent<NetworkIdentity>();
        userAvatar.AddComponent<NetworkTransform>();
        userAvatar.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        userAvatar.SetActive(true);
        userAvatar.transform.SetParent(headParent.transform);
        userAvatar.transform.localPosition = new Vector3(0f, -0.117f, -0.1f);
        userAvatar.transform.localScale = new Vector3(1f, 1f, 1f);
        userAvatar.transform.localEulerAngles = Vector3.zero;
        //RpcUpdateNetworkSpawn(userAvatar);
        //ClientScene.RegisterPrefab(userAvatar);
        //resetOrientation();
        //CmdSyncVarWithClients(userAvatar.name);
        ClientScene.RegisterPrefab(userAvatar);
        NetworkServer.Spawn(userAvatar);
        //print("Spawned User Avatar at:" + userAvatar.transform);
    }

    /*[ClientRpc]
    public void RpcSpawnAvatar() {
        userAvatar = Instantiate(avatar,
                         Vector3.zero,
                         new Quaternion(0f, 0f, 0f, 0f));
        userAvatar.name = avatar.name;
        userAvatar.AddComponent<NetworkIdentity>();
        userAvatar.AddComponent<NetworkTransform>();
        userAvatar.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        userAvatar.SetActive(true);
        userAvatar.transform.SetParent(headParent.transform);
        //userAvatar.transform.localPosition = new Vector3(0f, -0.117f, -0.1f);
        userAvatar.transform.localPosition = new Vector3(0f, -0.117f, 1f);
        userAvatar.transform.localScale = new Vector3(1f, 1f, 1f);
        userAvatar.transform.localEulerAngles = Vector3.zero;
        //RpcUpdateNetworkSpawn(userAvatar);
        //ClientScene.RegisterPrefab(userAvatar);
        //resetOrientation();
        //CmdSyncVarWithClients(userAvatar.name);
        NetworkServer.Spawn(userAvatar);
    }*/

    [ClientRpc]
    public void RpcUpdateNetworkSpawn(GameObject avatar) {
        playerStorage.spawnPrefabs.Add(avatar);
        ClientScene.RegisterPrefab(avatar);
    }

    public GameObject Find(GameObject objToFind, GameObject[] gameObjects) {
        foreach(GameObject obj in gameObjects) {
            if(objToFind.name == obj.name) {
                return obj;
            }
        }
        return null;
    }

    /*public void setTags() {
        foreach (GameObject obj in FindObjectsOfType(typeof(GameObject))) {
            if (obj.name == "HeadPrefab(Clone)") {
                obj.tag = "";
            }
        }
    }*/

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

        //assignParent();
    }

    IEnumerator Wait(float duration) {
        //This is a coroutine
        Debug.Log("Start Wait() function. The time is: " + Time.time);
        Debug.Log("Float duration = " + duration);
        userAvatar.transform.SetParent(this.transform);
        //userAvatar.transform.SetParent();
        //Transform rig = this.transform.Find("VRSimulator").GetComponent<cameraController>().cam.transform;
        //print("rig:" + rig);
        yield return new WaitForSeconds(duration);   //Wait
        Debug.Log("End Wait() function and the time is: " + Time.time);
        Transform rigg = this.transform.Find("VRSimulator").GetComponent<cameraController>().cam.transform;
        userAvatar.transform.SetParent(rigg);
        userAvatar.transform.localPosition = Vector3.zero;
        userAvatar.transform.localEulerAngles = Vector3.zero;
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
        if(NetworkServer.connections.Count >= 2 && isServer && !parentSet) {
            print("Setting parent..");
            StartCoroutine(Wait(2));
        }

        /*if(isClient && avatarName != "") {
            setupAvatar();
        }*/

        //print(NetworkServer.connections.Count);
        if(isLocalPlayer && AvatarInfo.STORED_CODE != null) {
            CmdSyncVarWithClients(AvatarInfo.STORED_CODE);
            if(userAvatar != null && isServer) {
                userAvatar.GetComponent<avatarData>().CmdSyncVarWithClients(avatarName);
                userAvatar.GetComponent<avatarData>().CmdSyncNetworkChange(this.GetComponent<NetworkIdentity>().netId.ToString());
            }
        }

        if(isLocalPlayer && !findAvatar) {
            print("Spawned a head?");
            findAvatar = true;
            CmdSpawnHead();
        }
    }

}