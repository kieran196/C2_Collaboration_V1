using UnityEngine;
using UnityEditor;
using System.Collections;

public class SetupProp : MonoBehaviour 
{
    [MenuItem("GameObject/Articulated Physics/FormatPrefab")]
    public static void FormatPrefab ()
    {
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            GameObject gameObject = Selection.gameObjects[i];
            if (gameObject.GetComponent<Animator>() != null) 
            {
                Component.DestroyImmediate(gameObject.GetComponent<Animator>());
            }

            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<MeshCollider>();
            }

            if (collider.sharedMesh == null)
            {
                Transform colliderChild = gameObject.transform.Find("collisionmesh");
                if (colliderChild == null)
                {
                    colliderChild = gameObject.transform.Find("collisionMesh");
                }
                if (colliderChild == null)
                {
                    colliderChild = gameObject.transform.Find("collisionMesh1");
                }
                if (colliderChild == null)
                {
                    colliderChild = gameObject.transform.Find("collisionMesh2");
                }
                if (colliderChild != null)
                {
                    collider.sharedMesh = colliderChild.GetComponent<MeshFilter>().sharedMesh;
                    colliderChild.GetComponent<Renderer>().enabled = false;
                }
                else
                {
                    Debug.LogError("Couldn't find collision mesh in "+gameObject.name);
                }
            }

            collider.convex = true;
            collider.enabled = false;

            foreach (Transform t in gameObject.transform)
            {
                if (t.GetComponent<MeshRenderer>() != null)
                {
                    SetMeshPivot setPivotHelper = new SetMeshPivot();
                    setPivotHelper.RecognizeSelectedObject(t.gameObject);
                    setPivotHelper.CenterPivot();
                }
                t.localPosition = Vector3.zero;
            }
            
            if (gameObject.GetComponent<tntRigidBody>() == null)
            {
                gameObject.AddComponent<tntRigidBody>().m_mass = 2f;
            }
            Debug.Log("processed "+gameObject.name);
        }
    }

}
