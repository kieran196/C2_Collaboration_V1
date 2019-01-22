using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class boxCollision : NetworkBehaviour {


    public float globalTimer = 0;

    public float SPAWN_SPEED; //Spawns an object for every "" seconds
    public float DESTROY_SPEED; //Destroys an object for every "" seconds
    public int SPAWN_AMOUNT; //Spawns * objects at a time

    public GameObject popupPrefab;
    private Transform cubeParent;

    public float TASK_DIFFICULTY;

    private float[,] positions = new float[,] { {-1f, 0.5f}, { -0.35f, 0.5f } , { 0.35f, 0.5f }, { 1f, 0.5f },
                                                {-1f, -0.175f}, { -0.35f, -0.175f } , { 0.35f, -0.175f }, { 1f, -0.175f},
                                                {-1f, 0.175f}, { -0.35f, 0.175f } , { 0.35f, 0.175f }, { 1f, 0.175f},
                                                {-1f, -0.5f}, { -0.35f, -0.5f } , { 0.35f, -0.5f }, { 1f, -0.5f }};
    private List<int> activePositions = new List<int>();

    //public GameObject selectedCube;
    public List<GameObject> activeCubes = new List<GameObject>();

    public List<float> reactionRates = new List<float>();

    void Start() {
        cubeParent = GameObject.Find("CubeParent").transform;
    }



    private float reactionRate = 0;

    [Command]
    public void CmdspawnCube(GameObject obj, int randomNum) {
        obj.name = randomNum.ToString();
        obj.transform.SetParent(cubeParent);
        obj.transform.localPosition = new Vector3(positions[randomNum, 0], 0.55f, positions[randomNum, 1]);
        obj.transform.localScale = new Vector3(0.5f, 0.15f, 0.25f);
        reactionRate = 0;
        NetworkServer.Spawn(obj);
        print("Spawned object..");
    }

    public GameObject randomlySelectCube() {
        if (activePositions.Count == positions.Length/2) { //Already full..
            print("Already full!" + activePositions.Count + " , " + positions.Length / 2);
            return null;
        }
        int randomNum = Random.Range(0, positions.Length/2);
        while (activePositions.Contains(randomNum)) {
            randomNum = Random.Range(0, positions.Length/2);
        }
        activePositions.Add(randomNum);
        GameObject obj = Instantiate(popupPrefab);
        print("Created new object:"+obj.name);
        CmdspawnCube(obj, randomNum);
        return obj;
    }


    public void initializeCube() {

    }

    public void onSelect(GameObject obj) {
        print("Selected object:" + obj.name);
        reactionRates.Add(reactionRate);
        reactionRate = 0;
        activePositions.Remove(int.Parse(obj.name));
        activeCubes.Remove(obj);
        Destroy(obj.gameObject);
        currentDifficulty += 0.1f;
        for (int i=0; i<(int)currentDifficulty&&increaseDifficulty; i++)
            activeCubes.Add(randomlySelectCube());
    }

    private int secondCounter = 0;
    private float currentDifficulty = 1;
    public bool increaseDifficulty = true;
    public bool started = false;

    public void addSecond() {
        secondCounter = (int)globalTimer;
        if ((int)(secondCounter % SPAWN_SPEED) == 0) {
            for(int i = 0; i < SPAWN_AMOUNT; i++)
                activeCubes.Add(randomlySelectCube());
        }
    }

    public void startApp() {
        started = true;
    }

    public bool hasStarted() {
        return started;
    }


    [Command]
    public void CmdSyncCubes() {
        foreach(GameObject cube in activeCubes.ToArray()) {
            cube.GetComponent<SyncTransperancy>().CmdSyncVarWithClients(cube.GetComponent<Renderer>().material.color.a - (Time.deltaTime / DESTROY_SPEED));

            cube.GetComponent<Renderer>().material.color = new Color(cube.GetComponent<Renderer>().material.color.r, cube.GetComponent<Renderer>().material.color.g, cube.GetComponent<Renderer>().material.color.b, cube.GetComponent<Renderer>().material.color.a - (Time.deltaTime / DESTROY_SPEED));
            cube.GetComponentInChildren<Text>().text = cube.GetComponent<Renderer>().material.color.a.ToString();
            //cube.GetComponent<SyncTransperancy>().colorAlpha = cube.GetComponent<Renderer>().material.color.a;
            //print(selectedCube.GetComponent<Renderer>().material.color.a + " | " + globalTimer);
            if(cube.GetComponent<Renderer>().material.color.a <= 0) {
                activePositions.Remove(int.Parse(cube.name));
                activeCubes.Remove(cube);
                Destroy(cube.gameObject);
                if (increaseDifficulty)
                    activeCubes.Add(randomlySelectCube());
                //currentDifficulty++;

            }
        }
    }


    void Update() {
        TASK_DIFFICULTY = (100f / (SPAWN_SPEED + DESTROY_SPEED)) * SPAWN_AMOUNT;
        if(!isLocalPlayer)
            return;
            //controller = SteamVR_Controller.Input((int)trackedObj.index);
        if(hasStarted()) {
            if((int)globalTimer != secondCounter) {
                addSecond();
            }
            if(globalTimer == 0) {
                print("Spawn first item..");
                activeCubes.Add(randomlySelectCube());
                /*if(increaseDifficulty) {
                    if(currentDifficulty > 10)
                        activeCubes.Add(randomlySelectCube());
                    if(currentDifficulty > 20)
                        activeCubes.Add(randomlySelectCube());
                }*/
            }
            if(activeCubes.Count >= 0) {
                reactionRate += Time.deltaTime;
                CmdSyncCubes();
                /*foreach(GameObject cube in activeCubes.ToArray()) {
                    cube.GetComponent<Renderer>().material.color = new Color(cube.GetComponent<Renderer>().material.color.r, cube.GetComponent<Renderer>().material.color.g, cube.GetComponent<Renderer>().material.color.b, cube.GetComponent<Renderer>().material.color.a - (Time.deltaTime / DESTROY_SPEED));
                    cube.GetComponentInChildren<Text>().text = cube.GetComponent<Renderer>().material.color.a.ToString();
                    cube.GetComponent<SyncTransperancy>().color = cube.GetComponent<Renderer>().material.color;
                    //print(selectedCube.GetComponent<Renderer>().material.color.a + " | " + globalTimer);
                    if(cube.GetComponent<Renderer>().material.color.a <= 0) {
                        activePositions.Remove(int.Parse(cube.name));
                        activeCubes.Remove(cube);
                        Destroy(cube.gameObject);
                        //activeCubes.Add(randomlySelectCube());
                        currentDifficulty++;

                        /*
                        print("---- ANALAYSIS ----");
                        int count = 0;
                        foreach(float RRate in reactionRates) {
                            count++;
                            print("Obj: " + count + " | Reaction Rate:" + RRate);
                        }
                        
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif*/

                  //  }
                //}
            }
            globalTimer += Time.deltaTime;
        }
    }
    

}
