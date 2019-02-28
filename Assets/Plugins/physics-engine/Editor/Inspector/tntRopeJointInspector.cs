using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntRopeJoint))]
[CanEditMultipleObjects]

public class tntRopeJointInspector : Editor
{	
	private SerializedProperty m_partA;
	private SerializedProperty m_partB;
	private SerializedProperty m_pivotA;
	private SerializedProperty m_pivotB;
	private SerializedProperty m_maxLength;
	private SerializedProperty m_maxForce;
	private SerializedProperty m_stiffness;
	private SerializedProperty m_damping;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;
	private SerializedProperty m_broken;
	private LineRenderer m_renderer;

	public void OnEnable()
	{
		tntRopeJoint rope = target as tntRopeJoint;
		m_renderer = rope.GetComponent<LineRenderer>();
		m_partA = serializedObject.FindProperty("m_partA");
		m_partB = serializedObject.FindProperty("m_partB");
		m_pivotA = serializedObject.FindProperty("m_pivotA");
		m_pivotB = serializedObject.FindProperty("m_pivotB");
		m_maxLength = serializedObject.FindProperty("m_maxLength");
		m_maxForce = serializedObject.FindProperty("m_maxForce");
		m_stiffness = serializedObject.FindProperty("m_stiffness");
		m_damping = serializedObject.FindProperty("m_damping");
		m_showJoint = serializedObject.FindProperty("m_showJoint");
		m_visualEditor = serializedObject.FindProperty("m_visualEditor");
		m_broken = serializedObject.FindProperty("m_broken");
	}
	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(m_partA);
		EditorGUILayout.PropertyField(m_partB);
		EditorGUILayout.PropertyField(m_pivotA);
		EditorGUILayout.PropertyField(m_pivotB);

		EditorGUILayout.PropertyField(m_showJoint);
		EditorGUILayout.PropertyField(m_visualEditor);
		EditorGUILayout.PropertyField(m_maxLength);
		EditorGUILayout.PropertyField(m_maxForce);
		EditorGUILayout.PropertyField(m_stiffness);
		EditorGUILayout.PropertyField(m_damping);
		EditorGUILayout.PropertyField(m_broken);

        if (GUI.changed) {
			// SaveOldProperties();
			serializedObject.ApplyModifiedProperties();
			// CheckModifiedProperties();
		}
	}

    public void OnSceneGUI()
    {
        if (!m_showJoint.boolValue && !m_visualEditor.boolValue)
            return;
        
		tntRopeJoint rope = target as tntRopeJoint;
        if (rope == null)
            return;
        
		Quaternion rotA = rope.m_partA.transform.rotation;
		Quaternion rotB = rope.m_partB.transform.rotation;

        if (m_visualEditor.boolValue)
        {
			Vector3 pivotA = rope.PivotAToWorld();
			Vector3 pivotB = rope.PivotBToWorld();

            EditorGUI.BeginChangeCheck();
            pivotA = Handles.PositionHandle(pivotA, rotA);
			pivotB = Handles.PositionHandle(pivotB, rotB);

            if (EditorGUI.EndChangeCheck())
            {
                rope.PivotAFromWorld(pivotA);
                rope.PivotBFromWorld(pivotB);
            }
        }
        		        
        if (m_showJoint.boolValue)
        {
			if (Application.isEditor && !Application.isPlaying) 
				rope.RenderRope(m_renderer);
			Vector3 pivotA = rope.PivotAToWorld();
            Vector3 pivotB = rope.PivotBToWorld();
			Handles.color = Color.yellow;
            Handles.CubeHandleCap(0, pivotA, rotA, HandleUtility.GetHandleSize(pivotA) / 10f, EventType.Repaint);
            Handles.color = Color.green;
            Handles.CubeHandleCap(0, pivotB, rotA, HandleUtility.GetHandleSize(pivotB) / 10f, EventType.Repaint);
        }
    }
}

