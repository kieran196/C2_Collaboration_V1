using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class boxCollision : NetworkBehaviour {

    /// ===============================
    /// AUTHOR: Kieran William May
    /// PURPOSE: This class is handles the core functionality of the box-selection game
    /// NOTES:
    /// 
    /// ===============================

    private float globalTimer = 0;

    public float SPAWN_SPEED; //Spawns an object for every "" seconds
    public float DESTROY_SPEED; //Destroys an object for every "" seconds
    public int SPAWN_AMOUNT; //Spawns * objects at a time
    public int BLOCK_SPAWN_LIMIT; //How many blocks will be spawned (e.g 50 will spawn a total of 50 blocks)

    public GameObject popupPrefab;
    private Transform cubeParent;

    public float TASK_DIFFICULTY;

    /// <summary>
    /// The three tasks measured:
    /// 1. DEFAULT_VALUES = The Spawn/Destroy speed stay at a constant value throughout the task
    /// 2. HOLOLENS_MANIPULATION_FULL = The Hololens user manipulates the task difficulty using HR activity & other relevant data
    /// 3. HOLOLENS_MANIPULATION_LIMITED = The hololens user manipulations the task difficulty without any data
    /// </summary>
    public enum TASK_TYPE {DEFAULT_VALUES, HOLOLENS_MANIPULATION_FULL, HOLOLENS_MANIPULATION_LIMITED}
    public TASK_TYPE Task_Type;

    private csvWriter csvWriter;
    private int blocksSpawned = 0;

    private float[,] positions = new float[,] { {-1f, 0.5f}, { -0.35f, 0.5f } , { 0.35f, 0.5f }, { 1f, 0.5f },
                                                {-1f, -0.175f}, { -0.35f, -0.175f } , { 0.35f, -0.175f }, { 1f, -0.175f},
                                                {-1f, 0.175f}, { -0.35f, 0.175f } , { 0.35f, 0.175f }, { 1f, 0.175f},
                                                {-1f, -0.5f}, { -0.35f, -0.5f } , { 0.35f, -0.5f }, { 1f, -0.5f }};
    private List<int> activePositions = new List<int>();

    //public GameObject selectedCube;
    public List<GameObject> activeCubes = new List<GameObject>();

    void Start() {
        cubeParent = GameObject.Find("CubeParent").transform;
        csvWriter = GameObject.Find("Debugger").GetComponent<csvWriter>();
        originalSpawnSpeed = SPAWN_SPEED;
        originalDestroySpeed = DESTROY_SPEED;
        originalSpawnAmount = SPAWN_AMOUNT;
        originalSpawnLimit = BLOCK_SPAWN_LIMIT;
    }

    private float reactionRate = 0;

    public void setSpawnSpeed(float speed) {
        SPAWN_SPEED = speed;
    }

    public void setDestroySpeed(float speed) {
        DESTROY_SPEED = speed;
    }

    public void setSpawnAmount(int amount) {
        SPAWN_AMOUNT = amount;
    }

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
        blocksSpawned++;
        print("Created new object:"+obj.name);
        CmdspawnCube(obj, randomNum);
        return obj;
    }


    public SteamVR_TrackedObject trackedObjL;
    public SteamVR_TrackedObject trackedObjR;
    public GameObject trackedObjH;

    //Param - True - User successfully selected a block, False - The block disappeared before the user could select it
    public void collectData(bool successful) {
        string HR = GetComponent<readPythonData>().currData;
        //HR, Movement, Reaction Rate, Difficulty (SPAWN, DESTROY, AMOUNT)
        //TASK, OVERALL TIME, SELECTED BLOCK?, BLOCK COUNT, SPAWN SPEED, DESTROY SPEED, AMOUNT SPAWNED, HEART RATE, REACTION RATE, Movement (Head, Left/Right Hand)
        float distL = trackedObjL.GetComponent<CountDistance>().totalDistance; //Left hand distance
        float distR = trackedObjR.GetComponent<CountDistance>().totalDistance; //Right hand distance
        float distH = trackedObjH.GetComponent<CountDistance>().totalDistance; //Head distance
        if(successful) {
            csvWriter.WriteLine(Task_Type+", " + Mathf.RoundToInt(globalTimer) + ", " + "TRUE, " + blocksSpawned + ", " + SPAWN_SPEED + ", " + DESTROY_SPEED + ", " + SPAWN_AMOUNT + ", " + HR + ", " + reactionRate + ", " + distH + ", " + distL + ", " + distR);
        } else {
            csvWriter.WriteLine(Task_Type + ", " + Mathf.RoundToInt(globalTimer) + ", " + "FALSE, " + blocksSpawned + ", " + SPAWN_SPEED + ", " + DESTROY_SPEED + ", " + SPAWN_AMOUNT + ", " + HR + ", " + "NULL" + ", " + distH + ", " + distL + ", " + distR);
        }
        //Resetting data
        trackedObjL.GetComponent<CountDistance>().resetProperties();
        trackedObjR.GetComponent<CountDistance>().resetProperties();
        trackedObjH.GetComponent<CountDistance>().resetProperties();
        reactionRate = 0;
    }

    public void onSelect(GameObject obj) {
        print("Selected object:" + obj.name);
        collectData(true);

        activePositions.Remove(int.Parse(obj.name));
        activeCubes.Remove(obj);
        Destroy(obj.gameObject);
        if(blocksSpawned == BLOCK_SPAWN_LIMIT) {
            resetTask = true;
        }
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
        if((int)(secondCounter % SPAWN_SPEED) == 0 && blocksSpawned < BLOCK_SPAWN_LIMIT) {
            print(blocksSpawned);
            for(int i = 0; i < SPAWN_AMOUNT; i++)
                activeCubes.Add(randomlySelectCube());
        } else if((int)(secondCounter % SPAWN_SPEED) == 0 && blocksSpawned == BLOCK_SPAWN_LIMIT && resetTask == true) {
            ResetTask();
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
                collectData(false);
                if (blocksSpawned == BLOCK_SPAWN_LIMIT) {
                    resetTask = true;
                }
                if(increaseDifficulty && hasStarted()) {
                    activeCubes.Add(randomlySelectCube());
                }
                //currentDifficulty++;

            }
        }
    }

    private float originalSpawnSpeed;
    private float originalDestroySpeed;
    private int originalSpawnAmount;
    private int originalSpawnLimit;
    private bool resetTask = false;

    void ResetTask() {
        print("Task has been reset automatically..");
        if(csvWriter.logData) {
            csvWriter.writeToFile();
        }
        //Resetting everything
        started = false;
        globalTimer = 0f;
        activePositions = new List<int>();
        activeCubes = new List<GameObject>();
        blocksSpawned = 0;
        secondCounter = 0;
        SPAWN_SPEED = originalSpawnSpeed; 
        DESTROY_SPEED = originalDestroySpeed;
        SPAWN_AMOUNT = originalSpawnAmount;
        BLOCK_SPAWN_LIMIT = originalSpawnLimit;
        trackedObjL.GetComponent<CountDistance>().counting = false;
        trackedObjR.GetComponent<CountDistance>().counting = false;
        trackedObjH.GetComponent<CountDistance>().counting = false;
        resetTask = false;
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
            if(globalTimer == 0 && hasStarted()) {

                //Start counting movement speed
                trackedObjL.GetComponent<CountDistance>().counting = true;
                trackedObjR.GetComponent<CountDistance>().counting = true;
                trackedObjH.GetComponent<CountDistance>().counting = true;

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
