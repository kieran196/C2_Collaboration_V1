using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tntHumanoidBlendSample))]
[CanEditMultipleObjects]

public class tntHumanoidBlendSampleInspector : Editor {
    // ---------------------- Control Parameters ---------------------
    SerializedProperty m_controlParams;
    tntHumanoidBlendSample m_sample;

    public void OnEnable() {
        m_sample = (tntHumanoidBlendSample)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        SerializedProperty fwdVel = serializedObject.FindProperty("m_fwdVel");
        fwdVel.floatValue = EditorGUILayout.FloatField("Forward Velocity", fwdVel.floatValue);
        SerializedProperty sideVel = serializedObject.FindProperty("m_sideVel");
        sideVel.floatValue = EditorGUILayout.FloatField("Sideway Velocity", sideVel.floatValue);
        SerializedProperty turnVel = serializedObject.FindProperty("m_turnVel");
        turnVel.floatValue = EditorGUILayout.FloatField("Turning Velocity", turnVel.floatValue);
        if (m_sample.m_paramsScriptObject == null) {
            EditorGUILayout.LabelField("----------------------- Please set control parameters -----------------------");
            EditorGUILayout.ObjectField(serializedObject.FindProperty("m_paramsScriptObject"), typeof(tntHumanoidControlParams));
        }
        else {
            SerializedObject scriptObj = new SerializedObject(serializedObject.FindProperty("m_paramsScriptObject").objectReferenceValue);
            m_controlParams = scriptObj.FindProperty("m_params");
            EditorGUILayout.LabelField("----------------------- Control Parameters  --------------------------");
            EditorGUILayout.ObjectField(serializedObject.FindProperty("m_paramsScriptObject"), typeof(tntHumanoidControlParams));
            tntHumanoidControlParamsInspector.DrawParamControlPanel(m_controlParams);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
