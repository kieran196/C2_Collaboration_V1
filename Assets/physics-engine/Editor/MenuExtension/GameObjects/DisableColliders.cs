using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DisableColliderWindow : EditorWindow {

    private DisableCollider disableColliderClass;

    [MenuItem ("GameObject/Articulated Physics/Disable Collider")]
    static void Init ()
    {
        DisableColliderWindow window = (DisableColliderWindow)EditorWindow.GetWindow (typeof (DisableColliderWindow));
        window.disableColliderClass = new DisableCollider();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            window.disableColliderClass.RecognizeSelectedObject(Selection.transforms[i].gameObject); 
        window.Show();
    }

    void OnGUI()
    {
        if (disableColliderClass == null)
            return;
        disableColliderClass.OnGUI();
    }

    //When a selection change notification is received
    //recalculate the variables and references for the new object
    void OnSelectionChange() 
    {
        if (disableColliderClass == null)
            return;
        disableColliderClass.ClearSelection();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            disableColliderClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
    }
}

public class DisableCollider
{
    private List<GameObject> objs; //Selected object in the Hierarchy

    public void ClearSelection()
    {
        objs.Clear();
    }

    public DisableCollider()
    {
        objs = new List<GameObject>();
    }

    public void DisableColliders()
    {
        foreach (GameObject obj in objs)
        {
            Component[] colliders;

            colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }
    }

    public void OnGUI()
    {
        if (objs.Count == 0)
        {
            GUILayout.Label("No tntLinks are found from the game objects selected");
            return;
        }

        if (GUILayout.Button("Disable Colliders"))
        {
            DisableColliders();
        }
    }

    //Gather references for the selected object and its components
    //and update the pivot vector if the object has a Mesh specified
    public void RecognizeSelectedObject(GameObject gameObject)
    {
        if (gameObject == null)
            return;
        objs.Add(gameObject);
    }
}