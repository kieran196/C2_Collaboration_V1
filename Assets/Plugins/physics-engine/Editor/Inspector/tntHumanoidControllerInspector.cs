using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tntHumanoidController))]

public class tntHumanoidControllerInspector : Editor
{
    // ---------------------- Control Parameters ---------------------
    private bool m_showParamMenu = true;
    private bool m_showControllerState = true;
    private bool m_showAdvanced;
    private bool m_showLimbs = true;
    tntHumanoidController controller;

    public void OnEnable()
    {
        controller = (tntHumanoidController)target;
    }

    private void AddToggle(string label, SerializedProperty toggle) {
        toggle.boolValue = EditorGUILayout.Toggle(label, toggle.boolValue);
    }

    private void AddFloat(string label, SerializedProperty val) {
        val.floatValue = EditorGUILayout.FloatField(label, val.floatValue);
    }

    private void AddObject(SerializedProperty obj) {
        EditorGUILayout.PropertyField(obj, true);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        m_showParamMenu = EditorGUILayout.Foldout(m_showParamMenu, "Parameters menu");
        if (m_showParamMenu) {
            EditorGUI.indentLevel++;
            AddToggle("Disable GRF Solver", serializedObject.FindProperty("m_disableGRFSolver"));
			AddToggle("Enable Constraint based PD Fix", serializedObject.FindProperty("m_constraintPDFix"));
            AddToggle("Use Blendspace", serializedObject.FindProperty("m_useBlendSpace"));
            AddToggle("Protect PD Param Set Script Object", serializedObject.FindProperty("m_protectPDParamSetScriptObject"));
            AddObject(serializedObject.FindProperty("m_pdParamSet"));
            AddToggle("Protect Control Params Script Object", serializedObject.FindProperty("m_protectControlParamsScriptObject"));
            if (controller.m_useBlendSpace) {
                AddFloat("Blendspace Granularity", serializedObject.FindProperty("m_blendGranularity"));
                if (Application.isPlaying) {
                    AddObject(serializedObject.FindProperty("m_controlParams"));
                }
                AddObject(serializedObject.FindProperty("m_controlParamsBlendSamples"));
            }
            else {
                AddObject(serializedObject.FindProperty("m_controlParams"));
            }
            EditorGUI.indentLevel--;
        }

        m_showLimbs = EditorGUILayout.Foldout(m_showLimbs, "Limbs");
        if (m_showLimbs) {
            EditorGUI.indentLevel++;
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_lowerBack"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_upperBack"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_neck"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_lShoulder"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_rShoulder"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_lToes"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_rToes"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_lHand"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_rHand"));

            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_headTarget"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_lHandTarget"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_rHandTarget"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_lFootTarget"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_rFootTarget"));
            AddObject(serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_rootTarget"));

            AddFloat("Limb Tracking Kp", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_limbTrackingKp"));
            AddFloat("Limb Tracking Kd", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_limbTrackingKd"));
            AddFloat("Limb Max Tracking Force", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_limbMaxTrackingForce"));

            AddFloat("Death Threshold for leg swing", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_deadThresholdSwingLeg"));
            AddFloat("Death Threshold for ground reaction", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_deadThresholdGRF"));
            AddToggle("Kick if Dead", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_kickIfDead"));

            AddToggle("Anti Leg Crossing", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_antiLegCrossing"));
            AddToggle("Step Relative to COM", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_stepRelativeToCOM"));
            AddToggle("Step Relative to Root", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_stepRelativeToRoot"));
            AddToggle("Enable Lean", serializedObject.FindProperty("m_limbs").FindPropertyRelative("m_enableLean"));
            EditorGUI.indentLevel--;
        }

        m_showAdvanced = EditorGUILayout.Foldout(m_showAdvanced, "Misc");
        if (m_showAdvanced) {
            EditorGUI.indentLevel++;
            AddToggle("Protect Desired Pose Script Object", serializedObject.FindProperty("m_protectDesiredPoseScriptObject"));
            AddToggle("Reflect Desired Pose", serializedObject.FindProperty("m_reflectDesiredPose"));
            AddToggle("Reflect Current Pose", serializedObject.FindProperty("m_reflectCurrentPose"));
            AddToggle("Reflect Controller State", serializedObject.FindProperty("m_reflectControllerState"));            
            AddToggle("Keep Root Position", serializedObject.FindProperty("m_keepRootPosition"));
            AddObject(serializedObject.FindProperty("m_currentPose"));
            AddObject(serializedObject.FindProperty("m_desiredPose"));
            m_showControllerState = EditorGUILayout.Foldout(m_showControllerState, "Controller state");
            if (m_showControllerState) {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                AddFloat("Stride Phase", serializedObject.FindProperty("controllerState").FindPropertyRelative("stridePhase"));
                AddFloat("L Stance Phase", serializedObject.FindProperty("controllerState").FindPropertyRelative("leftStancePhase"));
                AddFloat("R Stance Phase", serializedObject.FindProperty("controllerState").FindPropertyRelative("rightStancePhase"));
                AddFloat("L Swing Phase", serializedObject.FindProperty("controllerState").FindPropertyRelative("leftSwingPhase"));
                AddFloat("R Swing Phase", serializedObject.FindProperty("controllerState").FindPropertyRelative("rightSwingPhase"));
                AddToggle("Grounded", serializedObject.FindProperty("controllerState").FindPropertyRelative("grounded"));
                AddToggle("Stand still", serializedObject.FindProperty("controllerState").FindPropertyRelative("standStill"));
                AddToggle("Dead", serializedObject.FindProperty("controllerState").FindPropertyRelative("dead"));
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
            serializedObject.FindProperty("m_forward").vector3Value = EditorGUILayout.Vector3Field("Forward", serializedObject.FindProperty("m_forward").vector3Value);
            serializedObject.FindProperty("m_right").vector3Value = EditorGUILayout.Vector3Field("Right", serializedObject.FindProperty("m_right").vector3Value);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
