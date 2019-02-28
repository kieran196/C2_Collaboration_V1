using UnityEngine;
using System.Collections;

public class MonsterTruckController : MonoBehaviour {

    private tntHingeLink steeringFL;
    private tntHingeLink steeringFR;
    private tntHingeLink wheelFL;
    private tntHingeLink wheelFR;
    private tntHingeLink wheelRL;
    private tntHingeLink wheelRR;
    private float frontWheelRadius;
    private float rearWheelRadius;
    private float frontRearRatio;

    //private float vertical;
    //private float horizontal;
    
    private float velocity;
#if UNITY_IPHONE && !UNITY_EDITOR
    private UnityStandardAssets.CrossPlatformInput.Joystick joystick;
#endif
    public float acceleration = 286.5f;     // degrees
    public float breaking = 2865.0f;        // degrees
    public float maxSteering = 50f;         // degrees
    public float maxVelocity = 2865.0f;     // degrees

    // Use this for initialization
	void Start () {
        steeringFL = transform.Find("SteerFL").GetComponent<tntHingeLink>();
        steeringFR = transform.Find("SteerFR").GetComponent<tntHingeLink>();
        wheelFL = transform.Find("WheelFL").GetComponent<tntHingeLink>();
        wheelFR = transform.Find("WheelFR").GetComponent<tntHingeLink>();
        wheelRL = transform.Find("WheelRL").GetComponent<tntHingeLink>();
        wheelRR = transform.Find("WheelRR").GetComponent<tntHingeLink>();
        frontWheelRadius = wheelFL.GetComponent<tntCylinderCollider>().Radius;
        rearWheelRadius = wheelRL.GetComponent<tntCylinderCollider>().Radius;
        frontRearRatio = frontWheelRadius / rearWheelRadius;

        //vertical = horizontal = 0.0f;
        velocity = 0.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        float vertical = 0.0f;
        float horizontal = 0.0f;
#if UNITY_IPHONE && !UNITY_EDITOR
        //vertical = joystick.position.y;
        //horizontal = joystick.position.x;
#else
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
#endif

        if (vertical != 0.0f)
        {
            // acceleratoing
            if (Mathf.Abs(velocity) < maxVelocity)
                velocity += vertical * acceleration * Time.deltaTime;
        } else
        {
            // breaking
            if (velocity > breaking * Time.deltaTime)
                velocity -= breaking * Time.deltaTime;
            else if (velocity < -breaking * Time.deltaTime)
                velocity += breaking * Time.deltaTime;
        }
        
        wheelFL.m_dofData[0].m_desiredVelocity =
            wheelFR.m_dofData[0].m_desiredVelocity =
            wheelRL.m_dofData[0].m_desiredVelocity =
            wheelRR.m_dofData[0].m_desiredVelocity = velocity;

        wheelFL.SetMotorDesiredSpeed(0, velocity);
        wheelFR.SetMotorDesiredSpeed(0, velocity);
        wheelRL.SetMotorDesiredSpeed(0, velocity * frontRearRatio);
        wheelRR.SetMotorDesiredSpeed(0, velocity * frontRearRatio);
        
        steeringFL.m_dofData[0].m_desiredPosition =
            steeringFR.m_dofData[0].m_desiredPosition = horizontal * maxSteering;

        steeringFL.SetMotorDesiredPosition(0, steeringFL.m_dofData[0].m_desiredPosition);
        steeringFR.SetMotorDesiredPosition(0, steeringFR.m_dofData[0].m_desiredPosition);
    }
}
