using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(PDParamSet))]
[CanEditMultipleObjects]

public class PDParamsInspector : Editor
{
    private SerializedProperty m_pdParams;
    private SerializedProperty m_numLinks;
    private SerializedProperty m_linkNames;
    private bool[] m_showAdvanced;
    private bool[] m_controlled;
    private bool[] m_relToCharFrame;

    public void OnEnable()
    {
        m_pdParams = serializedObject.FindProperty("m_pdParams");
        m_numLinks = serializedObject.FindProperty("m_numLinks");
        m_linkNames = serializedObject.FindProperty("m_linkNames");
        m_showAdvanced = new bool[m_numLinks.intValue];
        m_controlled = new bool[m_numLinks.intValue];
        m_relToCharFrame = new bool[m_numLinks.intValue];
        for (int i = 0; i < m_numLinks.intValue; i++) {
            int index = i * (PDParamSet.PD_REL + 1);
            SerializedProperty controlParam = m_pdParams.GetArrayElementAtIndex(index + PDParamSet.PD_CONTROLLED);
            m_controlled[i] = controlParam.floatValue == 1;
            controlParam = m_pdParams.GetArrayElementAtIndex(index + PDParamSet.PD_REL);
            m_relToCharFrame[i] = controlParam.floatValue == 1;
        }
    }

    private void AddControlParamFloat(int index, string name)
    {
        SerializedProperty controlParam = m_pdParams.GetArrayElementAtIndex(index);
        controlParam.floatValue = EditorGUILayout.FloatField(new GUIContent(name), controlParam.floatValue);
    }

    private void AddControlParamSlider(int index, string name, float min, float max) {
        SerializedProperty controlParam = m_pdParams.GetArrayElementAtIndex(index);
        controlParam.floatValue = EditorGUILayout.Slider(new GUIContent(name), controlParam.floatValue, min, max);
    }

    private void AddControlParamToggle(int index, string name, bool[] val, int boolIndex) {
        SerializedProperty controlParam = m_pdParams.GetArrayElementAtIndex(index);
        val[boolIndex] = controlParam.floatValue == 1;
        val[boolIndex] = EditorGUILayout.Toggle(name, val[boolIndex]);
        controlParam.floatValue = val[boolIndex] ? 1 : 0;
    }

    private void AddControlParamVector3(int index, string name) {
        SerializedProperty x = m_pdParams.GetArrayElementAtIndex(index);
        SerializedProperty y = m_pdParams.GetArrayElementAtIndex(index + 1);
        SerializedProperty z = m_pdParams.GetArrayElementAtIndex(index + 2);
        Vector3 myVector = new Vector3(x.floatValue, y.floatValue, z.floatValue);
        myVector = EditorGUILayout.Vector3Field(new GUIContent(name), myVector);
        x.floatValue = myVector.x;
        y.floatValue = myVector.y;
        z.floatValue = myVector.z;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Note: the control parameter layout must match the engine internal control parameter layout
        EditorGUILayout.LabelField("----------------------- PD Parameter Set  --------------------------");

        int offset = 0;
        for (int i = 0; i < m_numLinks.intValue; i++)
        {
            EditorGUILayout.LabelField(m_linkNames.GetArrayElementAtIndex(i).stringValue);
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            {
                EditorGUIUtility.labelWidth = 40;
                EditorGUIUtility.fieldWidth = 40;
                SerializedProperty controlParam = m_pdParams.GetArrayElementAtIndex(offset + PDParamSet.PD_KP);
                controlParam.floatValue = EditorGUILayout.FloatField(new GUIContent("Kp"), controlParam.floatValue);
                controlParam = m_pdParams.GetArrayElementAtIndex(offset + PDParamSet.PD_KD);
                controlParam.floatValue = EditorGUILayout.FloatField(new GUIContent("Kd"), controlParam.floatValue);
                EditorGUIUtility.labelWidth = 80;
                EditorGUIUtility.fieldWidth = 5;
                AddControlParamToggle(offset + PDParamSet.PD_CONTROLLED, "Controlled", m_controlled, i);
                EditorGUIUtility.labelWidth = 0;
                EditorGUIUtility.fieldWidth = 0;
            }
            m_showAdvanced[i] = EditorGUILayout.Foldout(m_showAdvanced[i], "More");
            GUILayout.EndHorizontal();
            if (m_showAdvanced[i]) {
                GUILayout.BeginHorizontal();
                AddControlParamFloat(offset + PDParamSet.PD_MAX_ABS_TORQUE, "Maximum torque");
                AddControlParamFloat(offset + PDParamSet.PD_KPMOD, "Kp modifier");
                AddControlParamFloat(offset + PDParamSet.PD_KDMOD, "Kd modifier");
                GUILayout.EndHorizontal();
                AddControlParamVector3(offset + PDParamSet.PD_SCALE, "Scale");
                AddControlParamToggle(offset + PDParamSet.PD_REL, "Rel to char frame", m_relToCharFrame, i);
            }
            EditorGUI.indentLevel--;
            offset += PDParamSet.PD_REL + 1;
        }
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}