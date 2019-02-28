using UnityEngine;
using UnityEditor;

public class tntLinkInspector : Editor
{
    private SerializedProperty m_index;
    private SerializedProperty m_mass;
	private SerializedProperty m_moi;
    private SerializedProperty m_computeMoiFromColliders;
	private SerializedProperty m_material;
    private SerializedProperty m_mark;
    private SerializedProperty m_collidable;
    private SerializedProperty m_drag;
    private SerializedProperty m_angularDrag;

    protected virtual void BindProperties()
	{
        m_index = serializedObject.FindProperty("m_index");
        m_mass = serializedObject.FindProperty("m_mass");
		m_moi = serializedObject.FindProperty("m_moi");
        m_computeMoiFromColliders = serializedObject.FindProperty("m_computeMoiFromColliders");
		m_material = serializedObject.FindProperty("m_material");
        m_mark = serializedObject.FindProperty("m_mark");
        m_collidable = serializedObject.FindProperty("m_collidable");
        m_drag = serializedObject.FindProperty("m_drag");
        m_angularDrag = serializedObject.FindProperty("m_angularDrag");
    }

	protected virtual void UpdateGUI()
	{
		serializedObject.Update();
        GUI.enabled = false;
        EditorGUILayout.PropertyField(m_index);
        GUI.enabled = true;
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_mass);        
        EditorGUILayout.PropertyField(m_computeMoiFromColliders, new GUIContent("Moi auto-computed", "Moi computed from colliders and mass on simulation start"));
        {
            EditorGUI.indentLevel++;
            GUI.enabled = !m_computeMoiFromColliders.boolValue;

            // in play mode we always want to see MoI, whether auto-computed or not
            // in edit mode, if auto-compute is on, we will display zeros to reduce confusion
            if (m_computeMoiFromColliders.boolValue && !Application.isPlaying)
                EditorGUILayout.Vector3Field(new GUIContent("Moi"), Vector3.zero);                
            else
                EditorGUILayout.PropertyField(m_moi);

            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }
		EditorGUILayout.PropertyField(m_material);
        EditorGUILayout.PropertyField(m_mark);
        EditorGUILayout.PropertyField(m_collidable);
        EditorGUILayout.PropertyField(m_drag);
        EditorGUILayout.PropertyField(m_angularDrag);
    }

	protected virtual void SaveOldProperties()
	{
        // Only implement this if you wish to monitor the change to the above properties (e.g. dynamically sync them with engine)
	}

	protected virtual void CheckModifiedProperties()
	{
        // Only implement this if you wish to monitor the change to the above properties (e.g. dynamically sync them with engine)
    }
   
}

