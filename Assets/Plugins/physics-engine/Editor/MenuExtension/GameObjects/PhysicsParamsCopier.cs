using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// Copy non-topological Physics Parameters between APE rigs
public class PhysicsParamsCopierWindow : EditorWindow
{

    private PhysicsParamsCopier copierClass;

    [MenuItem("GameObject/Articulated Physics/Physics Parameters Copier")]
    static void Init()
    {
        PhysicsParamsCopierWindow window = (PhysicsParamsCopierWindow)EditorWindow.GetWindow(typeof(PhysicsParamsCopierWindow));
        window.copierClass = new PhysicsParamsCopier();
        for (int i = 0; i < Selection.transforms.Length; ++i)
            window.copierClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
        window.Show();
    }

    void OnGUI()
    {
        if (copierClass != null)
            copierClass.OnGUI();
    }

    //When a selection change notification is received
    //recalculate the variables and references for the new object
    void OnSelectionChange()
    {
        //for (int i = 0; i < Selection.transforms.Length; ++i)
        //    moverClass.RecognizeSelectedObject(Selection.transforms[i].gameObject);
    }
}

public class PhysicsParamsCopier
{
    private List<GameObject> objs; //Selected object in the Hierarchy
    int srcModelIndex, dstModelIndex;
    Component[] links;
    bool[] linkFlags;

    public PhysicsParamsCopier()
    {
        objs = new List<GameObject>();
        srcModelIndex = 0;
        dstModelIndex = 1;
    }

    private void UpdateSrcLinks()
    {
        links = objs[srcModelIndex].GetComponentsInChildren<tntLink>();
        linkFlags = new bool[links.Length];
        for (int i = 0; i < links.Length; ++i)
            linkFlags[i] = true;
    }

    private void CopyRigidBody(tntRigidBody srcBody, tntRigidBody dstBody)
    {
        dstBody.m_collidable = srcBody.m_collidable;
        dstBody.m_mass = srcBody.m_mass;
        dstBody.m_moi = srcBody.m_moi;
        dstBody.m_material = srcBody.m_material;
    }

    private void CopyTntLink(tntLink srcLink, tntLink dstLink)
    {
        dstLink.m_collidable = srcLink.m_collidable;
        dstLink.m_mass = srcLink.m_mass;
        dstLink.m_moi = srcLink.m_moi;
        dstLink.m_material = srcLink.m_material;
        dstLink.m_mark = srcLink.m_mark;

        // In case of compound collider need to copy masses under all child tntRigidBodies
        Component[] srcbodies = srcLink.GetComponentsInChildren<tntRigidBody>();
        Component[] dstbodies = dstLink.GetComponentsInChildren<tntRigidBody>();
        if (srcbodies.Length != dstbodies.Length)
        {
            GUILayout.Label("Source link " + srcLink.name + " has " + srcbodies.Length + "tntRigidBodies while Dest link " +
                dstLink.name + " has " + dstbodies.Length + " tntRigidBodies !  Partial Copy !!");
            Debug.LogWarning("Source link " + srcLink.name + " has " + srcbodies.Length + "tntRigidBodies while Dest link " +
                dstLink.name + " has " + dstbodies.Length + " tntRigidBodies !  Partial Copy!!");
            return;
        }
        for (int i = 0; i < Mathf.Min(srcbodies.Length, dstbodies.Length); ++i)
        {
            tntRigidBody srcBody = srcbodies[i] as tntRigidBody;
            tntRigidBody dstBody = dstbodies[i] as tntRigidBody;
            CopyRigidBody(srcBody, dstBody);
        }
    }

    private void CopyTntBase(tntBase srcBase, tntBase dstBase)
    {
        dstBase.m_enableSelfCollision = srcBase.m_enableSelfCollision;
        dstBase.m_highDefIntegrator = srcBase.m_highDefIntegrator;
        dstBase.m_simulationFrequencyMultiplier = srcBase.m_simulationFrequencyMultiplier;
        dstBase.m_useGlobalVel = srcBase.m_useGlobalVel;
        dstBase.m_linearDamping = srcBase.m_linearDamping;
        dstBase.m_angularDamping = srcBase.m_angularDamping;
        dstBase.m_maxCoordinateVelocity = srcBase.m_maxCoordinateVelocity;
        CopyTntLink(srcBase, dstBase);
    }

    private void CopyDofData(tntDofData srcDof, tntDofData dstDof)
    {
        dstDof.m_useMotor = srcDof.m_useMotor;
        dstDof.m_isPositionMotor = srcDof.m_isPositionMotor;
        dstDof.m_desiredPosition = srcDof.m_desiredPosition;
        dstDof.m_desiredVelocity = srcDof.m_desiredVelocity;
        dstDof.m_maxMotorForce = srcDof.m_maxMotorForce;
        dstDof.m_positionLockThreshold = srcDof.m_positionLockThreshold;
        dstDof.m_useAutomaticPositionLockThreshold = srcDof.m_useAutomaticPositionLockThreshold;
        dstDof.m_springStiffness = srcDof.m_springStiffness;
        dstDof.m_springDamping = srcDof.m_springDamping;
        dstDof.m_neutralPoint = srcDof.m_neutralPoint;
        dstDof.m_continuousForce = srcDof.m_continuousForce;
        dstDof.m_useLimit = srcDof.m_useLimit;
        dstDof.m_limitLow = srcDof.m_limitLow;
        dstDof.m_limitHigh = srcDof.m_limitHigh;
        dstDof.m_maxLimitForce = srcDof.m_maxLimitForce;
    }

    private void CopyTntChildLink(tntChildLink srcLink, tntChildLink dstLink)
    {
        CopyTntLink(srcLink, dstLink);
        dstLink.m_kp = srcLink.m_kp;
        dstLink.m_kd = srcLink.m_kd;
        dstLink.m_maxPDTorque = srcLink.m_maxPDTorque;

        for (int i = 0; i < srcLink.m_dofData.Length; ++i)
        {
            if (i >= srcLink.m_dofData.Length)
            {
                Debug.LogError("srcLink " + srcLink.name + " nDOF = " + srcLink.m_dofData.Length);
                continue;
            }
            if (i >= dstLink.m_dofData.Length)
            {
                Debug.LogError("dstLink " + dstLink.name + " nDOF = " + dstLink.m_dofData.Length);
                continue;
            }
            CopyDofData(srcLink.m_dofData[i], dstLink.m_dofData[i]);
        }
    }

    private void CopyParameters()
    {
        GameObject dstObj = objs[dstModelIndex];
        for (int i = 0; i < links.Length; ++i)
        {
            tntLink link = links[i] as tntLink;
            Transform childTrans = dstObj.transform.Find(link.name);
            if (childTrans == null)
            {
                GUILayout.Label("Cannot find " + link.name + " in destination model");
                Debug.LogWarning("Cannot find " + link.name + " in destination model");
                continue;
            }

            if (link as tntBase)
            {
                tntBase baseLink = link as tntBase;
                tntBase dstBase = childTrans.GetComponent<tntBase>();
                if (dstBase == null)
                {
                    GUILayout.Label("Cannot find base link named " + link.name + " in destination model");
                    return;
                }
                CopyTntBase(baseLink, dstBase);
            }
            else
            {
                tntChildLink childLink = links[i] as tntChildLink;
                tntChildLink dstLink = childTrans.GetComponent<tntChildLink>();
                if (dstLink == null)
                {
                    GUILayout.Label("Cannot find child link named " + link.name + " in destination model");
                    return;
                }
                CopyTntChildLink(childLink, dstLink);
            }
        }
    }

    public void OnGUI()
    {
        if (objs.Count < 2)
        {
            GUILayout.Label("Two APE models need to be selected!");
            return;
        }

        EditorGUILayout.LabelField("Source:       " + objs[srcModelIndex].name);
        EditorGUILayout.LabelField("Destination:  " + objs[dstModelIndex].name);
        if (GUILayout.Button("Swap source and destination"))
        {
            int tmp = srcModelIndex;
            srcModelIndex = dstModelIndex;
            dstModelIndex = tmp;
            UpdateSrcLinks();
        }

        for (int i = 0; i < links.Length; ++i)
        {
            tntLink link = links[i] as tntLink;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(link.name);
            linkFlags[i] = EditorGUILayout.Toggle(linkFlags[i]);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Copy Above Bones"))
        {
            CopyParameters();
        }
    }

    //Gather references for the selected object
    public void RecognizeSelectedObject(GameObject gameObject)
    {
        if (gameObject == null)
            return;
        objs.Add(gameObject);
        UpdateSrcLinks();
    }
}