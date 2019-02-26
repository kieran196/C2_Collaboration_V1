using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ShiftPivotWindow : EditorWindow {

    private APEShiftPivot shiftClass;

    [MenuItem ("GameObject/Articulated Physics/Shift bone pivots to collider centers")]
    static void Init ()
    {
        ShiftPivotWindow window = (ShiftPivotWindow)EditorWindow.GetWindow (typeof (ShiftPivotWindow));
        window.shiftClass = new APEShiftPivot();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            window.shiftClass.RecognizeSelectedObject(Selection.transforms[i].gameObject); 
        window.Show();
    }

    void OnGUI()
    {
        if (shiftClass == null)
            return;
        shiftClass.OnGUI();
    }

    //When a selection change notification is received
    //recalculate the variables and references for the new object
    void OnSelectionChange() 
    {
        if (shiftClass == null)
            return;
        shiftClass.ClearSelection();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            shiftClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
    }
}

public class APEShiftPivot
{
    private List<GameObject> objs; //Selected object in the Hierarchy

    public APEShiftPivot()
    {
        objs = new List<GameObject>();
    }

    public void ClearSelection()
    {
        objs.Clear();
    }

    // Assumption:  Local frames of all bones are axis aligned with the character frame
    static public void FixGameObjectPivots(GameObject obj)
    {
        Component[] links;
        
        links = obj.GetComponentsInChildren<tntLink>();
        
        foreach (tntLink link in links)
        {
            // TBD: Add support for tnt Colliders later
            Component[] components = link.transform.GetComponentsInChildren<Collider>();
            if (components.Length == 0)
                continue;
            Collider collider = components[0] as Collider; // Only consider the first collider
            Vector3 localOffset = collider.transform.localPosition; // save offset of collider in local frame
            Vector3 offset = collider.transform.position - link.transform.position;
            
            // disconnect all immediate children
            List<Transform> linkChildren = new List<Transform>();
            foreach (Transform child in link.transform)
            {
                linkChildren.Add(child);
                child.parent = null;
            }

            //Debug.Log("Offset " + link.name + " for:" + link.transform.position);
            link.transform.position += offset;      // shift the bone to collider center
            
            // reconnect all immediate children child collider
            for (int i = 0; i < linkChildren.Count; ++i)
            {
                linkChildren[i].parent = link.transform;
            }

            // Update pivotB of this link
            tntChildLink childLink = link as tntChildLink;
            if (childLink != null)
                childLink.AutoFillPivotB();
            
            // Update pivotA of the children of this link
            foreach (tntLink theLink in links)
            {
                childLink = theLink as tntChildLink;
                if (childLink == null || childLink.m_parent != link)
                    continue;
                // Update pivotA of this child
                if (childLink as tntBallLink)
                {
                    tntBallLink ball = childLink as tntBallLink;
                    ball.PivotA -= localOffset;
                } else if (childLink as tntHingeLink)
                {
                    tntHingeLink hinge = childLink as tntHingeLink;
                    hinge.PivotA -= localOffset;
                } else if (childLink as tntUniversalLink)
                {
                    tntUniversalLink universal = childLink as tntUniversalLink;
                    universal.PivotA -= localOffset;
                }
            }
        }
    }

    public void FixPivots()
    {
        foreach (GameObject obj in objs)
        {
            FixGameObjectPivots(obj);
       }
    }

    public void OnGUI()
    {
        if (objs.Count == 0)
        {
            GUILayout.Label("No tntLinks are found from the game objects selected");
            return;
        }

        if (GUILayout.Button("Fix Bone Pivots")) 
        {
            FixPivots();
        }
    }

    //Gather references for the selected object and its components
    //and update the pivot vector if the object has a Mesh specified
    public void RecognizeSelectedObject(GameObject gameObject)
    {
        if (gameObject == null)
            return;
        tntLink link = gameObject.GetComponentInChildren<tntLink>();
        if (link == null)
            return;
        objs.Add(gameObject);
    }
}