using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class CreateReducedState
{
    static int count = 0;
    private const string sObjectPath = "Assets/ReducedState";
    [MenuItem("Assets/Create/tntReducedState Object")]
    public static void CreateScriptObject()
    {
        count++;
        string actualPath = sObjectPath + "#" + count + ".asset";
        tntReducedState asset = ScriptableObject.CreateInstance<tntReducedState>();
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No APE model selected", "Please select the controller's container gameobject and re-try", "OK");
            return;
        }

		// depth-first index assignment
		tntChildLink[] allLinks = Selection.gameObjects[0].GetComponentsInChildren<tntChildLink>(false);
		tntChildLink[] links = System.Array.FindAll<tntChildLink> (allLinks, comp => comp.enabled && comp.m_parent != null);

		for (int i = 0; i < links.Length - 1; ++i)
			for (int j = i + 1; j < links.Length; ++j)
				if (links[i].GetIndex() > links[j].GetIndex())
				{
					tntChildLink tmp = links[i];
					links[i] = links[j];
					links[j] = tmp;
				}
		asset.NumOfChildLinks = links.Length;
        asset.AllocArrays();
        for (int i = 0; i < links.Length; ++i)
        {
            asset.m_names[i] = links[i].name;
            asset.m_editAsEulers[i] = false;
            // Set the initial pose to match the underlying articulation
            Quaternion parentRot = links[i].m_parent.transform.rotation;
            Quaternion childRot = links[i].transform.rotation;
            asset.SetJointOrientationToQuaternion(i, Quaternion.Inverse(parentRot) * childRot);
        }
        AssetDatabase.CreateAsset(asset, actualPath);
        EditorUtility.DisplayDialog("tntReducedState Created", actualPath, "OK");
    }
}