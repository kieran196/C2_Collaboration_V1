using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;


public class spawnSceneObject : NetworkBehaviour {

    public GameObject cyllinderPrefab;

    private void Awake() {
        //DontDestroyOnLoad(this);
    }

    public override void OnStartServer() {
        print("Server has started..");
        var obj = Instantiate(cyllinderPrefab);
        NetworkServer.Spawn(obj);
    }
}
