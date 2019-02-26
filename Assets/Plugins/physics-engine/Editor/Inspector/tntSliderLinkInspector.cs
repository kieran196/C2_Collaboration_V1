using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntSliderLink))]
[CanEditMultipleObjects]

public class tntSliderLinkInspector : tntChildLinkInspector
{
	private SerializedProperty m_axisA;
    private SerializedProperty m_showJoint;
    private SerializedProperty m_visualEditor;

	public void OnEnable()
	{
		BindProperties();
		m_axisA = serializedObject.FindProperty("m_axisA");
        m_showJoint = serializedObject.FindProperty("m_showJoint");
        m_visualEditor = serializedObject.FindProperty("m_visualEditor");
	}

	public override void OnInspectorGUI()
	{
		UpdateGUI();
		EditorGUILayout.PropertyField(m_axisA);
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
        
        tntSliderLink link = target as tntSliderLink;
        if (target == null)
            return;

        Transform transA = link.m_parent.transform;
        Transform transB = link.transform;
        Vector3 axisA = link.AxisAToWorld();
        Vector3 midPoint = (transA.position + transB.position) / 2f;

        if (m_visualEditor.boolValue)
        {
            Vector3 up = MathUtils.FindUpFromForward(axisA);
            EditorGUI.BeginChangeCheck();
            Quaternion newRotation = Handles.RotationHandle(Quaternion.LookRotation(axisA, up), 
                                                            midPoint);
            if (EditorGUI.EndChangeCheck())
            {
                axisA = newRotation * Vector3.forward;
                link.AxisAFromWorld(axisA);
            }
        }

        if (m_showJoint.boolValue)
        {
            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, midPoint, Quaternion.LookRotation(axisA),
                             HandleUtility.GetHandleSize(midPoint), EventType.Repaint);
            Handles.color = Color.yellow;
            Handles.CylinderHandleCap(0, midPoint, Quaternion.LookRotation(axisA),
                                HandleUtility.GetHandleSize(midPoint) / 10f, EventType.Repaint);
        }
    }
}

