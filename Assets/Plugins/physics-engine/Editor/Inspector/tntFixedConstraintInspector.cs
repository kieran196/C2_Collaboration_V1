using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntArticulationFixedConstraint))]
[CanEditMultipleObjects]

public class tntFixedConstraintInspector : Editor
{
    private SerializedProperty m_linkA;
    private SerializedProperty m_linkB;
    private SerializedProperty m_useBodyB;
    private SerializedProperty m_bodyB;
    private SerializedProperty m_pivotA;
    private SerializedProperty m_pivotB;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;
    private SerializedProperty m_maxImpulse;
    private SerializedProperty m_breakingImpulse;
    private SerializedProperty m_feedback;

    public void OnEnable()
    {
        m_linkA = serializedObject.FindProperty("m_linkA");
        m_linkB = serializedObject.FindProperty("m_linkB");
        m_useBodyB = serializedObject.FindProperty("m_useBodyB");
        m_bodyB = serializedObject.FindProperty("m_bodyB");
        m_pivotA = serializedObject.FindProperty("m_pivotA");
        m_pivotB = serializedObject.FindProperty("m_pivotB");
        m_showJoint = serializedObject.FindProperty("m_showJoint");
        m_visualEditor = serializedObject.FindProperty("m_visualEditor");
        m_maxImpulse = serializedObject.FindProperty("m_maxImpulse");
        m_breakingImpulse = serializedObject.FindProperty("m_breakingImpulse");
        m_feedback = serializedObject.FindProperty("m_feedback");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_linkA);
        EditorGUILayout.PropertyField(m_linkB);
        EditorGUILayout.PropertyField(m_useBodyB);
        EditorGUILayout.PropertyField(m_bodyB);
        EditorGUILayout.PropertyField(m_pivotA);
        EditorGUILayout.PropertyField(m_pivotB);

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto Fill pivotA"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntArticulationFixedConstraint fixedConstraint = targets[i] as tntArticulationFixedConstraint;
                    if (fixedConstraint != null)
                        fixedConstraint.AutoFillPivotA();
                }
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Auto Fill pivotB"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntArticulationFixedConstraint fixedConstraint = targets[i] as tntArticulationFixedConstraint;
                    if (fixedConstraint != null)
                        fixedConstraint.AutoFillPivotB();
                }
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(m_maxImpulse);
        EditorGUILayout.PropertyField(m_breakingImpulse);
        EditorGUILayout.PropertyField(m_feedback, true);
        EditorGUILayout.PropertyField(m_showJoint);
        EditorGUILayout.PropertyField(m_visualEditor);

        if (GUI.changed)
        {
            // SaveOldProperties();
            tntArticulationFixedConstraint fixedConstraint = (tntArticulationFixedConstraint)target;
            float oldBreak = fixedConstraint.m_breakingImpulse;

            serializedObject.ApplyModifiedProperties();

            // CheckModifiedProperties();
            float newBreak = fixedConstraint.m_breakingImpulse;
            if (oldBreak != newBreak)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntArticulationFixedConstraint aFixedConstraint = (tntArticulationFixedConstraint)targets[i];
                    aFixedConstraint.m_breakingImpulse = newBreak;
                    aFixedConstraint.SetBreakingImpulse(newBreak);
                }
            }
        }
    }

    public void OnSceneGUI()
    {
        if (!m_showJoint.boolValue && !m_visualEditor.boolValue)
            return;

        tntArticulationFixedConstraint fixedConstraint = target as tntArticulationFixedConstraint;
        if (fixedConstraint == null)
            return;

        Quaternion rotA = fixedConstraint.m_linkA.transform.rotation;

        if (m_visualEditor.boolValue)
        {
            Vector3 pivotA = fixedConstraint.PivotAToWorld();
            EditorGUI.BeginChangeCheck();
            pivotA = Handles.PositionHandle(pivotA, rotA);
            if (EditorGUI.EndChangeCheck())
            {
                fixedConstraint.PivotAFromWorld(pivotA);
            }
        }

        if (GUI.changed)
        {
            fixedConstraint.AutoFillPivotB();
            //EditorUtility.SetDirty(target);
        }

        if (m_showJoint.boolValue)
        {
            Vector3 pivotA = fixedConstraint.PivotAToWorld();
            Vector3 pivotB = fixedConstraint.PivotBToWorld();

            Handles.color = Color.yellow;
            Handles.CubeHandleCap(0, pivotA, rotA, HandleUtility.GetHandleSize(pivotA) / 10f, EventType.Repaint);
            Handles.color = Color.green;
            Handles.CubeHandleCap(0, pivotB, rotA, HandleUtility.GetHandleSize(pivotB) / 10f, EventType.Repaint);
        }
    }
}

