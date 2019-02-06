using UnityEngine;
using UnityEditor;
using System.Collections;

public class CreateHumanoidControlParams
{
    static int count = 0;
    private const string sObjectPath = "Assets/HumanoidCP-new";
    [MenuItem("Assets/Create/tntHumanoidControlParams Object")]
    public static void CreateScriptObject()
    {
        count++;
        string actualPath = sObjectPath + "#" + count + ".asset";
        tntHumanoidControlParams asset = ScriptableObject.CreateInstance<tntHumanoidControlParams>();
        AssetDatabase.CreateAsset(asset, actualPath);
        EditorUtility.DisplayDialog("tntHumanoidControlParams Created", actualPath, "OK");
    }
}