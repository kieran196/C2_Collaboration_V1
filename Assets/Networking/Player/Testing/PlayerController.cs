using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {
    public TextMesh playerText;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    public GameObject Sphere;
    public GameObject Cube;
    public bool VRActivated = false;
    private NetworkHandler netHandler;
    //public eventHandler events;

    private void Awake() {
        netHandler = GameObject.Find("NetworkManager").GetComponent<NetworkHandler>();
    }

    private void Start() {
        //events.spaceDown.AddListener(CmdFire);
        //playerText.text = "Player:" + netId;
        this.name = playerText.text;
        netHandler.playerObjects.Add(this.gameObject);
        netHandler.updatePerspective(this.gameObject);
        //camera.targetDisplay = 1;
    }

    [Command]
    public void CmdFire() {
        print("Fired bullet");
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);
        bullet.SetActive(true);
        ClientScene.RegisterPrefab(bullet);
        /*foreach(var prefab in m_SpawnPrefabs) {
            if(prefab != null) {
                ClientScene.RegisterPrefab(bullet);
            }
        }*/

        NetworkServer.Spawn(bullet);
        Destroy(bullet, 10f);
    }

    [Command]
    public void CmdSwitchClient() {
        print("VR ACTIVED :: " + VRActivated);
        bulletSpawn.gameObject.SetActive(!bulletSpawn.gameObject.activeInHierarchy);
        //VRActivated = !VRActivated;
        //Sphere.SetActive(!VRActivated);
        //Cube.SetActive(VRActivated);
    }

    void Update() {
        if(!isLocalPlayer) {
            return;
        } else {
            Camera cam = this.GetComponentInChildren<Camera>();
            if (cam != null && !cam.enabled) {
                cam.enabled = true;
                print("Enabled camera for:" + this.name);
            }
        }
    }

    public override void OnStartLocalPlayer() {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    

}