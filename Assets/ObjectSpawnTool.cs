using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectSpawnTool : NetworkBehaviour {

    [Command]
    public void CmdCreateObject(Vector3 hitPoint, GameObject obj) {
        //GameObject obj = Instantiate(prefab);
        //obj.transform.localPosition = hitPoint;
        obj.layer = LayerMask.NameToLayer("InteractableGameObjectGen");
        ClientScene.RegisterPrefab(obj);
        NetworkServer.Spawn(obj);
    }

}