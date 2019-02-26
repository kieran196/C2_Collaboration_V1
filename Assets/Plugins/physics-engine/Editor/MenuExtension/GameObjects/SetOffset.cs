using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class SetOffsetWindow : EditorWindow {

    private SetOffset offsetClass;

    [MenuItem ("GameObject/Articulated Physics/Set Offset")]
    static void Init ()
    {
        SetOffsetWindow window = (SetOffsetWindow)EditorWindow.GetWindow (typeof (SetOffsetWindow));
        window.offsetClass = new SetOffset();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            window.offsetClass.RecognizeSelectedObject(Selection.transforms[i].gameObject); 
        window.Show();
    }

    void OnGUI()
    {
        if (offsetClass == null)
            return;
        offsetClass.OnGUI();
    }

    //When a selection change notification is received
    //recalculate the variables and references for the new object
    void OnSelectionChange() 
    {
        if (offsetClass == null)
            return;
        offsetClass.ClearSelection();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            offsetClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
    }
}

public class SetOffset
{
    private List<GameObject> objs; //Selected object in the Hierarchy
    private Vector3 positionOffset;
    private string name;

    public void ClearSelection()
    {
        objs.Clear();
    }

    public SetOffset()
    {
        objs = new List<GameObject>();
    }

    public void OffsetPosition()
    {
        foreach (GameObject obj in objs)
        {
            Component[] trans;

            trans = obj.GetComponentsInChildren<Transform>();
            foreach (Transform tran in trans)
            {
                if (tran.name == name)
                {
                    tran.localPosition += positionOffset;
                }
            }
        }
    }

    public void OffsetCollider()
    {
        foreach (GameObject obj in objs)
        {
            Component[] colliders;
            
            colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider.transform.name == name)
                {
                    if (collider as BoxCollider != null)
                        ((BoxCollider)collider).center = Vector3.zero;
                    else if (collider as CapsuleCollider != null)
                        ((CapsuleCollider)collider).center = Vector3.zero;
                    else if (collider as SphereCollider != null)
                        ((SphereCollider)collider).center = Vector3.zero;
                }
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

        positionOffset.x = EditorGUILayout.FloatField("X offset", positionOffset.x);
        positionOffset.y = EditorGUILayout.FloatField("Y offset", positionOffset.y);
        positionOffset.z = EditorGUILayout.FloatField("Z offset", positionOffset.z);
        name = EditorGUILayout.TextField("Gameobject name", name);

        if (GUILayout.Button("Offset Position"))
        {
            OffsetPosition();
        }
        if (GUILayout.Button("Clear Collider Offset"))
        {
            OffsetCollider();
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