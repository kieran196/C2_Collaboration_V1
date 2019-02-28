using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(tntHumanoidControlParams))]
[CanEditMultipleObjects]

public class tntHumanoidControlParamsInspector : Editor
{
	// ---------------------- Control Parameters ---------------------
	private SerializedProperty m_controlParams;

    public void OnEnable()
    {
		m_controlParams = serializedObject.FindProperty("m_params");
    }

	static void AddControlParamSlider(SerializedProperty controlParams, int index, string name, float min, float max)
	{
		SerializedProperty controlParam = controlParams.GetArrayElementAtIndex(index);
		controlParam.floatValue = EditorGUILayout.Slider(new GUIContent(name), controlParam.floatValue, min, max);
	}
	
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
		
		// Note: the control parameter layout must match the engine internal control parameter layout
		Debug.Log("Number of Control Parameters = " + m_controlParams.arraySize);
        EditorGUILayout.LabelField("----------------------- Control Parameters  --------------------------");
        DrawParamControlPanel(m_controlParams);
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    public static void DrawParamControlPanel(SerializedProperty controlParams)
    {
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STRIDE_DURATION, "stride duration", 0.1f, 3.0f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT, "desired height", 0.0f, 2.0f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ1, "body frame height1", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ2, "body frame height2", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ3, "body frame height3", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ4, "body frame height4", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ5, "body frame height5", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ6, "body frame height6", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_KP_SAGITTAL, "COM vforce kp forward", -100000f, 100000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_KD_SAGITTAL, "COM vforce kd forward", -100000f, 100000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_KP_CORONAL, "COM vforce kp coronal", -100000f, 100000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_KD_CORONAL, "COM vforce kd coronal", -200000f, 200000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_KP_VERTICAL, "COM vforce kp vertical", -100000f, 100000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_KD_VERTICAL, "COM vforce kd vertical", -20000f, 20000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_VFORCE_FF_VERTICAL_WEIGHT_PERCENTAGE, "COM feedforward vforce vertical", 0f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BODYFRAME_TORQUE_KP, "BodyFrame orientation KP", 0f, 25000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BODYFRAME_TORQUE_KD, "BodyFrame orientation KD", 0f, 10000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD, "body frame lean forward", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS, "body frame lean sideways", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ1, "body frame forward lean1", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ2, "body frame forward lean2", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ3, "body frame forward lean3", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ4, "body frame forward lean4", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ5, "body frame forward lean5", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ6, "body frame forward lean6", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ1, "body frame side lean1", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ2, "body frame side lean2", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ3, "body frame side lean3", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ4, "body frame side lean4", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ5, "body frame side lean5", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ6, "body frame side lean6", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST, "spine twist", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD, "spine slouch forward", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS, "spine slouch sideways", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST_TRAJ1, "spine twist offset1", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST_TRAJ2, "spine twist offset2", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST_TRAJ3, "spine twist offset3", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST_TRAJ4, "spine twist offset4", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST_TRAJ5, "spine twist offset5", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_TWIST_TRAJ6, "spine twist offset6", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ1, "spine slouch forward offset1", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ2, "spine slouch forward offset2", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ3, "spine slouch forward offset3", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ4, "spine slouch forward offset4", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ5, "spine slouch forward offset5", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ6, "spine slouch forward offset6", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ1, "spine slouch sideways offset1", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ2, "spine slouch sideways offset2", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ3, "spine slouch sideways offset3", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ4, "spine slouch sideways offset4", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ5, "spine slouch sideways offset5", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ6, "spine slouch sideways offset6", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_LEG_PLANE_ANGLE_LEFT, "left leg rotation angle", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_LEG_PLANE_ANGLE_RIGHT, "right leg rotation angle", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ1, "swing foot height1", -0.5f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ2, "swing foot height2", -0.5f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ3, "swing foot height3", -0.5f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ4, "swing foot height4", -0.5f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ5, "swing foot height5", -0.5f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ6, "swing foot height6", -0.5f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_ANKLE_ROT1, "swing ankle rotation1", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_ANKLE_ROT2, "swing ankle rotation2", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_ANKLE_ROT3, "swing ankle rotation3", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_ANKLE_ROT4, "swing ankle rotation4", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_ANKLE_ROT5, "swing ankle rotation5", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_ANKLE_ROT6, "swing ankle rotation6", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_TOE_ROT1, "swing toe rotation1", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_TOE_ROT2, "swing toe rotation2", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_TOE_ROT3, "swing toe rotation3", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_TOE_ROT4, "swing toe rotation4", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_TOE_ROT5, "swing toe rotation5", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_TOE_ROT6, "swing toe rotation6", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_ANKLE_ROT1, "stance ankle rotation1", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_ANKLE_ROT2, "stance ankle rotation2", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_ANKLE_ROT3, "stance ankle rotation3", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_ANKLE_ROT4, "stance ankle rotation4", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_ANKLE_ROT5, "stance ankle rotation5", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_ANKLE_ROT6, "stance ankle rotation6", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_TOE_ROT1, "stance toe rotation1", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_TOE_ROT2, "stance toe rotation2", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_TOE_ROT3, "stance toe rotation3", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_TOE_ROT4, "stance toe rotation4", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_TOE_ROT5, "stance toe rotation5", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STANCE_TOE_ROT6, "stance toe rotation6", -3.14f, 3.14f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_WIDTH, "step width", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_FORWARD_OFFSET, "step forward offset", -0.5f, 0.5f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_VFORCE_KP, "swing foot tracking vforce kp", 0f, 100000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_VFORCE_KD, "swing foot tracking vforce kd", 0f, 20000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_SWING_FOOT_MAX_FORCE, "swing foot tracking max vforce", 0f, 25000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_LEFT_LEG_SWING_START, "gait: left leg swing start", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_LEFT_LEG_SWING_END, "gait: left leg swing end", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_RIGHT_LEG_SWING_START, "gait: right leg swing start", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_RIGHT_LEG_SWING_END, "gait: right leg swing end", -1f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION1, "swing step target interp1", 0f, 1.2f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION2, "swing step target interp2", 0f, 1.2f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION3, "swing step target interp3", 0f, 1.2f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION4, "swing step target interp4", 0f, 1.2f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION5, "swing step target interp5", 0f, 1.2f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION6, "swing step target interp6", 0f, 1.2f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_GRF_REGULARIZER, "Minimize GRF", 0f, 1f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_GRF_TORQUE_TO_FORCE_OBJECTIVE_RATIO, "GRF Objective Quality", 0f, 1000f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_MAX_CONTACT_POINTS_PER_FOOT, "Max FCCP per foot", 0f, 8f);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_ROTATION_FRICTION_KD, "Foot rot friction kd", 0, 10000);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_GYRO_RATIO, "Gyro ratio", 0, 1);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_STAND_STILL_THRESHOLD, "Stand still threshold", 0, 10);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_EARLY_SWING_TERMINATE, "Early swing termination due to contact", 0, 1);
        AddControlParamSlider(controlParams, tntHumanoidControlParams.P_LATE_SWING_TERMINATE, "Late swing termination due to lack of contact", 0, 1);

    }
}