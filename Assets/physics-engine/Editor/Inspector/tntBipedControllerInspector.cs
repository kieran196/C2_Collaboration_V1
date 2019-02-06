using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tntBipedController))]
[CanEditMultipleObjects]

public class tntBipedControllerInspector : Editor
{
    private SerializedProperty m_rootControlParams;

    private SerializedProperty m_controlParams;
    private int m_controlParamsSize;

	private SerializedProperty m_isIKVMOn;
    private SerializedProperty m_isInvertedPendulumOn;
    private SerializedProperty m_isGravityCompenstaionOn;
    public SerializedProperty m_stepWidth;
    private SerializedProperty m_isRagdollOn;
    private SerializedProperty m_useExplicitPDControllers;
    private SerializedProperty m_useImplicitPositionError;
    private SerializedProperty m_useMOIAboutJointPosition;
    private SerializedProperty m_stanceHipDamping;
    private SerializedProperty m_stanceHipMaxVelocity;
    private SerializedProperty m_rootPredictiveTorqueScale;
    private SerializedProperty m_maxGyro;
    private SerializedProperty m_startingState;
    private SerializedProperty m_startingStance;

    private SerializedProperty m_states;
	private SerializedProperty m_controllerState;

    public void OnEnable()
    {
        m_rootControlParams = serializedObject.FindProperty("rootControlParams");
        m_controlParams = serializedObject.FindProperty("controlParams");
		m_isIKVMOn = serializedObject.FindProperty("isIKVMOn");
        m_isInvertedPendulumOn = serializedObject.FindProperty("isInvertedPendulumOn");
        m_isGravityCompenstaionOn = serializedObject.FindProperty("isGravityCompensationOn");
        m_stepWidth = serializedObject.FindProperty("stepWidth");
        m_isRagdollOn = serializedObject.FindProperty("isRagdollOn");
        m_maxGyro = serializedObject.FindProperty("maxGyro");
        m_useExplicitPDControllers = serializedObject.FindProperty("useExplicitPDControllers");
        m_useImplicitPositionError = serializedObject.FindProperty("useImplicitPositionError");
        m_useMOIAboutJointPosition = serializedObject.FindProperty("useMOIAboutJointPosition");
        m_stanceHipDamping = serializedObject.FindProperty("stanceHipDamping");
        m_stanceHipMaxVelocity = serializedObject.FindProperty("stanceHipMaxVelocity");
        m_rootPredictiveTorqueScale = serializedObject.FindProperty("rootPredictiveTorqueScale");
        m_startingState = serializedObject.FindProperty("startingState");
        m_startingStance = serializedObject.FindProperty("startingStance");

        m_states = serializedObject.FindProperty("states");
		m_controllerState = serializedObject.FindProperty("controllerState");
    }

    private void DrawControlParameters(SerializedProperty controlParams)
    {
        SerializedProperty name = controlParams.FindPropertyRelative("name");
        SerializedProperty controlled = controlParams.FindPropertyRelative("controlled");
        SerializedProperty kp = controlParams.FindPropertyRelative("kp");
        SerializedProperty kd = controlParams.FindPropertyRelative("kd");
        SerializedProperty maxAbsTorque = controlParams.FindPropertyRelative("maxAbsTorque");
        SerializedProperty scale = controlParams.FindPropertyRelative("scale");

        EditorGUILayout.LabelField("name,controlled, kp,          kd,      maxAbsTorque,   scale");
        EditorGUILayout.BeginHorizontal();
        name.stringValue = EditorGUILayout.TextField(name.stringValue);
        controlled.boolValue = EditorGUILayout.Toggle(controlled.boolValue);
        kp.floatValue = EditorGUILayout.FloatField(kp.floatValue);
        kd.floatValue = EditorGUILayout.FloatField(kd.floatValue);
        maxAbsTorque.floatValue = EditorGUILayout.FloatField(maxAbsTorque.floatValue);
        scale.vector3Value = EditorGUILayout.Vector3Field("", scale.vector3Value);
        EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ----------------------- PDParams --------------------------------
        EditorGUILayout.BeginHorizontal();
        m_controlParamsSize = m_controlParams.arraySize;
        m_controlParamsSize = EditorGUILayout.IntField ("PDParams List Size", m_controlParamsSize);
        
        if(m_controlParamsSize != m_controlParams.arraySize){
            while(m_controlParamsSize > m_controlParams.arraySize){
                m_controlParams.InsertArrayElementAtIndex(m_controlParams.arraySize);
            }
            while(m_controlParamsSize < m_controlParams.arraySize){
                m_controlParams.DeleteArrayElementAtIndex(m_controlParams.arraySize - 1);
            }
        }
        EditorGUILayout.EndHorizontal();

        DrawControlParameters(m_rootControlParams);
        for (int i = 0; i < m_controlParams.arraySize; i++)
        {
            SerializedProperty controlParams = m_controlParams.GetArrayElementAtIndex(i);
            DrawControlParameters(controlParams);
        }

        // ------------------------ Init State -------------------------------
		EditorGUILayout.PropertyField(m_isIKVMOn);
        if (m_isIKVMOn.boolValue)
        {
            EditorGUILayout.PropertyField(m_isInvertedPendulumOn);
            if (m_isInvertedPendulumOn.boolValue)
                EditorGUILayout.PropertyField(m_stepWidth);
            EditorGUILayout.PropertyField(m_isGravityCompenstaionOn);
        }
        EditorGUILayout.PropertyField(m_isRagdollOn);
        EditorGUILayout.PropertyField(m_useExplicitPDControllers);
        if (!m_useExplicitPDControllers.boolValue)
        {
            EditorGUILayout.PropertyField(m_useImplicitPositionError);
            EditorGUILayout.PropertyField(m_useMOIAboutJointPosition);
        }
		EditorGUILayout.PropertyField(m_stanceHipDamping);
        EditorGUILayout.PropertyField(m_stanceHipMaxVelocity);
        EditorGUILayout.PropertyField(m_rootPredictiveTorqueScale);
        EditorGUILayout.PropertyField(m_maxGyro);
        EditorGUILayout.PropertyField(m_startingState);
        EditorGUILayout.PropertyField(m_startingStance);

        // ------------------------ Controller States --------------------
        EditorGUILayout.PropertyField(m_states, new GUIContent("Controller States"), true);

		// ------------------------ Controller State Sensor --------------------
		EditorGUILayout.PropertyField(m_controllerState, new GUIContent("Current State"), true);

		
		// ------------------------------------------------------------------------
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
