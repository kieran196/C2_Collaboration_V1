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
    private PlayerStorage playerStorage;
    public eventHandler events;

    private void Awake() {
        playerStorage = GameObject.Find("NetworkManager").GetComponent<PlayerStorage>();
    }

    private void Start() {
        events.spaceDown.AddListener(CmdFire);
        playerText.text = "Player:" + netId;
        this.name = playerText.text;
        playerStorage.playerObjects.Add(this.gameObject);
        playerStorage.updatePerspective(this.gameObject);
        //camera.targetDisplay = 1;
    }

    [Command]
    public void CmdFire() {
        print("Fired bullet");
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6f;
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2f);
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
            if (!cam.enabled) {
                cam.enabled = true;
                print("Enabled camera for:" + this.name);
            }
        }
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
        if(Input.GetKeyDown(KeyCode.Space)) {
           events.spaceDown.Invoke();
           // events.spaceDown.
           // CmdFire();
            print("Firing bullet..");
            //CmdSwitchClient();
        }
    }

    public override void OnStartLocalPlayer() {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    

}