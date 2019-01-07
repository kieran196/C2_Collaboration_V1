using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerStorage : NetworkManager {

    public static List<string> players = new List<string>();

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        print("Spawned a new player.. ... ..");
        
    }

    public List<GameObject> playerObjects = new List<GameObject>();

	public void updatePerspective(GameObject currPlayer) {
        foreach (GameObject player in playerObjects) {
            Camera cam = player.GetComponentInChildren<Camera>();
            cam.enabled = false;
        }
    }

    public void Update() {
        //print(players.Count);
    }

}
