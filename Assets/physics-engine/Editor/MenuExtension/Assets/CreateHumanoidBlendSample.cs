using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateHumanoidBlendSample {
    static int count = 0;
    private const string sObjectPath = "Assets/HumanoidBS-new";
    [MenuItem("Assets/Create/tntHumanoidBlendSample Object")]
    public static void CreateScriptObject() {
        count++;
        string actualPath = sObjectPath + "#" + count + ".asset";
        tntHumanoidBlendSample asset = ScriptableObject.CreateInstance<tntHumanoidBlendSample>();
        if (!Directory.Exists(sObjectPath)) {
            Directory.CreateDirectory(sObjectPath);
        }
        AssetDatabase.CreateAsset(asset, actualPath);
        EditorUtility.DisplayDialog("tntHumanoidBlendSample Created", actualPath, "OK");
    }
}
