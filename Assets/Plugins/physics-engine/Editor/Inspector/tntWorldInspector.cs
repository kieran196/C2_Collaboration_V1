using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntWorld))]
public class tntWorldInspector : Editor
{
    protected void OnEnable()
    {}
    
    public override void OnInspectorGUI()
    {
        tntWorld targetWorld = target as tntWorld;
        serializedObject.Update();

        DrawDefaultInspector();       

        int newNumIterations = EditorGUILayout.IntField(new GUIContent("Num iterations"), targetWorld.m_numIterations);
        if (newNumIterations != targetWorld.m_numIterations)
            targetWorld.SetConstraintSolverIterationCount(newNumIterations);

        SolverFidelityIndex newFidelityIndex = (SolverFidelityIndex)EditorGUILayout.EnumPopup("Solver Fidelity Index", targetWorld.DefaultSolverFidelityIndex);
        targetWorld.DefaultSolverFidelityIndex = newFidelityIndex;
        

        Vector3 gravity = EditorGUILayout.Vector3Field(new GUIContent("Gravity"), targetWorld.m_gravity);
        if (gravity != targetWorld.m_gravity)
        {
            targetWorld.SetGravity(gravity);
        }
    }

}