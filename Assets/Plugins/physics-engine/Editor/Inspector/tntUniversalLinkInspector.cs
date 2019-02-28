using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntUniversalLink))]
[CanEditMultipleObjects]

public class tntUniversalLinkInspector : tntChildLinkInspector
{
	private SerializedProperty m_pivotA;
	private SerializedProperty m_pivotB;
	private SerializedProperty m_axisA;
	private SerializedProperty m_axisB;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;

	public void OnEnable()
	{
		BindProperties();
		m_pivotA = serializedObject.FindProperty("m_pivotA");
		m_pivotB = serializedObject.FindProperty("m_pivotB");
		m_axisA = serializedObject.FindProperty("m_axisA");
		m_axisB = serializedObject.FindProperty("m_axisB");
        m_showJoint = serializedObject.FindProperty("m_showJoint");
        m_visualEditor = serializedObject.FindProperty("m_visualEditor");
	}

	public override void OnInspectorGUI()
	{
		UpdateGUI();
		EditorGUILayout.PropertyField(m_pivotA);
        EditorGUILayout.PropertyField(m_axisA);
        EditorGUILayout.PropertyField(m_pivotB);
        EditorGUILayout.PropertyField(m_axisB);
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto Fill pivotA"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntUniversalLink universal = targets[i] as tntUniversalLink;
                    if (universal != null)
                        universal.AutoFillPivotA();
                }
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Auto Fill pivotB"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntUniversalLink universal = targets[i] as tntUniversalLink;
					if (universal != null)
						universal.AutoFillPivotB();
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

		tntUniversalLink link = target as tntUniversalLink;
        if (target == null)
            return;

        if (m_visualEditor.boolValue)
        {
            Vector3 pivotA = link.PivotAToWorld();
            Vector3 axisA = link.AxisAToWorld();        // Forward of the handle frame       
            Vector3 axisB = link.AxisBToWorld();        // Up of the handle frame
            if (Mathf.Abs(axisB.magnitude - 1f) < 1e-6)
            {
                // axisB is unassigned. let's auto generate one
                axisB = MathUtils.FindUpFromForward(axisA);
            }
            EditorGUI.BeginChangeCheck();
            pivotA = Handles.PositionHandle(pivotA, Quaternion.LookRotation(axisA, axisB));
            
            Quaternion newRotation = Handles.RotationHandle(
                Quaternion.LookRotation(axisA, axisB), pivotA);

            if (EditorGUI.EndChangeCheck())
            {
                link.PivotAFromWorld(pivotA);
                link.AxisAFromWorld(newRotation * Vector3.forward);
                link.AxisBFromWorld(newRotation * Vector3.up);
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
            Vector3 axisA = link.AxisAToWorld();
			Vector3 axisB = link.AxisBToWorld();
			Vector3 upA = MathUtils.FindUpFromForward(axisA);
			Vector3 upB = MathUtils.FindUpFromForward(axisB);

			Handles.color = Color.red;
			Handles.ArrowHandleCap(0, pivotA, Quaternion.LookRotation(axisA, upA),
			                 HandleUtility.GetHandleSize(pivotA), EventType.Repaint);
            Handles.color = Color.green;
			Handles.ArrowHandleCap(0, pivotB, Quaternion.LookRotation(axisB, upB),
			                 HandleUtility.GetHandleSize(pivotB), EventType.Repaint);
        }
    }
}

