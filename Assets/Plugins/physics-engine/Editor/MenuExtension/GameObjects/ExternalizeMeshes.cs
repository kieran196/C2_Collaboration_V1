using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExternalizeMeshes : MonoBehaviour
{
    static List<Mesh> meshList;

    [MenuItem("GameObject/Articulated Physics/Externalize meshes")]
    static void DoExternalizeMeshes()
    {
        meshList = new List<Mesh>();
        GameObject[] go = Selection.gameObjects;
        foreach (GameObject g in go)
        {
            ExternalizeGO(g);
        }
		AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    static bool IsEqual(Mesh a, Mesh b)
    {
        if (a.vertexCount != b.vertexCount)
            return false;
        if (a.triangles.Length != b.triangles.Length)
            return false;
        if (a.uv.Length != b.uv.Length)
            return false;
        for (int i = 0; i < a.vertexCount; ++i)
            if (a.vertices[i] != b.vertices[i])
                return false;
        for (int i = 0; i < a.triangles.Length; ++i)
            if (a.triangles[i] != b.triangles[i])
                return false;
        return true;
    }

    static Mesh FindDuplicate(Mesh a)
    {
        for (int i = 0; i < meshList.Count; ++i)
            if (IsEqual(meshList[i], a))
                return meshList[i];
        return null;
    }

    static void ExternalizeGO(GameObject obj)
    {
        if (obj != null)
        {
            string basePath = "Runtime/Meshes/externalized/";
            /*
            string parentPath = "";

            Transform trans = obj.transform;
            if (trans.parent != null)
            {
                parentPath = trans.parent.name;
            }

            if (!Directory.Exists("Assets/" + basePath + parentPath))
            {
                AssetDatabase.CreateFolder("Assets", basePath + parentPath);
            }
            */

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf)
            {
                Mesh dup =  (mf.sharedMesh == null) ? null : FindDuplicate(mf.sharedMesh);
                if (dup == null)
                {
                    Mesh newMesh = Mesh.Instantiate<Mesh>(mf.sharedMesh);
                    newMesh.name = obj.name;
                    string assetPath = "Assets/" + basePath + obj.name + ".asset";

                    int i = 1;
                    while (File.Exists(assetPath))
                    {
                        assetPath = "Assets/" + basePath + obj.name + i + ".asset";
                        ++i;
                    }
                    AssetDatabase.CreateAsset(newMesh, assetPath);
                    mf.sharedMesh = newMesh;
                    meshList.Add(newMesh);
                } else
                {
                    Debug.Log("Merge the MeshFilter of " + obj.name + " with existing mesh:" + dup.name);
                    mf.sharedMesh = dup;
                }
            }

            MeshCollider mc = obj.GetComponent<MeshCollider>();
            if (mc)
            {
                Mesh dup = (mc.sharedMesh == null) ? null : FindDuplicate(mc.sharedMesh);
                if (dup == null)
                {
                    Mesh newMesh = Mesh.Instantiate<Mesh>(mc.sharedMesh);
                    newMesh.name = obj.name;
                    string assetPath = "Assets/" + basePath + obj.name + ".asset";

                    int i = 1;
                    while (File.Exists(assetPath))
                    {
                        assetPath = "Assets/" + basePath + obj.name + i + ".asset";
                        ++i;
                    }
                    AssetDatabase.CreateAsset(newMesh, assetPath);
                    mc.sharedMesh = newMesh;
                    meshList.Add(newMesh);
                }
                else
                {
                    Debug.Log("Merge the MeshCollider of " + obj.name + " with existing mesh:" + dup.name);
                    mc.sharedMesh = dup;
                }
            }

            // Now recurse through each child GO (if there are any):
            foreach (Transform childT in obj.transform)
            {
                //Debug.Log("Searching " + childT.name  + " " );
                ExternalizeGO(childT.gameObject);
            }
        }
    }
}