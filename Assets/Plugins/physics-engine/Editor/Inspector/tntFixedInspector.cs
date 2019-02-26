using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntFixedJoint))]
[CanEditMultipleObjects]

// Rigid body joint inspector
public class tntFixedInspector : Editor
{
    private SerializedProperty m_bodyA;
    private SerializedProperty m_bodyB;
	private SerializedProperty m_disableSelfCollision;
	private SerializedProperty m_pivotA;
	private SerializedProperty m_pivotB;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;
    private SerializedProperty m_breakingImpulse;
	private SerializedProperty m_overrideNumIterations;
	private SerializedProperty m_feedback;

	public void OnEnable()
	{

		m_bodyA = serializedObject.FindProperty("m_bodyA");
		m_bodyB = serializedObject.FindProperty("m_bodyB");
		m_disableSelfCollision = serializedObject.FindProperty("m_disableSelfCollision");
		m_pivotA = serializedObject.FindProperty("m_pivotA");
		m_pivotB = serializedObject.FindProperty("m_pivotB");
        m_showJoint = serializedObject.FindProperty("m_showJoint");
        m_visualEditor = serializedObject.FindProperty("m_visualEditor");
		m_breakingImpulse = serializedObject.FindProperty("m_breakingImpulse");
		m_overrideNumIterations = serializedObject.FindProperty("m_overrideNumIterations");
		m_feedback = serializedObject.FindProperty("m_feedback");
    }
    
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(m_bodyA);
		EditorGUILayout.PropertyField(m_bodyB);
		EditorGUILayout.PropertyField(m_disableSelfCollision);
		EditorGUILayout.PropertyField(m_pivotA);
        EditorGUILayout.PropertyField(m_pivotB);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto Fill pivotA"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
					tntFixedJoint joint = targets[i] as tntFixedJoint;
                    if (joint != null)
						joint.AutoFillPivotA();
                }
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Auto Fill pivotB"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
					tntFixedJoint joint = targets[i] as tntFixedJoint;
                    if (joint != null)
						joint.AutoFillPivotB();
                }
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(m_breakingImpulse);
		EditorGUILayout.PropertyField(m_overrideNumIterations);
		EditorGUILayout.PropertyField(m_feedback, true);
        EditorGUILayout.PropertyField(m_showJoint);
        EditorGUILayout.PropertyField(m_visualEditor);

        if (GUI.changed)
	    {
			// SaveOldProperties();
			tntFixedJoint link = (tntFixedJoint)target;
			float oldBreak = link.m_breakingImpulse;
			
			serializedObject.ApplyModifiedProperties ();
			
			// CheckModifiedProperties();
			float newBreak = link.m_breakingImpulse;
			if (oldBreak != newBreak) {
				for (int i = 0; i < targets.Length; ++i) {
					link = (tntFixedJoint)targets [i];
					link.m_breakingImpulse = newBreak;
					link.SetBreakingImpulse(newBreak);
				}
			}
	    }
	}

    public void OnSceneGUI()
    {
        if (!m_showJoint.boolValue && !m_visualEditor.boolValue)
            return;
        
		tntFixedJoint joint = target as tntFixedJoint;
        if (target == null)
            return;
        
        Quaternion rotA = joint.bodyA.transform.rotation;
        
        if (m_visualEditor.boolValue)
        {
            Vector3 pivotA = joint.PivotAToWorld();
            EditorGUI.BeginChangeCheck();
            pivotA = Handles.PositionHandle(pivotA, rotA);
            if (EditorGUI.EndChangeCheck())
            {
                joint.PivotAFromWorld(pivotA);
            }
        }
        
        if (GUI.changed)
        {
            joint.AutoFillPivotB();
            //EditorUtility.SetDirty(target);
        }
        
        if (m_showJoint.boolValue)
        {
			Vector3 pivotA = joint.PivotAToWorld();
			Vector3 pivotB = joint.PivotBToWorld();
            
            Handles.color = Color.yellow;
            Handles.CubeHandleCap(0, pivotA, rotA, HandleUtility.GetHandleSize(pivotA) / 10f, EventType.Repaint);
            Handles.color = Color.green;
            Handles.CubeHandleCap(0, pivotB, rotA, HandleUtility.GetHandleSize(pivotB) / 10f, EventType.Repaint);
        }
    }
}

