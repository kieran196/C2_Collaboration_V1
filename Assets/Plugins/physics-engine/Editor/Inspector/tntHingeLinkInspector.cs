using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntHingeLink))]
[CanEditMultipleObjects]

public class tntHingeLinkInspector : tntChildLinkInspector
{
	private SerializedProperty m_pivotA;
	private SerializedProperty m_pivotB;
	private SerializedProperty m_axisA;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;

	public void OnEnable()
	{
		BindProperties();
		m_pivotA = serializedObject.FindProperty("m_pivotA");
		m_pivotB = serializedObject.FindProperty("m_pivotB");
		m_axisA = serializedObject.FindProperty("m_axisA");
        m_showJoint = serializedObject.FindProperty("m_showJoint");
        m_visualEditor = serializedObject.FindProperty("m_visualEditor");
	}

	public override void OnInspectorGUI()
	{
		UpdateGUI();
		EditorGUILayout.PropertyField(m_pivotA);
        EditorGUILayout.PropertyField(m_axisA);
        EditorGUILayout.PropertyField(m_pivotB);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto Fill pivotA"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntHingeLink hinge = targets[i] as tntHingeLink;
                    if (hinge != null)
                        hinge.AutoFillPivotA();
                }
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Auto Fill pivotB"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntHingeLink hinge = targets[i] as tntHingeLink;
                    if (hinge != null)
                        hinge.AutoFillPivotB();
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

        tntHingeLink link = target as tntHingeLink;
        if (target == null)
            return;

        if (m_visualEditor.boolValue)
        {
            Vector3 pivotA = link.PivotAToWorld();
            Vector3 axisA = link.AxisAToWorld();
            Vector3 up = MathUtils.FindUpFromForward(axisA);

            EditorGUI.BeginChangeCheck();
            pivotA = Handles.PositionHandle(pivotA, Quaternion.LookRotation(axisA, up));
            
            Quaternion newRotation = Handles.RotationHandle(
                Quaternion.LookRotation(axisA, up), pivotA);

            if (EditorGUI.EndChangeCheck())
            {
                link.PivotAFromWorld(pivotA);
                link.AxisAFromWorld(newRotation * Vector3.forward);
            }
        }

        if (GUI.changed)
        {
            link.AutoFillPivotB();
            //EditorUtility.SetDirty(target);
        }

        if (m_showJoint.boolValue)
        {
            Handles.color = Color.blue;
            Vector3 pivotA = link.PivotAToWorld();
            Vector3 pivotB = link.PivotBToWorld();
            Vector3 axisA = link.AxisAToWorld();
            Vector3 up = MathUtils.FindUpFromForward(axisA);
            Handles.ArrowHandleCap(0, pivotB, Quaternion.LookRotation(axisA, up),
                             HandleUtility.GetHandleSize(pivotB), EventType.Repaint);
            Handles.color = Color.yellow;
            Handles.CylinderHandleCap(0, pivotA, Quaternion.LookRotation(axisA, up),
                                HandleUtility.GetHandleSize(pivotB) / 10f, EventType.Repaint);
        }
    }
}

