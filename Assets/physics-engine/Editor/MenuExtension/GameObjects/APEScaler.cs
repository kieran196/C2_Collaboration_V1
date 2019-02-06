using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class APEModelScalerWindow : EditorWindow {

    private APEModelScaler scalerClass;

    [MenuItem ("GameObject/Articulated Physics/APE Model Scaler")]
    static void Init ()
    {
        APEModelScalerWindow window = (APEModelScalerWindow)EditorWindow.GetWindow (typeof (APEModelScalerWindow));
        window.scalerClass = new APEModelScaler();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            window.scalerClass.RecognizeSelectedObject(Selection.transforms[i].gameObject); 
        window.Show();
    }

    void OnGUI()
    {
        if (scalerClass == null)
            return;
        scalerClass.OnGUI();
    }

    //When a selection change notification is received
    //recalculate the variables and references for the new object
    void OnSelectionChange()
    {
        if (scalerClass == null)
            return;
        scalerClass.ClearSelection();
        for (int i = 0; i < Selection.transforms.Length; ++i)
        { 
            scalerClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
        }
    }
}

public class APEModelScaler
{
    private GameObject m_obj; //Selected object in the Hierarchy
    private bool isHumanoid = false;
    private bool lastIsHumanoid = false;
    private APEModelScalerUtils.DIR charFaceDir;
    private tntLink lShoulder;
    private tntLink rShoulder;
    private tntLink lowerback_torso;
    private float geometryScalar;
    private float lastGeometryScalar;
    private float heightScalar;
    private float wingspanScalar;
    private float massScalar;
    private float moiScalar;
    private float forceScalar;
    private float torqueScalar;
    private float stiffnessScalar;
    private float damperScalar;
    private float velocityScalar;

    public APEModelScaler()
    {
        m_obj = null;
        lastGeometryScalar = geometryScalar = wingspanScalar = heightScalar = massScalar = moiScalar =
            forceScalar = torqueScalar = stiffnessScalar = damperScalar = velocityScalar = 1.0f;
    }

    public void ClearSelection()
    {
        m_obj = null;
    }

    public void OnGUI()
    {
        if (m_obj == null)
        {
            GUILayout.Label("No tntLinks are found from the game objects selected");
            return;
        }
        Component[] links = m_obj.GetComponentsInChildren<tntLink>();
        Dictionary<tntLink, bool> traversed = new Dictionary<tntLink, bool>();
        for (int i = 0; i < links.Length; ++i)
        {
            tntLink link = links[i] as tntLink;
            tntChildLink childLink = link as tntChildLink;
            if (childLink != null && !traversed.ContainsKey(childLink.m_parent))
            {
                GUILayout.Label("The tntLinks are not sorted in root => leaf order. Fix it first !");
                return;
            }
            traversed.Add(link, true);
        }

        isHumanoid = EditorGUILayout.Toggle ("is humanoid", isHumanoid);
        if (isHumanoid) {
            if (!lastIsHumanoid)
            {
                lastIsHumanoid = isHumanoid;
                RecognizeSelectedObject(m_obj);
            }
            lShoulder = (tntLink)EditorGUILayout.ObjectField ("left shoulder", lShoulder as UnityEngine.Object, typeof(tntLink), true);
            rShoulder = (tntLink)EditorGUILayout.ObjectField ("right shoulder", rShoulder as UnityEngine.Object, typeof(tntLink), true);
            lowerback_torso = (tntLink)EditorGUILayout.ObjectField ("lowerback torso", lowerback_torso as UnityEngine.Object, typeof(tntLink), true);

            heightScalar = EditorGUILayout.FloatField ("height scalar", heightScalar);
            wingspanScalar = EditorGUILayout.FloatField ("wingspan scalar", wingspanScalar);
            charFaceDir = (APEModelScalerUtils.DIR)EditorGUILayout.EnumPopup ("character facing direction", charFaceDir);
            geometryScalar = APEModelScalerUtils.getUnformScalar(wingspanScalar, heightScalar);
        } 
        else 
        {
            geometryScalar = EditorGUILayout.FloatField ("geometry scalar", geometryScalar);
        }
        
        if (geometryScalar != lastGeometryScalar)
        {
            lastGeometryScalar = geometryScalar;
            massScalar = APEModelScalerUtils.GetMassScalar(geometryScalar);
            moiScalar = APEModelScalerUtils.GetMOIScalar(geometryScalar);
            stiffnessScalar = APEModelScalerUtils.GetStiffnessScalar(geometryScalar);
            damperScalar = APEModelScalerUtils.GetDamperScalar(geometryScalar);
            forceScalar = APEModelScalerUtils.GetForceScalar(geometryScalar);
            torqueScalar = APEModelScalerUtils.GetTorqueScalar(geometryScalar);
            velocityScalar = APEModelScalerUtils.GetVelocityScalar(geometryScalar);
        }

        massScalar = EditorGUILayout.FloatField("mass scalar", massScalar);
        moiScalar = EditorGUILayout.FloatField("moi scalar", moiScalar);
        forceScalar = EditorGUILayout.FloatField("force scalar", forceScalar);
        torqueScalar = EditorGUILayout.FloatField("torque scalar", torqueScalar);
        if (GUILayout.Button("Scale Geometry")) 
        {
            RegisterUndo ();
            APEModelScalerUtils.ScaleGeometry(m_obj, heightScalar, wingspanScalar,
                                              lShoulder, rShoulder, lowerback_torso,
                                              geometryScalar, isHumanoid, charFaceDir);
        }
        if (GUILayout.Button("Scale Mass")) 
        {
            RegisterUndo ();
            APEModelScalerUtils.ScaleMass(m_obj, massScalar);
        }
        if (GUILayout.Button("Scale MOI")) 
        {
            RegisterUndo ();
            APEModelScalerUtils.ScaleMOI(m_obj, moiScalar);
        }
        if (GUILayout.Button("Scale Force and Torque Limits")) 
        {
            RegisterUndo ();
            APEModelScalerUtils.ScaleStrengths(m_obj, geometryScalar, stiffnessScalar, damperScalar,
                torqueScalar, forceScalar, velocityScalar);
        }
    }

    void RegisterUndo ()
    {
        Undo.RegisterFullObjectHierarchyUndo(m_obj, "APE rescale");
    }

    //Gather references for the selected object and its components
    //and update the pivot vector if the object has a Mesh specified
    public void RecognizeSelectedObject(GameObject _gameObject)
    {
        GameObject go = APEModelScalerUtils.ConfigCheckAPEObject(_gameObject, out lShoulder, out rShoulder, out lowerback_torso, isHumanoid);
        if (go != null)
            m_obj = go;      
    }
}