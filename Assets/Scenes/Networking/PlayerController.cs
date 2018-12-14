using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {
    public TextMesh playerText;
    public Camera camera;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    private void Start() {
        playerText.text = "Player:" + netId;
        //camera.targetDisplay = 1;
    }

    [Command]
    public void CmdFire() {

        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6f;
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2f);
    }

    void Update() {
        /*if(!isLocalPlayer) {
            return;
        }*/
        /*var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);*/
        if(Input.GetKeyDown(KeyCode.Space)) {
            CmdFire();
        }
    }

    public override void OnStartLocalPlayer() {
        //GetComponent<MeshRenderer>().material.color = Color.blue;
    }

}