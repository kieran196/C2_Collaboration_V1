using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
class DMAvatarLiveSyncServer
{
    private delegate void MessageHandler(string str);

    [DllImport("libLiveSyncServer64")]
    private static extern
        void StartLiveSyncServer(IntPtr handler);

    [DllImport("libLiveSyncServer64")]
    private static extern
        void StopLiveSyncServer();

    [DllImport("libLiveSyncServer64")]
    private static extern
        void TickLiveSyncServer(float dt);

    [DllImport("libLiveSyncServer64")]
    private static extern
        void SetMessageHandler(IntPtr handler);

    public static void LiveSync(string avatarDescription)
    {
        DeepMotionAvatar[] avatars = UnityEngine.Object.FindObjectsOfType<DeepMotionAvatar>();

        foreach (DeepMotionAvatar avatar in avatars)
        {
            if (avatar.liveSynced)
            {
                Transform character = avatar.gameObject.transform;

                // remove all child game object
                List<GameObject> children = new List<GameObject>();
                foreach (Transform child in character)
                {
                    children.Add(child.gameObject);
                }
                foreach (GameObject obj in children)
                {
                    GameObject.DestroyImmediate(obj);
                }

                // re-deserialize from the json string
                createInstance.BlueRobot = character.gameObject;
                createInstance.CreateFromJSON(avatarDescription);

                // highlight the gameobject and mark current scene dirty
                EditorGUIUtility.PingObject(character.gameObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }

    // Use this for initialization
    static DMAvatarLiveSyncServer()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            StartLiveSyncServer(Marshal.GetFunctionPointerForDelegate(new MessageHandler(LiveSync)));
        else
            SetMessageHandler(Marshal.GetFunctionPointerForDelegate(new MessageHandler(LiveSync)));

        EditorApplication.update += Update;
    }

    ~DMAvatarLiveSyncServer()
    {
        EditorApplication.update -= Update;
        StopLiveSyncServer();
    }

    // Update is called once per frame
    static void Update()
    {
        TickLiveSyncServer(Time.deltaTime);
    }
}
