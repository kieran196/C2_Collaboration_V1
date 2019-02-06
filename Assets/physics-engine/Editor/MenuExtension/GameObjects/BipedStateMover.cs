using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class BipedStateMoverWindow : EditorWindow {

    private BipedStateMover moverClass;

    [MenuItem ("GameObject/Articulated Physics/Biped State Mover")]
    static void Init () {
        BipedStateMoverWindow window = (BipedStateMoverWindow)EditorWindow.GetWindow (typeof (BipedStateMoverWindow));
        window.moverClass = new BipedStateMover();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            window.moverClass.RecognizeSelectedObject(Selection.transforms[i].gameObject); 
        window.Show ();
    }

    void OnGUI()
    {
        if (moverClass != null)
            moverClass.OnGUI();
    }

    //When a selection change notification is received
    //recalculate the variables and references for the new object
    void OnSelectionChange() 
    {
        //for (int i = 0; i < Selection.transforms.Length; ++i)
        //    moverClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
    }
}

public class BipedStateMover
{
    private List<GameObject> objs; //Selected object in the Hierarchy
    private List<tntBipedController> controllers; // biped controllers of the selected objects
    private int startStateIndex;
    private int endStateIndex;
    private int destIndex;
    private int srcController;
    private int dstController;

    public BipedStateMover()
    {
        objs = new List<GameObject>();
        controllers = new List<tntBipedController>();
        startStateIndex = endStateIndex = destIndex = -1;
        srcController = 0;
        dstController = 1;
    }

    public void CopyOrMoveToSameController(bool copy)
    {
        List<BipedConState> newStates = new List<BipedConState>();
        if (destIndex >= startStateIndex && destIndex <= endStateIndex)
        {
            GUILayout.Label("Destination index must be outside the source range");
        }
        else
        {
            if (destIndex < startStateIndex)
            {
                for (int i = 0; i <= destIndex; ++i)
                    newStates.Add(controllers[0].states[i]);
                for (int i = startStateIndex; i <= endStateIndex; ++i)
                    newStates.Add(controllers[0].states[i]);
                if (copy)
                {
                    for (int i = destIndex + 1; i < controllers[0].states.Count; ++i)
                        newStates.Add(controllers[0].states[i]);
                } else
                {
                    for (int i = destIndex + 1; i < startStateIndex; ++i)
                        newStates.Add(controllers[0].states[i]);
                    for (int i = endStateIndex + 1; i < controllers[0].states.Count; ++i)
                        newStates.Add(controllers[0].states[i]);
                }
            } else
            {
                if (copy)
                {
                    for (int i = 0; i <= destIndex; ++i)
                        newStates.Add(controllers[0].states[i]);
                } else
                {
                    for (int i = 0; i < startStateIndex; ++i)
                        newStates.Add(controllers[0].states[i]);
                    for (int i = endStateIndex + 1; i <= destIndex; ++i)
                        newStates.Add(controllers[0].states[i]);
                }
                for (int i = startStateIndex; i <= endStateIndex; ++i)
                    newStates.Add(controllers[0].states[i]);
                for (int i = destIndex + 1; i < controllers[0].states.Count; ++i)
                    newStates.Add(controllers[0].states[i]);
            }
            controllers[0].states = newStates;
        }
    }

    public void CopyOrMoveToAnotherController()
    {
        List<BipedConState> newStates = new List<BipedConState>();
        if (destIndex >= controllers[dstController].states.Count)
        {
            GUILayout.Label("Destination index is out of rangerange");
        }
        else
        {
            for (int i = 0; i <= destIndex; ++i)
                newStates.Add(controllers[dstController].states[i]);
            for (int i = startStateIndex; i <= endStateIndex; ++i)
                newStates.Add(controllers[srcController].states[i]);
            for (int i = destIndex + 1; i < controllers[dstController].states.Count; ++i)
                 newStates.Add(controllers[dstController].states[i]);
            controllers[dstController].states = newStates;
        }
    }

    public void OnGUI()
    {
        if (objs.Count == 0)
        {
            GUILayout.Label("No Biped Controller game objects selected!");
            return;
        }
        if (objs.Count == 1)
        {
            startStateIndex = EditorGUILayout.IntSlider("Start State Index", startStateIndex,
                                                     0, controllers[0].states.Count - 1);
            endStateIndex = EditorGUILayout.IntSlider("End State Index", endStateIndex,
                                                   startStateIndex, controllers[0].states.Count - 1);
            destIndex = EditorGUILayout.IntSlider("Dest State Index", destIndex,
                                               0, controllers[0].states.Count - 1);
            if (GUILayout.Button("Move")) 
            {
                CopyOrMoveToSameController(false);
            }
            if (GUILayout.Button("Copy")) 
            {
                CopyOrMoveToSameController(true);
            }
        } else
        {
            startStateIndex = EditorGUILayout.IntSlider("Start: " +
                                                        controllers[srcController].transform.parent.parent.name,
                                                        startStateIndex,
                                                        0,
                                                        controllers[srcController].states.Count - 1);
            endStateIndex = EditorGUILayout.IntSlider("End: " +
                                                      controllers[srcController].transform.parent.parent.name,
                                                      endStateIndex,
                                                      startStateIndex,
                                                      controllers[srcController].states.Count - 1);
            destIndex = EditorGUILayout.IntSlider("Dest: " +
                                                  controllers[dstController].transform.parent.parent.name,
                                                  destIndex,
                                                  0,
                                                  controllers[dstController].states.Count - 1);
            if (GUILayout.Button("Copy")) 
            {
                CopyOrMoveToAnotherController();
            }
            if (GUILayout.Button("Swap source and dest controller")) 
            {
                int tmp = srcController;
                srcController = dstController;
                dstController = tmp;
            }
        }
    }

    //Gather references for the selected object and its components
    //and update the pivot vector if the object has a Mesh specified
    public void RecognizeSelectedObject(GameObject gameObject)
    {
        if (gameObject == null)
            return;
        tntBipedController controller = gameObject.GetComponent(typeof(tntBipedController)) as tntBipedController;
        if (controller == null)
            return;
        objs.Add(gameObject);
        controllers.Add(controller);
    }
}