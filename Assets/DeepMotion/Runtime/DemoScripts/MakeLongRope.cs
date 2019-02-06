using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeLongRope : MonoBehaviour
{
    public GameObject capsule;
    private GameObject[] capsules;
    private Vector3 objPos;
    private Quaternion objRot;
    [Tooltip("The mass of the base link. Generated child links use 0.1 as the mass.")]
    public float basemass;
    public bool enableSelfCollision;
    [Tooltip("Set the simulation frequency multiplier.")]
    public int simMulti;
    [Tooltip("Generates the rope in a horizontal configuration. Use with base mass = 0 to create a swinging rope.")]
    public bool generateHorizontal;
	[Tooltip("Enable velocity motor for damping")]
	public bool enableVelocityMotor = false;
	[Tooltip("Enable pose controller for damping")]
	public bool enablePoseController = false;
    public int numLinks;
    public bool useJointLimits;
    [Tooltip("The high and low joint limit value. Value with set both (limit High as positive, Low as negative of the input value.")]
    public float limitLowHigh;
    public float limitStrength;

    void Start()
    {
        capsules = new GameObject[numLinks];
        if (limitLowHigh < 0) limitLowHigh *= -1;
        objPos = gameObject.transform.position;
        if (generateHorizontal)
            capsule.transform.eulerAngles = new Vector3(0, 0, -90f);
        objRot = capsule.transform.rotation;
        capsule.SetActive(false);
        CreateRope();
        EnableRope();
    }

    private void CreateRope()
    {
        for (int i = 0; i < numLinks; i++)
        {
            GameObject go = GameObject.Instantiate(capsule, objPos, objRot, gameObject.transform);
            go.name = capsule.name + i.ToString();
            if (i == 0)
            {
                tntBase goBase = go.AddComponent<tntBase>();
                goBase.mass = basemass;
                goBase.m_enableSelfCollision = enableSelfCollision;
                goBase.m_simulationFrequencyMultiplier = simMulti;
				goBase.m_useGlobalVel = false;
				goBase.m_highDefIntegrator = false;
				goBase.m_linearDamping = 0.1f;
				goBase.m_angularDamping = 0.1f;

				if (enablePoseController) {
					GameObject poseController = new GameObject ();
					poseController.transform.SetParent (goBase.transform);
					tntPoseController pose = poseController.AddComponent<tntPoseController> ();
					pose.m_limbs = new LimbConfiguration ();
					pose.m_trackingChains = new tntTrackingChain[0];
				}
            }
            else
            {
				tntBallLink goLink = go.AddComponent<tntBallLink>();
                goLink.mass = 1f;
                goLink.m_parent = capsules[i - 1].GetComponent<tntLink>();
                goLink.PivotB = new Vector3(0, 1.2f, 0);
                goLink.AutoFillPivotA();
				goLink.m_kp = 10f;
				goLink.m_kd = 1f;
				goLink.m_maxPDTorque = 100f;
				goLink.m_useSoftConstraint = true;
     
                for (int j = 0; j < 2; j++)
                {
					tntDofData gLDoF = goLink.m_dofData [j];
					if (useJointLimits) {
	
						gLDoF.m_useLimit = true;
						gLDoF.m_limitHigh = limitLowHigh;
						gLDoF.m_limitLow = -limitLowHigh;
						gLDoF.m_maxLimitForce = Mathf.Abs (limitStrength);
					}
					if (enableVelocityMotor) {
						gLDoF.m_useMotor = true;
						gLDoF.m_isPositionMotor = false;
						gLDoF.m_desiredVelocity = 0;
						gLDoF.m_maxMotorForce = 100;
					}
                }
            }
            capsules[i] = go;
            if (!generateHorizontal)
                objPos = objPos + new Vector3(0, -1.98f, 0);
            else
                objPos = objPos + new Vector3(-1.98f, 0, 0);
        }
    }

    private void EnableRope()
    {
        for(int i = numLinks - 1; i > -1; i--)
        {
            capsules[i].SetActive(true);
        }
    }
}
