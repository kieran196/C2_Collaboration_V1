using UnityEngine;
using UnityEditor;
using System.Collections;

public class SaveHumanoidReducedState
{
    private const string sObjectPath = "Assets/Script Objects/Reduced States/HumanoidReducedState";
    [MenuItem("Assets/Save/HumanoidReducedState")]
    public static void CreateScriptObject()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No APE model selected", "Please select the HumanoidReducedState's container gameobject and re-try", "OK");
            return;
        }
        Component[] components = Selection.gameObjects[0].GetComponentsInChildren<tntHumanoidController>();
        if (components.Length == 0)
        {
            EditorUtility.DisplayDialog("No APE model selected", "Please select the HumanoidReducedState's container gameobject and re-try", "OK");
            return;
        }
        tntHumanoidController controller = components[0] as tntHumanoidController;
        EditorUtility.SetDirty(controller.m_desiredPose);
        EditorUtility.SetDirty(controller.m_currentPose);
        AssetDatabase.SaveAssets();
    }
}