using UnityEngine;
using UnityEditor;

public class tntChildLinkInspector : tntLinkInspector 
{
    private SerializedProperty m_parent;
    private SerializedProperty m_collideWithParent;
    private SerializedProperty m_mirroredLink;
    private SerializedProperty[] m_dofDataElements;
    private SerializedProperty m_feedback;
    private SerializedProperty m_kp;
    private SerializedProperty m_kd;
    private SerializedProperty m_maxPDTorque;
    private tntHumanoidController m_controller;
    private tntBase m_rootBase;
    private SerializedProperty m_useSoftConstraint;
    private SerializedProperty m_breakingReactionImpulse;
    private tntChildLink m_childLink;
    private Transform m_topMostParent;

    private GUIContent[] m_doflabels;
    private tntDofData[] m_oldDofData;
    private int m_tntDofDataSize;
    private float m_oldMass;
    private tntLink m_oldParent;

    // inspectors are recreated each time you select/deselect a target and thus inspector state is reset ..
    // .. so we need to use statics if we want to preserve settings; seems to match the way Unity's ..
    // .. works by default too (see m_feeedback folding as an example)
    static private bool m_showPoseTrackingRelatedMembers = false;

    protected override void BindProperties() 
    {
        base.BindProperties();
        m_childLink = (tntChildLink)target;
        m_parent = serializedObject.FindProperty("m_parent");
        m_collideWithParent = serializedObject.FindProperty("m_collideWithParent");
        m_mirroredLink = serializedObject.FindProperty("m_mirroredLink");
        m_feedback = serializedObject.FindProperty("m_feedback");
        m_kp = serializedObject.FindProperty("m_kp");
        m_kd = serializedObject.FindProperty("m_kd");
        m_maxPDTorque = serializedObject.FindProperty("m_maxPDTorque");
        m_topMostParent = m_childLink.transform;
        while (m_topMostParent != null && m_controller == null && m_rootBase == null)
        {
            m_controller = m_topMostParent.GetComponentInChildren<tntHumanoidController>();
            m_rootBase = m_topMostParent.GetComponentInChildren<tntBase>();
            m_topMostParent = m_topMostParent.parent;
        }

        m_useSoftConstraint = serializedObject.FindProperty("m_useSoftConstraint");
        m_breakingReactionImpulse = serializedObject.FindProperty("m_breakingReactionImpulse");

        SerializedProperty dofSize = serializedObject.FindProperty("m_dofData.Array.size");
        if (dofSize == null) 
        {
            m_tntDofDataSize = 0;
        }
        else 
        {
            m_tntDofDataSize = dofSize.intValue;
            m_oldDofData = new tntDofData[m_tntDofDataSize];
            m_dofDataElements = new SerializedProperty[m_tntDofDataSize];
            m_doflabels = new GUIContent[m_tntDofDataSize];
            for (int i = 0; i < m_tntDofDataSize; ++i) 
            {
                m_dofDataElements[i] = serializedObject.FindProperty(
                    string.Format("m_dofData.Array.data[{0}]", i));
                m_doflabels[i] = new GUIContent(string.Format("DOF #{0} Settings", i + 1),
                                                "Per Dof mobilizer data");
            }
        }
    }

    protected override void UpdateGUI() 
    {
        tntChildLink childLink = target as tntChildLink;

        base.UpdateGUI();
        EditorGUILayout.PropertyField(m_parent);
        EditorGUILayout.PropertyField(m_collideWithParent);
        EditorGUILayout.PropertyField(m_mirroredLink);

        EditorGUILayout.PropertyField(m_feedback, true);
        m_showPoseTrackingRelatedMembers = EditorGUILayout.Foldout(m_showPoseTrackingRelatedMembers, "Controller pose tracking settings", EditorStyles.foldout);
        if (m_showPoseTrackingRelatedMembers) 
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_kp);
            EditorGUILayout.PropertyField(m_kd);
            EditorGUILayout.PropertyField(m_maxPDTorque);
            EditorGUILayout.PropertyField(m_useSoftConstraint, true);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying) 
            {
                if (m_controller == null || m_rootBase == null)
                    Debug.LogWarning("Child link has no controller or root in heirarchy");
                else
                    m_controller.UpdatePdParams(m_rootBase.NameToIndex(m_childLink.name), m_kp.floatValue, m_kd.floatValue, m_maxPDTorque.floatValue);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(m_breakingReactionImpulse, true);

        if (m_tntDofDataSize > 0) 
        {
            EditorGUILayout.IntField("Number of DOF", m_tntDofDataSize);
            for (int i = 0; i < m_tntDofDataSize; ++i) 
            {
                EditorGUILayout.PropertyField(m_dofDataElements[i], m_doflabels[i], true);
                EditorGUILayout.FloatField(string.Format("DOF #{0} Position", i + 1),
                    childLink.CurrentPosition[i]);
                EditorGUILayout.FloatField(string.Format("DOF #{0} Velocity", i + 1),
                    childLink.CurrentVelocity[i]);
            }
        }
    }

    protected override void SaveOldProperties() 
    {
        base.SaveOldProperties();

        tntChildLink chlidLink = (tntChildLink)target;
        m_oldMass = chlidLink.m_mass;

        m_oldParent = chlidLink.m_parent;

        // Save current values of reflected properties
        tntChildLink childLink = (tntChildLink)target;
        if (childLink.m_dofData == null)
            return;
        for (int i = 0; i < childLink.m_dofData.Length; ++i)
            m_oldDofData[i] = new tntDofData(childLink.m_dofData[i]);
    }

    protected override void CheckModifiedProperties() 
    {
        base.CheckModifiedProperties();

        tntChildLink chlidLink = (tntChildLink)target;
        if (chlidLink.m_parent == chlidLink)
        {
            Debug.LogError("tntChildLink '" + chlidLink.name + "' parent cannot be set to itself. Please set it to a different object.");
            chlidLink.m_parent = null;
            return;
        }

        float newMass = chlidLink.m_mass;
        if (m_oldMass != newMass) 
        {
            for (int i = 0; i < targets.Length; ++i) 
            {
                chlidLink = (tntChildLink)targets[i];
                chlidLink.m_mass = newMass;
                chlidLink.SetMass(newMass);
            }
        }

        tntLink newParent = chlidLink.m_parent;
        if (m_oldParent != newParent)
        {
            for (int i = 0; i < targets.Length; ++i)
            {
                chlidLink = (tntChildLink)targets[i];
                chlidLink.m_parent = newParent;
            }

            chlidLink.AssignValidChildLinkIndices();
        }

        tntChildLink childLink = (tntChildLink)target;
        if (childLink.m_dofData == null)
            return;

        for (int i = 0; i < childLink.m_dofData.Length; ++i) 
        {
            tntDofData dofData = childLink.m_dofData[i];
            if (dofData.m_useMotor != m_oldDofData[i].m_useMotor) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    if (dof.m_useMotor)
                        link.AddMotor(i, dof.m_isPositionMotor, dof.m_desiredVelocity,
                                      dof.m_desiredPosition, dof.m_maxMotorForce, dof.m_positionLockThreshold, dof.m_useAutomaticPositionLockThreshold);
                    else
                        link.RemoveMotor(i);
                }
            }

            if (dofData.m_isPositionMotor != m_oldDofData[i].m_isPositionMotor) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetMotorIsPositional(i, dof.m_isPositionMotor);
                }
            }

            if (dofData.m_desiredVelocity != m_oldDofData[i].m_desiredVelocity) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetMotorDesiredSpeed(i, dof.m_desiredVelocity);
                }
            }

            if (dofData.m_desiredPosition != m_oldDofData[i].m_desiredPosition) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetMotorDesiredPosition(i, dof.m_desiredPosition);
                }
            }

            if (dofData.m_maxMotorForce != m_oldDofData[i].m_maxMotorForce) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetMotorMaxForce(i, dof.m_maxMotorForce);
                }
            }

            if (dofData.m_positionLockThreshold != m_oldDofData[i].m_positionLockThreshold) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetMotorPositionLockThreshold(i, dof.m_positionLockThreshold);
                }
            }

            if (dofData.m_useAutomaticPositionLockThreshold != m_oldDofData[i].m_useAutomaticPositionLockThreshold)
            {
                for (int j = 0; j < targets.Length; ++j)
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetMotorUseAutomaticPositionLockThreshold(i, dof.m_useAutomaticPositionLockThreshold);
                }
            }

            if (dofData.m_useLimit != m_oldDofData[i].m_useLimit) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    if (dof.m_useLimit)
                        link.AddLimits(i, dof.m_limitLow, dof.m_limitHigh, dof.m_maxLimitForce);
                    else
                        link.RemoveLimit(i);
                }
            }

            if (dofData.m_limitLow != m_oldDofData[i].m_limitLow) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetLimitLower(i, dof.m_limitLow);
                }
            }

            if (dofData.m_limitHigh != m_oldDofData[i].m_limitHigh) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetLimitUpper(i, dof.m_limitHigh);
                }
            }

            if (dofData.m_maxLimitForce != m_oldDofData[i].m_maxLimitForce) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetLimitMaxForce(i, dof.m_maxLimitForce);
                }
            }

            if (dofData.m_springStiffness != m_oldDofData[i].m_springStiffness) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetSpringStiffness(i, dof.m_springStiffness);
                }
            }

            if (dofData.m_springDamping != m_oldDofData[i].m_springDamping) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetSpringDamping(i, dof.m_springDamping);
                }
            }

            if (dofData.m_neutralPoint != m_oldDofData[i].m_neutralPoint) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetSpringNeutralPoint(i, dof.m_neutralPoint);
                }
            }

            if (dofData.m_continuousForce != m_oldDofData[i].m_continuousForce) 
            {
                for (int j = 0; j < targets.Length; ++j) 
                {
                    tntChildLink link = (tntChildLink)targets[j];
                    tntDofData dof = link.m_dofData[i];
                    link.SetContinuousForceActuator(i, dof.m_continuousForce);
                }
            }
        }
    }
}

