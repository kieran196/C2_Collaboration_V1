using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tntReducedState))]
//[CanEditMultipleObjects]

public class tntReducedStateInspector : Editor
{
	// ---------------------- Control Parameters ---------------------
    private SerializedProperty m_names;
	private SerializedProperty m_reducedState;
    private SerializedProperty m_editAsEulers;
    private SerializedProperty m_eulers;

    public void OnEnable()
    {
        m_names = serializedObject.FindProperty("m_names");
        m_reducedState = serializedObject.FindProperty("m_values");
        m_editAsEulers = serializedObject.FindProperty("m_editAsEulers");
        m_eulers = serializedObject.FindProperty("m_eulers");
    }

    private void AddControlParamFloat(int index, string name)
    {
        EditorGUILayout.LabelField(name);		
        SerializedProperty x = m_reducedState.GetArrayElementAtIndex(index);
        x.floatValue = EditorGUILayout.FloatField(x.floatValue);
    }

	private void AddControlParamVector3(int index, string name)
	{
        SerializedProperty x = m_reducedState.GetArrayElementAtIndex(index);
        SerializedProperty y = m_reducedState.GetArrayElementAtIndex(index + 1);
        SerializedProperty z = m_reducedState.GetArrayElementAtIndex(index + 2);
        Vector3 myVector = new Vector3(x.floatValue, y.floatValue, z.floatValue);
        myVector = EditorGUILayout.Vector3Field(new GUIContent(name), myVector);
        x.floatValue = myVector.x;
        y.floatValue = myVector.y;
        z.floatValue = myVector.z;
	}

    private void AddControlParamQuaternion(int index, string name)
    {
        SerializedProperty x = m_reducedState.GetArrayElementAtIndex(index);
        SerializedProperty y = m_reducedState.GetArrayElementAtIndex(index + 1);
        SerializedProperty z = m_reducedState.GetArrayElementAtIndex(index + 2);
        SerializedProperty w = m_reducedState.GetArrayElementAtIndex(index + 3);
        Vector4 myVector4 = new Vector4(x.floatValue, y.floatValue, z.floatValue, w.floatValue);
        myVector4 = EditorGUILayout.Vector4Field(name, myVector4);
        x.floatValue = myVector4.x;
        y.floatValue = myVector4.y;
        z.floatValue = myVector4.z;
        w.floatValue = myVector4.w;
    }

    private void AddControlParamQuaternionEuler(int index, string name, bool asEulerBefore)
    {
        if (m_eulers == null)
        {
            return;
        }
        int jointIndex = (index - 17) / 7;

        Quaternion quat;
        SerializedProperty eulers = m_eulers.GetArrayElementAtIndex(jointIndex);
        SerializedProperty x = m_reducedState.GetArrayElementAtIndex(index);
        SerializedProperty y = m_reducedState.GetArrayElementAtIndex(index + 1);
        SerializedProperty z = m_reducedState.GetArrayElementAtIndex(index + 2);
        SerializedProperty w = m_reducedState.GetArrayElementAtIndex(index + 3);

        if (!asEulerBefore)
        {
            quat = new Quaternion(x.floatValue, y.floatValue, z.floatValue, w.floatValue);
            eulers.vector3Value = quat.eulerAngles;
        }
        EditorGUILayout.LabelField(name);
        eulers.vector3Value = new Vector3(EditorGUILayout.Slider("X", eulers.vector3Value.x, -360f, 360f),
                                          EditorGUILayout.Slider("Y", eulers.vector3Value.y, -360f, 360f),
                                          EditorGUILayout.Slider("Z", eulers.vector3Value.z, -360f, 360f));
        tntReducedState reducedState = (tntReducedState)target;
        reducedState.SetJointOrientationToEulers(jointIndex, eulers.vector3Value);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Note: the control parameter layout must match the engine internal control parameter layout
        EditorGUILayout.PropertyField(m_names, new GUIContent("Joint Name Array"), true);
        AddControlParamVector3(0, "Heading Axis");
        AddControlParamFloat(3, "Heading");
        AddControlParamVector3(4, "Root Position");
        AddControlParamQuaternion(7, "Root Orientation");
        //AddControlParamQuaternionEuler(7, "Root Orientation");
        AddControlParamVector3(11, "Root Velocity");
        AddControlParamVector3(14, "Root Angular Velocity");

        for (int i = 17; i < m_reducedState.arraySize; i += 7)
        {
            int jointIndex = (i - 17) / 7;
            string jointName;
            if (m_names != null && m_names.arraySize > jointIndex)
                jointName = m_names.GetArrayElementAtIndex(jointIndex).stringValue;
            else
                jointName = "Joint #" + jointIndex;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(jointName);
            if (m_editAsEulers == null)
            {
                AddControlParamQuaternion(i, "Orientation");
                AddControlParamVector3(i + 4, "Ang Velocity");
                continue;
            }
            SerializedProperty editAsEuler = m_editAsEulers.GetArrayElementAtIndex(jointIndex);
            bool asEulerBefore = editAsEuler.boolValue;
            editAsEuler.boolValue = EditorGUILayout.Toggle("edit as Eulers", asEulerBefore);
            EditorGUILayout.EndHorizontal();
            if (editAsEuler.boolValue)
                AddControlParamQuaternionEuler(i, "Orientation", asEulerBefore);
            else
                AddControlParamQuaternion(i, "Orientation");
            AddControlParamVector3(i + 4, "Ang Velocity");
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }


        tntReducedState state = serializedObject.targetObject as tntReducedState;
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool ret = GUILayout.Button("Show Json", GUILayout.Width(300));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (ret)
        {
            Debug.Log(state.GetJsonText());
        }
    }
}