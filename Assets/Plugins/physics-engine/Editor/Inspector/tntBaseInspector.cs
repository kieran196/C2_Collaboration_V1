using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntBase))]
[CanEditMultipleObjects]

public class tntBaseInspector : tntLinkInspector
{
	private SerializedProperty m_enableSelfCollision;
    private SerializedProperty m_highDefIntegrator;
    private SerializedProperty m_useGlobalVel;
    private SerializedProperty m_simulationFrequencyMultiplier;
    private SerializedProperty m_useWorldSolverFidelityIndex;
    private SerializedProperty m_solverFidelityIndex;
    private SerializedProperty m_linearDamping;
    private SerializedProperty m_angularDamping;
    private SerializedProperty m_maxCoordinateVelocity;
    private SerializedProperty m_bKinematic;

	public void OnEnable()
	{
		BindProperties();
		m_enableSelfCollision = serializedObject.FindProperty("m_enableSelfCollision");
        m_highDefIntegrator = serializedObject.FindProperty("m_highDefIntegrator");
        m_simulationFrequencyMultiplier = serializedObject.FindProperty("m_simulationFrequencyMultiplier");
        m_useWorldSolverFidelityIndex = serializedObject.FindProperty("m_useWorldSolverFidelityIndex");
        m_solverFidelityIndex = serializedObject.FindProperty("m_simulationFrequencyMultiplier");
        m_useGlobalVel = serializedObject.FindProperty("m_useGlobalVel");
        m_linearDamping = serializedObject.FindProperty("m_linearDamping");
        m_angularDamping = serializedObject.FindProperty("m_angularDamping");
        m_maxCoordinateVelocity = serializedObject.FindProperty("m_maxCoordinateVelocity");
        m_bKinematic = serializedObject.FindProperty("m_IsKinematic");
	}

    // Hack to get velocity transferred to articulations
    int transferVelocitiesFromPreviousFrame;
    float previousFrameTime;
    Vector3 previousFrameVelocity;
    Vector3 previousFrameAngularVelocity;

    public override void OnInspectorGUI()
	{
        tntBase baseLink = (tntBase)target;

        UpdateGUI();
        GUI.enabled = false;
        EditorGUILayout.FloatField(new GUIContent("Total Mass"), baseLink.GetTotalMass());
        GUI.enabled = true;
        EditorGUILayout.PropertyField(m_enableSelfCollision);
        EditorGUILayout.PropertyField(m_highDefIntegrator);
        EditorGUILayout.PropertyField(m_simulationFrequencyMultiplier);

        EditorGUILayout.PropertyField(m_useWorldSolverFidelityIndex);
        baseLink.m_useWorldSolverFidelityIndex = m_useWorldSolverFidelityIndex.boolValue;

        EditorGUI.BeginDisabledGroup(baseLink.m_useWorldSolverFidelityIndex);
        SolverFidelityIndex newFidelityIndex = (SolverFidelityIndex)EditorGUILayout.EnumPopup("Solver Fidelity Index", baseLink.RequiredSolverFidelityIndex); 
        baseLink.RequiredSolverFidelityIndex = newFidelityIndex;
        EditorGUI.EndDisabledGroup();


        EditorGUILayout.PropertyField(m_useGlobalVel);
        EditorGUILayout.PropertyField(m_linearDamping);
        EditorGUILayout.PropertyField(m_angularDamping);
        EditorGUILayout.PropertyField(m_maxCoordinateVelocity);
        Vector3 newLinVel = EditorGUILayout.Vector3Field(new GUIContent("Linear Velocity", tntRigidBodyInspector.linearVelocityTooltip), baseLink.linearVelocity);
        Vector3 newAngVel = EditorGUILayout.Vector3Field(new GUIContent("Angular Velocity", tntRigidBodyInspector.angularVelocityTooltip), baseLink.angularVelocity);

        EditorGUILayout.PropertyField(m_bKinematic);

        GUI.enabled = !Application.isPlaying;
        {
            if (GUILayout.Button("Recompute link indices"))
            {
                if (EditorUtility.DisplayDialog(
                    "Are you sure you want to recompute indices?",
                    "This will force link indices for the entire articulation to be recomputed which can make certain dependent assets outdated",
                    "Yes", "Cancel"))
                {
                    baseLink.AssignValidChildLinkIndices(true);
                    
                    // brute force method to enable saving the changes
                    // TODO: implement a more elegant solution (mind you: all child links need to be set 'dirty')
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
            }
            EditorGUILayout.Space();
        }
        GUI.enabled = true;

        if (GUI.changed)
	    {
			float oldMass = baseLink.m_mass;
            bool oldKinematic = baseLink.GetKinematic();
			SaveOldProperties();
			serializedObject.ApplyModifiedProperties();
			float newMass = baseLink.m_mass;
			if (oldMass != newMass)
			{
				// Debug.Log("Old mass=" + oldMass + " New mass=" + newMass);
				for (int i = 0; i < targets.Length; ++i)
				{
					tntBase theBase = (tntBase)targets[i];
                    theBase.m_mass = newMass;
                    theBase.SetMass(newMass);
				}
			}
            bool newKinematic = baseLink.GetKinematic();
            if (oldKinematic != newKinematic)
            {
                baseLink.ForceSetKinematic();
            }

            if (newLinVel != baseLink.linearVelocity || newAngVel != baseLink.angularVelocity)
            {
                baseLink.linearVelocity = previousFrameVelocity;
                baseLink.angularVelocity = previousFrameAngularVelocity;
                previousFrameVelocity = newLinVel;
                previousFrameAngularVelocity = newAngVel;
                transferVelocitiesFromPreviousFrame = 1;
                previousFrameTime = Time.time;

            }

            CheckModifiedProperties();
	    }

        // Hack to get velocity transferred to articulations: velocity must be applied on the following frame. (and also sometimes it failed, hence we tried applying it multiple times)
        if (transferVelocitiesFromPreviousFrame > 0 && previousFrameTime != Time.time)
        {
            baseLink.linearVelocity = previousFrameVelocity;
            baseLink.angularVelocity = previousFrameAngularVelocity;
            transferVelocitiesFromPreviousFrame--;
        }
    }
}

