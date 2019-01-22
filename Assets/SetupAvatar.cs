using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SetupAvatar : NetworkBehaviour {

    [SyncVar]
    public GameObject userAvatar;
    public GameObject headPrefab;

    [SyncVar]
    public string avatarName;

    [Command]
    public void CmdSpawnHead() {
        userAvatar = Instantiate(headPrefab,
        Vector3.zero,
        new Quaternion(0f, 0f, 0f, 0f));
        ClientScene.RegisterPrefab(userAvatar);
        NetworkServer.Spawn(userAvatar);
    }

    [Command]
    public void CmdUpdateName() {
        userAvatar.GetComponent<avatarData>().CmdSyncVarWithClients("test");
    }

    private bool spawned = false;

    void Update() {
        if(isLocalPlayer && spawned == false && userAvatar != null) {
            CmdSpawnHead();
            CmdUpdateName();
            spawned = true;
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        CmdSpawnHead();
    }


}
