using UnityEngine;
using System.Collections;

public enum MultiPlayerMode {
	ServerAndClient, //Currently not sure if can functional properly
    Server, //Currently not sure if can functional properly
    Client,
	Host,
    Singleplayer //Same as host
}

public enum VRTrackerType
{
    Head,
    LeftHand,
    RightHand,
    Root,
    LeftFoot,
    RightFoot,
}

[System.Serializable]
public class SixPointTrackingOptions
{
    public bool trackHead = false; //Initial value is important
    public bool trackLeftHand = false;
    public bool trackRightHand = false;
    public bool trackRoot = false;
    public bool trackLeftFoot = false;
    public bool trackRightFoot = false;

    public SixPointTrackingOptions GetShallowCopy()
    {
        return (SixPointTrackingOptions)this.MemberwiseClone();
    }
}

public class ThreePointTrackingSettings : MonoBehaviour
{
    public enum BehaviorOnChaos
    {
        Death, //Disconnect from server
        ResyncHead,
    }

    private static ThreePointTrackingSettings instance;
	public bool VRMode = false;
    public SixPointTrackingOptions sixPointTrackingOptions;
    public bool autoRescale = false; //Character auto rescale based on vive device positions
    public bool rescaleWidth = false; //Will character rescale based on player hands distance on start game
    public bool rescaleWidthAutoCorrect = true; //Auto correct extreme cases
    public bool displayTrackers = false; //Display tracker visualizer
    public BehaviorOnChaos behaviorOnChaos = BehaviorOnChaos.ResyncHead;
    public GameObject selfAvatar;
    public Material opponentDefaultMaterial; //This material will be used only when opponent avatar is null
    public GameObject opponentAvatar;
	public MultiPlayerMode multiplayerMode = MultiPlayerMode.ServerAndClient;
    public string serverIP = "localhost";
    public int serverPort = 7777;
    [System.NonSerialized]
    public float minRestartTime = 5.0f; //This is better to be longer than the timeout parameter for clients and server
    public PlayerAsset playerAsset;

    public EntityAsset[] serverEntityAssets;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static ThreePointTrackingSettings GetInstance()
    {
        return instance;
    }
}