using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntBallLink))]
[CanEditMultipleObjects]

public class tntBallLinkInspector : tntChildLinkInspector
{
	private SerializedProperty m_pivotA;
	private SerializedProperty m_pivotB;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;
    
	public void OnEnable()
	{
		BindProperties();
		m_pivotA = serializedObject.FindProperty("m_pivotA");
		m_pivotB = serializedObject.FindProperty("m_pivotB");
        m_showJoint = serializedObject.FindProperty("m_showJoint");
        m_visualEditor = serializedObject.FindProperty("m_visualEditor");
    }
    
	public override void OnInspectorGUI()
	{
		UpdateGUI();
		EditorGUILayout.PropertyField(m_pivotA);
	    EditorGUILayout.PropertyField(m_pivotB);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto Fill pivotA"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntBallLink ball = targets[i] as tntBallLink;
                    if (ball != null)
                        ball.AutoFillPivotA();
                }
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Auto Fill pivotB"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntBallLink ball = targets[i] as tntBallLink;
                    if (ball != null)
                        ball.AutoFillPivotB();
                }
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(m_showJoint);
        EditorGUILayout.PropertyField(m_visualEditor);

        if (GUI.changed)
	    {
			SaveOldProperties();
			serializedObject.ApplyModifiedProperties();
			CheckModifiedProperties();
	    }
	}

    public void OnSceneGUI()
    {
        if (!m_showJoint.boolValue && !m_visualEditor.boolValue)
            return;
        
        tntBallLink link = target as tntBallLink;
        if (target == null)
            return;

        Quaternion rotA = link.m_parent.transform.rotation;

        if (m_visualEditor.boolValue)
        {
            Vector3 pivotA = link.PivotAToWorld();
            EditorGUI.BeginChangeCheck();
            pivotA = Handles.PositionHandle(pivotA, rotA);
            if (EditorGUI.EndChangeCheck())
            {
                link.PivotAFromWorld(pivotA);
            }
        }
        
        if (GUI.changed)
        {
            link.AutoFillPivotB();
            //EditorUtility.SetDirty(target);
        }
        
        if (m_showJoint.boolValue)
        {
            Vector3 pivotA = link.PivotAToWorld();
            Vector3 pivotB = link.PivotBToWorld();

            Handles.color = Color.yellow;
            Handles.CubeHandleCap(0, pivotA, rotA, HandleUtility.GetHandleSize(pivotA) / 10f, EventType.Repaint);
            Handles.color = Color.green;
            Handles.CubeHandleCap(0, pivotB, rotA, HandleUtility.GetHandleSize(pivotB) / 10f, EventType.Repaint);
        }
    }
}
