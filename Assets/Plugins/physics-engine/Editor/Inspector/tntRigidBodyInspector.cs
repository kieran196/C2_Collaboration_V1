using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(tntRigidBody))]
[CanEditMultipleObjects]

public class tntRigidBodyInspector : Editor
{
    public const string linearVelocityTooltip = "Current linear velocity; you can set body velocity by modifying these values.";
    public const string angularVelocityTooltip = "Current angular velocity; you can set body velocity by modifying these values.";

    private SerializedProperty m_collidable;
    private SerializedProperty m_mass;
    private SerializedProperty m_moi;
    private SerializedProperty m_material;
    private SerializedProperty m_initialLinearVelocity;
    private SerializedProperty m_initialAngularVelocity;
    private SerializedProperty m_bKinematic;
    private SerializedProperty m_drag;
    private SerializedProperty m_angularDrag;

    public void OnEnable()
    {
        m_collidable = serializedObject.FindProperty("m_collidable");
        m_mass = serializedObject.FindProperty("m_mass");
        m_moi = serializedObject.FindProperty("m_moi");
        m_material = serializedObject.FindProperty("m_material");
        m_bKinematic = serializedObject.FindProperty("m_IsKinematic");
        m_drag = serializedObject.FindProperty("m_drag");
        m_angularDrag = serializedObject.FindProperty("m_angularDrag");

        m_initialLinearVelocity = serializedObject.FindProperty("m_initialLinearVelocity");
        m_initialAngularVelocity = serializedObject.FindProperty("m_initialAngularVelocity");

    }

    public override void OnInspectorGUI()
    {
        tntRigidBody rigidbody = (tntRigidBody)target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_collidable);
        EditorGUILayout.PropertyField(m_mass);
        EditorGUILayout.PropertyField(m_drag);
        EditorGUILayout.PropertyField(m_angularDrag);
		EditorGUILayout.PropertyField(m_moi);
		EditorGUILayout.PropertyField(m_material);
        Vector3 newLinVel = EditorGUILayout.Vector3Field(new GUIContent("Linear Velocity", linearVelocityTooltip), rigidbody.linearVelocity);
        Vector3 newAngVel = EditorGUILayout.Vector3Field(new GUIContent("Angular Velocity", angularVelocityTooltip), rigidbody.angularVelocity);

        EditorGUILayout.PropertyField(m_initialLinearVelocity);
        m_initialLinearVelocity.serializedObject.ApplyModifiedProperties();
        EditorGUILayout.PropertyField(m_initialAngularVelocity);
        m_initialAngularVelocity.serializedObject.ApplyModifiedProperties();

        EditorGUILayout.PropertyField(m_bKinematic);

        if (GUI.changed)
        {
            tntRigidBody rigidBody = (tntRigidBody)target;
            float oldMass = rigidBody.m_mass;
            bool oldKinematic = rigidBody.GetKinematic();
            serializedObject.ApplyModifiedProperties();
            float newMass = rigidBody.m_mass;
            if (oldMass != newMass)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    tntRigidBody body = (tntRigidBody)targets[i];
                    body.m_mass = newMass;
                    body.SetMass(newMass);
                }
            }
            bool newKinematic = rigidBody.GetKinematic();
            if (oldKinematic != newKinematic)
            {
                rigidBody.ForceSetKinematic();
            }
            if (newLinVel != rigidbody.linearVelocity || newAngVel != rigidbody.angularVelocity)
            {
                rigidbody.linearVelocity = newLinVel;
                rigidbody.angularVelocity = newAngVel;
            }
            CheckModifiedProperties();
        }
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

